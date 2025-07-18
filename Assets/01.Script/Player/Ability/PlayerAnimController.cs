using System.Collections;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;
public class PlayerAnimController : MonoBehaviour
{
    private Player _owner;
    private void Awake()
    {
        _owner= GetComponent<Player>();
    }
    private void Start()
    {
        _owner.InputController.OnWateringInput.AddListener(OnTriggerWateringAnim);
    }
    private void OnTriggerWateringAnim()
    {
        _owner.InputController.SetPlayerMoveInput(true);
        _owner.Animator.SetTrigger("Watering");
    }
    private void OnCompleteWateringAnim()
    {
        _owner.InputController.SetPlayerMoveInput(false);
        _owner.GetAbility<PlayerFarmingAbility>().OnWaterCrop();
    }
}
