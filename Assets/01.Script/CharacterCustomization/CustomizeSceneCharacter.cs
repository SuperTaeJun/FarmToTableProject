using UnityEngine;
using System.Collections.Generic;

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
            ChangePart(info.Key, info.Value);
        }

    }
    private void ChangePart(ECustomizationPartType part, int index)
    {
        var partObject = _partsList.Find(p => p.Part == part);
        if (partObject == null) return;

        // 일단 모든 오브젝트 끄기
        foreach (var obj in partObject.Objects)
        {
            if (obj == null) 
                return;

            obj.SetActive(false);

        }

        // index가 0보다 크고 유효한 범위내면 해당 오브젝트 켜기
        if (index > 0 && index <= partObject.Objects.Count)
        {
            partObject.Objects[index - 1].SetActive(true);
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
    public ECustomizationPartType Part;
    public List<GameObject> Objects;
}