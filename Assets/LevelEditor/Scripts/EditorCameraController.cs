using UnityEngine;

public class EditorCameraController : MonoBehaviour
{
    [Header("移动速度")]
    public float moveSpeed = 10f;
    [Header("旋转速度")]
    public float lookSpeed = 3f;
    [Header("缩放速度")]
    public float zoomSpeed = 10f;

    private float yaw;
    private float pitch;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal"); 
        float v = Input.GetAxisRaw("Vertical");  
        
        Vector3 forwardMovement = transform.forward * v;
        
        Vector3 rightVector = transform.right;
        rightVector.y = 0;
        rightVector.Normalize();
        Vector3 rightMovement = rightVector * h;
        
        Vector3 moveDir = (forwardMovement + rightMovement);
        
        transform.position += moveDir * moveSpeed * Time.deltaTime;

        if (Input.GetMouseButton(1))
        {
            yaw   += Input.GetAxis("Mouse X") * lookSpeed;
            pitch -= Input.GetAxis("Mouse Y") * lookSpeed;
            pitch = Mathf.Clamp(pitch, -80f, 80f);
            transform.eulerAngles = new Vector3(pitch, yaw, 0);
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        transform.position += transform.forward * scroll * zoomSpeed;
    }
}