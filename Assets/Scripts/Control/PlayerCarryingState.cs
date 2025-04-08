using UnityEngine;

public class PlayerCarryingState : PlayerStateBase
{
    public override void Enter(PlayerController player)
    {
        base.Enter(player);
        // 搬运状态特有的初始化
    }
    
    public override void HandleMovement()
    {
        base.HandleMovement();
        
        // 更新搬运物体位置
        if (player.currentCarryBlock != null)
        {
            float blockHeight = player.GetBlockHeight(player.currentCarryBlock);
            Vector3 desiredPosition = player.interactionPoint.position + new Vector3(0, 1f + blockHeight / 2f, 0);
            
            if (controller.isGrounded)
            {
                player.carryBounceVelocity = Vector3.zero;
                player.currentCarryBlock.transform.position = Vector3.Lerp(player.currentCarryBlock.transform.position, desiredPosition, Time.deltaTime * player.Smoothness);
            }
            else
            {
                player.currentCarryBlock.transform.position = Vector3.Lerp(player.currentCarryBlock.transform.position, desiredPosition, Time.deltaTime * player.Smoothness) 
                                                    + player.carryBounceVelocity * Time.deltaTime;
                player.carryBounceVelocity = Vector3.Lerp(player.carryBounceVelocity, Vector3.zero, 0.1f * Time.deltaTime);
            }
            
            // 保持旋转一致
            player.currentCarryBlock.transform.rotation = transform.rotation * player.carryRotationOffset;
        }
        
        // 检查是否可以放置方块
        player.canPlaceBlock = player.CheckIfCanPlaceBlock();
    }
    
    public override void HandleCarrying()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            player.AttemptStopCarrying();
        }
    }
}