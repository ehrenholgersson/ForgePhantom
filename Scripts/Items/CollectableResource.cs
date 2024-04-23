using Godot;
using System;

[GlobalClass]
public partial class CollectableResource : Resource, IDraggable
{
    [Export] public Texture2D Icon;
    [Export] public Mesh _model;
    [Export] public PackedScene _object;
    [Export] protected string _type;
    public float Health = 10;
    Vector3 _size = Vector3.Zero;

    public Vector3 Size { get => (_size != Vector3.Zero) ? _size : CalculateSize(); }
    public PackedScene Object { get => _object; }
    public string Type { get => _type; }

    public CollectableResource()
	{
        Icon = null;
        _model = null;
        Health = 10;
	}
	public CollectableResource(CollectableResource original)
	{
        Icon = original.Icon;
        Health = original.Health;
        _model = original._model;
        _type = original._type;
    }

    public Node3D GetMeshObject(Material material)
    {
        MeshInstance3D mesh = new MeshInstance3D();
        mesh.Mesh = _model;
        mesh.MaterialOverride = material;
        return mesh;
    }

	public CollectableResource newInstance()
	{
		return new CollectableResource((CollectableResource)this);
	}

    Vector3 CalculateSize()
    {
        if (_model != null)
        {
            _size = _model.GetAabb().Size;
        }
        return _size;
    }
}