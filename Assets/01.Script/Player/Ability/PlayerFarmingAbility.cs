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
    //        case null: // �۹��� ����
    //                   //if (CanPlantAt(selectedPos))
    //            OnPlantCrop();
    //            //else
    //            //    Debug.Log("���⿡�� ���� �� �����ϴ�.");
    //            break;

    //        case ECropGrowthStage.Seed:
    //            Debug.Log("���� �ܰ迡���� ���� �� �� �����ϴ�.");
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
            Debug.Log($"{_currentSeed.ToString()}�� �ɾ����ϴ�: {selectedPos}");
        }
    }

    public void OnWaterCrop()
    {
        Debug.Log("���ִ��Լ� ȣ��");

        Vector3 selectedPos = _owner.CurrentSelectedPos;
        string chunkId = WorldManager.GetChunkId(selectedPos);

        if (CropsManager.Instance != null)
        {
            Chunk chunk = WorldManager.Instance.GetChunkAtWorldPosition(selectedPos);
            Vector3 localPos = WorldManager.Instance.GetLocalPositionInChunk(selectedPos, chunk.Position);
            _ = CropsManager.Instance.WaterCrop(chunkId, localPos);
            Debug.Log($"���� �־����ϴ�: {selectedPos}");
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
            Debug.Log($"��Ȯ�߽��ϴ�: {selectedPos}");
            //�κ��丮�� ����
        }
    }

}
