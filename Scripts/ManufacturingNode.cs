using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class ManufacturingNode : Node3D
{
	Area3D _input;
	Node3D _output;
	float _startTime = 0;
	[Export] Recipe _recipe;
	string[] _requiredIngredients;
	List<string> _inventory = new List<string>(); // rather than bouncing around strings we could build a dictionary for this and use integer keys

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_input = GetNode("Input") as Area3D;
		_output = GetNode("Output") as Node3D;
		_input.BodyEntered += (body) => MaterialInput(body);
		_requiredIngredients = _recipe?.Ingredients;

    }

	public override void _PhysicsProcess(double delta)
	{
		if (_startTime == 0)
		{
			var invChecker = _inventory.ToList(); // check if we have all thge ingredients, probably not most efficent way possible to create a whole new list, but it doesn't really matter
			foreach (string required in _requiredIngredients)
			{
				if (!invChecker.Contains(required))
				{
					return;
				}
				invChecker.Remove(required);
			}
			_inventory = invChecker;
			_startTime = Time.GetTicksMsec();
		}
		else
		{
			if (Time.GetTicksMsec() > _startTime + _recipe?.Time)
			{
                Collectable newObject = new Collectable();
                newObject.SetItem(_recipe.Result);
                GameController.Singleton.AddChild(newObject);
                newObject.GlobalPosition = _output.GlobalPosition;
				_startTime = 0;
            }
		}
	}
	void MaterialInput(Node3D body)
	{
		if (body is Collectable)
		{
			var item = ((Collectable)body).ItemType;
			if (_requiredIngredients.Contains(item.Type))
			{
				_inventory.Add(item.Type);
				body.QueueFree();
			}
		}
	}
}
