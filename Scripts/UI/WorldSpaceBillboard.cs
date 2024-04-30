using Godot;
using System;

public partial class WorldSpaceBillboard : Node3D
{
	static string _prefabPath = "res://Scenes/Prefabs/billboard.tscn";

    protected MeshInstance3D Mesh;
    protected Node3D Target;
    protected Vector3 Offset = new Vector3(0,2,0);
    public RichTextLabel Text;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Mesh = GetNode<MeshInstance3D>("MeshInstance3D");
        Text = GetNode<RichTextLabel>("SubViewport/RichTextLabel");
        // workaround for godot viewport bug....
        RenderingServer.FramePostDraw += OnFramePostDraw;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
		Mesh.GlobalPosition = Target.GlobalPosition + Offset;
		LookAt(GameController.MainCamera?.GlobalPosition?? Vector3.Zero, GameController.MainCamera?.Basis.Y ?? Vector3.Up);
    }

	public static WorldSpaceBillboard New3DBillboard(Node3D target)
	{
        WorldSpaceBillboard instance = GD.Load<PackedScene>(_prefabPath).Instantiate() as WorldSpaceBillboard;
		target.AddChild(instance); // is there a better way to get the root node??
		instance.Target = target;
		return instance;
	}

    public static WorldSpaceBillboard New3DBillboard(Node3D target, Vector3 offset)
    {
        WorldSpaceBillboard instance = GD.Load<PackedScene>(_prefabPath).Instantiate() as WorldSpaceBillboard;
        target.AddChild(instance);
        instance.Name = "Billboard";
        instance.Target = target;
        instance.Offset = offset;
        return instance;
    }

    public static WorldSpaceBillboard New3DBillboard(Node3D target, Vector3 offset, string name)
    {
        var prefab = ResourceLoader.Load<PackedScene>(_prefabPath).Instantiate<Node3D>();
        target.AddChild(prefab);
        WorldSpaceBillboard instance = (WorldSpaceBillboard)prefab;//= GD.Load<PackedScene>(_prefabPath).Instantiate() as WorldSpaceBillboard;
        instance.Name = name;
        instance.Target = target;
        instance.Offset = offset;
        return instance;
    }
    // as per https://github.com/godotengine/godot/issues/66247#issuecomment-1483786200
    // reconfigure materials in code to fix viewport not loading correctly
    private void OnFramePostDraw()
    {
        SubViewport subViewport = GetNode<SubViewport>("SubViewport");
        var viewportTexture = subViewport.GetTexture();

        StandardMaterial3D mat = new()
        {
            AlbedoTexture = viewportTexture
        };

        Mesh.SetSurfaceOverrideMaterial(0, mat);

        RenderingServer.FramePostDraw -= OnFramePostDraw;
    }

}
