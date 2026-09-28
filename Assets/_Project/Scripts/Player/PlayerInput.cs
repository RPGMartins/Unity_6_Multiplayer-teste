using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    private Vector2 movement;
    private int currentSlotPressed = -1;
    private Vector2 currentMousePosition;
    private bool interacting;
    private bool shooting;
    public event Action<int> onSlotPressed;

    private Camera cam;

    private Keyboard keyboard;
    private Mouse mouse;
    private Gamepad gamepad;

    public void Initialize()
    {
        if (cam == null)
        {
            cam = Camera.main;
        }

        keyboard = Keyboard.current;
        mouse = Mouse.current;

        if (Gamepad.all.Count > 0)
        {
            gamepad = Gamepad.all[0];
        }
    }

    private void Update()
    {
        interacting = false;
        shooting = false;

        #region Movement

        movement = Vector2.zero;

        if (keyboard.wKey.isPressed == true || keyboard.upArrowKey.isPressed == true)
        {
            movement.y += 1;
        }

        if (keyboard.sKey.isPressed == true || keyboard.downArrowKey.isPressed == true)
        {
            movement.y += -1;
        }

        if (keyboard.aKey.isPressed == true || keyboard.leftArrowKey.isPressed == true)
        {
            movement.x += -1;
        }

        if (keyboard.dKey.isPressed == true || keyboard.rightArrowKey.isPressed == true)
        {
            movement.x += 1;
        }

        else
        {
            if (gamepad != null)
            {
                movement = gamepad.leftStick.ReadValue();
            }
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

        if (gamepad != null)
        {
            if (gamepad.dpad.up.wasPressedThisFrame)
            {
                currentSlotPressedThisFrame = 0;
            }

            if (gamepad.dpad.right.wasPressedThisFrame)
            {
                currentSlotPressedThisFrame = 1;
            }

            if (gamepad.dpad.down.wasPressedThisFrame)
            {
                currentSlotPressedThisFrame = 2;
            }

            if (gamepad.dpad.left.wasPressedThisFrame)
            {
                currentSlotPressedThisFrame = 3;
            }
        }

        if (currentSlotPressedThisFrame != -1 && currentSlotPressedThisFrame != currentSlotPressed)
        {
            currentSlotPressed = currentSlotPressedThisFrame;
            onSlotPressed?.Invoke(currentSlotPressed);
        }

        #endregion

        #region Interacting

        if (keyboard.eKey.isPressed == true)
        {
            interacting = true;
        }

        if (gamepad != null)
        {
            if (gamepad.buttonSouth.isPressed == true)
            {
                interacting = true;
            }
        }

        #endregion

        #region Shooting

        if (keyboard.spaceKey.wasPressedThisFrame == true)
        {
            shooting = true;
        }

        if (gamepad != null)
        {
            if (gamepad.rightTrigger.wasPressedThisFrame == true)
            {
                shooting = true;
            }
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

    public bool Shooting()
    {
        return shooting;
    }
}
