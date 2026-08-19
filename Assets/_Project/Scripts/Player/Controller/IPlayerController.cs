using Unity.Collections;
using UnityEngine;

public interface IPlayerController
{
    public void Initialize(FixedString64Bytes authenticateID, ulong clientID);
    public Transform Transform { get; }

    public FixedString64Bytes AuthenticateID { get; set; }
    public PlayerLocalInventoryController PlayerLocalInventoryController { get; }
}
