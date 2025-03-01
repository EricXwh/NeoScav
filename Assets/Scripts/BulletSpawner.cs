using UnityEngine;
using System.Collections;

public enum SpawningPattern
{
    Circle,   // 普通环形
    Spiral    // 螺旋
}

public class BulletSpawner : MonoBehaviour
{
    [Header("基础设置")]
    public GameObject bulletPrefab;
    public float spawnInterval = 1f;  
    public int bulletCount = 10;     

    public SpawningPattern spawnPattern = SpawningPattern.Circle;

    [Header("弹幕参数")]
    public float angleRange = 360f;   
    public float spiralSpeed = 10f;   

    private float spiralAngle = 0f;   

    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            switch (spawnPattern)
            {
                case SpawningPattern.Circle:
                    SpawnCircleBullets();
                    break;
                case SpawningPattern.Spiral:
                    SpawnSpiralBullets();
                    break;
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    // 1. 普通环形弹幕
    void SpawnCircleBullets()
    {
        for (int i = 0; i < bulletCount; i++)
        {
            float angle = i * (angleRange / bulletCount);
            float rad = angle * Mathf.Deg2Rad;

            // 在XZ平面上计算方向: (x,z)，y=0
            Vector3 dir = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad));

            GameObject bulletObj = BulletPool.Instance.GetBullet();
            bulletObj.transform.position = transform.position;

            Bullet bullet = bulletObj.GetComponent<Bullet>();
            bullet.moveDirection = dir;
        }
    }

    // 2. 螺旋弹幕
    void SpawnSpiralBullets()
    {
        // 相比Circle，起始角度带上了螺旋Angle
        for (int i = 0; i < bulletCount; i++)
        {
            float angle = spiralAngle + i * (angleRange / bulletCount);
            float rad = angle * Mathf.Deg2Rad;

            Vector3 dir = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad));

            GameObject bulletObj = BulletPool.Instance.GetBullet();
            bulletObj.transform.position = transform.position;

            Bullet bullet = bulletObj.GetComponent<Bullet>();
            bullet.moveDirection = dir;
        }

        // 每发射一波后，让螺旋角度累加
        spiralAngle += spiralSpeed;
    }
}
