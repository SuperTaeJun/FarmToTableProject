using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum EPlayerUiType
{
    Chunk,
}
public class PlayerUiController : MonoBehaviour
{
    [SerializeField] private GameObject _dialogBox;
    [SerializeField] private TextMeshProUGUI _dialogText;

    public void DisActiveDialogBox()
    {
        _dialogBox.SetActive(false);
    }
    public void ActiveDialogBox(EPlayerUiType type)
    {
        _dialogBox.SetActive(true);
        switch (type)
        {
            case EPlayerUiType.Chunk:
                _dialogText.text = "<Color=red>F</Color>를 눌러\n땅을 구매할까...";
                break;
        }
    }
}
