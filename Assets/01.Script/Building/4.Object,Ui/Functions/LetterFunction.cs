using UnityEngine;

public enum ELetterState
{
    Empty,      // 빈 상태
    Generating, // 이미지 생성 중
    Ready       // 이미지 준비됨
}

public class LetterFunction : IBuildingFunction
{
    private BuildingObject _buildingObject;
    private ELetterState currentState = ELetterState.Empty;
    private Texture2D generatedImage;

    public LetterFunction(BuildingObject buildingObject)
    {
        _buildingObject = buildingObject;

        if (ImageGenerationManager.Instance != null)
        {
            ImageGenerationManager.Instance.OnImageGenerationComplete += OnImageGenerationComplete;
            ImageGenerationManager.Instance.OnImageConfirmed += OnImageConfirmed;
        }
    }

    public void Execute()
    {
        switch (currentState)
        {
            case ELetterState.Empty:
                //돈없으면 못만듬
                if (CanAffordImageGeneration() == false) return;

                _ = CurrencyManager.Instance.TrySpendCurrency(ECurrencyType.Money, 2000);
                _ = CurrencyManager.Instance.TrySpendCurrency(ECurrencyType.Gem, 2);

                PopupManager.Instance.Open(EPopupType.UI_ImageGenerator);
                currentState = ELetterState.Generating;
                break;

            case ELetterState.Generating:
                Debug.Log("이미지 생성 중입니다...");
                break;

            case ELetterState.Ready:
                // 생성된 이미지를 대기중 이미지로 설정
                ImageGenerationManager.Instance.SetPendingImage(generatedImage);

                // 이미지 프리뷰 팝업 열기
                PopupManager.Instance.Open(EPopupType.UI_ImagePreview);
                break;
        }
    }

    private bool CanAffordImageGeneration()
    {
        if (CurrencyManager.Instance.CanAfford(ECurrencyType.Money, 2000) == false
            || CurrencyManager.Instance.CanAfford(ECurrencyType.Gem, 2) == false)
        {
            _buildingObject.Player.GetComponent<Player>().GetAbility<PlayerNotificationAbility>().ActiveDialogBox(EPlayerNotificationType.LackOfMoney);
            return false;
        }

        return true;
    }

    public void OnImageGenerationComplete(Texture2D image)
    {
        generatedImage = image;
        currentState = ELetterState.Ready;
    }

    private void OnImageConfirmed()
    {
        currentState = ELetterState.Empty;
        generatedImage = null;
    }

    public void Update()
    {
    }

}
