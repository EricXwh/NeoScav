using UnityEngine;

public class PlayerIdleState : PlayerStateBase
{
    public override void Enter(PlayerController player)
    {
        base.Enter(player);
    }
    
    public override void HandleMovement()
    {
        base.HandleMovement();
        
        player.DetectCarryBlockInFront();
    }
}