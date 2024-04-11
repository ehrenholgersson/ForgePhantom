using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class ToolBar : VBoxContainer
{
	[Export] float _buttonHeldTime;
    [Export] PackedScene _itemDragPrefab;
	[Export] Button _buildMenuButton;
    [Export] Button _inventoryButton;
	[Export] Control _inventoryBar;
    [Export] Control _buildMenu;
    [Export] Node _worldScene;
	[Export] float _gridSize;
	[Export] ColorPicker _colorPicker;

    float _buttonTimer = 0;

	List<Button> _inventorybuttons = new List<Button>();
	int _selectedButton;

    [Export] Godot.Collections.Array<BuildingResource> _availableBuildings = new Godot.Collections.Array<BuildingResource>();

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

                }

				i++;
			}
		}

        _buildMenuButton.Pressed += SelectBuildMenu;
		_inventoryButton.Pressed += SelectInventory;
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

    public void OnBuildButtonClick(int index)
    {
        GD.Print("pressed button " + index);
		if (_availableBuildings.Count > index)
        _item = _availableBuildings[index];
        _selectedButton = index;

        _worldScene.AddChild(_itemDragPrefab.Instantiate());
        _tempItem = _worldScene.GetNode("Drag_Item") as Node3D;
        MeshInstance3D mesh = _tempItem.GetNode<MeshInstance3D>("Model");
        mesh.Mesh = _item?.Model;
        mesh.MaterialOverride = _tempItemMat;
		_tempItemSize = mesh.GetAabb().Size;
		// "Rotate" the size to match our eventual basis
        if (Mathf.Abs(_tempItemRotation) == 90 || Mathf.Abs(_tempItemRotation) == 270)
        {
            float X = _tempItemSize.X;
            _tempItemSize.X = _tempItemSize.Y;
            _tempItemSize.Z = X;
        }
        GD.Print("size is " + _tempItemSize);
		GD.Print("rotation is " + _tempItemRotation);
        _drag = true;
    }

	void SelectBuildMenu()
	{
		_buildMenu.Visible = true;
		_inventoryBar.Visible = false;
	}

	void SelectInventory()
	{
        _buildMenu.Visible = false;
        _inventoryBar.Visible = true;
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
						MeshInstance3D mesh = _tempItem.GetNode<MeshInstance3D>("Model");
						mesh.Mesh = _item?.Model;
						mesh.MaterialOverride = _tempItemMat;
                        _tempItemSize = mesh.GetAabb().Size;
						if (_tempItemRotation == 90 || _tempItemRotation == 270)
						{
							float X = _tempItemSize.X;
							_tempItemSize.X = _tempItemSize.Z;
							_tempItemSize.Z = X;
						}
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
                        if (Mathf.Abs(mousePos.X - buttonPos.X) < button.Size.X/2 && Mathf.Abs(mousePos.Y - buttonPos.Y) < button.Size.Y / 2)
						{
							GD.Print("place in slot " + _inventorybuttons.IndexOf(button));
							// attempt to place in new slot
							if (GameController.MainPlayer.PickUpObject((CollectableResource)_item,_inventorybuttons.IndexOf(button)))
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
			if (_item is BuildingResource)
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
                    Building newObject = new Building();
                    newObject.SetResource((BuildingResource)_item);
                    _worldScene.AddChild(newObject);
                    newObject.GlobalPosition = _tempItem.GlobalPosition;
					newObject.GlobalRotation = _tempItem.GlobalRotation;
					newObject.SetColor(_colorPicker?.Color ?? new Color(1, 1, 1, 1));
                    _drag = false;
                    // kill our placeholder
                    _tempItem.QueueFree();
                    _item = null;
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
				// swap around our size values to match
                float X = _tempItemSize.X;
                _tempItemSize.X = _tempItemSize.Z;
                _tempItemSize.Z = X;

				_tempItemRotation = rotation;
                _tempItem.GlobalRotationDegrees = new Vector3(0, _tempItemRotation, 0);
			}
        }
        else
        {
			float rayLength = 500;

            // move the placeholder object wherever the mouse is pointing
            var spaceState = _tempItem.GetWorld3D().DirectSpaceState;
			var origin = GameController.MainCamera?.ProjectRayOrigin(_mousePosition) ?? Vector3.Zero;
			var end = (origin + GameController.MainCamera.ProjectRayNormal(_mousePosition) * rayLength);
			var query = PhysicsRayQueryParameters3D.Create(origin, end);
			var result = spaceState.IntersectRay(query);
			Vector3 position = (Vector3)result["position"];

			if (_item is CollectableResource)
			{
				// move our position along the collision normal half a block so we aren't intersecting stuff (hopefully)
				position += (Vector3)result["normal"] * 0.6f;
			}
			else if (_item is BuildingResource)
			{
				//snap building to grid, adjusting position based on the ray intersection normal and mesh size
				// On the normal axis of the ray intersection (the direction of the face we hit), we want to round to the closest value aligned with our grid, then pull back 1/2 the size of our item so that it doesn't clip through the floor/wall whatever
				// for the other axis we are just rounding downward to the closest multiple of our grid size
				Vector3 normal = (Vector3)result["normal"];
				if (MathF.Abs(normal.X) > MathF.Abs(normal.Y) && MathF.Abs(normal.X) > MathF.Abs(normal.Z))
				{
					float normalDirection = (normal.X) / MathF.Abs(normal.X);
					position = new Vector3(Mathf.Round((position.X) / _gridSize) * _gridSize + _tempItemSize.X / 2 * normalDirection, Mathf.Floor((position.Y) / _gridSize) * _gridSize, Mathf.Floor((position.Z) / _gridSize) * _gridSize);
					position += new Vector3(0, (_tempItemSize.Y / 2) % _gridSize, (_tempItemSize.Z / 2) % _gridSize);
				}
				else if (MathF.Abs(normal.Y) > MathF.Abs(normal.Z))
				{
					float normalDirection = (normal.Y) / MathF.Abs(normal.Y);
					position = new Vector3(Mathf.Floor((position.X) / _gridSize) * _gridSize, Mathf.Round((position.Y) / _gridSize) * _gridSize + _tempItemSize.Y / 2 * normalDirection, Mathf.Floor((position.Z) / _gridSize) * _gridSize);
					position += new Vector3((_tempItemSize.X / 2) % _gridSize, 0, (_tempItemSize.Z / 2) % _gridSize);
				}
				else
				{
					float normalDirection = (normal.Z) / MathF.Abs(normal.Z);
					position = new Vector3(Mathf.Floor((position.X) / _gridSize) * _gridSize, Mathf.Floor((position.Y) / _gridSize) * _gridSize, Mathf.Round((position.Z) / _gridSize) * _gridSize + (_tempItemSize.Z / 2) * normalDirection);
					position += new Vector3((_tempItemSize.X / 2) % _gridSize, (_tempItemSize.Y / 2) % _gridSize, 0);
				}
			}
            _tempItem.GlobalPosition = position;           
            _inputRotation = _tempItemRotation;
        }

    }
}
