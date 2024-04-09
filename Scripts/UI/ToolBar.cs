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

    float _buttonTimer = 0;

	List<Button> _inventorybuttons = new List<Button>();
	int _selectedButton;

    [Export] Godot.Collections.Array<BuildingResource> _availableBuildings = new Godot.Collections.Array<BuildingResource>();

	bool _drag;
	IDraggable _item;

	Node3D _tempItem;
	Material _tempItemMat;
	Vector2 _mousePosition;

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
					((Button)child).Icon = _availableBuildings[i].icon;
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
        _drag = true;
    }

  //  public override void _Input(InputEvent @event) // record mouse position
  //  {
  //      base._Input(@event);
		//if (@event is InputEventMouseMotion eventMouse)
		//{
		//	_mousePosition = eventMouse.Position;
		//}
  //  }

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
		float rayLength = 500;
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
						_drag = true;
					}

                    // move the placeholder object wherever the mouse is pointing
                    MoveObjectPlacement();

                    //var spaceState = _tempItem.GetWorld3D().DirectSpaceState;
                    //var origin = GameController.MainCamera?.ProjectRayOrigin(_mousePosition) ?? Vector3.Zero;
                    //var end = (origin + GameController.MainCamera.ProjectRayNormal(_mousePosition) * rayLength);
                    //var query = PhysicsRayQueryParameters3D.Create(origin, end);
                    //var result = spaceState.IntersectRay(query);
                    //Vector3 position = (Vector3)result["position"];

                    //// move our position along the collision normal half a block so we aren't intersecting stuff (hopefully)
                    //position += (Vector3)result["normal"] * 0.6f;
                    //_tempItem.GlobalPosition = position;
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

                    // attempt to place item in world
                    Collectable newObject = new Collectable();
					newObject.SetItem((CollectableResource)_item);
					_worldScene.AddChild(newObject);
					newObject.GlobalPosition = _tempItem.GlobalPosition;
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
				MoveObjectPlacement();
				// snap to grid
				_tempItem.GlobalPosition = new Vector3(Mathf.Round(_tempItem.GlobalPosition.X/4)*4, _tempItem.GlobalPosition.Y, Mathf.Round(_tempItem.GlobalPosition.Z/4)*4);

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
        position += (Vector3)result["normal"] * 0.6f;
        _tempItem.GlobalPosition = position;
    }
}
