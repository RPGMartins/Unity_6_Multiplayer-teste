using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class IniciadorDoPlayerNetwork : NetworkBehaviour, IIniciadorDoPlayer
{
    public static IniciadorDoPlayerNetwork instance;
    [SerializeField] private IniciadorDoPlayer iniciadorDoPlayer;

    private void Awake()
    {
        instance = this;

        iniciadorDoPlayer.OnDefaultInventoryAdded += ClientRebuildLocalInventoryRPC;
    }

    [Rpc(SendTo.NotMe)]
    private void ClientRebuildLocalInventoryRPC()
    {
        iniciadorDoPlayer.RebuildLocalInventory();
    }

    [Rpc(SendTo.Server)]
    public void InitializeInventoryRPC(FixedString64Bytes playerID)
    {
        iniciadorDoPlayer.InitializeInventoryRPC(playerID);
    }

    [Rpc(SendTo.Server)]
    public void InitializePlayersRPC(FixedString64Bytes[] clientsIDs)
    {
        iniciadorDoPlayer.InitializePlayersRPC(clientsIDs);
    }
}
