using System;  
using System.Collections;
using System.Collections.Generic;
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

    [Header("Carry/Push Parameters")]
    public float CarryForce = 10f;        
    public float PushSpeed = 5f; 
    public float Smoothness = 20f;    
    public LayerMask pushDetectionLayers;   

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private GameObject currentCarryBlock = null;
    private GameObject currentPushBlock = null;     
    private bool isCarrying = false;  
    private bool isPushing = false;
    private float CarryBlockWidth;

    private Quaternion carryBlockInitialRotation;

    private bool isRotating = false;
    private float carriedRotationY = 0f;        
    private float targetRotationY = 0f;        
    public float rotationSpeedDegreesPerSecond = 450f; 

    private Vector3 pushDirection; 

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        HandleMovement();
        // HandleJump(); // 跳跃功能保留，如果需要的话可以取消注释
        HandleCarry();
        HandlePush(); 
        HandleRotation(); 
        ApplyGravity();

        if (isCarrying && currentCarryBlock != null)
        {
            if (carriedRotationY != targetRotationY)
            {
                float step = rotationSpeedDegreesPerSecond * Time.deltaTime;
                carriedRotationY = Mathf.MoveTowards(carriedRotationY, targetRotationY, step);
            }
        }
    }

    void HandleMovement()
    {
        if(!isCarrying) DetectCarryBlockInFront();
        // 不再需要在这里检测推送块，因为推送现在在HandlePush中处理
        // if(!isPushing) DetectPushBlockInFront(); 

        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; 
        }

        if (isPushing)
        {
            return;
        }

        float moveX = Input.GetAxis("Horizontal"); 
        float moveZ = Input.GetAxis("Vertical");  

        Vector3 move = new Vector3(moveX, 0, moveZ).normalized;
        if(isCarrying && currentCarryBlock != null)
        {
            float blockHeight = GetBlockHeight(currentCarryBlock);
            Vector3 desiredPosition = interactionPoint.position + new Vector3(0, 1f + blockHeight / 2f, 0);

            Vector3 start = currentCarryBlock.transform.position;
            currentCarryBlock.transform.position = Vector3.Lerp(start, desiredPosition, Time.deltaTime * Smoothness);
            
            Quaternion desiredRotation = transform.rotation * carryBlockInitialRotation * Quaternion.Euler(0f, carriedRotationY, 0f);
            currentCarryBlock.transform.rotation = desiredRotation;
        }        

        if (move.magnitude >= 0.1f)
        {
            // 移除推动时对方块位置和旋转的控制，因为推动现在通过手动移动实现
            /*
            if(isPushing && currentPushBlock != null)
            {
                Vector3 desiredPosition = interactionPoint.position + transform.forward * (1f + PushBlockWidth / 2);
                desiredPosition.y = PushBlockHeight / 2;
                currentPushBlock.transform.position = desiredPosition;
                
                Quaternion desiredRotation = transform.rotation * pushBlockInitialRotation;
                currentPushBlock.transform.rotation = desiredRotation;
            }
            */

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

    void HandleCarry()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (isCarrying)
            {
                StopCarrying();
            }
            else
            {
                if (currentCarryBlock != null)
                {
                    StartCarrying(currentCarryBlock);
                }
            }
        }
    }

    void StartCarrying(GameObject block)
    {
        isCarrying = true;
        currentCarryBlock = block;

        Rigidbody blockRb = currentCarryBlock.GetComponent<Rigidbody>();
        if (blockRb != null)
        {
            blockRb.isKinematic  = true;
        }

        carryBlockInitialRotation = Quaternion.Inverse(transform.rotation) * currentCarryBlock.transform.rotation;

        float blockHeight = GetBlockHeight(currentCarryBlock);
        CarryBlockWidth = GetBlockWidth(currentCarryBlock);
        Vector3 desiredPosition = interactionPoint.position + new Vector3(0, 1f + blockHeight / 2f, 0);
        Vector3 start = currentCarryBlock.transform.position;
        currentCarryBlock.transform.position = desiredPosition;
        //currentCarryBlock.transform.position = Vector3.Lerp(start, desiredPosition, Time.deltaTime * Smoothness);

        carriedRotationY = 0f;
        targetRotationY = 0f;
    }

    void StopCarrying()
    {
        isCarrying = false;
        if (currentCarryBlock != null)
        {
            float width = GetActualWidth(currentCarryBlock);
            Vector3 desiredPosition = interactionPoint.position + transform.forward * (1f + width / 2) + new Vector3(0, 1f, 0);
            //Vector3 desiredPosition = interactionPoint.position + transform.forward * (1f + CarryBlockWidth / 2) + new Vector3(0, 1f, 0);
            currentCarryBlock.transform.position = desiredPosition;

            Rigidbody blockRb = currentCarryBlock.GetComponent<Rigidbody>();
            if (blockRb != null)
            {
                blockRb.isKinematic  = false;
            }
            currentCarryBlock = null;
        }
    }

    void HandlePush()
    {
        if (isCarrying)
        {
            if (isPushing)
            {
                StopPush();
            }
            return;
        }

        if (Input.GetMouseButtonDown(1))
        {
            StartPush();
        }
        
        if (Input.GetMouseButton(1))
        {
            ContinuePush();
        }
        
        if (Input.GetMouseButtonUp(1))
        {
            StopPush();
        }
    }

    void StartPush()
    {
        if (isPushing) return;
        
        Vector3 origin = interactionPoint.position;
        Vector3 direction = transform.forward;
        RaycastHit hit;
        
        if (Physics.Raycast(origin, direction, out hit, interactionDistance, blockLayer))
        {
            GameObject block = hit.collider.gameObject;
            if (block != null)
            {
                isPushing = true;
                currentPushBlock = block;
                pushDirection = transform.forward; 
            }
        }
    }

