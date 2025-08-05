using System.Collections;
using UnityEngine;

public class PlayerAnimAbility : PlayerAbility
{
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
                _owner.GetAbility<PlayerVisualAbility>()?.SetActiveVisualPart(EVisualPart.Shovel);
                _owner.InputController.SetPlayerMoveInputLock(true);
                break;

            case EPlayerMode.Farming:
                _owner.InputController.SetPlayerMoveInputLock(true);
                HandleFarmingAnimation();
                break;

            case EPlayerMode.Construction:
                break;
            case EPlayerMode.Vehicle:
                break;
        }
    }

    private void HandleFarmingAnimation()
    {
        Vector3 selectedPos = _owner.CurrentSelectedPos;
        var growthStage = CropsManager.Instance?.GetCropGrowthStageAtWorldPosition(selectedPos);
        switch (growthStage)
        {
            case null:
                if (CanPlantAt(selectedPos))
                {
                    ECropType currentCrop = _owner.GetAbility<PlayerFarmingAbility>().CurrentSeed;
                    if (InventoryManager.Instance.HasSeed(currentCrop) == false)
                    {
                        _owner.GetAbility<PlayerNotificationAbility>()?.ActiveDialogBox(EPlayerNotificationType.LackOfSeed);
                        _owner.InputController.SetPlayerMoveInputLock(false);
                        return;
                    }
                    _owner.Animator.SetTrigger("Plant");
                }
                else
                {
                    _owner.Animator.SetTrigger("Cultivate");
                    _owner.GetAbility<PlayerVisualAbility>()?.SetActiveVisualPart(EVisualPart.Rake);
                }
                break;
            case ECropGrowthStage.Seed:
                Debug.Log("씨앗 단계에서는 물을 줄 수 없습니다.");
                break;
            case ECropGrowthStage.Vegetative:
            case ECropGrowthStage.Mature:
                // 이미 물을 준 상태인지 확인
                if (CanWaterAtPosition(selectedPos))
                {
                    Debug.Log("물을주는중");
                    _owner.Animator.SetTrigger("Watering");
                    _owner.GetAbility<PlayerVisualAbility>()?.SetActiveVisualPart(EVisualPart.WateringCan);
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

            Vector3 currentEuler = effect.transform.eulerAngles;
            float yRotation = currentEuler.y;

            Vector3 playerMoveDirection = _owner.gameObject.transform.forward; 

            if (playerMoveDirection.magnitude > 0.1f)
            {
                // 이동방향의 반대
                Vector3 oppositeDirection = -playerMoveDirection;
                oppositeDirection.y = 0; 

                yRotation = Mathf.Atan2(oppositeDirection.x, oppositeDirection.z) * Mathf.Rad2Deg;
            }
            effect.transform.rotation = Quaternion.Euler(currentEuler.x, yRotation, currentEuler.z);
        }
        SoundManager.Instance.PlaySFX(SFXType.Step);
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
        _owner.GetAbility<PlayerVisualAbility>()?.SetDisActiveVisualAllPart();
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
