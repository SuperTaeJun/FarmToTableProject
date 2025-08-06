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
                if (CropsManager.Instance.CanPlant(currentSelectedPos))
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
                if (CropsManager.Instance.CanWatering(currentSelectedPos))
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
        
        // 3x3 범위에서 씨앗 심기
        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                Vector3 plantPosition = selectedPos + new Vector3(x * 2, 0, z * 2);
                string chunkId = WorldManager.GetChunkId(plantPosition);

                if (CropsManager.Instance.CanPlant(selectedPos)&& CropsManager.Instance != null)
                {
                    // 이미 작물이 있는지 체크
                    var existingCrop = CropsManager.Instance.GetCropAtWorldPosition(plantPosition);
                    if (existingCrop == null)
                    {
                        _ = CropsManager.Instance.PlantCrop(selectedCrop, chunkId, plantPosition);
                        if (ObjectPoolManager.Instance)
                            ObjectPoolManager.Instance.Get(PoolType.Spark, plantPosition);
                    }
                }
            }
        }
    }

    private void WaterCropAtPosition(Vector3 position)
    {
        // 3x3 범위에서 물주기
        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                Vector3 waterPosition = position + new Vector3(x * 2, 0, z * 2);
                
                if (CropsManager.Instance.CanWatering(position))
                {
                    string chunkId = WorldManager.GetChunkId(waterPosition);

                    if (CropsManager.Instance != null)
                    {
                        Chunk chunk = WorldManager.Instance.GetChunkAtWorldPosition(waterPosition);
                        Vector3 localPos = WorldManager.Instance.GetLocalPositionInChunk(waterPosition, chunk.Position);
                        _ = CropsManager.Instance.WaterCrop(chunkId, localPos);
                    }
                }
            }
        }
    }

    private void HarvestCropAtPosition(Vector3 position)
    {
        // 3x3 범위에서 수확하기
        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                Vector3 harvestPosition = position + new Vector3(x * 2, 0, z * 2);
                string chunkId = WorldManager.GetChunkId(harvestPosition);

                var growthStage = CropsManager.Instance?.GetCropGrowthStageAtWorldPosition(harvestPosition);
                
                if (growthStage == ECropGrowthStage.Harvest && CropsManager.Instance != null)
                {
                    Chunk chunk = WorldManager.Instance.GetChunkAtWorldPosition(harvestPosition);
                    Vector3 localPos = WorldManager.Instance.GetLocalPositionInChunk(harvestPosition, chunk.Position);
                    _ = CropsManager.Instance.HarvestCrop(chunkId, localPos);
                    if (ObjectPoolManager.Instance)
                        ObjectPoolManager.Instance.Get(PoolType.Dust, harvestPosition);
                }
            }
        }
    }

    protected override void OnPlayerMounted(Player player)
    {
        base.OnPlayerMounted(player);

        var selectAbility = currentDriver.GetAbility<PlayerSelectAbility>();
        selectAbility.SetGridSize(new Vector2Int(3,3));
        
        // 다음 프레임에 강제로 업데이트
        StartCoroutine(UpdateGridNextFrame());
    }
    
    private IEnumerator UpdateGridNextFrame()
    {
        yield return null; // 한 프레임 대기
        if (currentDriver != null)
        {
            currentDriver.GetAbility<PlayerSelectAbility>().SetGridSize(new Vector2Int(3,3));
        }
    }

    protected override void OnPlayerDismounted(Player player)
    {
        base.OnPlayerDismounted(player);

        currentDriver.GetAbility<PlayerSelectAbility>().SetGridSize(new Vector2Int(1, 1));
        Debug.Log("트랙터 하차");
    }
}