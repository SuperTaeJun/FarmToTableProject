using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UI_Building : UI_Popup
{
    [SerializeField] private List<ButtonInfo> _buildingButtons = new List<ButtonInfo>();
    [SerializeField] private Button _closeButton;
    private void Start()
    {
        Player player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        PlayerBuildAbility ability = player.GetAbility<PlayerBuildAbility>();

        _closeButton.onClick.AddListener(Close);

        //각 버튼들 이벤트 등록
        foreach (var button in _buildingButtons)
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
