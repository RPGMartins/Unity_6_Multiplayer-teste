using System.Collections.Generic;
using Unity.Collections;

public interface IIniciadorDoPlayer
{
    public abstract void InitializeInventoryRPC(FixedString64Bytes playerID);
    public abstract void InitializePlayersRPC(FixedString64Bytes[] clientsIDs);
}
