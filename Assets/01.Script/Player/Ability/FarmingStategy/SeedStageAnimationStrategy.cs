using UnityEngine;

public class SeedStageAnimationStrategy : IFarmingAnimationStrategy
{
    public void ExecuteAnimation(Player player, Vector3 selectedPos)
    {
        Debug.Log("씨앗 단계에서는 할 수 있는 것이 없습니다.");
        player.GetAbility<PlayerInputAbility>()?.SetPlayerMoveInputLock(false);
    }
}