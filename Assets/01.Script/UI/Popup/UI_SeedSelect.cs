using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class UI_SeedSelect : UI_Popup
{
    [SerializeField] private List<SeedButtonInfo> Buttons = new List<SeedButtonInfo>();

    private void Start()
    {
        Player player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        PlayerFarmingAbility ability = player.GetAbility<PlayerFarmingAbility>();

        //�̺�Ʈ ���
        foreach (var button in Buttons)
        {
            //button.Button.onClick.AddListener(() => ability.SetCurrentSeed(button.Type));
            ECropType cropType = button.Type; // ���� ������ ����                                           
            button.Button.onClick.AddListener(() => { ability.SetCurrentSeed(cropType); Close(); });

        }
    }
}

[Serializable]
public struct SeedButtonInfo
{
    public ECropType Type;
    public Button Button;
}

