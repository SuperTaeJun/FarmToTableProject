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
        }
    }

    public void Execute()
    {
        switch (currentState)
        {
            case ELetterState.Empty:
                // 이미지 생성 팝업 열기
                FadeManager.Instance.FadeScreenWithEvent(() => PopupManager.Instance.Open(EPopupType.UI_ImageGenerator));
                break;
            
            case ELetterState.Generating:
                // 생성 중일 때는 상태 메시지 표시
                Debug.Log("이미지 생성 중입니다...");
                break;
            
            case ELetterState.Ready:
                // 이미지 수령 - 플레이어에게 전달
                DeliverImageToPlayer();
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

    private void DeliverImageToPlayer()
    {
        if (generatedImage != null)
        {
            // 플레이어에게 이미지 전달
            Player player = _buildingObject.Player.GetComponent<Player>();
            if (player != null)
            {
                player.AddGeneratedImage(generatedImage);
                Debug.Log("이미지를 플레이어에게 전달했습니다!");
            }
            else
            {
                Debug.LogError("플레이어를 찾을 수 없습니다!");
            }
            
            // 상태 초기화
            generatedImage = null;
            currentState = ELetterState.Empty;
        }
    }
    public void Update()
    {
    }

    private void OnDestroy()
    {
        if (ImageGenerationManager.Instance != null)
        {
            ImageGenerationManager.Instance.OnImageGenerationComplete -= OnImageGenerationComplete;
        }
    }
}
