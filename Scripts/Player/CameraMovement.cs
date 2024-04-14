using Godot;
using System;
using System.Linq.Expressions;

public partial class CameraMovement : Camera3D
{

	[Export] Node3D _focus;
	[Export] Vector2 mouse_sensitivity;
	[Export] float _zoomSpeed;
	[Export] bool _invertMouse = true;
	[Export(PropertyHint.Range, "5,100,")] float _maxZoom;
    [Export(PropertyHint.Range, "5,100,")] float _minZoom;
	[Export(PropertyHint.Range, "5,85,")] float _cameraAngle = 60;
	[Export(PropertyHint.Range, "0,359,")] float _cameraRotation = 0;
	[Export(PropertyHint.Range, "5,100,")] float _cameraDistance = 10;
	[Export] bool _allowCameraRotation = true;	
	[Export] bool _allowCameraTilt = true;
	[Export] bool _allowZoom = true;
	float _zoomInput;
	Vector2 _lastMousePosition = Vector2.Zero;
	Vector2 _mouseVelocity = Vector2.Zero;
	public Vector3 CameraForwards {get => (GlobalTransform.Basis.Z - GlobalTransform.Basis.Z.Project(Basis.Identity.Y)).Normalized();} // camera facing direction without any Y component - for player movement direction
	public float CameraRotation {get => _cameraRotation; }
	int MouseInversion 
	{ 
		get 
		{
			if (_invertMouse)
			{
				return -1;
			}
			return 1;
		}
	
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print(" " + new Vector2(Mathf.Tan(Mathf.DegToRad(180)), 1).Normalized());
        GD.Print(" " + new Vector2(Mathf.Tan(Mathf.DegToRad(60)), 1).Normalized());

        GD.Print(" " + new Vector2(Mathf.Tan(Mathf.DegToRad(300)), 1).Normalized());
    }

	public void RotateCamera(float angle)
	{
		_cameraRotation += angle;
	}
		public void PitchCamera(float angle)
	{
		_cameraAngle += angle;
	}

	public override void _Input(InputEvent @event)
{
    if (@event is InputEventMouseMotion mouseEvent)
	{
        _mouseVelocity = mouseEvent.Relative;
	}
	else if (@event is InputEventMouseButton mouseButton)
	{
		if (mouseButton.IsPressed())
		{
			if (mouseButton.ButtonIndex == MouseButton.WheelDown)
			{
				_zoomInput = 1;
			}
			else if (mouseButton.ButtonIndex == MouseButton.WheelUp)
			{
				_zoomInput = -1;
			}
		}
	}
}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (PlayerController.Control == PlayerController.ControlMode.Mixed)
		{

			if (Input.IsActionJustPressed("camera_modifier"))
			{
				Input.MouseMode = Input.MouseModeEnum.Captured;
			}
			else if (Input.IsActionJustReleased("camera_modifier"))
			{
                Input.MouseMode = Input.MouseModeEnum.Visible;
            }
		}

		//take inputs to modify values for camera angle and rotation
		if (PlayerController.Control==PlayerController.ControlMode.Player || (PlayerController.Control == PlayerController.ControlMode.Mixed && Input.IsActionPressed("camera_modifier")))
		{

			if (_allowCameraRotation)
			{
				RotateCamera(_mouseVelocity.X * (float)delta * mouse_sensitivity.X);
			}
			if (_allowCameraTilt)
			{
				PitchCamera(_mouseVelocity.Y * (float)delta * mouse_sensitivity.Y* MouseInversion);
			}
		}
		
		// update stored values
		if (_allowZoom)
		{
            _zoomInput *= (float)delta * _zoomSpeed;
            _cameraDistance = Mathf.Clamp(_cameraDistance + _zoomInput, _minZoom,_maxZoom);
		}
		_cameraAngle = Mathf.Clamp(_cameraAngle, 5f,85f);
		_cameraRotation = _cameraRotation % 360;
		
		// apply to camera position and rotation
		// the below works but feels slightly jank
		GlobalPosition = _focus?.Position + (new Vector3(0,Mathf.Tan(Mathf.DegToRad(_cameraAngle)),1) * Quaternion.FromEuler(new Vector3(0,Mathf.DegToRad(_cameraRotation),0))).Normalized() * _cameraDistance ?? GlobalPosition; 
		LookAt(_focus?.Position ?? Vector3.Zero, Basis.Identity.Y,false);
		// reset mouse velocity value as this only updates when the mouse moves, should be zero whenever its not updating
		 _mouseVelocity = Vector2.Zero;
		 _zoomInput = 0;
	}
}
