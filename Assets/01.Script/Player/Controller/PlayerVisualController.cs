using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public enum EVisualPart
{
    Shovel,
    WateringCan,
    Rake
}
public class PlayerVisualController : MonoBehaviour
{
    [SerializeField] private List<PartObjectList> _partsList;
    [SerializeField] private List<VisualPart> _visualPartList;


    void Start()
    {
        SetupCharacterParts();
    }
    private void SetupCharacterParts()
    {
        CharacterCustomization customInfo = CustomizationManager.Instance.CurrentCustomization;

        foreach (var info in customInfo.PartIndexMap)
        {
            foreach (var partObject in _partsList)
            {
                if (partObject.Part == info.Key)
                {
                    if (info.Value == 0)
                    {
                        foreach (var obj in partObject.Objects)
                        {
                            obj.SetActive(false);
                            return;
                        }
                    }
                    foreach (var obj in partObject.Objects)
                    {
                        obj.SetActive(false);
                    }
                    partObject.Objects[info.Value - 1].SetActive(true);
                }
            }
        }
    }

    public void SetActiveVisualPart(EVisualPart part)
    {
        _visualPartList.Find((VisualPart t) => t.PartType == part).PartObject.SetActive(true);
    }
    public void SetDisActiveVisualAllPart()
    {
        foreach (var partObject in _visualPartList)
        {
            partObject.PartObject.SetActive(false);
        }
    }
}
[Serializable]
public class VisualPart
{
    public EVisualPart PartType;
    public GameObject PartObject;
}