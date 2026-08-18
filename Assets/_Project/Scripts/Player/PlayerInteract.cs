using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    private InterfaceInteractHandler interfaceInteractHandler;
    private IInteractable currentInteractable;

    private Func<bool> interacting;

    private bool initialized = false;

    public void Initialize(Func<bool> interacting, ulong clientID, FixedString64Bytes playerID)
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening == true)
        {
            interfaceInteractHandler = GetComponentInChildren<NetworkInteractHandler>();
        }
        else
        {
            interfaceInteractHandler = GetComponentInChildren<LocalInteractHandler>();
        }

        interfaceInteractHandler.Initialize(playerID, clientID);
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

        interfaceInteractHandler.Interact(currentInteractable);
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
