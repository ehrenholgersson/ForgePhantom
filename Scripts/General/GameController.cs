using Godot;
using System;

public partial class GameController : Node
{
	[Export]PlayerController _player;
	[Export]Camera3D _camera;
	static GameController _instance;
	
	public static PlayerController MainPlayer {get => _instance?._player;}
	public static Camera3D MainCamera { get => _instance?._camera;}
	public static GameController Singleton { get => _instance; }
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (_instance == null)
		{
			_instance = this;
		}
		else 
		{
			// I actually don't think we can just remove a script like we would a component in unity?
			// could destroy the node here, but this is intended to be attached the the base node for the scene, so that seems like a dangerous move
		}

		// if variables are not set in the editor, try to find them ourselves
		if (_camera == null)
		{
			Camera3D camera = GetNode("Camera3D") as Camera3D;
		}
		if (_player == null)
		{
			_player = GetNode("Player") as PlayerController;
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
