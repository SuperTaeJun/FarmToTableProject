using UnityEngine;

public class PlayerFarmingAbility : PlayerAbility
{
    private ECropType _currentSeed = ECropType.Carrot;
    public ECropType CurrentSeed => _currentSeed;

    public void SetCurrentSeed(ECropType type)
    {
        _currentSeed = type;
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

            // ���� ���� ��������
            Block currentBlock = chunk.GetBlock((int)localPos.x, (int)localPos.y, (int)localPos.z);

            if (currentBlock != null && (currentBlock.Type == EBlockType.Dirt || currentBlock.Type == EBlockType.Grass))
            {
                Vector3 abovePosition = selectedPos + Vector3.up * (ChunkGenerator.Instance.blockOffset.y * 0.5f);
                WorldManager.Instance.SetBlock(abovePosition, EBlockType.Farmland);

                if(ObjectPoolManager.Instance)
                    ObjectPoolManager.Instance.Get(PoolType.Smoke, selectedPos);
            }
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
            if (ObjectPoolManager.Instance)
                ObjectPoolManager.Instance.Get(PoolType.Dust, selectedPos);
            //�κ��丮�� ����
        }
    }

}