void ContinuePush()
{
    if (!isPushing || currentPushBlock == null)
        return;
    
    Vector3 moveOffset = pushDirection * PushSpeed * Time.deltaTime;
    Vector3 newPosition = currentPushBlock.transform.position + moveOffset;

    // 获取方块的碰撞体
    Collider blockCollider = currentPushBlock.GetComponent<Collider>();
    if (blockCollider == null)
    {
        Debug.LogWarning("当前推送的方块没有碰撞体！");
        StopPush();
        return;
    }

    // 计算BoxCast的尺寸和方向
    Vector3 boxCastSize = blockCollider.bounds.size / 2;
    Vector3 castOrigin = currentPushBlock.transform.position;

    // 进行BoxCast检测
    RaycastHit hit;
    float castDistance = PushSpeed * Time.deltaTime; 

    bool isObstacle = Physics.BoxCast(
        castOrigin,
        boxCastSize,
        pushDirection,
        out hit,
        currentPushBlock.transform.rotation,
        castDistance,
        pushDetectionLayers
    );

    if (isObstacle)
    {
        // 检测到障碍物，停止推送
        StopPush();
    }
    else
    {
        // 无障碍物，移动方块
        currentPushBlock.transform.position = newPosition;
        controller.Move(moveOffset);
    }
}

    void StopPush()
    {
        if (!isPushing) return;
        
        isPushing = false;
        currentPushBlock = null;
    }

    void HandleRotation()
    {
        if (isCarrying && currentCarryBlock != null && !isRotating)
        {
            float scroll = Input.mouseScrollDelta.y;

            if (scroll > 0f)
            {
                StartCoroutine(RotateObject(90f));
            }
            else if (scroll < 0f)
            {
                StartCoroutine(RotateObject(-90f));
            }
        }
    }

    IEnumerator RotateObject(float angle)
    {
        isRotating = true;
        float rotationDuration = Mathf.Abs(angle) / rotationSpeedDegreesPerSecond;
        float elapsed = 0f;
        float initialRotationY = targetRotationY;
        float targetRotationYLocal = targetRotationY + angle;

        while (elapsed < rotationDuration)
        {
            carriedRotationY = Mathf.Lerp(initialRotationY, targetRotationYLocal, elapsed / rotationDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        carriedRotationY = targetRotationYLocal;
        targetRotationY = targetRotationYLocal;
        isRotating = false;
        if (isCarrying && currentCarryBlock != null)
        {
            CarryBlockWidth = GetActualWidth(currentCarryBlock);
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

    void DetectCarryBlockInFront()
    {
        Vector3 origin = interactionPoint.position;
        Vector3 direction = transform.forward;
        RaycastHit hit;

        if (Physics.Raycast(origin, direction, out hit, interactionDistance, blockLayer))
        {
            if (hit.collider != null && (!isPushing || currentPushBlock != hit.collider.gameObject))
            {
                currentCarryBlock = hit.collider.gameObject;
            }
            else
            {
                currentCarryBlock = null;
            }
        }
        else
        {
            currentCarryBlock = null;
        }            
    }

    /*
    void DetectPushBlockInFront()
    {
        Vector3 origin = interactionPoint.position;
        Vector3 direction = transform.forward;
        RaycastHit hit;

        if (Physics.Raycast(origin, direction, out hit, interactionDistance, blockLayer))
        {
            if (hit.collider != null)
            {
                currentPushBlock = hit.collider.gameObject;
            }
            else
            {
                currentPushBlock = null;
            }
        }
        else
        {
            currentPushBlock = null;
        }            
    }
    */

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying && isPushing && currentPushBlock != null)
        {
            Gizmos.color = Color.green;
            Collider blockCollider = currentPushBlock.GetComponent<Collider>();
            if (blockCollider != null)
            {
                Vector3 boxCastSize = blockCollider.bounds.size / 2;
                Vector3 castOrigin = currentPushBlock.transform.position;
                float castDistance = PushSpeed * Time.deltaTime + 0.1f;

                Gizmos.DrawWireCube(castOrigin + pushDirection * castDistance / 2, boxCastSize * 2);
            }
        }
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Vector3 origin = interactionPoint.position;
            Vector3 direction = transform.forward;
            Gizmos.DrawRay(origin, direction * interactionDistance);
        }
    }

    float GetBlockHeight(GameObject block)
    {
        Collider collider = block.GetComponent<Collider>();
        if (collider != null)
        {
            return collider.bounds.size.y;
        }
        else
        {
            return 1f;
        }
    }

    float GetBlockWidth(GameObject block)
    {
        var size = block.GetComponent<MeshFilter>().mesh.bounds.size;
        if (block.GetComponent<Collider>() != null)
        {
            Vector3 lossyScale = block.transform.lossyScale;
            float width = size.z * lossyScale.z;
            return width;
        }
        else
        {
            return 1f;
        }
    }

    float GetActualWidth(GameObject block)
    {
        float yRotation = Mathf.Round(block.transform.eulerAngles.y / 90f) * 90f;

        if (yRotation % 180 != 0)
        {
            var mesh = block.GetComponent<MeshFilter>().mesh;
            Vector3 lossyScale = block.transform.lossyScale;
            float width = mesh.bounds.size.x * lossyScale.x;
            return width;
        }
        else
        {
            return GetBlockWidth(block);
        }
    }
}
