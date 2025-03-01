using UnityEngine;
using UnityEngine.Pool;

public class BulletPool : MonoBehaviour
{
    public static BulletPool Instance;  

    public GameObject bulletPrefab;     
    public int initialPoolSize = 20;     

    private ObjectPool<GameObject> pool;

    void Awake()
    {
        Instance = this;
        pool = new ObjectPool<GameObject>(
            CreatePooledItem,
            OnTakeFromPool,
            OnReturnedToPool,
            OnDestroyPoolObject,
            false,       // collectionCheck
            initialPoolSize,
            initialPoolSize * 2
        );
    }

    // 创建新对象
    GameObject CreatePooledItem()
    {
        GameObject bullet = Instantiate(bulletPrefab);
        bullet.SetActive(false);
        return bullet;
    }

    // 从池中取出时
    void OnTakeFromPool(GameObject bullet)
    {
        bullet.SetActive(true);
    }

    // 回收时
    void OnReturnedToPool(GameObject bullet)
    {
        bullet.SetActive(false);
    }

    // 销毁对象时
    void OnDestroyPoolObject(GameObject bullet)
    {
        Destroy(bullet);
    }

    // 对外接口：获取子弹对象
    public GameObject GetBullet()
    {
        return pool.Get();
    }

    // 对外接口：归还子弹对象
    public void ReturnBullet(GameObject bullet)
    {
        pool.Release(bullet);
    }
}
