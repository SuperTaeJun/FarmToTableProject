using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UI_Building : UI_Popup
{
    [SerializeField] private List<ButtonInfo> Buttons = new List<ButtonInfo>();

    private void Start()
    {
        Player player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        PlayerBuildAbility ability = player.GetAbility<PlayerBuildAbility>();

        //각 버튼들 이벤트 등록
        foreach (var button in Buttons)
        {
            EBuildingType type = button.Type;
            button.Button.onClick.AddListener(() => { ability.SetSelectedType(type); Close(); });
        }
    }
}

[Serializable]
public class ButtonInfo
{
    public EBuildingType Type;
    public Button Button;
}
