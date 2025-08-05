using UnityEngine;

public interface IFarmingAnimationStrategy
{
    void ExecuteAnimation(Player player, Vector3 selectedPos);
}