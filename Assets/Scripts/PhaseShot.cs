using System.Collections;
using UnityEngine;

public class PhaseShot : MonoBehaviour
{
    public float speed = 2f;
    public float life = 3f;
    [Tooltip("激活 LockBlock 后保持激活的时长（秒）")]
    public float activationDuration = 1f; 

    private Rigidbody rb;

    void Awake()
    {
        Destroy(gameObject, life);
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = transform.forward * speed;
    }

    void OnTriggerEnter(Collider other)
    {
        // if(other.CompareTag("Player"))
        // {
        //     Destroy(gameObject);
        // }

        LockBlock lockBlock = other.GetComponent<LockBlock>();
        if (lockBlock != null)
        {
            lockBlock.TriggerActivate();
            
            lockBlock.StartCoroutine(DelayedDeactivate(lockBlock, activationDuration));
        }

        Destroy(gameObject);
    }

    IEnumerator DelayedDeactivate(LockBlock lockBlock, float delay)
    {
        yield return new WaitForSeconds(delay);
        lockBlock.TriggerDeactivate();
    }
}
