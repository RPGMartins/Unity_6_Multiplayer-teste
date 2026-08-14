using System;
using Unity.Netcode;
using UnityEngine;

public class NetworkInteractHandler : NetworkBehaviour, InterfaceInteractHandler
{
    [SerializeField] private LocalInteractHandler localInteractHandler;

    public void Initialize(PlayerInventory inventory)
    {
    }

    public void Interact(IInteractable interactable)
    {
        if (interactable is GroundItem groundItem)
        {
            NetworkObject currnetNetworkObject = groundItem.GetComponentInParent<NetworkObject>();
            CheckWithServerIfGetGroundItemRPC(new NetworkObjectReference(currnetNetworkObject));
        }
    }

    [Rpc(SendTo.Server)]
    private void CheckWithServerIfGetGroundItemRPC(NetworkObjectReference netoNetworkObjectReference, RpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

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

        PlayerInventoryNetwork inventoryNetwork = playerObject.GetComponentInChildren<PlayerInventoryNetwork>();

        if (inventoryNetwork.ServerTryAddItemToInventory(groundItem.CurrentItem) == false)
        {
            SendResultOfGetGroundItem(clientId, false);
        }

        groundItem.CallDespawn();
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
