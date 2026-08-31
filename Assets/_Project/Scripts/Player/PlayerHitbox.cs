using UnityEngine;

public class PlayerHitbox : MonoBehaviour
{
    private Transform playerTransform;

    public Transform PlayerTransform => playerTransform;

    public void Initilize(Transform playerTransform)
    {
        this.playerTransform = playerTransform;
    }
}
