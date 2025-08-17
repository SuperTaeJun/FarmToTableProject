using UnityEngine;
using UnityEngine.UI;

public class PaintingFunction : IBuildingFunction
{
    private BuildingObject _buildingObject;
    private string _buildingKey;
    private bool _imageLoaded = false;

    public PaintingFunction(BuildingObject buildingObject)
    {
        _buildingObject = buildingObject;
        _buildingKey = GetBuildingKey();
    }

    public void Execute()
    {
        // Manager 인스턴스 존재 여부 확인
        if (ImageGenerationManager.Instance == null)
        {
            Debug.Log("ImageGenerationManager가 초기화되지 않았습니다!");
            return;
        }

        // 저장된 이미지가 없는 경우 처리
        if (ImageGenerationManager.Instance.GeneratedImages.Count == 0)
        {
            Debug.Log("저장된 이미지가 없습니다!");
            return;
        }

        // 이 PaintingFunction만을 위한 콜백 등록
        ImageGenerationManager.Instance.SetImageSelectedCallback(OnImageSelected);

        // 이미지 선택 팝업 열기
        PopupManager.Instance.Open(EPopupType.UI_ImageSelector);
    }
    private void OnImageSelected(Texture2D selectedTexture)
    {
        // 선택된 이미지를 그림에 적용
        if (_buildingObject?.ExecuteInfoTransform != null && selectedTexture != null)
        {
            var rawImage = _buildingObject.ExecuteInfoTransform.GetComponent<RawImage>();
            if (rawImage != null)
            {
                rawImage.texture = selectedTexture;

                // 선택된 이미지 인덱스 찾아서 저장
                int imageIndex = ImageGenerationManager.Instance.GeneratedImages.IndexOf(selectedTexture);
                if (imageIndex >= 0)
                {
                    SaveAppliedImage(imageIndex);
                    Debug.Log($"그림이 변경되고 저장되었습니다! (인덱스: {imageIndex})");
                }

            }
        }
    }

    public void Update()
    {
        // ImageGenerationManager가 준비되면 저장된 이미지 로드 시도
        if (!_imageLoaded && ImageGenerationManager.Instance != null)
        {
            LoadAppliedImage();
            _imageLoaded = true;
        }
    }

    private string GetBuildingKey()
    {
        ChunkPosition chunkPosition = WorldManager.Instance.GetChunkAtWorldPosition(_buildingObject.transform.position).Position;
        Vector3 pos = _buildingObject.transform.position;
        Vector3 chunkLocalPos = WorldManager.Instance.GetLocalPositionInChunk(pos, chunkPosition);

        return $"Painting_{chunkPosition.ToChunkId()}_{chunkLocalPos.x}_{chunkLocalPos.z}";
    }

    private void SaveAppliedImage(int imageIndex)
    {
        PlayerPrefs.SetInt(_buildingKey, imageIndex);
        PlayerPrefs.Save();
    }

    private void LoadAppliedImage()
    {
        int imageIndex = PlayerPrefs.GetInt(_buildingKey, -1);

        if (imageIndex >= 0 && imageIndex < ImageGenerationManager.Instance.GeneratedImages.Count)
        {
            var texture = ImageGenerationManager.Instance.GeneratedImages[imageIndex];
            if (texture != null && _buildingObject?.ExecuteInfoTransform != null)
            {
                var rawImage = _buildingObject.ExecuteInfoTransform.GetComponent<RawImage>();
                if (rawImage != null)
                {
                    rawImage.texture = texture;
                    Debug.Log($"저장된 이미지를 불러왔습니다! (인덱스: {imageIndex})");
                }
            }
        }
    }
}
