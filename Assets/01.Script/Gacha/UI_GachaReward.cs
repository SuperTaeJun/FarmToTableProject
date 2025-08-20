using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_GachaReward : MonoBehaviour
{
    [SerializeField] private Image _rewardImage;
    [SerializeField] private TextMeshProUGUI _rewardText;
    [SerializeField] private Button _rewardButton;


    private void Awake()
    {
        _rewardButton.onClick.AddListener(() => FadeManager.Instance.FadeToScene("MainScene"));

        GachaScene.Instance.OnVehicleRewarded += RefreshReward;
    }
    private void RefreshReward(Sprite sprite,string name)
    {
        _rewardImage.sprite = sprite;
        _rewardText.text = name;
    }

}
