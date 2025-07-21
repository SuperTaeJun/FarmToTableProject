using UnityEngine;

public class PlayerFarmingAbility : PlayerAbility
{
    private ECropType _currentSeed = ECropType.Carrot;

    public void SetCurrentSeed(int type)
    {
        _currentSeed = (ECropType)type;
    }

    public void OnPlantCrop()
    {
        Vector3 selectedPos = _owner.CurrentSelectedPos;
        string chunkId = WorldManager.GetChunkId(selectedPos);

        if (CropsManager.Instance != null)
        {
            _ = CropsManager.Instance.PlantCrop(_currentSeed, chunkId, selectedPos);
            if (ObjectPoolManager.Instance)
                ObjectPoolManager.Instance.Get(PoolType.Spark, selectedPos);
        }
    }
    public void OnCultivate()
    {
        Vector3 selectedPos = _owner.CurrentSelectedPos;
        string chunkId = WorldManager.GetChunkId(selectedPos);

        if (WorldManager.Instance != null)
        {
            Chunk chunk = WorldManager.Instance.GetChunkAtWorldPosition(selectedPos);
            Vector3 localPos = WorldManager.Instance.GetLocalPositionInChunk(selectedPos, chunk.Position);

            // 현재 블록 가져오기
            Block currentBlock = chunk.GetBlock((int)localPos.x, (int)localPos.y, (int)localPos.z);

            if (currentBlock != null && (currentBlock.Type == EBlockType.Dirt || currentBlock.Type == EBlockType.Grass))
            {
                Vector3 abovePosition = selectedPos + Vector3.up * 0.25f;
                WorldManager.Instance.SetBlock(abovePosition, EBlockType.Farmland);

                if(ObjectPoolManager.Instance)
                    ObjectPoolManager.Instance.Get(PoolType.Smoke, selectedPos);
            }
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
            if (ObjectPoolManager.Instance)
                ObjectPoolManager.Instance.Get(PoolType.Dust, selectedPos);
            //인벤토리랑 연동
        }
    }

}
