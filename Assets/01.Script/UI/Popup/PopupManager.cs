using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public enum EPopupType
{
    UI_ChunkPopup,
    UI_SeedSelectPopup
}
public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance;
    public List<UI_Popup> Popups;
    private Stack<UI_Popup> _openPopups = new Stack<UI_Popup>();
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
    }
}
