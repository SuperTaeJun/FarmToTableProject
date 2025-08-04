using System;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerChunkController
{
    private Player _owner;

    // 이벤트
    public event Action<ChunkPosition> OnChunkPurchased;

    public PlayerChunkController(Player owner)
    {
        _owner = owner;
    }

    public async Task<bool> TryGenerateChunkAsync()
    {
        try
        {
            // 돈 지불 체크
            bool canBuy = await CurrencyManager.Instance.TrySpendCurrency(ECurrencyType.Money, 500);
            if (!canBuy)
            {
                _owner.GetAbility<PlayerNotificationAbility>()?.ActiveDialogBox(EPlayerNotificationType.LackOfMoney);
                return false;
            }

            // 청크 생성
            var targetPos = CalculateTargetChunkPosition();

            if (targetPos == null)
            {
                return false;
            }

            if (WorldManager.Instance.HasChunk(targetPos.Value))
            {
                return false;
            }

            // 청크 생성 실행
            FadeManager.Instance.FadeScreenWithEvent(() => WorldManager.Instance.GenerateAndBuildChunk(targetPos.Value));

            OnChunkPurchased?.Invoke(targetPos.Value);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"청크 생성 중 오류 발생: {ex.Message}");
            return false;
        }
    }

    private ChunkPosition? CalculateTargetChunkPosition()
    {
        Vector3 pos = _owner.transform.position;

        // 청크 크기 계산
        float chunkSizeX = Chunk.ChunkSize * WorldManager.Instance.dynamicGenerator.blockOffset.x;
        float chunkSizeZ = Chunk.ChunkSize * WorldManager.Instance.dynamicGenerator.blockOffset.z;

        // 현재 청크 좌표 계산
        int chunkX = Mathf.FloorToInt(pos.x / chunkSizeX);
        int chunkZ = Mathf.FloorToInt(pos.z / chunkSizeZ);

        // 청크 원점 계산
        float chunkOriginX = chunkX * chunkSizeX;
        float chunkOriginZ = chunkZ * chunkSizeZ;

        // 플레이어의 청크 내 로컬 위치
        float localX = pos.x - chunkOriginX;
        float localZ = pos.z - chunkOriginZ;

        // 각 경계까지의 거리 계산
        float distLeft = localX;
        float distRight = chunkSizeX - localX;
        float distBack = localZ;
        float distForward = chunkSizeZ - localZ;

        // 가장 가까운 경계 찾기
        float minDist = Mathf.Min(distLeft, distRight, distBack, distForward);

        // 경계에서 너무 멀면 청크 생성 불가
        if (minDist > 3.0f)
        {
            return null;
        }

        // 생성할 청크 방향 결정
        int moveX = 0;
        int moveZ = 0;

        if (minDist == distLeft)
            moveX = -1;
        else if (minDist == distRight)
            moveX = +1;
        else if (minDist == distBack)
            moveZ = -1;
        else if (minDist == distForward)
            moveZ = +1;

        if (moveX == 0 && moveZ == 0)
        {
            Debug.Log("청크 생성 방향을 결정할 수 없습니다.");
            return null;
        }

        // 목표 청크 위치 계산
        int targetChunkX = chunkX + moveX;
        int targetChunkZ = chunkZ + moveZ;

        return new ChunkPosition(targetChunkX, 0, targetChunkZ);
    }

}