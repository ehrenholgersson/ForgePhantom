using Godot;
using System;
using System.Collections.Generic;
using System.Net;

public partial class ToolBar : VBoxContainer
{
	[Export] float _buttonHeldTime;
    [Export] PackedScene _itemDragPrefab;
	[Export] Button _buildMenuButton;
    [Export] Button _inventoryButton;
    [Export] Button _linkButton;
    [Export] Control _inventoryBar;
    [Export] Control _buildMenu;
    [Export] Node _worldScene;
	[Export] float _gridSize;
	[Export] ColorPicker _colorPicker;

    [Export] Control _LinkBar;
    Label _linkText;
    Node3D _linkOrigin;

    float _terrainHeight = 1; //Objects snap to this on y axis instead of _gridSize as to match the terrain levels, is not modified by terrain itself so need to change manually when required

    float _buttonTimer = 0;

	List<Button> _inventorybuttons = new List<Button>();
	int _selectedButton;

    [Export] Godot.Collections.Array<Building> _availableBuildings = new Godot.Collections.Array<Building>();

	bool _drag;
	IDraggable _item;

	Node3D _tempItem;
	Material _tempItemMat;

	Vector2 _mousePosition;
	Vector2 _lastMousePosition;
    Vector2 _mouseVelocity = Vector2.Zero;
    Vector3 _tempItemSize;
	float _inputRotation;
	float _tempItemRotation;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		_worldScene = GetNode("/root/Scene");
		foreach (Node child in GetNode("Inventory").GetChildren())
		{
			if (child is Button)
			{
				_inventorybuttons.Add((Button)child);
				((Button)child).ButtonDown += () => OnInventoryButtonClick(_inventorybuttons.IndexOf((Button)child));

			}

		}

		_linkText = _LinkBar.GetNode("LinkText") as Label;

			int i = 0;
		foreach (Node child in GetNode("BuildMenu/Buttons").GetChildren())
		{

			if (child is Button)
			{
				((Button)child).ButtonDown += () => OnBuildButtonClick((int)((Button)child).GetMeta("buttonIndex") );
				if (_availableBuildings.Count > i)
				{
					((Button)child).Icon = _availableBuildings[i].Icon;
                    ((Button)child).Text = _availableBuildings[i].BuildingName;
					//_availableBuildings[i].ScaleMesh(_availableBuildings[i].MinHeight);
                }
				i++;
			}
		}

