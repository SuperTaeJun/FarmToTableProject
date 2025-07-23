using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UI_Building : UI_Popup
{
    [SerializeField] private List<ButtonInfo> Buttons = new List<ButtonInfo>();

    private void Start()
    {
        //각 버튼들 이벤트 등록
        foreach (var button in Buttons)
        {
            BuildingType type = button.Type;
            button.Button.onClick.AddListener(() => { OnClikedButton(type); Close(); });
        }
    }

    private void OnClikedButton(BuildingType buildingType)
    {
        //todo 각버튼별 상호작용 -> 프리뷰 객체 뛰워야함
    }



}

[Serializable]
public class ButtonInfo
{
    public BuildingType Type;
    public Button Button;
}
