using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class InteractManager : NetworkBehaviour
{
    public static InteractManager instance;

    [SerializeField] private GroundItemSpawnerController groundItemSpawnerController;

    private void Awake()
    {
        instance = this;
    }

    public void TryToInteractWithGroundItem(ulong itemIdentificationID, FixedString64Bytes playerID, ulong clientID)
    {
        if (NetworkConnectionChecker.CheckConnection() == true)
        {
            if (NetworkConnectionChecker.CheckIfHasAuthority() == true)
            {
                HandleItem(itemIdentificationID, playerID, clientID);
            }
            else
            {
                HandleItemRPC(itemIdentificationID, playerID);
            }
        }
        else
        {
            HandleItem(itemIdentificationID, playerID, clientID);
        }
    }

    private void HandleItem(ulong itemIdentificationID, FixedString64Bytes playerID, ulong clientID)
    {
        if (groundItemSpawnerController.SpawnedGroundItemsDictionary.TryGetValue(itemIdentificationID, out GroundItem groundItem) == false)
        {
            HandleItemCallback(false, clientID);

            return;
        }

        foreach (PlayerController playerController in PlayerInitializer.instance.PlayerCreatedList)
        {
            if (playerController.AuthenticateID == playerID)
            {
                //Verificar se player esta perto, se está caindo ou impossivel de pegar algum item etc
                if (false)
                {
                    HandleItemCallback(false, clientID);

                    return;
                }
            }
        }

        if (InventoryManager.Instance.PlayerCanAddItem(playerID, groundItem.ItemID) == false)
        {
            HandleItemCallback(false, clientID);

            return;
        }

        AddItemToPlayer(playerID, groundItem.CurrentItem);
        HandleItemCallback(true, clientID);

        groundItemSpawnerController.DestroyItem(groundItem.ItemIdentificationID, false);
    }

    private void AddItemToPlayer(FixedString64Bytes playerID, BaseItem baseItem)
    {
        InventoryManager.Instance.TryAddItem(baseItem, playerID);
    }

    private void HandleItemCallback(bool success, ulong clientID)
    {
        if (NetworkConnectionChecker.CheckConnection() == false)
        {
            HandleItemCallback(success);

            return;
        }

        HandleItemCallbackRPC(success, RpcTarget.Single(clientID, RpcTargetUse.Temp));
    }

    private void HandleItemCallback(bool success)
    {
        //callback do player que pegou o item, chamar um handler de pegar item, que vai soltar a particula , som etc, mas não pega item
    }

    #region RPC

    [Rpc(SendTo.Server)]
    private void HandleItemRPC(ulong itemIdentificationID, FixedString64Bytes playerID, RpcParams rpcParams = default)
    {
        HandleItem(itemIdentificationID, playerID, rpcParams.Receive.SenderClientId);
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void HandleItemCallbackRPC(bool validItem, RpcParams rpcParams = default)
    {
        HandleItemCallback(validItem);
    }

    #endregion
}
