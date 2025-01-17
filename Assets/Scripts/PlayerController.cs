using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    private GameObject highlightedBlock = null;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        HandleMovement();
        // HandleJump(); // 如需跳跃可取消注释
        HandleCarry();
        HandlePush(); 
        HandleRotation(); 
        ApplyGravity();

        // 如果正在拿着方块，并且该方块存在，就平滑旋转
        if (isCarrying && currentCarryBlock != null)
        {
            if (carriedRotationY != targetRotationY)
            {
                float step = rotationSpeedDegreesPerSecond * Time.deltaTime;
                carriedRotationY = Mathf.MoveTowards(carriedRotationY, targetRotationY, step);
            }
        }
    }

    // =========== 1. 移动/旋转 ===========

    void HandleMovement()
    {
        // 如果没在背方块，我们就检测一下面前是否有可Carry的方块
        // （推的检测放在HandlePush里或者StartPush里）
        if(!isCarrying && !isPushing)
        {
            DetectCarryBlockInFront();
        }
        else
        {
            // 如果正在搬运或推，取消对前方可交互方块的高亮
            HighlightBlock(null);
        }

        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; 
        }

        // 如果正在推，就不处理移动（由推逻辑决定）
        if (isPushing) return;

        float moveX = Input.GetAxisRaw("Horizontal"); 
        float moveZ = Input.GetAxisRaw("Vertical");  

        Vector3 move = new Vector3(moveX, 0, moveZ).normalized;

        // 如果正在背着方块，实时更新方块位置和旋转
        if(isCarrying && currentCarryBlock != null)
        {
            float blockHeight = GetBlockHeight(currentCarryBlock);
            Vector3 desiredPosition = interactionPoint.position + new Vector3(0, 1f + blockHeight / 2f, 0);

            Vector3 start = currentCarryBlock.transform.position;
            currentCarryBlock.transform.position = Vector3.Lerp(start, desiredPosition, Time.deltaTime * Smoothness);
            
            Quaternion desiredRotation = transform.rotation * carryBlockInitialRotation * Quaternion.Euler(0f, carriedRotationY, 0f);
            currentCarryBlock.transform.rotation = desiredRotation;
        }        

        // 玩家自身的移动和旋转
        if (move.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            controller.Move(move * moveSpeed * Time.deltaTime);
        }
    }

    // =========== 2. 跳跃(可选) ===========

    void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    // =========== 3. 搬起/放下 ===========

    void HandleCarry()
    {
        // 右键点击：如果正在拿就放下，如果没拿且有可拿的方块就拿起
        if (Input.GetMouseButtonDown(1))
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

        // 拿起时，不再高亮
        HighlightBlock(null);

        Rigidbody blockRb = currentCarryBlock.GetComponent<Rigidbody>();
        if (blockRb != null)
        {
            blockRb.isKinematic  = true;
        }

        carryBlockInitialRotation = Quaternion.Inverse(transform.rotation) * currentCarryBlock.transform.rotation;

        float blockHeight = GetBlockHeight(currentCarryBlock);
        CarryBlockWidth = GetBlockWidth(currentCarryBlock);
        Vector3 desiredPosition = interactionPoint.position + new Vector3(0, 1f + blockHeight / 2f, 0);
        currentCarryBlock.transform.position = desiredPosition;

        carriedRotationY = 0f;
        targetRotationY = 0f;
    }

    void StopCarrying()
    {
        isCarrying = false;
        if (currentCarryBlock != null)
        {
            float width = GetActualWidth(currentCarryBlock);
            Vector3 desiredPosition = interactionPoint.position 
                + transform.forward * (1f + width / 2) 
                + new Vector3(0, 1f, 0);

            currentCarryBlock.transform.position = desiredPosition;

            Rigidbody blockRb = currentCarryBlock.GetComponent<Rigidbody>();
            if (blockRb != null)
            {
                blockRb.isKinematic  = false;
            }
            currentCarryBlock = null;
        }
    }

    // =========== 4. 推动相关 ===========

    void HandlePush()
    {
        // 如果正在背东西，就不能推。如果正在推，就等鼠标抬起才结束
        if (isCarrying)
        {
            if (isPushing)
            {
                StopPush();
            }
            return;
        }

        // 按下左键尝试推
        if (Input.GetMouseButtonDown(0))
        {
            StartPush();
        }
        
        // 按住左键持续推
        if (Input.GetMouseButton(0))
        {
            ContinuePush();
        }
        
        // 松开左键停止推
        if (Input.GetMouseButtonUp(0))
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
            if (block != null && !isCarrying)
            {
                isPushing = true;
                currentPushBlock = block;
                pushDirection = transform.forward; 

                // 一旦开始推，就取消高亮
                HighlightBlock(null);
            }
        }
    }

    void ContinuePush()
    {
        if (!isPushing || currentPushBlock == null) return;
    
        Vector3 moveOffset = pushDirection * PushSpeed * Time.deltaTime;
        Vector3 newPosition = currentPushBlock.transform.position + moveOffset;

        Collider blockCollider = currentPushBlock.GetComponent<Collider>();
        if (blockCollider == null)
        {
            Debug.LogWarning("当前推送的方块没有碰撞体！");
            StopPush();
            return;
        }

        Vector3 boxCastSize = blockCollider.bounds.size / 2;
        Vector3 castOrigin = currentPushBlock.transform.position;
        float castDistance = PushSpeed * Time.deltaTime; 

        bool isObstacle = Physics.BoxCast(
            castOrigin,
            boxCastSize,
            pushDirection,
            out RaycastHit hit,
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
            // 无障碍物，移动方块，同时玩家也跟着移动一点
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

    // =========== 5. 旋转搬运物体 ===========

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

    // =========== 6. 重力处理 ===========

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

    // =========== 7. 检测可Carry的方块 ===========

    void DetectCarryBlockInFront()
    {
        Vector3 origin = interactionPoint.position;
        Vector3 direction = transform.forward;
        RaycastHit hit;

        if (Physics.Raycast(origin, direction, out hit, interactionDistance, blockLayer))
        {
            // 命中了某个方块并且（没在推或推的不是它自己）
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

        // 如果“当前可以操作但还没操作”，就高亮，否则不高亮
        // 条件：(!isCarrying && !isPushing) 并且 currentCarryBlock != null
        if (!isCarrying && !isPushing)
        {
            HighlightBlock(currentCarryBlock);
        }
        else
        {
            HighlightBlock(null);
        }
    }

    // =========== 8. Debug 可视化 ===========

    private void OnDrawGizmosSelected()
    {
        // 推动时的BoxCast可视化
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
        // 交互射线
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Vector3 origin = interactionPoint.position;
            Vector3 direction = transform.forward;
            Gizmos.DrawRay(origin, direction * interactionDistance);
        }
    }

    // =========== 9. 获取方块尺寸 ===========

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

    // =========== 10. 高亮管理函数 ===========

    /// 管理高亮，仅在“可操作但尚未操作”的物体身上启用 Outline。
    /// 如果之前有其它高亮，先把它关掉；然后把新的目标物体高亮。
    private void HighlightBlock(GameObject block)
    {
        // 如果两次传入相同的 block，就不用重复操作
        if (highlightedBlock == block) return;

        // 如果之前有高亮的方块，就先把它的 Outline 关掉
        if (highlightedBlock != null)
        {
            Outline oldOutline = highlightedBlock.GetComponent<Outline>();
            if (oldOutline != null)
            {
                oldOutline.enabled = false;
            }
        }

        // 如果传进来的 block 不为 null，则给它启用 Outline
        if (block != null)
        {
            Outline newOutline = block.GetComponent<Outline>();
            if (newOutline != null)
            {
                newOutline.enabled = true;
            }
        }

        // 更新记录
        highlightedBlock = block;
    }
}
