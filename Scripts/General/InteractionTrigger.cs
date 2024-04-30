using Godot;
using System;

/// <summary>
/// to direct interaction requests to the appropriate IInteractable node in cases where that node is not directly attached to the collider
/// </summary>
public partial class InteractionTrigger : Node3D, IInteractable
{
	IInteractable _interactable;
	// Called when the node enters the scene tree for the first time.

	public InteractionTrigger(IInteractable target)
	{
		_interactable = target;
	}

    public InteractionTrigger()
    {
        _interactable = null;
    }
    public void SetInteractionTarget(IInteractable target)
	{
		_interactable = target;
	}

	public void Interact()
	{
		_interactable?.Interact();
	}
	public void OnSelect()
	{
		_interactable?.OnSelect();
	}

	public void OnDeselect()
	{
		_interactable?.OnDeselect();
	}
}
