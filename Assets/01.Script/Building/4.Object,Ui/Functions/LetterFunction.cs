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
                PopupManager.Instance.Open(EPopupType.UI_ImageGenerator);
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

    public void StartImageGeneration()
    {
        currentState = ELetterState.Generating;
        Debug.Log("이미지 생성을 시작합니다.");
    }

    public void OnImageGenerationComplete(Texture2D image)
    {
        generatedImage = image;
        currentState = ELetterState.Ready;
        Debug.Log("이미지 생성이 완료되었습니다!");
    }

    private void OnImageConfirmed()
    {
        // 이미지 확정 후 상태 초기화
        currentState = ELetterState.Empty;
        generatedImage = null;
    }

    public void Update()
    {
    }

}
