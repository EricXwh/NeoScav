using UnityEngine;

public class BouncyPlate : MonoBehaviour
{
    // 可调节的弹力大小
    public float bounceForce = 10f;

    private void OnTriggerEnter(Collider other)
    {
        
        Vector3 bounceDirection = transform.up;
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(bounceDirection * bounceForce, ForceMode.Impulse);
        }
        else
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.Bounce(bounceDirection, bounceForce);
            }
        }
    }
  
}
