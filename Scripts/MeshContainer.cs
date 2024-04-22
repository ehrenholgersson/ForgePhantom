using Godot;
using System.Collections.Generic;

// Script to handle nodes containing only MeshInstances (maybe colliders?)
public partial class MeshContainer : Node3D
{
    Vector3 _size = Vector3.Zero;
    public Vector3 Size { get => (_size != Vector3.Zero) ? _size : CalculateSize(); }
    List<MeshInstance3D> _meshInstances = new List<MeshInstance3D>();

    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        if (_meshInstances.Count < 1)
        {
            GetMeshes();
        }
        CalculateSize();
    } 

    void GetMeshes()
    {
        _meshInstances.Clear();
        foreach (Node child in GetChildren())
        {
            if (child is MeshInstance3D)
            {
                _meshInstances.Add((MeshInstance3D)child);
            }
            if (child.GetChildCount() > 0)
            {
                GetChildMesh(child.GetPath());
            }
        }
        GD.Print("found " + _meshInstances.Count + " meshes in " + Name);
    }

    void GetChildMesh(string node)
    {
        foreach (Node child in GetNode(node).GetChildren())
        {
            if (child is MeshInstance3D)
            {
                _meshInstances.Add((MeshInstance3D)child);
            }
            if (child.GetChildCount() > 0)
            {
                GetChildMesh(child.GetPath());
            }
        }
    }

    public void SetMaterialOverride(Material material)
    {
        if (_meshInstances.Count < 1)
        {
            GetMeshes();
        }
        foreach (MeshInstance3D mesh in  _meshInstances)
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

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public Vector3 CalculateSize() // Iterate through each mesh instance to determine the maximum size along each axis
    {
        Vector3 start = Vector3.Zero;
        Vector3 end = Vector3.Zero;
        foreach (MeshInstance3D child in _meshInstances)
            if (child is MeshInstance3D)
            {
                var offset = child.GlobalPosition - GlobalPosition; // child may be a few levels deep into heiracy so can't just use position to get its releative location

                var aabb = ((MeshInstance3D)child).GetAabb();
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

        GD.Print("Building " + Name + " size is " + _size+ ", center is " + (end + start)/2 );

        Position -= (end + start) / 2; // should do this somewhere mopre obvious
        return _size;
    }
}
