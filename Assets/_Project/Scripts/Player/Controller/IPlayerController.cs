using Unity.Collections;
using UnityEngine;

public interface IPlayerController
{
    public Transform Transform { get; }

    public FixedString64Bytes AuthenticateID { get; set; }
    public ulong ClientID { get; set; }
    public PlayerLocalInventoryController PlayerLocalInventoryController { get; }
}
