using DG.Tweening.Core.Easing;
using System.Collections.Generic;
using UnityEngine;
using System;
public enum EPopupType
{
    UI_OptionPopup,
    UI_BuildMenu,
    UI_SkillPopup,
    UI_RewardPopup
}

public class PopUpManager : MonoBehaviourSingleton<PopUpManager>
{
    public List<UI_PopUp> Popups;
    private Stack<UI_PopUp> _openPopups = new Stack<UI_PopUp>();

    protected override void Awake()
    {
        base.Awake();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_openPopups.Count > 0)
            {
                while (_openPopups.Count > 0)
                {
                    UI_PopUp popup = _openPopups.Pop();
                    bool opened = popup.isActiveAndEnabled;
                    popup.Close();
                    // Peek() 대신 그냥 break
                    if (opened)
                        break;
                }
            }
            else
            {
                Open(EPopupType.UI_OptionPopup);
            }
        }
    }
    public T OpenPopup<T>(EPopupType popupType, Action closeCallback = null) where T : UI_PopUp
    {
        foreach (UI_PopUp pop in Popups)
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
        foreach (UI_PopUp pop in Popups)
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
