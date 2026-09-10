using System;
using Unity.Collections;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    private IInteractable currentInteractable;

    private Func<bool> interacting;

    private bool initialized = false;
    private FixedString64Bytes playerID;
    private ulong clientID;

    public void Initialize(ulong clientID, FixedString64Bytes playerID)
    {
        this.playerID = playerID;
        this.clientID = clientID;
    }

    public void InitializeInput(Func<bool> interacting)
    {
        this.interacting = interacting;
        initialized = true;
    }

    private void Update()
    {
        if (initialized == false)
        {
            return;
        }

        if (interacting.Invoke() == false)
        {
            return;
        }

        if (currentInteractable == null)
        {
            return;
        }

        Interact();
    }

    private void Interact()
    {
        if (currentInteractable is GroundItem groundItem)
        {
            InteractManager.instance.TryToInteractWithGroundItem(groundItem.ItemIdentificationID, playerID, clientID);
        }
        else
        {
            currentInteractable.Interact();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        IInteractable interactable = other.gameObject.transform.parent.GetComponentInChildren<IInteractable>();

        if (interactable == null)
        {
            return;
        }

        currentInteractable = interactable;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        IInteractable interactable = other.gameObject.transform.parent.GetComponentInChildren<IInteractable>();

        if (interactable == null)
        {
            return;
        }

        currentInteractable = null;
    }
}
