using UnityEngine;
using UnityEngine.UI;

public class MainHudManager : MonoBehaviour
{
    public static MainHudManager Instance;

    [SerializeField] private Button _seedSelectButton;
    private void Awake()
    {
        Instance = this;

        _seedSelectButton.onClick.AddListener(OnClikedSeedSelectButton);
    }
    private void OnClikedSeedSelectButton() => PopupManager.Instance.Open(EPopupType.UI_SeedSelectPopup);


    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