        _buildMenuButton.Pressed += SelectBuildMenu;
		_inventoryButton.Pressed += SelectInventory;
		_linkButton.Pressed += SelectLink;
        _tempItemMat = GD.Load("res://Art/Materials/Translucent.tres") as Material;
    }

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion mouseEvent)
		{
			_mouseVelocity = mouseEvent.Relative;
		}
	}

	public void OnInventoryButtonClick(int index)
	{
		GD.Print("pressed button " + index);
		_buttonTimer = Time.GetTicksMsec() + _buttonHeldTime * 1000; // convert sec to msec
		_item = GameController.MainPlayer.GetInventoryItem(index);
		_selectedButton = index;
	}

    public void OnBuildButtonClick(int index) // need to add some comments in here
    {
        GD.Print("pressed button " + index);
		if (_availableBuildings.Count > index)
        _item = _availableBuildings[index];
        _selectedButton = index;

		if (IsInstanceValid(_tempItem))
		{
			_tempItem.QueueFree();
		}
		_tempItem = _itemDragPrefab.Instantiate() as Node3D;
        _worldScene.AddChild(_tempItem);

		var mesh = _item?.GetMeshObject(_tempItemMat);
		_tempItem.AddChild(mesh);
		_tempItemSize = _item.Size;

        // "Rotate" the size to match our eventual basis
        //if (Mathf.Abs(_tempItemRotation) == 90 || Mathf.Abs(_tempItemRotation) == 270)
        //{
        //    float X = _tempItemSize.X;
        //    _tempItemSize.X = _tempItemSize.Z; // this was y, pretty sure z is correct
        //    _tempItemSize.Z = X;
        //}

        GD.Print("size is " + _tempItemSize);
		GD.Print("rotation is " + _tempItemRotation);
        _drag = true;
    }

	void SelectBuildMenu()
	{
		_buildMenu.Visible = true;
		_inventoryBar.Visible = false;
		_LinkBar.Visible = false;
	}

	void SelectInventory()
	{
        _buildMenu.Visible = false;
        _inventoryBar.Visible = true;
        _LinkBar.Visible = false;
    }

	void SelectLink()
	{
		_linkOrigin = null;
        _buildMenu.Visible = false;
        _inventoryBar.Visible = false;
        _LinkBar.Visible = true;
		_linkText.Text = "Select origin node to link";
    }

    public override void _PhysicsProcess(double delta)
	{
        _mousePosition = GetViewport().GetMousePosition();

		if (_inventoryBar.Visible)
		{
			if (Input.GetMouseButtonMask() == MouseButtonMask.Left && _item is CollectableResource)
			{
				if (Time.GetTicksMsec() > _buttonTimer)
				{

					// button is "held", drag item out of inventory
					// spawn a placeholder object if not already done
					if (!_drag)
					{
						//tempItem = itemDragPrefab.Instantiate() as Node3D;
						_worldScene.AddChild(_itemDragPrefab.Instantiate());
						_tempItem = _worldScene.GetNode("Drag_Item") as Node3D;
						_tempItem.AddChild(_item.GetMeshObject(_tempItemMat));
						//MeshInstance3D mesh = _tempItem.GetNode<MeshInstance3D>("Model");
						//mesh.Mesh = _item?.Model;
						//mesh.MaterialOverride = _tempItemMat;
						_tempItemSize = _item.Size;
						//if (_tempItemRotation == 90 || _tempItemRotation == 270)
						//{
						//	float X = _tempItemSize.X;
						//	_tempItemSize.X = _tempItemSize.Z;
						//	_tempItemSize.Z = X;
						//}
						_drag = true;
					}

					// move the placeholder object wherever the mouse is pointing
					MoveObjectPlacement();
				}
			}
			else if (_buttonTimer > 0)
			{
				_buttonTimer = 0;
				if (_drag)
				{
					// remove object from player inventory
					GameController.MainPlayer.RemoveObject(_selectedButton);
					_inventorybuttons[_selectedButton].Icon = null;

					// check if we are over another inventory slot
					foreach (Button button in _inventorybuttons)
					{
						Vector2 mousePos = button.GetGlobalMousePosition();
						Vector2 buttonPos = button.GlobalPosition + button.Size / 2;
						if (Mathf.Abs(mousePos.X - buttonPos.X) < button.Size.X / 2 && Mathf.Abs(mousePos.Y - buttonPos.Y) < button.Size.Y / 2)
						{
							GD.Print("place in slot " + _inventorybuttons.IndexOf(button));
							// attempt to place in new slot
							if (GameController.MainPlayer.PickUpObject((CollectableResource)_item, _inventorybuttons.IndexOf(button)))
							{
								goto ITEMCLEANUP; // C# or QBASIC?? - Skip creating new object instance and go to the part where we delete the placeholder
							}
						}
						else
						{
							//GD.Print("mouse at " + mousePos + " outside bounds of button " + _inventorybuttons.IndexOf(button) + " at " + buttonPos + " +/- " + button.Size);
						}

					}

					// attempt to place item in world
					Collectable newObject = new Collectable();
					newObject.SetItem((CollectableResource)_item);
					_worldScene.AddChild(newObject);
					newObject.GlobalPosition = _tempItem.GlobalPosition;

				ITEMCLEANUP:
					_drag = false;
					// kill our placeholder
					_tempItem.QueueFree();
				}
				else
				{
					// button was "clicked" and not held, select/equip item or whatever is appropriate
				}
				_item = null;
			}
		}
		else if (_buildMenu.Visible)
		{
			if (_item is Building)
			{

				MoveObjectPlacement();

				if (_drag) //first click not released
				{
					if (Input.GetMouseButtonMask() != MouseButtonMask.Left)
					{
						_drag = false;
					}

				}
				else if (Input.GetMouseButtonMask() == MouseButtonMask.Left)
				{
					// place the thing
					BuildingNode newObject = ((Building)_item).SpawnBuilding() as BuildingNode;
					//newObject.SetResource((BuildingResource)_item);
					//_worldScene.AddChild(newObject);
					newObject.GlobalPosition = _tempItem.GlobalPosition;
					newObject.GlobalRotation = _tempItem.GlobalRotation;
					newObject.Building.SetColor(_colorPicker?.Color ?? new Color(1, 1, 1, 1));
					_drag = false;
					// kill our placeholder
					_tempItem.QueueFree();
					_item = null;
				}
			}
		}
		else if (_LinkBar.Visible)
		{
			if (Input.GetMouseButtonMask() == MouseButtonMask.Left)
			{
				if (_linkOrigin == null)
				{
					// do a raycast and check if we hit a transporter
					float rayLength = 500;

					// do raycast to check where mouse is pointing
					var spaceState = GameController.MainPlayer.GetWorld3D().DirectSpaceState;
					var origin = GameController.MainCamera?.ProjectRayOrigin(_mousePosition) ?? Vector3.Zero;
					var end = (origin + GameController.MainCamera.ProjectRayNormal(_mousePosition) * rayLength);
					var query = PhysicsRayQueryParameters3D.Create(origin, end);
					var result = spaceState.IntersectRay(query);

					if (result.TryGetValue("collider", out var collider))
					{
						Node bld = ((Node3D)collider).GetParent(); // surely raycast won't hit anything not in 3D space, so we should always be OK to cast to node3D?? probably....
						if (bld is BuildingNode)
						{
							_linkOrigin = bld as Node3D;
							_linkText.Text = "Select destination node to connect to " + ((BuildingNode)bld).Building?.BuildingName;
						}
					}

				}
				else
				{
					// do a raycast and check if we hit a transporter
					float rayLength = 500;

                    // do raycast to check where mouse is pointing
                    var spaceState = GameController.MainPlayer.GetWorld3D().DirectSpaceState;
                    var origin = GameController.MainCamera?.ProjectRayOrigin(_mousePosition) ?? Vector3.Zero;
					var end = (origin + GameController.MainCamera.ProjectRayNormal(_mousePosition) * rayLength);
					var query = PhysicsRayQueryParameters3D.Create(origin, end);
					var result = spaceState.IntersectRay(query);

					if (result.TryGetValue("collider", out var collider))
					{
						Node bld = ((Node3D)collider).GetParent(); // surely raycast won't hit anything not in 3D space, so we should always be OK to cast to node3D?? probably....
						if (bld is BuildingNode && bld != _linkOrigin)
						{
							// if so set the transporter as destination node on origin and reset text/origin
							((BuildingNode)_linkOrigin).Building?.SetNode((Node3D)bld);
							_linkOrigin = null;
                            _linkText.Text = "Select origin node to link";
							SelectBuildMenu(); // as we aren't checking for a sinle click its easiest just to jump back to build menu rather than inevitably "click on" the same building again and set it as origin unintentionally
                        }
					}
				}
			}

		}
	}

	void MoveObjectPlacement()
	{
        if (Input.GetMouseButtonMask() == MouseButtonMask.Right)
        {
			float rotation;

            _inputRotation += _mouseVelocity.X;
            rotation = (Mathf.Round(_inputRotation / 90) * 90)%360;
			if (rotation != _tempItemRotation)
			{
				//// swap around our size values to match
    //            float X = _tempItemSize.X;
    //            _tempItemSize.X = _tempItemSize.Z;
    //            _tempItemSize.Z = X;

				_tempItemRotation = rotation;
                _tempItem.GlobalRotationDegrees = new Vector3(0, _tempItemRotation, 0);
				((Building)_item).CalculateSize();
				_tempItemSize = _item.Size;
			}
        }
        else
        {
			float rayLength = 500;

            // do raycast to check where mouse is pointing
            var spaceState = _tempItem.GetWorld3D().DirectSpaceState;
			var origin = GameController.MainCamera?.ProjectRayOrigin(_mousePosition) ?? Vector3.Zero;
			var end = (origin + GameController.MainCamera.ProjectRayNormal(_mousePosition) * rayLength);
			var query = PhysicsRayQueryParameters3D.Create(origin, end);
			var result = spaceState.IntersectRay(query);

			// get the result and move _tempItem there, unless we didn't hit anything
			Vector3 position = Vector3.Zero;
			if (result.TryGetValue("position", out var pos))
			{
				position = (Vector3)pos;
			}
			else
			{
				return;
			}

			if (_item is CollectableResource)
			{
				// move our position along the collision normal half a block so we aren't intersecting stuff (hopefully)
				position += (Vector3)result["normal"] * 0.6f;
			}
			else if (_item is Building)
			{
				//snap building to grid, adjusting position based on the ray intersection normal and mesh size
				// On the normal axis of the ray intersection (the direction of the face we hit), we want to round to the closest value aligned with our grid, then pull back 1/2 the size of our item so that it doesn't clip through the floor/wall whatever
				// for the other axis we are just rounding downward to the closest multiple of our grid size
				Vector3 normal = (Vector3)result["normal"];
				if (MathF.Abs(normal.X) > MathF.Abs(normal.Y) && MathF.Abs(normal.X) > MathF.Abs(normal.Z))
				{
					float normalDirection = (normal.X) / MathF.Abs(normal.X);
					position = new Vector3(Mathf.Round((position.X) / _gridSize) * _gridSize + _tempItemSize.X / 2 * normalDirection, Mathf.Floor((position.Y) / _terrainHeight) * _terrainHeight, Mathf.Floor((position.Z) / _gridSize) * _gridSize);
					position += new Vector3(0, (_tempItemSize.Y / 2) % _terrainHeight, (_tempItemSize.Z / 2) % _gridSize);
				}
				else if (MathF.Abs(normal.Y) > MathF.Abs(normal.Z))
				{
					float normalDirection = (normal.Y) / MathF.Abs(normal.Y);
					position = new Vector3(Mathf.Floor((position.X) / _gridSize) * _gridSize, Mathf.Round((position.Y) / _terrainHeight) * _terrainHeight + _tempItemSize.Y / 2 * normalDirection, Mathf.Floor((position.Z) / _gridSize) * _gridSize);
					position += new Vector3((_tempItemSize.X / 2) % _gridSize, 0, (_tempItemSize.Z / 2) % _gridSize);
				}
				else
				{
					float normalDirection = (normal.Z) / MathF.Abs(normal.Z);
					position = new Vector3(Mathf.Floor((position.X) / _gridSize) * _gridSize, Mathf.Floor((position.Y) / _terrainHeight) * _terrainHeight, Mathf.Round((position.Z) / _gridSize) * _gridSize + (_tempItemSize.Z / 2) * normalDirection);
					position += new Vector3((_tempItemSize.X / 2) % _gridSize, (_tempItemSize.Y / 2) % _terrainHeight, 0);
				}
			}
            _tempItem.GlobalPosition = position;           
            _inputRotation = _tempItemRotation;
        }

    }
}
