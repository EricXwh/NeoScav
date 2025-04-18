using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhaseCannon : MonoBehaviour
{
    public GameObject phaseShotPrefab; 
    public float fireInterval = 4f;    
    public Transform firePoint;        

    private float timer;

    void Start()
    {
        if (firePoint == null)
        {
            firePoint = this.transform; 
        }
        timer = fireInterval;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= fireInterval)
        {
            FirePhaseShot();
            timer = 0f;
        }
    }

    void FirePhaseShot()
    {
        Instantiate(phaseShotPrefab, firePoint.position, firePoint.rotation);
        Debug.Log("Phase shot fired");
    }
}
