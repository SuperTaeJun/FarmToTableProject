using UnityEngine;

public class EmptyLandAnimationStrategy : IFarmingAnimationStrategy
{
    public void ExecuteAnimation(Player player, Vector3 selectedPos)
    {
        if (ForageManager.Instance.GetForageAtWorldPosition(selectedPos) != null)
        {
            player.GetAbility<PlayerInputAbility>()?.SetPlayerMoveInputLock(false);
            return;
        }

        if (CropsManager.Instance.CanPlant(selectedPos))
        {
            ECropType currentCrop = player.GetAbility<PlayerFarmingAbility>().CurrentSeed;
            if (InventoryManager.Instance.HasSeed(currentCrop) == false)
            {
                player.GetAbility<PlayerNotificationAbility>()?.ActiveDialogBox(EPlayerNotificationType.LackOfSeed);
                player.GetAbility<PlayerInputAbility>()?.SetPlayerMoveInputLock(false);
                return;
            }
            player.Animator.SetTrigger("Plant");
        }
        else
        {
            player.Animator.SetTrigger("Cultivate");
            player.GetAbility<PlayerVisualAbility>()?.SetActiveVisualPart(EVisualPart.Rake);
        }
    }
}