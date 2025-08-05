using UnityEngine;

public class HarvestStageAnimationStrategy : IFarmingAnimationStrategy
{
    public void ExecuteAnimation(Player player, Vector3 selectedPos)
    {
        player.Animator.SetTrigger("Harvest");
    }
}