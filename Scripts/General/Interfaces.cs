using Godot;
using System;

public interface IInteractable
{
    public void Interact();
}

public interface IDraggable
{
    public Mesh Model { get; }
}
