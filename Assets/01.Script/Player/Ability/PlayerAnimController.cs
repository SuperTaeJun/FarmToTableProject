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
        Debug.Log(growthStage);

        switch (growthStage)
        {
            case null: // 작물이 없음 - 심기
                _owner.Animator.SetTrigger("Plant");
                //if (CanPlantAt(selectedPos))
                //{
                //    _owner.Animator.SetTrigger("Plant");
                //}
                //else
                //{
                //    Debug.Log("여기에는 심을 수 없습니다.");
                //    _owner.InputController.SetPlayerMoveInputLock(false);
                //    return;
                //}
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
        return blockType == EBlockType.Dirt || blockType == EBlockType.Grass;
    }
}
