using UnityEngine;

public class PlayerFarmingAbility : PlayerAbility
{
    private ECropType _currentSeed = ECropType.Carrot;

    public void SetCurrentSeed(int type)
    {
        _currentSeed = (ECropType)type;
    }

    //public void OnFarmingInput()
    //{
    //    Vector3 selectedPos = _owner.CurrentSelectedPos;
    //    var growthStage = CropsManager.Instance?.GetCropGrowthStageAtWorldPosition(selectedPos);

    //    switch (growthStage)
    //    {
    //        case null: // 작물이 없음
    //                   //if (CanPlantAt(selectedPos))
    //            OnPlantCrop();
    //            //else
    //            //    Debug.Log("여기에는 심을 수 없습니다.");
    //            break;

    //        case ECropGrowthStage.Seed:
    //            Debug.Log("씨앗 단계에서는 물을 줄 수 없습니다.");
    //            break;

    //        case ECropGrowthStage.Vegetative:
    //        case ECropGrowthStage.Mature:
    //            OnWaterCrop();
    //            break;

    //        case ECropGrowthStage.Harvest:
    //            OnHarvestCrop();
    //            break;
    //    }
    //}

    private bool CanPlantAt(Vector3 position)
    {
        EBlockType blockType = WorldManager.Instance.GetBlockType(position);
        return blockType == EBlockType.Dirt || blockType == EBlockType.Grass;
    }

    public void OnPlantCrop()
    {
        Vector3 selectedPos = _owner.CurrentSelectedPos;
        string chunkId = WorldManager.GetChunkId(selectedPos);

        if (CropsManager.Instance != null)
        {
            _ = CropsManager.Instance.PlantCrop(_currentSeed, chunkId, selectedPos);
            Debug.Log($"{_currentSeed.ToString()}을 심었습니다: {selectedPos}");
        }
    }

    public void OnWaterCrop()
    {
        Debug.Log("물주는함수 호출");

        Vector3 selectedPos = _owner.CurrentSelectedPos;
        string chunkId = WorldManager.GetChunkId(selectedPos);

        if (CropsManager.Instance != null)
        {
            Chunk chunk = WorldManager.Instance.GetChunkAtWorldPosition(selectedPos);
            Vector3 localPos = WorldManager.Instance.GetLocalPositionInChunk(selectedPos, chunk.Position);
            _ = CropsManager.Instance.WaterCrop(chunkId, localPos);
            Debug.Log($"물을 주었습니다: {selectedPos}");
        }
    }

    public void OnHarvestCrop()
    {
        Vector3 selectedPos = _owner.CurrentSelectedPos;
        string chunkId = WorldManager.GetChunkId(selectedPos);

        if (CropsManager.Instance != null)
        {
            Chunk chunk = WorldManager.Instance.GetChunkAtWorldPosition(selectedPos);
            Vector3 localPos = WorldManager.Instance.GetLocalPositionInChunk(selectedPos, chunk.Position);
            _ = CropsManager.Instance.HarvestCrop(chunkId, localPos);
            Debug.Log($"수확했습니다: {selectedPos}");
            //인벤토리랑 연동
        }
    }

}
