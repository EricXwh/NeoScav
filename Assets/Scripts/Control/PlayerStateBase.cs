using UnityEngine;

public abstract class PlayerStateBase : IPlayerState
{
    protected PlayerController player;
    protected Transform transform;
    protected CharacterController controller;
    
    public virtual void Enter(PlayerController player)
    {
        this.player = player;
        this.transform = player.transform;
        this.controller = player.GetComponent<CharacterController>();
    }
    
    public virtual void Exit() { }
    
    public virtual void HandleInput() { }
    
    public virtual void Update()
    {
        ApplyGravity();
        //HandleJump();
        HandleCarrying();
        HandleMovement();
    }
    public virtual void HandleMovement()
    {
        player.isGrounded = controller.isGrounded;
        if (player.isGrounded && player.velocity.y < 0)
        {
            player.velocity.y = -2f;
        }

        player.UpdateCurrentPlatform();

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        Vector3 move = new Vector3(moveX, 0, moveZ).normalized;

        if (move.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, player.rotationSpeed * Time.deltaTime);
        }

        Vector3 platformOffset = Vector3.zero;
        if (player.isGrounded && player.currentPlatform != null)
        {
            MovingPlatform mp = player.currentPlatform as MovingPlatform;
            if (mp != null)
            {
                platformOffset = mp.CurrentVelocity * Time.deltaTime;
            }
            else
            {
                DescendingBlock db = player.currentPlatform as DescendingBlock;
                if (db != null)
                {
                    platformOffset = db.CurrentVelocity * Time.deltaTime;
                }
            }
        }

        controller.Move(move * player.moveSpeed * Time.deltaTime + platformOffset);
    }
    
    public virtual void HandleCarrying()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            player.HandleCarryInput();
        }
    }
    
    public virtual void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && player.isGrounded)
        {
            player.velocity.y = Mathf.Sqrt(player.jumpHeight * -2f * player.gravity);
        }
    }
    
    public virtual void ApplyGravity()
    {
        if (player.velocity.y < 0)
        {
            player.velocity.y += player.gravity * player.fallMultiplier * Time.deltaTime;
        }
        else if (player.velocity.y > 0 && !Input.GetButton("Jump"))
        {
            player.velocity.y += player.gravity * player.lowJumpMultiplier * Time.deltaTime;
        }
        else
        {
            player.velocity.y += player.gravity * Time.deltaTime;
        }
        
        controller.Move(player.velocity * Time.deltaTime);
        if (controller.isGrounded)
        {
            player.velocity = new Vector3(0, player.velocity.y, 0);
        }
    }
}