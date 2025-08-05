using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimAbility : PlayerAbility
{
    private Dictionary<string, IFarmingAnimationStrategy> _farmingAnimationStrategies;
    private void Start()
    {
        _owner.GetAbility<PlayerInputAbility>()?.OnRightMouseInput.AddListener(OnTriggerLeftMouseAnim);
        InitFarmingAnimStrategies();
    }

    private void InitFarmingAnimStrategies()
    {
        _farmingAnimationStrategies = new Dictionary<string, IFarmingAnimationStrategy>
        {
            { "Empty", new EmptyLandAnimationStrategy() },
            {ECropGrowthStage.Seed.ToString(), new SeedStageAnimationStrategy() },
            { ECropGrowthStage.Vegetative.ToString(), new GrowingStageAnimationStrategy() },
            {ECropGrowthStage.Mature.ToString(), new GrowingStageAnimationStrategy() },
            { ECropGrowthStage.Harvest.ToString(), new HarvestStageAnimationStrategy() }
        };
    }
    private void OnTriggerLeftMouseAnim(EPlayerMode currentMode)
    {
        switch (currentMode)
        {
            case EPlayerMode.BlockEdit:
                _owner.Animator.SetTrigger("Dig");
                _owner.GetAbility<PlayerVisualAbility>()?.SetActiveVisualPart(EVisualPart.Shovel);
                _owner.GetAbility<PlayerInputAbility>()?.SetPlayerMoveInputLock(true);
                break;

            case EPlayerMode.Farming:
                _owner.GetAbility<PlayerInputAbility>()?.SetPlayerMoveInputLock(true);
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
        ECropGrowthStage? growthStage = CropsManager.Instance?.GetCropGrowthStageAtWorldPosition(selectedPos);

        string stageKey = growthStage?.ToString() ?? "Empty";

        if (_farmingAnimationStrategies.TryGetValue(stageKey, out IFarmingAnimationStrategy strategy))
        {
            strategy.ExecuteAnimation(_owner, selectedPos);
        }
    }

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
        _owner.GetAbility<PlayerInputAbility>()?.SetPlayerMoveInputLock(false);
        _owner.GetAbility<PlayerVisualAbility>()?.SetDisActiveVisualAllPart();
    }

}
