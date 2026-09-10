using UnityEngine;

public interface IInteractable
{
    public ulong ItemIdentificationID { get; set; }

    public virtual void Interact()
    {
    }
}
