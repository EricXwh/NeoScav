using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 5f;      
    public float lifetime = 3f;   

    [HideInInspector]
    public Vector3 moveDirection; 
    private float timer = 0f;

    void Update()
    {
        // 计时并销毁
        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            timer = 0f;
            BulletPool.Instance.ReturnBullet(gameObject);
            return;
        }

        transform.position += moveDirection.normalized * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            LevelManager.Instance.ResetCurrentLevel();
            BulletPool.Instance.ReturnBullet(gameObject);
        }
        else if (!other.CompareTag("BulletSpawner"))
        {
            
            BulletPool.Instance.ReturnBullet(gameObject);
        }
        
    }
}
