using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Parameters")]
    public bool canMove = true;
    public float moveSpeed = 5f;                
    public float rotationSpeed = 10f;           
    public float jumpHeight = 3f;               
    public float gravity = -20f;                
    public float fallMultiplier = 2.5f;         
    public float lowJumpMultiplier = 2f; 
    public bool isBouncing = false;

    [Header("Interaction Parameters")]
    public float interactionDistance = 2f; 
    public LayerMask blockLayer;   
    public Transform interactionPoint;

    [Header("Carry Parameters")]
    public float CarryForce = 10f;        
    public float Smoothness = 20f;    

    // 状态管理
    private Dictionary<Type, IPlayerState> states = new Dictionary<Type, IPlayerState>();
    private IPlayerState currentState;
    
    // 内部变量
    [HideInInspector] public CharacterController controller;
    [HideInInspector] public Vector3 velocity;
    [HideInInspector] public bool isGrounded;
    [HideInInspector] public GameObject currentCarryBlock = null; 
    [HideInInspector] public bool isCarrying = false;  
    [HideInInspector] public bool canPlaceBlock = false;
    [HideInInspector] public Vector3 carryBounceVelocity;
    [HideInInspector] public Quaternion carryRotationOffset;
    [HideInInspector] public MonoBehaviour currentPlatform;
    
    private Vector3 gizmoCenter;
    private Vector3 gizmoSize;
    private Quaternion gizmoRotation;
    private GameObject highlightedBlock = null;
    private int originalBlockLayer;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        
        // 初始化所有状态
        states[typeof(PlayerIdleState)] = new PlayerIdleState();
        states[typeof(PlayerCarryingState)] = new PlayerCarryingState();
        states[typeof(PlayerJumpingState)] = new PlayerJumpingState();
        
        // 设置初始状态
        SetState<PlayerIdleState>();
    }

    void Update()
    {
        if (!canMove || currentState == null)
            return;
            
        currentState.Update();
    }
    
    // 切换状态
    public void SetState<T>() where T : IPlayerState
    {
        var type = typeof(T);
        
        if (currentState != null)
        {
            currentState.Exit();
        }
        
        currentState = states[type];
        currentState.Enter(this);
        
        Debug.Log($"Switched to state: {type.Name}");
    }
    
    // 更新当前平台
    public void UpdateCurrentPlatform()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.2f))
        {
            DescendingBlock db = hit.collider.GetComponentInParent<DescendingBlock>();
            if (db != null)
            {
                currentPlatform = db;
                return;
            }

            MovingPlatform mp = hit.collider.GetComponentInParent<MovingPlatform>();
            if (mp != null)
            {
                currentPlatform = mp;
                return;
            }
        }
        currentPlatform = null;
    }
    
    // 处理搬运输入
    public void HandleCarryInput()
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
    
    // 尝试放下物体
    public void AttemptStopCarrying()
    {
        if (CheckIfCanPlaceBlock())
        {
            StopCarrying();
            SetState<PlayerIdleState>();
        }
        else
        {
            Debug.Log("目标位置被阻挡，无法放置方块。");
        }
    }
    
    // 开始搬运物体
    public void StartCarrying(GameObject block)
    {
        isCarrying = true;
        currentCarryBlock = block;
        
        // 搬起时取消高亮
        HighlightBlock(null);

        // 如果该方块挂有 CarryBlock 组件，则隐藏其提示
        CarryBlock carryBlock = block.GetComponent<CarryBlock>();
        if (carryBlock != null)
        {
            carryBlock.HideHint();
        }
        
        Rigidbody blockRb = currentCarryBlock.GetComponent<Rigidbody>();
        if (blockRb != null)
        {
            blockRb.isKinematic = true;
        }

        float blockHeight = GetBlockHeight(currentCarryBlock);
        Vector3 desiredPosition = interactionPoint.position + new Vector3(0, 1f + blockHeight / 2f, 0);
        currentCarryBlock.transform.position = desiredPosition;
        originalBlockLayer = block.layer;
        block.layer = LayerMask.NameToLayer("CarriedBlock");
        
        // 对齐携带物体的一个轴与玩家朝向
        AlignCarryBlockWithPlayer();

        // 计算并存储旋转偏移，用于后续保持一致的旋转
        carryRotationOffset = Quaternion.Inverse(transform.rotation) * currentCarryBlock.transform.rotation;
        controller.height += 1.5f;
        controller.center += new Vector3(0, 0.75f, 0);
        
        // 切换到搬运状态
        SetState<PlayerCarryingState>();
    }
    
    // 停止搬运
    public void StopCarrying()
    {
        isCarrying = false;
        if (currentCarryBlock != null)
        {
            float width = GetBlockLength(currentCarryBlock);
            Vector3 desiredPosition;
            Debug.Log(controller.isGrounded);
            if (controller.isGrounded)
            {
                // 在地面时，使用 interactionPoint 位置，在前方放置方块
                desiredPosition = interactionPoint.position 
                    + transform.forward * (1f + width / 2f) 
                    + new Vector3(0, 1f, 0);
                currentCarryBlock.transform.position = desiredPosition;        
            }

            Rigidbody blockRb = currentCarryBlock.GetComponent<Rigidbody>();
            if (blockRb != null)
            {
                blockRb.isKinematic = false;
            }
            
            currentCarryBlock.layer = originalBlockLayer;
            currentCarryBlock = null;
            controller.height -= 1.5f;
            controller.center -= new Vector3(0, 0.75f, 0);
        }
    }
    
    // 检测前方可搬运的方块
    public void DetectCarryBlockInFront()
    {
        if(isCarrying)
            return;

        Vector3 origin = interactionPoint.position;
        // 在 interactionDistance 半径内查找所有符合 blockLayer 的碰撞体
        Collider[] colliders = Physics.OverlapSphere(origin, interactionDistance, blockLayer);
        GameObject closestBlock = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider col in colliders)
        {
            // 计算从交互点到该方块的方向
            Vector3 directionToBlock = col.transform.position - origin;
            // 筛选出玩家前方45°以内的
            float angle = Vector3.Angle(transform.forward, directionToBlock);
            if (angle <= 45f)
            {
                float distance = directionToBlock.magnitude;
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestBlock = col.gameObject;
                }
            }
        }
        
        currentCarryBlock = closestBlock;
        
        if (!isCarrying)
        {
            HighlightBlock(currentCarryBlock);
        }
        else
        {
            HighlightBlock(null);
        }
    }
    
    // 检查当前携带的方块能否在"预计放下的位置"放置
    public bool CheckIfCanPlaceBlock()
    {
        if (currentCarryBlock == null)
            return false;
            
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
    
    // 对齐携带物体与玩家朝向
    private void AlignCarryBlockWithPlayer()
    {
        // 获取玩家的前方向
        Vector3 playerForward = transform.forward;

        // 定义携带物体的四个轴方向
        Vector3[] carryAxes = new Vector3[]
        {
            currentCarryBlock.transform.right,        // X轴
            currentCarryBlock.transform.forward,       // Z轴
            -currentCarryBlock.transform.right,        // -X轴
            -currentCarryBlock.transform.forward        // -Z轴
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
    
    // 高亮显示可交互方块
    private void HighlightBlock(GameObject block)
    {
        // 如果两次传入相同的物体，则不重复操作
        if (highlightedBlock == block) return;

        // 如果之前有高亮的物体，则先关闭其 Outline
        if (highlightedBlock != null)
        {
            MyOutline oldOutline = highlightedBlock.GetComponent<MyOutline>();
            if (oldOutline != null)
            {
                oldOutline.enabled = false;
            }
        }

        // 如果传入的物体不为 null，则启用其 Outline
        if (block != null)
        {
            MyOutline newOutline = block.GetComponent<MyOutline>();
            if (newOutline != null)
            {
                newOutline.enabled = true;
            }
        }

        highlightedBlock = block;
    }
    
    // 处理弹跳
    public void Bounce(Vector3 direction, float force)
    {
        isBouncing = true;
        velocity = direction * force * Mathf.Sqrt(lowJumpMultiplier);
        if (isCarrying && currentCarryBlock != null)
        {
            carryBounceVelocity = velocity;
        }
        
        // 切换到跳跃状态
        SetState<PlayerJumpingState>();
    }
    
    // 获取方块高度
    public float GetBlockHeight(GameObject block)
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

    // 获取方块宽度
    public float GetBlockWidth(GameObject block)
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

    // 获取方块长度
    public float GetBlockLength(GameObject block)
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
    
    // 辅助方法
    public bool IsCarrying()
    {
        return isCarrying && currentCarryBlock != null;
    }

    // 停止携带并销毁当前物体
    public void StopCarryingAndDestroy()
    {
        if (isCarrying && currentCarryBlock != null)
        {
            StopCarrying();
            Destroy(currentCarryBlock);
            currentCarryBlock = null;
            SetState<PlayerIdleState>();
        }
    }
    
    // 调试绘制
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
        // 如果正在携带，就绘制一个包围盒来表示"如果此时放置，会放在哪里"
        if (isCarrying && currentCarryBlock != null)
        {
            // 根据 canPlaceBlock 来决定颜色
            Gizmos.color = canPlaceBlock ? Color.green : Color.red;

            // 画一个线框盒子
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(gizmoCenter, gizmoRotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, gizmoSize);
            Gizmos.matrix = oldMatrix;
        }
    }
}