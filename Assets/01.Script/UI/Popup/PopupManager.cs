using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public enum EPopupType
{
    UI_SeedSelectPopup,
    UI_OptionPopup,
    UI_BuildingPopup,
    UI_InventoryPopup,
    UI_ShopPopup
}
public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance;
    public List<UI_Popup> Popups;
    private Stack<UI_Popup> _openPopups = new Stack<UI_Popup>();
    public Stack<UI_Popup> OpenPopups => _openPopups;

    public event System.Action<bool> OnPopupStateChanged;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public T OpenPopup<T>(EPopupType popupType, Action closeCallback = null) where T : UI_Popup
    {
        foreach (UI_Popup pop in Popups)
        {
            if (pop.name == popupType.ToString())
            {
                pop.Open(closeCallback);
                _openPopups.Push(pop);
                return pop as T;
            }
        }
        return null;
    }
    public void Open(EPopupType popupType, Action callBack = null)
    {
        PopUpOpen(popupType.ToString(), callBack);
        OnPopupStateChanged?.Invoke(true);
    }
    
    public void OpenInventory(EInventoryType inventoryType = EInventoryType.Player, Action callBack = null)
    {
        var inventoryPopup = OpenPopup<UI_Inventory>(EPopupType.UI_InventoryPopup, callBack);
        if (inventoryPopup != null)
        {
            inventoryPopup.OpenInventory(inventoryType);
        }
        OnPopupStateChanged?.Invoke(true);
    }
    
    public void OpenShop(Action callBack = null)
    {
        var shopPopup = OpenPopup<UI_Shop>(EPopupType.UI_ShopPopup, callBack);
        if (shopPopup != null)
        {
            shopPopup.OpenShop();
        }
        OnPopupStateChanged?.Invoke(true);
    }

    public void PopUpOpen(string popupName, Action closeCallback)
    {
        foreach (UI_Popup pop in Popups)
        {
            if (pop.name == popupName)
            {
                pop.Open(closeCallback);
                _openPopups.Push(pop);
                break;
            }
        }
    }

    public void PopUpClose(Action callBack = null)
    {
        _openPopups.Pop();
        OnPopupStateChanged?.Invoke(_openPopups.Count > 0); // �˾� ���� �˸�
    }
}
