using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System;

public enum ECustomizeCharacterAnimType
{
    Idle,
    Dance_1,
    Dance_2,
    Cry
}

public class CustomizeSceneCharacter : MonoBehaviour
{
    [SerializeField] private List<PartObjectList> _partsList;
    [SerializeField] private float _rotationSpeed = 2.0f;
    private Animator _animator;


    private bool isDragging = false;
    private float previousMouseX;
    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }
    private void Start()
    {
        CustomizationManager.Instance.OnPartChanged += ChangePart;
        CustomizationManager.Instance.OnPlayAnim += PlayeAnim;
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
    private void ChangePart(CustomizationPart part, int index)
    {
        foreach (var partObject in _partsList)
        {
            if (partObject.Part == part)
            {
                if (index == 0)
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
                partObject.Objects[index - 1].SetActive(true);
            }
        }
    }

    void Update()
    {
        HandleMouseDrag();
    }

    private void HandleMouseDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            previousMouseX = Input.mousePosition.x;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            float currentMouseX = Input.mousePosition.x;
            float deltaX = currentMouseX - previousMouseX;

            float rotationY = deltaX * _rotationSpeed;
            transform.Rotate(Vector3.up, rotationY, Space.World);

            previousMouseX = currentMouseX;
        }
    }

    private void PlayeAnim(ECustomizeCharacterAnimType type)
    {
        _animator.Play(type.ToString());
    }
}
[System.Serializable]
public class PartObjectList
{
    public CustomizationPart Part;
    public List<GameObject> Objects;
}