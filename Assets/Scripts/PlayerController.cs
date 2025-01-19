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

    [Header("Carry Parameters")]
    public float CarryForce = 10f;        
    public float Smoothness = 20f;    

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private GameObject currentCarryBlock = null; 
    private bool isCarrying = false;  
    private bool canPlaceBlock = false;

    private Vector3 gizmoCenter;
    private Vector3 gizmoSize;
    private Quaternion gizmoRotation;


    private GameObject highlightedBlock = null;

    // 用于保持携带物体与玩家之间的旋转偏移
    private Quaternion carryRotationOffset;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        HandleMovement();
        // HandleJump(); // 如需跳跃可取消注释
        HandleCarry();
        ApplyGravity();
        if(isCarrying && currentCarryBlock != null)
        {
            canPlaceBlock = CheckIfCanPlaceBlock();
        }
    }

    // =========== 1. 移动/旋转 ===========
    
    void HandleMovement()
    {

        // 如果没在携带，我们就检测一下面前是否有可Carry的方块
        if(!isCarrying)
        {
            DetectCarryBlockInFront();
        }
        else
        {
            // 如果正在携带，取消对前方可交互方块的高亮
            HighlightBlock(null);
        }

        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; 
        }

        float moveX = Input.GetAxisRaw("Horizontal"); 
        float moveZ = Input.GetAxisRaw("Vertical");  

        Vector3 move = new Vector3(moveX, 0, moveZ).normalized;

        // 如果正在携带方块，实时更新方块位置和旋转
        if(isCarrying && currentCarryBlock != null)
        {
            float blockHeight = GetBlockHeight(currentCarryBlock);
            Vector3 desiredPosition = interactionPoint.position + new Vector3(0, 1f + blockHeight / 2f, 0);

            // 平滑移动方块到目标位置
            Vector3 start = currentCarryBlock.transform.position;
            currentCarryBlock.transform.position = Vector3.Lerp(start, desiredPosition, Time.deltaTime * Smoothness);
            
            // 保持旋转一致，使用初始旋转偏移
            currentCarryBlock.transform.rotation = transform.rotation * carryRotationOffset;
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

    // =========== 2. 跳跃 ===========
    
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
        // 右键点击：如果正在携带就放下，如果没携带且有可携带的方块就携带
        if (Input.GetMouseButtonDown(1))
        {
            if (isCarrying)
            {
                AttemptStopCarrying();
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

        float blockHeight = GetBlockHeight(currentCarryBlock);
        Vector3 desiredPosition = interactionPoint.position + new Vector3(0, 1f + blockHeight / 2f, 0);
        currentCarryBlock.transform.position = desiredPosition;

        // 1. 对齐携带物体的一个轴与玩家的朝向
        AlignCarryBlockWithPlayer();

        // 2. 计算并存储旋转偏移，用于后续保持一致的旋转
        carryRotationOffset = Quaternion.Inverse(transform.rotation) * currentCarryBlock.transform.rotation;
    }

    void AlignCarryBlockWithPlayer()
    {
        // 获取玩家的前方向
        Vector3 playerForward = transform.forward;

        // 定义携带物体的四个轴方向
        Vector3[] carryAxes = new Vector3[]
        {
            currentCarryBlock.transform.right,        // X轴
            currentCarryBlock.transform.forward,      // Z轴
            -currentCarryBlock.transform.right,       // -X轴
            -currentCarryBlock.transform.forward      // -Z轴
        };

        // 找到与玩家前方向夹角最小的轴
        float smallestAngle = float.MaxValue;
        Vector3 bestMatchAxis = Vector3.zero;

        foreach (Vector3 axis in carryAxes)
        {
            float angle = Vector3.Angle(playerForward, axis);
            if (angle < smallestAngle)
            {
                smallestAngle = angle;
                bestMatchAxis = axis;
            }
        }

        if (bestMatchAxis != Vector3.zero)
        {
            // 计算旋转，使得 bestMatchAxis 对齐 playerForward
            Quaternion alignRotation = Quaternion.FromToRotation(bestMatchAxis, playerForward);
            currentCarryBlock.transform.rotation = alignRotation * currentCarryBlock.transform.rotation;
        }
    }

    /// 执行“放下”逻辑，但只有在能放置的情况下才真正放下。
    void AttemptStopCarrying()
    {
        // 如果可以放置则调用 StopCarrying
        if (CheckIfCanPlaceBlock())
        {
            StopCarrying();
        }
        else
        {
            Debug.Log("目标位置被阻挡，无法放置方块。");
        }
    }

    void StopCarrying()
    {
        isCarrying = false;
        if (currentCarryBlock != null)
        {
            // 计算最终放下的位置
            float width = GetBlockLength(currentCarryBlock);
            Vector3 desiredPosition = interactionPoint.position 
                + transform.forward * (1f + width / 2f) 
                + new Vector3(0, 1f, 0);

            currentCarryBlock.transform.position = desiredPosition;

            Rigidbody blockRb = currentCarryBlock.GetComponent<Rigidbody>();
            if (blockRb != null)
            {
                blockRb.isKinematic = false;
            }
            currentCarryBlock = null;
        }
    }

    /// 检查当前携带的方块能否在“预计放下的位置”放置
    bool CheckIfCanPlaceBlock()
    {
        float width  = GetBlockWidth(currentCarryBlock);
        float length = GetBlockLength(currentCarryBlock);
        float height = GetBlockHeight(currentCarryBlock);

        Vector3 desiredPosition = interactionPoint.position 
            + transform.forward * (1f + length / 2f) 
            + new Vector3(0, 1f, 0);

        Vector3 halfExtents = new Vector3(width / 2f, height / 2f, length / 2f);

        gizmoCenter   = desiredPosition;
        gizmoSize     = new Vector3(width, height, length);
        gizmoRotation = transform.rotation; 

        Collider[] hits = Physics.OverlapBox(desiredPosition, halfExtents, gizmoRotation);

        List<Collider> validHits = new List<Collider>();
        foreach (var hit in hits)
        {
            if (hit.gameObject != currentCarryBlock)
            {
                validHits.Add(hit);
            }
        }

        return validHits.Count == 0;
    }



    // =========== 4. 重力处理 ===========
    
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

    // =========== 5. 检测可Carry的方块 ===========
    
    void DetectCarryBlockInFront()
    {
        Vector3 origin = interactionPoint.position;
        Vector3 direction = transform.forward;
        RaycastHit hit;

        if (Physics.Raycast(origin, direction, out hit, interactionDistance, blockLayer))
        {
            if (hit.collider != null)
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
        if (!isCarrying)
        {
            HighlightBlock(currentCarryBlock);
        }
        else
        {
            HighlightBlock(null);
        }
    }

    // =========== 6. Debug 可视化 ===========
    
    private void OnDrawGizmosSelected()
    {
        // 交互射线
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Vector3 origin = interactionPoint.position;
            Vector3 direction = transform.forward;
            Gizmos.DrawRay(origin, direction * interactionDistance);
        }
    }

    private void OnDrawGizmos()
    {
        // 如果正在携带，就绘制一个包围盒来表示“如果此时放置，会放在哪里”
        if (isCarrying && currentCarryBlock != null)
        {
            // 根据 canPlaceBlock 来决定颜色
            Gizmos.color = canPlaceBlock ? Color.green : Color.red;

            // 画一个线框盒子
            Matrix4x4 oldMatrix = Gizmos.matrix;
            // 先将 Gizmos 的矩阵切换为盒子所在位置和旋转
            Gizmos.matrix = Matrix4x4.TRS(gizmoCenter, gizmoRotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, gizmoSize);
            Gizmos.matrix = oldMatrix;
        }
    }

    // =========== 7. 获取方块尺寸 ===========
    
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
        Vector3 playerRight = transform.right;

        Vector3 localRight = block.transform.InverseTransformDirection(playerRight);

        localRight = new Vector3(Mathf.Abs(localRight.x), Mathf.Abs(localRight.y), Mathf.Abs(localRight.z));

        if (localRight.x > localRight.y && localRight.x > localRight.z)
        {
            return block.transform.localScale.x; 
        }
        else if (localRight.y > localRight.x && localRight.y > localRight.z)
        {
            return block.transform.localScale.y;
        }
        else
        {
            return block.transform.localScale.z;
        }
    }

    float GetBlockLength(GameObject block)
    {
        Vector3 playerForward = transform.forward;

        Vector3 localForward = block.transform.InverseTransformDirection(playerForward);

        localForward = new Vector3(Mathf.Abs(localForward.x), Mathf.Abs(localForward.y), Mathf.Abs(localForward.z));

        if (localForward.x > localForward.y && localForward.x > localForward.z)
        {
            return block.transform.localScale.x; 
        }
        else if (localForward.y > localForward.x && localForward.y > localForward.z)
        {
            return block.transform.localScale.y; 
        }
        else
        {
            return block.transform.localScale.z; 
        }
    }



    float GetActualWidth(GameObject block)
    {
        Vector3 characterForward = transform.forward;
        Vector3 worldXAxis = Vector3.right;
        float angle = Mathf.Abs(90 - Vector3.Angle(characterForward, worldXAxis));

        Collider collider = block.GetComponent<Collider>();
        if (collider != null)
        {
            float x = collider.bounds.size.x; 
            float z = collider.bounds.size.z; 

            float radians = angle * Mathf.Deg2Rad;

            float denominator = Mathf.Pow(Mathf.Cos(radians), 2) - Mathf.Pow(Mathf.Sin(radians), 2);

            if (Mathf.Abs(denominator) < 1e-6f) // 防止分母为0
            {
                Debug.LogError("Invalid angle causing denominator to be zero.");
                return 1f; 
            }

            float length = (x * Mathf.Abs(Mathf.Cos(radians)) - z * Mathf.Abs(Mathf.Sin(radians))) / denominator;
            float width = (z * Mathf.Abs(Mathf.Cos(radians)) - x * Mathf.Abs(Mathf.Sin(radians))) / denominator;

            Debug.Log("localscale: "+block.transform.localScale.x);
            return Mathf.Abs(width); 
        }
        else
        {
            return 1f;
        }
    }

    float GetActualLength(GameObject block)
    {
        Vector3 characterForward = transform.forward;
        Vector3 worldXAxis = Vector3.right;
        float angle = Mathf.Abs(90 - Vector3.Angle(characterForward, worldXAxis));

        Collider collider = block.GetComponent<Collider>();
        if (collider != null)
        {
            float x = collider.bounds.size.x; 
            float z = collider.bounds.size.z; 

            float radians = angle * Mathf.Deg2Rad;

            float denominator = Mathf.Pow(Mathf.Cos(radians), 2) - Mathf.Pow(Mathf.Sin(radians), 2);

            if (Mathf.Abs(denominator) < 1e-6f) // 防止分母为0
            {
                Debug.LogError("Invalid angle causing denominator to be zero.");
                return 1f; 
            }

            float length = (x * Mathf.Abs(Mathf.Cos(radians)) - z * Mathf.Abs(Mathf.Sin(radians))) / denominator;
            float width = (z * Mathf.Abs(Mathf.Cos(radians)) - x * Mathf.Abs(Mathf.Sin(radians))) / denominator;


            return Mathf.Abs(length); 
        }
        else
        {
            return 1f;
        }
    }

    // =========== 8. 高亮管理函数 ===========
    
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
