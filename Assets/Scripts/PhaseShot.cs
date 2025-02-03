using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhaseShot : MonoBehaviour
{
    public float speed = 2f; 
    public float life = 3f;
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
        if (other.CompareTag("Player"))
        {
            Debug.Log("hit");
        }
        Destroy(gameObject);
    }
}
