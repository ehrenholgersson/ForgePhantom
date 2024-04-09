using Godot;
using System;

public partial class Building : RigidBody3D
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
            GD.Print(_meshInstance.Name);

            AddChild(_meshInstance);
        }
        if (_resource != null)
        {
            _meshInstance.Mesh = _resource.model;
            _meshInstance.CreateConvexCollision(true, true);
            GD.Print(_meshInstance.Name + '/' + _meshInstance.GetChild(0).Name + '/' + _meshInstance.GetChild(0).GetChild(0).Name);
            _collider = GetNode<CollisionShape3D>("Model/Model_col/CollisionShape3D");
            _collider.GetParent().RemoveChild(_collider);
            AddChild(_collider);
            GetNode("Model/Model_col")?.QueueFree();
        }
    }

    public void SetResource(BuildingResource res)
    {
        _resource = res;
    }
}
