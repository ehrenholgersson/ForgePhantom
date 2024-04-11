using Godot;
using System;

public partial class Building : StaticBody3D
{
    [Export] BuildingResource _resource;
    MeshInstance3D _meshInstance;
    CollisionShape3D _collider;

    public override void _Ready()
    {
        _resource = _resource.newInstance(); ; // create an instance of our item
        if (GetChildren().Count > 0)
        {
            _meshInstance = GetNode<MeshInstance3D>("Model");
        }
        if (_meshInstance == null)
        {
            _meshInstance = new MeshInstance3D();
            _meshInstance.Name = "Model";
            AddChild(_meshInstance);
        }
        if (_resource != null)
        {
            _meshInstance.Mesh = _resource.Model;
            _meshInstance.CreateConvexCollision(true, true);
            _collider = GetNode<CollisionShape3D>("Model/Model_col/CollisionShape3D");
            _collider.GetParent().RemoveChild(_collider);
            AddChild(_collider);
            GetNode("Model/Model_col")?.QueueFree();
        }
    }

    public void SetColor(Color col)
    {
        //_meshInstance.Mesh.SurfaceGetMaterial(0).alb
        StandardMaterial3D material = new StandardMaterial3D();

        material.AlbedoColor = col;
        _meshInstance.MaterialOverride = material;
    }

    public void SetResource(BuildingResource res)
    {
        _resource = res;
    }
}
