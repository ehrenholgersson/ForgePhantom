using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class Building : Resource, IDraggable
{
    [Flags] public enum LoadaedScenes {None, Mesh, Collider, Other }
    [Flags] public enum BuildingFunctions { None, Transport, Manufacture }

    //[Export] BuildingResource _resource;
    [Export] protected PackedScene _meshScene; // node containing model and animations
    [Export] protected PackedScene _colliderScene; // node containing collider
    [Export] protected PackedScene _otherScene; // node containing any other functionality
    
    [Export] public Texture2D Icon;
    [Export] public string BuildingName;
    [Export] BuildingFunctions _functions;
    
    public float Health = 10;

    Node3D _meshNode;
    Node3D _colliderNode;
    Node3D _otherNode;

    Vector3 _size = Vector3.Zero;
    List<MeshInstance3D> _meshInstances = new List<MeshInstance3D>();
    public Vector3 Size { get => (_size != Vector3.Zero) ? _size : CalculateSize(); }
    public BuildingFunctions Functions { get => _functions; }

    public delegate void NodeMessage(Node3D node);
    public NodeMessage OnNodeSet;

    public Building(Building original)
    {
        if (original == null)
        {
            return;
        }

        _meshScene = original._meshScene;
        _colliderScene = original._colliderScene;
        _otherScene = original._otherScene;

        _meshNode = original._meshNode;
        _colliderNode = original._colliderNode;
        _otherNode = original._otherNode;

        Icon = original.Icon;
        BuildingName = original.BuildingName;
        Health = original.Health;
        _size = original.Size;
    }

    public Building()
    {
        _size = Vector3.Zero;
        Health = 10;
    }

    public Node3D SpawnBuilding()
    {
        _size = Vector3.Zero;
        _meshInstances.Clear();

        // setup the mesh
        if (_meshScene != null)
        {
            _meshNode = _meshScene.Instantiate() as Node3D;
        }

        //setup collider
        if (_colliderScene!=null)
        {
            _colliderNode = _colliderScene.Instantiate() as Node3D;
        }

        // setup any other functionality
        if (_otherScene!= null)
        {
            _otherNode = _otherScene.Instantiate() as Node3D;           
        }

        var building = new BuildingNode(this);

        return building;
    }

    public void SetNode(Node3D node)
    {
        GD.Print("Set Node to " + node);
        OnNodeSet?.Invoke(node);
    }

    public void TakeNodes(BuildingNode main)
    {
        if (_meshNode!=null)
        {
            main.AddChild( _meshNode );
            GetMeshFromChildren(_meshNode.GetPath());
        }
        if (_colliderNode!=null)
        {
            main.AddChild(_colliderNode );
        }
        if ( _otherNode!=null)
        {
            main.AddChild(_otherNode);
        }
    }

    public Node3D GetMeshObject(Material material) // create an instance of the mesh/visible object only and return that node (for dragging building into place)
    {
        _meshNode = _meshScene.Instantiate() as Node3D;
        GameController.Singleton.AddChild(_meshNode);
        _meshInstances.Clear();
        GetMeshFromChildren(_meshNode.GetPath());
        SetMaterialOverride(material);
        GameController.Singleton.RemoveChild(_meshNode);
        return _meshNode;
    }

    public void SetColor(Color col)
    {
        //_meshInstance.Mesh.SurfaceGetMaterial(0).alb
        StandardMaterial3D material = new StandardMaterial3D();

        material.AlbedoColor = col;

        foreach (MeshInstance3D mesh in _meshInstances)
        {
            mesh.MaterialOverride = material;
        }
         
    }

    void GetMeshFromChildren(string node)
    {
        foreach (Node child in GameController.Singleton.GetNode(node).GetChildren())
        {
            if (child is MeshInstance3D)
            {
                _meshInstances.Add((MeshInstance3D)child);
            }
            if (child.GetChildCount() > 0)
            {
                GetMeshFromChildren(child.GetPath());
            }
        }
    }

    public void SetMaterialOverride(Material material)
    {
        if (_meshInstances.Count < 1 && _meshNode != null) 
        {
            GetMeshFromChildren(_meshNode.GetPath());
        }
        foreach (MeshInstance3D mesh in _meshInstances)
        {
            mesh.MaterialOverride = material;
        }
    }

    public void RemoveMaterialOverride()
    {
        foreach (MeshInstance3D mesh in _meshInstances)
        {
            mesh.MaterialOverride = null;
        }
    }

    public Vector3 CalculateSize() // Iterate through each mesh instance to determine the maximum size along each axis
    {
        if (_meshNode == null)
        {
            return Vector3.Zero;
        }
        Vector3 start = Vector3.Zero;
        Vector3 end = Vector3.Zero;
        Quaternion rotation = Quaternion.Identity;
        foreach (MeshInstance3D child in _meshInstances)
            if (child is MeshInstance3D)
            {
                var offset = child.GlobalPosition - _meshNode.GlobalPosition; // child may be a few levels deep into heiracy so can't just use position to get its releative location
                rotation = child.Basis.GetRotationQuaternion();

                var aabb = ((MeshInstance3D)child).GetAabb() * new Transform3D(child.GlobalBasis,Vector3.Zero);
                start.X = Mathf.Min(start.X, (aabb.GetCenter() + offset).X - aabb.Size.X / 2);
                start.Y = Mathf.Min(start.Y, (aabb.GetCenter() + offset).Y - aabb.Size.Y / 2);
                start.Z = Mathf.Min(start.Z, (aabb.GetCenter() + offset).Z - aabb.Size.Z / 2);
                end.X = Mathf.Max(end.X, (aabb.GetCenter() + offset).X + aabb.Size.X / 2);
                end.Y = Mathf.Max(end.Y, (aabb.GetCenter() + offset).Y + aabb.Size.Y / 2);
                end.Z = Mathf.Max(end.Z, (aabb.GetCenter() + offset).Z + aabb.Size.Z / 2);
            }
        _size.X = end.X - start.X;
        _size.Y = end.Y - start.Y;
        _size.Z = end.Z - start.Z;
        //_size *= rotation;

        GD.Print("Building " + BuildingName + " size is " + _size + ", center is " + (end + start) / 2);

        //_meshNode.Position -= (end + start) / 2; // should do this somewhere more obvious, also seens I'm compensation for an adjustment made in toolbar.cs (making work for self)

        return _size;
    }
}
