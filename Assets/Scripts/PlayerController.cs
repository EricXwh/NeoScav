using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Parameters")]
    public float moveSpeed = 5f;                
    public float rotationSpeed = 10f;           
    public float jumpHeight = 3f;               
    public float gravity = -20f;                
    public float fallMultiplier = 2.5f;        
    public float lowJumpMultiplier = 2f; 

    [Header("Interaction Parameters")]
    public float interactionDistance = 2f; 
    public LayerMask blockLayer;   
    public Transform interactionPoint;

     [Header("Push/Pull Parameters")]
    public float pushForce = 10f;             
    public float pushSmoothness = 20f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private GameObject currentBlock = null;     
    private bool isPushing = false;  

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        HandleMovement();
        //HandleJump();
        HandlePush();
        ApplyGravity();
    }

    void HandleMovement()
    {
        if(!isPushing) DetectBlockInFront();
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; 
        }

        float moveX = Input.GetAxis("Horizontal"); 
        float moveZ = Input.GetAxis("Vertical");  

        Vector3 move = new Vector3(moveX, 0, moveZ).normalized;
        
        if (move.magnitude >= 0.1f)
        {
            if(isPushing)
            {
                if(currentBlock != null)
                {
                    Vector3 desiredPosition = interactionPoint.position + new Vector3(0, 1.5f, 0);
                    currentBlock.transform.position = desiredPosition;
                    currentBlock.transform.rotation = transform.rotation;
                } 
            }
            float targetAngle = Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            Vector3 moveDirection = transform.forward * move.magnitude;
            controller.Move(moveDirection * moveSpeed * Time.deltaTime);
        }
    }

    void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }
    void HandlePush()
    {
        if (isPushing)
        {
            if (Input.GetMouseButton(0))
            {
                // 保持推动状态
            }
            else
            {
                StopPushing();
            }
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {

            if (currentBlock != null)
            {
                StartPushing(currentBlock);
            }
        }
    }
    void StartPushing(GameObject block)
    {
        isPushing = true;
        currentBlock = block;

        Rigidbody blockRb = currentBlock.GetComponent<Rigidbody>();
        if (blockRb != null)
        {
            blockRb.isKinematic = true;
        }

        Vector3 desiredPosition = interactionPoint.position + new Vector3(0, 1.5f, 0);
        currentBlock.transform.position = desiredPosition;
        currentBlock.transform.rotation = transform.rotation;
    }

    void StopPushing()
    {
        isPushing = false;
        Vector3 desiredPosition = interactionPoint.position + transform.forward * 1.5f + new Vector3(0,1.5f,0);
        currentBlock.transform.position = desiredPosition;

        // 恢复方块的物理属性
        if (currentBlock != null)
        {
            Rigidbody blockRb = currentBlock.GetComponent<Rigidbody>();
            if (blockRb != null)
            {
                blockRb.isKinematic = false;
            }
            currentBlock = null;
        }
    }

    void ApplyGravity()
    {
        if (velocity.y < 0)
        {
            velocity.y += gravity * fallMultiplier * Time.deltaTime;
        }
        else if (velocity.y > 0 && !Input.GetButton("Jump"))
        {
            velocity.y += gravity * lowJumpMultiplier * Time.deltaTime;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        controller.Move(velocity * Time.deltaTime);
    }

    void DetectBlockInFront()
    {
        Vector3 origin = interactionPoint.position;
        Vector3 direction = transform.forward;
        RaycastHit hit;

        if (Physics.Raycast(origin, direction, out hit, interactionDistance, blockLayer))
        {
            if (hit.collider != null)
            {
                currentBlock = hit.collider.gameObject;
            }
            else
            {
                currentBlock = null;
            }
        }
        else
        {
            currentBlock = null;
        }            
    }

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Vector3 origin = interactionPoint.position;
            Vector3 direction = transform.forward;
            Gizmos.DrawRay(origin, direction * interactionDistance);
        }
    }

    

}
