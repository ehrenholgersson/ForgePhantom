using Godot;
using System.Collections.Generic;

public interface IInteractable
{
    public void Interact();
    public void OnSelect();
    public void OnDeselect();
}

public interface IDraggable
{
    public Vector3 Size { get; }

    public Node3D GetMeshObject(Material material);
}

public interface IMagicNode
{
    public int DrawPower(float request);
    public Dictionary<int, float> ServicedRequests();
    public float PowerState { get; }
    public Magic.PowerTypes PowerType { get; }

}
