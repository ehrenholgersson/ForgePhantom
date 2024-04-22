using Godot;
using System;

public interface IInteractable
{
    public void Interact();
}

public interface IDraggable
{
    public Vector3 Size { get; }

    public Node3D GetMeshObject(Material material);
}
