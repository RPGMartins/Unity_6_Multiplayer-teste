using Unity.Collections;
using UnityEngine;

public interface InterfaceInteractHandler
{
    public abstract void Initialize(FixedString64Bytes playerID, ulong clientID);

    public abstract void Interact(IInteractable interactable);
}
