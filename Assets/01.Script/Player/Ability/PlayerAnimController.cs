using System.Collections;
using UnityEngine;

public class PlayerAnimController : MonoBehaviour
{
    private Player _owner;

    private void Awake()
    {
        _owner = GetComponent<Player>();
    }

    private void Start()
    {
        _owner.InputController.OnLeftMouseInput.AddListener(OnTriggerLeftMouseAnim);
    }

    private void OnTriggerLeftMouseAnim(EPlayerMode currentMode)
    {
        switch (currentMode)
        {
            case EPlayerMode.BlockEdit:
                _owner.Animator.SetTrigger("Dig");
                _owner.InputController.SetPlayerMoveInputLock(true);
                break;

            case EPlayerMode.Farming:
                HandleFarmingAnimation();
                _owner.InputController.SetPlayerMoveInputLock(true);
                break;

            case EPlayerMode.Construction:
                break;
        }
    }

    private void HandleFarmingAnimation()
    {
        Vector3 selectedPos = _owner.CurrentSelectedPos;
        var growthStage = CropsManager.Instance?.GetCropGrowthStageAtWorldPosition(selectedPos);
        switch (growthStage)
        {
            case null: // 작물이 없음 - 심기
                if (CanPlantAt(selectedPos))
                {
                    _owner.Animator.SetTrigger("Plant");
                }
                else
                {
                    _owner.Animator.SetTrigger("Cultivate");
                }
                break;
            case ECropGrowthStage.Seed:
                Debug.Log("씨앗 단계에서는 물을 줄 수 없습니다.");
                return;
            case ECropGrowthStage.Vegetative:
            case ECropGrowthStage.Mature:
                // 이미 물을 준 상태인지 확인
                if (CanWaterAtPosition(selectedPos))
                {
                    Debug.Log("물을주는중");
                    _owner.Animator.SetTrigger("Watering");
                }
                break;

            case ECropGrowthStage.Harvest:
                _owner.Animator.SetTrigger("Harvest");
                break;
        }
    }


    // 애니메이션 이벤트들
    private void OnFootStepVfx()
    {
        Vector3 effectPosition = transform.position;
        GameObject effect = ObjectPoolManager.Instance.Get(PoolType.FootStep, effectPosition);

        if (effect != null)
        {
            // 기존 회전값 가져오기
            Vector3 currentEuler = effect.transform.eulerAngles;
            float yRotation = currentEuler.y; // 기본값

            // _owner(플레이어)의 이동방향 가져오기
            Vector3 playerMoveDirection = _owner.gameObject.transform.forward; // 또는 적절한 메서드명

            if (playerMoveDirection.magnitude > 0.1f)
            {
                // 이동방향의 반대
                Vector3 oppositeDirection = -playerMoveDirection;
                oppositeDirection.y = 0; // 수평 방향만

                // Y축 회전값 계산
                yRotation = Mathf.Atan2(oppositeDirection.x, oppositeDirection.z) * Mathf.Rad2Deg;
            }

            // X, Z축은 기존값 유지하고 Y축만 변경
            effect.transform.rotation = Quaternion.Euler(currentEuler.x, yRotation, currentEuler.z);
            //Vector3 effectPosition = transform.position;

            //GameObject effect =ObjectPoolManager.Instance.Get(PoolType.FootStep, effectPosition);

        }
    }
    private void OnCultivateAnim()
    {
        _owner.GetAbility<PlayerFarmingAbility>().OnCultivate();

    }
    private void OnCompleteBlockAnim()
    {
        _owner.GetAbility<PlayerBlockAbility>().OnBlockEditInput();
    }

    private void OnCompletePlantAnim()
    {
        _owner.GetAbility<PlayerFarmingAbility>().OnPlantCrop();
    }

    private void OnCompleteWaterAnim()
    {
        _owner.GetAbility<PlayerFarmingAbility>().OnWaterCrop();
    }

    private void OnCompleteHarvestAnim()
    {
        _owner.GetAbility<PlayerFarmingAbility>().OnHarvestCrop();
    }

    private void OnFinisAnim()
    {
        _owner.InputController.SetPlayerMoveInputLock(false);
    }

    private bool CanWaterAtPosition(Vector3 position)
    {
        var crop = CropsManager.Instance?.GetCropAtWorldPosition(position);
        if (crop == null) return false;

        return crop.GrowthStage != ECropGrowthStage.Seed &&
               crop.GrowthStage != ECropGrowthStage.Harvest &&
               !crop.IsWateredForCurrentStage();
    }
    private bool CanPlantAt(Vector3 position)
    {
        EBlockType blockType = WorldManager.Instance.GetBlockType(position);
        return blockType == EBlockType.Farmland;
    }
}
