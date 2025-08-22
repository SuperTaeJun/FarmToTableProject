using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class UI_SeedSelect : UI_Popup
{
    [SerializeField] private List<SeedButtonInfo> Buttons = new List<SeedButtonInfo>();
    [SerializeField] private Button _closeButton;
    private void Start()
    {
        Player player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        PlayerFarmingAbility ability = player.GetAbility<PlayerFarmingAbility>();
            
        _closeButton.onClick.AddListener(()=>PopupManager.Instance.PopUpClose());

        //이벤트 등록
        foreach (var button in Buttons)
        {
            ECropType cropType = button.Type; // 지역 변수로 복사                                           
            button.Button.onClick.AddListener(() => { ability.SetCurrentSeed(cropType); PopupManager.Instance.PopUpClose(); });

        }
    }

}

[Serializable]
public struct SeedButtonInfo
{
    public ECropType Type;
    public Button Button;
}

