using Unity.Collections;
using Unity.Netcode;

public class GroundItem : NetworkBehaviour, IInteractable
{
    private FixedString64Bytes itemID;
    private ulong itemIdentificationID;
    private BaseItem currentItem;
    private NetworkVariable<FixedString64Bytes> networkItemID = new NetworkVariable<FixedString64Bytes>();
    private NetworkVariable<ulong> networkItemIdentificationID = new NetworkVariable<ulong>();

    public BaseItem CurrentItem => currentItem;
    public FixedString64Bytes ItemID => itemID;
    public ulong ItemIdentificationID => itemIdentificationID;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        networkItemID.OnValueChanged += OnNetworkItemIDChanged;
        networkItemIdentificationID.OnValueChanged += OnNetworkItemIdentificationIDChanged;

        TryApplyNetworkData();
    }

    public override void OnNetworkDespawn()
    {
        networkItemID.OnValueChanged -= OnNetworkItemIDChanged;
        networkItemIdentificationID.OnValueChanged -= OnNetworkItemIdentificationIDChanged;

        base.OnNetworkDespawn();
    }

    public void Initialize(FixedString64Bytes itemID, bool network, ulong itemIdentificationID)
    {
        if (network == true)
        {
            networkItemIdentificationID.Value = itemIdentificationID;
            networkItemID.Value = itemID;

            return;
        }

        ApplyItemData(itemID, itemIdentificationID);
    }

    private void OnNetworkItemIDChanged(FixedString64Bytes previousValue, FixedString64Bytes newValue)
    {
        TryApplyNetworkData();
    }

    private void OnNetworkItemIdentificationIDChanged(ulong previousValue, ulong newValue)
    {
        TryApplyNetworkData();
    }

    private void TryApplyNetworkData()
    {
        if (networkItemID.Value.IsEmpty == true)
        {
            return;
        }

        ApplyItemData(networkItemID.Value, networkItemIdentificationID.Value);
    }

    private void ApplyItemData(FixedString64Bytes itemID, ulong itemIdentificationID)
    {
        this.itemID = itemID;
        this.itemIdentificationID = itemIdentificationID;
        InitializeItemFromID();
    }

    private void InitializeItemFromID()
    {
        currentItem = ItemDataList.Instance.GetItemById(itemID);
    }
}
