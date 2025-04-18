using UnityEngine;

public interface IPlayerState
{
    void Enter(PlayerController player);
    void Exit();
    void HandleInput();
    void Update();
    void HandleMovement();
    void HandleCarrying();
    void HandleJump();
    void ApplyGravity();
}