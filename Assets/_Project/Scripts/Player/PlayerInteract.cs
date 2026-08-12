using System;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    private Interactable currentInteractable;
    private PlayerInventory playerInventory;

    private Func<bool> interacting;

    public void Initialize(PlayerInventory playerInventory, Func<bool> interacting)
    {
        this.playerInventory = playerInventory;
        this.interacting = interacting;
    }

    private void Update()
    {
        bool interact = interacting.Invoke();

        if (interact == true)
        {
            Interact();
        }
    }

    private void Interact()
    {
        if (currentInteractable != null)
        {
            if (currentInteractable is GroundItem groundItem)
            {
                groundItem.Interact(playerInventory);
            }
            else
            {
                currentInteractable.Interact();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Interactable interactable = other.gameObject.GetComponentInParent<Interactable>();

        if (interactable == null)
        {
            return;
        }

        currentInteractable = interactable;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Interactable interactable = other.gameObject.GetComponentInParent<Interactable>();

        if (interactable == null)
        {
            return;
        }

        currentInteractable = null;
    }
}
