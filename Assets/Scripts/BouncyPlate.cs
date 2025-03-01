using UnityEngine;

public class BouncyPlate : MonoBehaviour
{
    // 可调节的弹力大小
    public float bounceForce = 10f;
    public AudioClip bounceSound;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Block") || other.CompareTag("Battery"))
        {
            if (bounceSound != null)
            {
                audioSource.PlayOneShot(bounceSound, 1.5f);
            }   
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
  
}
