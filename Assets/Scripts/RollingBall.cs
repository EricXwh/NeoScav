using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollingBall : MonoBehaviour
{
    private Vector3 initialPosition;
    public float resetInterval = 6f;
    private Rigidbody rb;
    private bool isTriggered = false;
    // Start is called before the first frame update
    void Start()
    {
        initialPosition = transform.position;
        // transform.position = initialPosition;
        rb = GetComponent<Rigidbody>();
        StartCoroutine(ResetPositionRoutine());
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("PressurePlate"))
        {
            isTriggered = true;
            //rb.isKinematic = true;
        }
    }

    private IEnumerator ResetPositionRoutine()
    {
        while(true)
        {
            yield return new WaitForSeconds(resetInterval);
            if(!isTriggered) ResetPosition();
        }
    }

    private void ResetPosition()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = initialPosition;
    }
}
