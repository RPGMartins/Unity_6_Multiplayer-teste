using UnityEngine;

public interface InterfaceInteractHandler
{
    public abstract void Initialize(PlayerInventory inventory);

    public abstract void Interact(IInteractable interactable);
}
