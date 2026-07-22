using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float velocity = 1;
    public override void OnNetworkSpawn()
    {
        if (IsOwner == false)
        {
            Destroy(this);
        }
    }
    
    private void Update()
    {
        Vector3 position = Vector3.zero;
        
        if (Keyboard.current.wKey.isPressed == true || Keyboard.current.upArrowKey.isPressed == true)
        {
            position.y += 1;
        }
        if (Keyboard.current.sKey.isPressed == true || Keyboard.current.downArrowKey.isPressed == true)
        {
            position.y += -1;
        }
        if (Keyboard.current.aKey.isPressed == true || Keyboard.current.leftArrowKey.isPressed == true)
        {
            position.x += -1;
        }
        if (Keyboard.current.dKey.isPressed == true || Keyboard.current.rightArrowKey.isPressed == true)
        {
            position.x += 1;
        }
        
        transform.position += position * velocity;
    }
}
