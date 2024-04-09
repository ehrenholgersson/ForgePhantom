using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class ToolBar : VBoxContainer
{
	[Export] float buttonHeldTime;
    [Export] PackedScene itemDragPrefab;
    float buttonTimer = 0;
	List<Button> inventorybuttons = new List<Button>();
	int selectedButton;
	bool drag;
	CollectableInfo item;
    [Export] Node worldScene;
	Node3D tempItem;
	Vector2 mousePosition;
	bool buttonpressed;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		worldScene = GetNode("/root/Scene");
		foreach (Node child in GetNode("Inventory").GetChildren())
		{
			if (child is Button)
			{
				inventorybuttons.Add((Button)child);
				((Button)child).ButtonDown += () => OnInventoryButtonClick(inventorybuttons.IndexOf((Button)child));

			}
		}
	}

	public void OnInventoryButtonClick(int index)
	{
		GD.Print("pressed button" + index);
        buttonTimer = Time.GetTicksMsec() + buttonHeldTime * 1000; // convert sec to msec
        item = GameController.MainPlayer.GetInventoryItem(index);
		selectedButton = index;
		GD.Print("selected item " + (item?.ResourceName ?? "nothing"));
	}

    public override void _Input(InputEvent @event) // record mouse position
    {
        base._Input(@event);
		if (@event is InputEventMouseMotion eventMouse)
		{
			mousePosition = eventMouse.Position;
		}
    }

    public override void _PhysicsProcess(double delta)
	{
		float rayLength = 500;
        

        if (Input.GetMouseButtonMask() == MouseButtonMask.Left && item!=null)
		{
			if (Time.GetTicksMsec() > buttonTimer)
			{

				// button is "held", drag item out of inventory
				// spawn a placeholder object if not already done
				if (!drag)
				{
					inventorybuttons[selectedButton].Icon = null;
					//tempItem = itemDragPrefab.Instantiate() as Node3D;
					worldScene.AddChild(itemDragPrefab.Instantiate());
					tempItem = worldScene.GetNode("Drag_Item")as Node3D;
					tempItem.GetNode<MeshInstance3D>("Model").Mesh = item?.model;
					drag = true;
				}

                // move the placeholder object wherever the mouse is pointing
				var spaceState = tempItem.GetWorld3D().DirectSpaceState;
                var origin = GameController.MainCamera?.ProjectRayOrigin(mousePosition)??Vector3.Zero;
				var end = (origin + GameController.MainCamera.ProjectRayNormal(mousePosition)*rayLength);
				var query = PhysicsRayQueryParameters3D.Create(origin, end);
				var result = spaceState.IntersectRay(query);
				Vector3 position = (Vector3)result["position"];

                // move our position along the collision normal half a block so we aren't intersecting stuff (hopefully)
                position += (Vector3)result["normal"] *0.6f;
				tempItem.GlobalPosition = position;
				GD.Print("placeholder position " + position);
			}
		}
		else if (buttonTimer > 0)
		{
			buttonTimer = 0;
			if (drag)
			{
				// remove object from player inventory
				GameController.MainPlayer.RemoveObject(selectedButton);

                // attempt to place item in world
                Collectable newObject = new Collectable();
				newObject.SetItem(item);
				worldScene.AddChild(newObject);
				newObject.GlobalPosition = tempItem.GlobalPosition;
				drag = false;
                // kill our placeholder
                tempItem.QueueFree();
            }
			else
			{
				// button was "clicked" and not held, select/equip item or whatever is appropriate
			}
			item = null;
			GD.Print("button released");
		}
		buttonpressed = false;
	}
}
