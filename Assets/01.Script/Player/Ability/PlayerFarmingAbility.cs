using UnityEngine;

public class PlayerFarmingAbility : PlayerAbility
{
    private ECropType _currentSeed = ECropType.Carrot;

    private void Start()
    {
        //_owner.InputController.OnFarmingInput.AddListener(HandleFarmingInput);
        //_owner.InputController.OnWateringInput.AddListener(OnWaterCrop);
    }

    private void HandleFarmingInput()
    {
        //현재 선택된거에따라서 다르게 작동해야함
        //선택된게 작물이있는 블럭이 아니면 조기리턴 시켜라

        OnPlantCrop();

        //OnWaterCrop();
        //OnHarvestCrop();
    }

    public void SetCurrentSeed(int type)
    {
        _currentSeed = (ECropType)type;
    }
    private void OnPlantCrop()
    {
        // 현재 청크 ID 계산
        string chunkId = WorldManager.GetChunkId(_owner.CurrentSelectedPos);

        // 작물 심기 (당근 예시)
        if (CropsManager.Instance != null)
        {
            _ = CropsManager.Instance.PlantCrop(_currentSeed, chunkId, _owner.CurrentSelectedPos);
            Debug.Log($"{_currentSeed.ToString()}을 심었습니다: {_owner.CurrentSelectedPos}");
        }
    }

    public void OnWaterCrop()
    {
        string chunkId = WorldManager.GetChunkId(_owner.CurrentSelectedPos);

        if (CropsManager.Instance != null)
        {
            Chunk chunk = WorldManager.Instance.GetChunkAtWorldPosition(_owner.CurrentSelectedPos);
            Vector3 localPos = WorldManager.Instance.GetLocalPositionInChunk(_owner.CurrentSelectedPos, chunk.Position);
            _ = CropsManager.Instance.WaterCrop(chunkId, localPos);
            Debug.Log($"물을 주었습니다: {_owner.CurrentSelectedPos}");
        }
    }

    private void OnHarvestCrop()
    {
        string chunkId = WorldManager.GetChunkId(_owner.CurrentSelectedPos);

        if (CropsManager.Instance != null)
        {
            _ = CropsManager.Instance.HarvestCrop(chunkId, _owner.CurrentSelectedPos);
            Debug.Log($"수확했습니다: {_owner.CurrentSelectedPos}");

            //인벤토리랑 연동
        }
    }


}
