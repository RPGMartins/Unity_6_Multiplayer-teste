using System;
using UnityEngine;

public class GroundItem : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject parentGameObject;
    private BaseItem currentItem;
    private Action<GroundItem> despawnAction;

    public BaseItem CurrentItem => currentItem;
    public GameObject ParentGameObject => parentGameObject;

    public void Initialize(BaseItem item, Action<GroundItem> despawnAction)
    {
        this.currentItem = item;
        this.despawnAction = despawnAction;
    }

    public void CallDespawn()
    {
        despawnAction.Invoke(this);
    }
}
