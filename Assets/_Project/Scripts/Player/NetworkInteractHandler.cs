using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class NetworkInteractHandler : NetworkBehaviour, InterfaceInteractHandler
{
    [SerializeField] private LocalInteractHandler localInteractHandler;

    private ulong clientID;
    private FixedString64Bytes playerID;

    public void Initialize(FixedString64Bytes playerID, ulong clientID)
    {
        this.clientID = clientID;
        this.playerID = playerID;
    }

    public void Interact(IInteractable interactable)
    {
        if (interactable is GroundItem groundItem)
        {
            NetworkObject currnetNetworkObject = groundItem.GetComponentInParent<NetworkObject>();
            CheckWithServerIfGetGroundItemRPC(new NetworkObjectReference(currnetNetworkObject), clientID, playerID);
        }
        else if (false)
        {
        }
    }

    [Rpc(SendTo.Server)]
    private void CheckWithServerIfGetGroundItemRPC(NetworkObjectReference netoNetworkObjectReference, ulong clientId, FixedString64Bytes playerID)
    {
        if (netoNetworkObjectReference.TryGet(out NetworkObject networkObject) == false)
        {
            SendResultOfGetGroundItem(clientId, false);

            return;
        }

        GroundItem groundItem = networkObject.GetComponentInChildren<GroundItem>();

        if (groundItem == null)
        {
            SendResultOfGetGroundItem(clientId, false);

            return;
        }

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out NetworkClient client) == false)
        {
            return;
        }

        NetworkObject playerObject = client.PlayerObject;

        if (playerObject == null)
        {
            return;
        }

        if (NetworkInventoryManager.Instance.ServerAddItem(groundItem.CurrentItem, playerID) == false)
        {
            SendResultOfGetGroundItem(clientId, false);
        }

        //groundItem.CallDespawn();
        SendResultOfGetGroundItem(clientId, true);
    }

    private void SendResultOfGetGroundItem(ulong clientId, bool success)
    {
        CallbackGetGroundItemRPC(success, RpcTarget.Single(clientId, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void CallbackGetGroundItemRPC(bool result, RpcParams rpcParams = default)
    {
        localInteractHandler.GetGroundItemFeedBack(result);
    }
}
