using Mirror;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    public float speed = 10f;
    public float destroyTime = 5f;

    void Start()
    {
        // 子弹生成后 5 秒自动销毁
        if (isServer)
        {
            Invoke(nameof(DestroySelf), destroyTime);
        }
    }

    void Update()
    {
        // 让子弹向前飞（如果在 Prefab 上加了 NetworkTransform，服务端飞，客户端跟着同步）[cite: 1]
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    // 只有服务端负责判定碰撞，防止客户端作弊！[cite: 1, 4]
    [ServerCallback]
    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"发生碰撞");
        
        // 尝试获取被击中对象的玩家脚本
        PlayerNetworkController targetPlayer = other.GetComponent<PlayerNetworkController>();
        
        if (targetPlayer != null)
        {
            // 扣血 (修改 SyncVar 变量， Mirror 会自动广播最新血量给所有客户端)[cite: 1, 4]
            targetPlayer.health -= 10;

            // 销毁子弹（NetworkServer.Destroy 会自动通知所有客户端一并销毁）[cite: 1]
            NetworkServer.Destroy(gameObject);
        }
    }

    [Server]
    void DestroySelf()
    {
        NetworkServer.Destroy(gameObject);
    }
}