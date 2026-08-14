using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    private InterfaceInteractHandler interfaceInteractHandler;
    private IInteractable currentInteractable;

    private Func<bool> interacting;

    public void Initialize(PlayerInventory playerInventory, Func<bool> interacting)
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening == true)
        {
            interfaceInteractHandler = GetComponentInChildren<NetworkInteractHandler>();
        }
        else
        {
            interfaceInteractHandler = GetComponentInChildren<LocalInteractHandler>();
        }

        this.interacting = interacting;
        interfaceInteractHandler.Initialize(playerInventory);
    }

    private void Update()
    {
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
