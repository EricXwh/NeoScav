using UnityEngine;

public class PlayerJumpingState : PlayerStateBase
{
    private float jumpStartTime;
    
    public override void Enter(PlayerController player)
    {
        base.Enter(player);
        jumpStartTime = Time.time;
        
         if (!player.isBouncing)
        {
            player.velocity.y = Mathf.Sqrt(player.jumpHeight * -2f * player.gravity);
        }
        else
        {
            player.isBouncing = false;
        }
    }
    
    public override void Update()
    {
        base.Update();
        if (player.isCarrying && player.currentCarryBlock != null)
        {
            float blockHeight = player.GetBlockHeight(player.currentCarryBlock);
            Vector3 desiredPosition = player.interactionPoint.position + new Vector3(0, 1f + blockHeight / 2f, 0);
            
            // 在跳跃状态中，确保方块跟随玩家
            player.currentCarryBlock.transform.position = Vector3.Lerp(
                player.currentCarryBlock.transform.position, 
                desiredPosition, 
                Time.deltaTime * player.Smoothness
            ) + player.carryBounceVelocity * Time.deltaTime;
            
            // 缓慢减小弹跳效果
            player.carryBounceVelocity = Vector3.Lerp(player.carryBounceVelocity, Vector3.zero, 2f * Time.deltaTime);
            
            // 保持旋转一致
            player.currentCarryBlock.transform.rotation = transform.rotation * player.carryRotationOffset;
        }
        // 检查是否应该返回到之前的状态
        if (player.isGrounded && Time.time - jumpStartTime > 0.1f)
        {
            if (player.IsCarrying())
                player.SetState<PlayerCarryingState>();
            else
                player.SetState<PlayerIdleState>();
        }
    }
    
    public override void HandleJump()
    {
        // 跳跃状态不处理新的跳跃输入
    }
}
