using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UI_Building : UI_Popup
{
    [SerializeField] private List<ButtonInfo> Buttons = new List<ButtonInfo>();

    private void Start()
    {
        //�� ��ư�� �̺�Ʈ ���
        foreach (var button in Buttons)
        {
            BuildingType type = button.Type;
            button.Button.onClick.AddListener(() => { OnClikedButton(type); Close(); });
        }
    }

    private void OnClikedButton(BuildingType buildingType)
    {
        //todo ����ư�� ��ȣ�ۿ� -> ������ ��ü �ٿ�����
    }



}

[Serializable]
public class ButtonInfo
{
    public BuildingType Type;
    public Button Button;
}
