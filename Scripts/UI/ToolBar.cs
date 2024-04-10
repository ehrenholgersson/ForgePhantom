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

    float _buttonTimer = 0;

	List<Button> _inventorybuttons = new List<Button>();
	int _selectedButton;

    [Export] Godot.Collections.Array<BuildingResource> _availableBuildings = new Godot.Collections.Array<BuildingResource>();

	bool _drag;
	IDraggable _item;

	Node3D _tempItem;
	Material _tempItemMat;
	Vector2 _mousePosition;
	Vector3 _tempItemSize;

	public int number(int theNumber)
	{
		return theNumber;
	}

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
		foreach (Node child in GetNode("BuildMenu").GetChildren())
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
							GD.Print("mouse at " + mousePos + " outside bounds of button " + _inventorybuttons.IndexOf(button) + " at " + buttonPos + " +/- " + button.Size);
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
				GD.Print("button released");
			}
		}
		else if (_buildMenu.Visible)
		{
			if (_item is BuildingResource)
			{
				// feel like there is a neater way to do this
				//Get a vector3 where each axis is +/- 1 depending on the direction from the placeholder item to the camera
				Vector3 cameraDirection = _tempItem.GlobalPosition - GameController.MainCamera.GlobalPosition;
				if (cameraDirection.X != 0)
				{
					cameraDirection.X /= MathF.Abs(cameraDirection.X);
				}
				if (cameraDirection.Y != 0)
				{
					cameraDirection.Y /= MathF.Abs(cameraDirection.Y);
				}
				if (cameraDirection.Z != 0)
				{
					cameraDirection.Z /= MathF.Abs(cameraDirection.Z);
				}

				MoveObjectPlacement();
				// snap to "grid"
				//_tempItem.GlobalPosition = new Vector3(Mathf.Floor((_tempItem.GlobalPosition.X)/_gridSize)* _gridSize, Mathf.Round((_tempItem.GlobalPosition.Y)/_gridSize)*_gridSize + _tempItemSize.Y / 2, Mathf.Floor((_tempItem.GlobalPosition.Z)/_gridSize)*_gridSize);
				// we have snapped the center of our building to the grid, but we want to line up the edges
//				_tempItem.GlobalPosition += new Vector3((_tempItemSize.X / 2) % _gridSize, 0, (_tempItemSize.Z / 2) % _gridSize);

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
        float rayLength = 500;

        // move the placeholder object wherever the mouse is pointing
        var spaceState = _tempItem.GetWorld3D().DirectSpaceState;
        var origin = GameController.MainCamera?.ProjectRayOrigin(_mousePosition) ?? Vector3.Zero;
        var end = (origin + GameController.MainCamera.ProjectRayNormal(_mousePosition) * rayLength);
        var query = PhysicsRayQueryParameters3D.Create(origin, end);
        var result = spaceState.IntersectRay(query);
        Vector3 position = (Vector3)result["position"];

		// move our position along the collision normal half a block so we aren't intersecting stuff (hopefully)
		if (_item is CollectableResource)
		{
			position += (Vector3)result["normal"] * 0.6f;
		}
		// or snap building to grid, adjusting position based on the collision normal and mesh size
		else if (_item is BuildingResource) 
		{
			Vector3 normal = (Vector3)result["normal"];

            // feel like there is a neater way to do this
            //Get a vector3 where each axis is +/- 1 depending on the direction from the placeholder item to the camera
            Vector3 cameraDirection =  GameController.MainCamera.GlobalPosition - _tempItem.GlobalPosition;
            if (cameraDirection.X != 0)
            {
                cameraDirection.X /= MathF.Abs(cameraDirection.X);
            }
            if (cameraDirection.Y != 0)
            {
                cameraDirection.Y /= MathF.Abs(cameraDirection.Y);
            }
            if (cameraDirection.Z != 0)
            {
                cameraDirection.Z /= MathF.Abs(cameraDirection.Z);
				GD.Print("Z "+cameraDirection.Z);
            }
			GD.Print(normal);

            if (MathF.Abs(normal.X) > MathF.Abs(normal.Y) && MathF.Abs(normal.X) > MathF.Abs(normal.Z))
			{
                position = new Vector3(Mathf.Round((position.X) / _gridSize) * _gridSize + _tempItemSize.X / 2 * cameraDirection.X, Mathf.Floor((position.Y) / _gridSize) * _gridSize, Mathf.Floor((position.Z) / _gridSize) * _gridSize);
                position += new Vector3(0, (_tempItemSize.Y / 2) % _gridSize, (_tempItemSize.Z / 2) % _gridSize);
            }
			else if (MathF.Abs(normal.Y) > MathF.Abs(normal.Z))
			{
                position = new Vector3(Mathf.Floor((position.X) / _gridSize) * _gridSize, Mathf.Round((position.Y) / _gridSize) * _gridSize + _tempItemSize.Y / 2 * cameraDirection.Y, Mathf.Floor((position.Z) / _gridSize) * _gridSize);
                position += new Vector3((_tempItemSize.X / 2) % _gridSize, 0, (_tempItemSize.Z / 2) % _gridSize);
            }
			else
			{
				position = new Vector3(Mathf.Floor((position.X) / _gridSize) * _gridSize, Mathf.Floor((position.Y) / _gridSize) * _gridSize, Mathf.Round((position.Z) / _gridSize) * _gridSize + (_tempItemSize.Z / 2) * cameraDirection.Z);
                position += new Vector3((_tempItemSize.X / 2) % _gridSize, (_tempItemSize.Y / 2) % _gridSize, 0);
            }
            // this is cheating as we know the ground is at 0, OK for a quick prototype
			if (position.Y -_tempItemSize.Y/2 < 0 )
			{
				position.Y = _tempItemSize.Y/2;
			}
        }
        _tempItem.GlobalPosition = position;
    }
}
