using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollingBall : MonoBehaviour
{
    public Vector3 initialPosition = new Vector3(9.57f, 7.43f, 19.88f);
    public float resetInterval = 6f;
    private Rigidbody rb;
    private bool isTriggered = false;
    // Start is called before the first frame update
    void Start()
    {
        transform.position = initialPosition;
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
