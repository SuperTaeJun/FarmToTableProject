using System.Collections.Generic;
using UnityEngine;

public class PlayerVisualController : MonoBehaviour
{
    [SerializeField] private List<PartObjectList> _partsList;
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
    void Update()
    {
        
    }
}
