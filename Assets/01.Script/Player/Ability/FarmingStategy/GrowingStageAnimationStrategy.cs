using UnityEngine;

public class GrowingStageAnimationStrategy : IFarmingAnimationStrategy
{
    public void ExecuteAnimation(Player player, Vector3 selectedPos)
    {
        if (CropsManager.Instance.CanWatering(selectedPos))
        {
            Debug.Log("물을주는중");
            player.Animator.SetTrigger("Watering");
            player.GetAbility<PlayerVisualAbility>()?.SetActiveVisualPart(EVisualPart.WateringCan);
        }
        else
        {
            player.GetAbility<PlayerInputAbility>()?.SetPlayerMoveInputLock(false);
        }
    }

}