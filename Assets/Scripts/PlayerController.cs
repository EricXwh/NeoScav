using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movment Setting")]
    public float moveSpeed = 5f;

    [Header("Jump Setting")]
    public float jumpForce = 20f; 
    public Transform groundCheck;
    public float groundDistance = 0.2f;
    public LayerMask groundMask;

    [Header("Gravity Setting")]
    public float gravityMultiplier = 2.5f; 
    public float fallMultiplier = 2.5f;    

    private Rigidbody rb;
    private Vector3 movement;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation; 
    }

    void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        movement = new Vector3(moveX, 0f, moveZ).normalized * moveSpeed;


        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z); 
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void FixedUpdate()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (rb.velocity.y < 0)
        {
            rb.AddForce(Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * rb.mass, ForceMode.Acceleration);
        }
        else if (rb.velocity.y > 0 && !Input.GetButton("Jump"))
        {
            rb.AddForce(Vector3.up * Physics.gravity.y * (gravityMultiplier - 1) * rb.mass, ForceMode.Acceleration);
        }

        Vector3 newPosition = rb.position + new Vector3(movement.x, 0f, movement.z) * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
    }
}
