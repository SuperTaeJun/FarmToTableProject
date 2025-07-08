using UnityEngine;
using UnityEngine.UI;
public class CustomizationUI : MonoBehaviour
{
    public Button HairButton;
    public Button HatButton;

    private void Start()
    {
        // �Ŵ��� �̱��� ����
        CustomizationManager.Instance.OnPartChanged += HandlePartChanged;

        HairButton.onClick.AddListener(() =>
            CustomizationManager.Instance.ChangePart(CustomizationPart.Hair, "Hair_01"));

        HatButton.onClick.AddListener(() =>
            CustomizationManager.Instance.ChangePart(CustomizationPart.Hat, "Hat_01"));

    }

    private void HandlePartChanged(CustomizationPart part, string newId)
    {
        Debug.Log($"[UI] {part}�� {newId}�� �����.");

    }

    private void OnDestroy()
    {
        if (CustomizationManager.Instance != null)
            CustomizationManager.Instance.OnPartChanged -= HandlePartChanged;
    }
}
