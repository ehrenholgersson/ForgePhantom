using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class MagicRepeater : MagicNode, IInteractable
{
	bool _enabled = true;
    WorldSpaceBillboard _interactionPrompt;
	Building _building;
	MagicPower _power;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		base._Ready();
        _connectedNodes.Clear();
        CheckNodeConnection();
		_powerLevel = 0;
        _parent?.SetColor(Magic.PowerColors[(int)_powerType] * 0.5f);
		_building = (GetParent() as BuildingNode)?.Building;
		if (_building != null)
		{
			_building.AddInteractable(this);
		}
    }

    public override void _PhysicsProcess(double delta)
    {
        _powerLevel = 0;
		if (_enabled)
		{
			float highestlevel = 0;
			_power.Clear();
			foreach (MagicNode mNode in _connectedNodes)
			{
				float recievedpower = mNode.PowerLevel * Magic.MagicMultiplier[(int)_powerType, (int)mNode.PowerType];
				_power.Add(mNode.PowerLevel, mNode.PowerType);
				highestlevel = Mathf.Max(recievedpower, highestlevel);
			}
			// power is max recieved from a single source, so we cannot amplify power or signals with these
			_powerLevel = Mathf.Clamp(_power.GetByType(_powerType), 0, highestlevel);
		}
        _parent?.SetColor(Magic.PowerColors[(int)_powerType] * Mathf.Clamp(_powerLevel * 20,0.5f,5));
    }

    async void CheckRange()
	{
		while (this.IsInsideTree())
		{
			// check nodes
			await Task.Delay(5000);
		}
	}

	public int DrawPower(float request)
	{
		return 0;
	}

	public Dictionary<int, float> ServicedRequests()
	{
		return new Dictionary<int, float>();
	}

	public void Interact()
	{
		_enabled = !_enabled;
		if (_interactionPrompt?.Text?.Text != null)
		{
            _interactionPrompt.Text.Text = ((_enabled) ? "Turn Off(E)" : "Turn On (E)");
        }

	}

    public void OnSelect()
    {
        _interactionPrompt = WorldSpaceBillboard.New3DBillboard(this, Vector3.Up * 2f, "InteractionBillboard");
		_interactionPrompt.Text.Text = ((_enabled) ? "Turn Off(E)" : "Turn On (E)");
    }

    public void OnDeselect()
    {
        _interactionPrompt?.QueueFree();
    }
}
