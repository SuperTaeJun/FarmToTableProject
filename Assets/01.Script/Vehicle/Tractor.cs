using UnityEngine;
using DG.Tweening;
using System.Collections;


public class Tractor : Vehicle
{
    [Header("Tractor Specific")]
    public float plowingSpeed = 2f;
    public float plowingWidth = 3f;
    public LayerMask farmlandLayer;
    
    [Header("Animation Settings")]
    public float animationDuration = 0.5f;
    public float scaleMultiplier = 1.8f;
    private bool isAnimating = false;

    protected override void Awake()
    {
        base.Awake();
        vehicleType = EVehicleType.Tractor;
    }

    public void HandleFarming()
    {
        if (isAnimating) return; // 애니메이션 중이면 실행 안함
        
        Vector3 currentSelectedPos = currentDriver.CurrentSelectedPos;
        
        // 스케일 애니메이션과 함께 농사 작업 실행
        StartCoroutine(PlayFarmingAnimation(currentSelectedPos));
    }

    private IEnumerator PlayFarmingAnimation(Vector3 currentSelectedPos)
    {
        isLockMovement = true;
        isAnimating = true;
        Vector3 originalScale = transform.localScale;

        Sequence farmingSequence = DOTween.Sequence();
        farmingSequence
            // 작아지면서 움츠러드는 느낌
            .Append(transform.DOScale(originalScale * 0.7f, animationDuration * 0.2f).SetEase(Ease.InQuad))

            // 빠르게 커지면서 통통 튀는 느낌
            .Append(transform.DOScale(originalScale * 1.3f, animationDuration * 0.3f).SetEase(Ease.OutBounce))

            // 살짝 작아졌다가
            .Append(transform.DOScale(originalScale * 0.85f, animationDuration * 0.2f).SetEase(Ease.InOutSine))

            // 원래 크기로 부드럽게 복귀
            .Append(transform.DOScale(originalScale, animationDuration * 0.3f).SetEase(Ease.OutElastic))

            .OnComplete(() => HandleFarmingAtPosition(currentSelectedPos));

        yield return farmingSequence.WaitForCompletion();
        isAnimating = false;
        isLockMovement = false;
    }

    private void HandleFarmingAtPosition(Vector3 currentSelectedPos)
    {
        var growthStage = CropsManager.Instance?.GetCropGrowthStageAtWorldPosition(currentSelectedPos);

        switch (growthStage)
        {
            case null:
                if (CanPlantAt(currentSelectedPos))
                {
                    ECropType selectedCrop = currentDriver.GetAbility<PlayerFarmingAbility>().CurrentSeed;
                    // 인벤토리에 씨앗이 있는지 체크
                    if (InventoryManager.Instance.HasSeed(selectedCrop) == false)
                    {
                        currentDriver.GetAbility<PlayerNotificationAbility>()?.ActiveDialogBox(EPlayerNotificationType.LackOfSeed);
                        return;
                    }
                    PlantCropAtPosition(selectedCrop);
                }
                else
                {
                    // 경작
                    CultivateAtPosition(currentSelectedPos);
                }
                break;

            case ECropGrowthStage.Seed:
                // 씨앗 단계에서는 작업 안함
                break;

            case ECropGrowthStage.Vegetative:
            case ECropGrowthStage.Mature:
                // 물주기
                if (CanWaterAtPosition(currentSelectedPos))
                {
                    WaterCropAtPosition(currentSelectedPos);
                }
                break;

            case ECropGrowthStage.Harvest:
                // 수확
                HarvestCropAtPosition(currentSelectedPos);
                break;
        }
    }

    private bool CanPlantAt(Vector3 position)
    {
        EBlockType blockType = WorldManager.Instance.GetBlockType(position);
        return blockType == EBlockType.Farmland;
    }

    private bool CanWaterAtPosition(Vector3 position)
    {
        var crop = CropsManager.Instance?.GetCropAtWorldPosition(position);
        if (crop == null) return false;

        return crop.GrowthStage != ECropGrowthStage.Seed &&
               crop.GrowthStage != ECropGrowthStage.Harvest &&
               !crop.IsWateredForCurrentStage();
    }

    private void CultivateAtPosition(Vector3 position)
    {
        string chunkId = WorldManager.GetChunkId(position);

        if (WorldManager.Instance != null)
        {
            Chunk chunk = WorldManager.Instance.GetChunkAtWorldPosition(position);

            Vector3 localPos = WorldManager.Instance.GetLocalPositionInChunk(position, chunk.Position);
            Block currentBlock = chunk.GetBlock((int)localPos.x, (int)localPos.y, (int)localPos.z);

            if (currentBlock != null && (currentBlock.Type == EBlockType.Dirt || currentBlock.Type == EBlockType.Grass))
            {
                Vector3 abovePosition = position + Vector3.up * (ChunkGenerator.Instance.blockOffset.y * 0.5f);

                WorldManager.Instance.SetBlock(abovePosition, EBlockType.Farmland, 3);

                if (ObjectPoolManager.Instance)
                    ObjectPoolManager.Instance.Get(PoolType.SomkeL, position);
            }

        }
    }

    private void PlantCropAtPosition(ECropType selectedCrop)
    {
        Vector3 selectedPos = currentDriver.CurrentSelectedPos;
        string chunkId = WorldManager.GetChunkId(selectedPos);

        if (CropsManager.Instance != null)
        {
            _ = CropsManager.Instance.PlantCrop(selectedCrop, chunkId, selectedPos);
            if (ObjectPoolManager.Instance)
                ObjectPoolManager.Instance.Get(PoolType.Spark, selectedPos);
        }
    }

    private void WaterCropAtPosition(Vector3 position)
    {
        ObjectPoolManager.Instance.Get(PoolType.CropWater, position);

        string chunkId = WorldManager.GetChunkId(position);

        if (CropsManager.Instance != null)
        {
            Chunk chunk = WorldManager.Instance.GetChunkAtWorldPosition(position);
            Vector3 localPos = WorldManager.Instance.GetLocalPositionInChunk(position, chunk.Position);
            _ = CropsManager.Instance.WaterCrop(chunkId, localPos);
        }
    }

    private void HarvestCropAtPosition(Vector3 position)
    {
        string chunkId = WorldManager.GetChunkId(position);

        if (CropsManager.Instance != null)
        {
            Chunk chunk = WorldManager.Instance.GetChunkAtWorldPosition(position);
            Vector3 localPos = WorldManager.Instance.GetLocalPositionInChunk(position, chunk.Position);
            _ = CropsManager.Instance.HarvestCrop(chunkId, localPos);
            if (ObjectPoolManager.Instance)
                ObjectPoolManager.Instance.Get(PoolType.Dust, position);
        }
    }

    protected override void OnPlayerMounted(Player player)
    {
    }

    protected override void OnPlayerDismounted(Player player)
    {
        Debug.Log("트랙터 하차");
    }
}