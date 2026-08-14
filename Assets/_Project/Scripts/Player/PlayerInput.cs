using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    private Vector2 movement;
    private int currentSlotPressed = -1;
    private Vector2 currentMousePosition;
    private bool interacting;
    public event Action<int> onSlotPressed;

    private Camera cam;

    private void Awake()
    {
        if (cam == null)
        {
            cam = Camera.main;
        }
    }

    private void Update()
    {
        #region Movement

        movement = Vector2.zero;

        if (Keyboard.current.wKey.isPressed == true || Keyboard.current.upArrowKey.isPressed == true)
        {
            movement.y += 1;
        }

        if (Keyboard.current.sKey.isPressed == true || Keyboard.current.downArrowKey.isPressed == true)
        {
            movement.y += -1;
        }

        if (Keyboard.current.aKey.isPressed == true || Keyboard.current.leftArrowKey.isPressed == true)
        {
            movement.x += -1;
        }

        if (Keyboard.current.dKey.isPressed == true || Keyboard.current.rightArrowKey.isPressed == true)
        {
            movement.x += 1;
        }

        #endregion

        #region Mouse Aim

        Vector2 mousePos = Mouse.current.position.value;

        if (float.IsFinite(mousePos.x) == false || float.IsFinite(mousePos.y) == false)
        {
        }
        else
        {
            currentMousePosition = cam.ScreenToWorldPoint(mousePos);
        }

        #endregion

        #region Inventory

        int currentSlotPressedThisFrame = -1;

        if (Keyboard.current.digit1Key.isPressed == true)
        {
            currentSlotPressedThisFrame = 0;
        }

        if (Keyboard.current.digit2Key.isPressed == true)
        {
            currentSlotPressedThisFrame = 1;
        }

        if (Keyboard.current.digit3Key.isPressed == true)
        {
            currentSlotPressedThisFrame = 2;
        }

        if (Keyboard.current.digit4Key.isPressed == true)
        {
            currentSlotPressedThisFrame = 3;
        }

        if (currentSlotPressedThisFrame != -1 && currentSlotPressedThisFrame != currentSlotPressed)
        {
            currentSlotPressed = currentSlotPressedThisFrame;
            onSlotPressed?.Invoke(currentSlotPressed);
        }

        #endregion

        #region Interacting

        interacting = false;

        if (Keyboard.current.eKey.isPressed == true)
        {
            interacting = true;
        }

        #endregion
    }

    public Vector2 GetMovement()
    {
        return movement;
    }

    public Vector3 GetMousePosition()
    {
        return currentMousePosition;
    }

    public bool Interacting()
    {
        return interacting;
    }
}
