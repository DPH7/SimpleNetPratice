using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNetworkController : NetworkBehaviour
{
    [Header("UI 绑定 (可选)")]
    public Text healthText;

    // 1. [SyncVar] 变量自动同步：当 Server 端修改 health 时，自动同步至所有客户端并触发 OnHealthChanged
    [SyncVar(hook = nameof(OnHealthChanged))]
    public int health = 100;
    
    public GameObject bulletPrefab; // 必须事先注册到 NetworkManager 的 Spawnable Prefabs 列表中
    public Transform bulletSpawnPoint;

    void Update()
    {
        // 确保只有当前客户端控制的本地玩家，才能响应按键输入
        if (!isLocalPlayer) return;

        // 【按 J 键】请求攻击/发射子弹 (测试 [Command])
        if (Input.GetKeyDown(KeyCode.J))
        {
            CmdRequestAttack();
        }

        // 【按 K 键】请求扣血 (测试 [Command] 改变 [SyncVar])
        if (Input.GetKeyDown(KeyCode.K))
        {
            CmdTakeDamage(10);
        }

        // 【按 L 键】请求私聊/个人结算通知 (测试 [TargetRpc])
        if (Input.GetKeyDown(KeyCode.L))
        {
            CmdRequestPrivateMessage();
        }
    }

    // =========================================================================
    // 1. 变量同步回调 (SyncVar Hook)
    // =========================================================================
    // hook 函数必须包含 (oldValue, newValue) 两个参数
    void OnHealthChanged(int oldHealth, int newHealth)
    {
        Debug.Log($"[Client] 玩家 {netId} 血量变化: {oldHealth} -> {newHealth}");
        if (healthText != null)
        {
            healthText.text = $"HP: {newHealth}";
        }
    }

    // =========================================================================
    // 2. 客户端调用服务端 ([Command])
    // =========================================================================
    // 客户端调用，代码只在 Server 端运行
    [Command]
    void CmdRequestAttack()
    {
        Debug.Log($"[Server] 收到来自客户端 {connectionToClient.connectionId} 的攻击请求！");
        
        // 生成子弹实体
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position + Vector3.forward * 2, bulletSpawnPoint.rotation);

        // 3. 告诉 Mirror 将该子弹网络化并同步给所有客户端（自动广播生成！）[cite: 1]
        NetworkServer.Spawn(bullet);
        
        // 服务端处理完攻击逻辑后，广播给全场播放特效
        RpcPlayExplosionEffect();
    }

    [Command]
    void CmdTakeDamage(int damage)
    {
        // 服务端修改 SyncVar 变量，Mirror 会自动广播最新值给所有客户端
        health = Mathf.Max(0, health - damage);
        Debug.Log($"[Server] 玩家 {netId} 受到 {damage} 点伤害，剩余血量: {health}");
    }

    [Command]
    void CmdRequestPrivateMessage()
    {
        // 仅向调用该指令的这个客户端发送单播响应
        TargetSendPrivateNotice(connectionToClient, $"这是专属于 connectionId:{connectionToClient.connectionId} 的私密消息！");
    }

    // =========================================================================
    // 3. 服务端广播客户端 ([ClientRpc] 与 [TargetRpc])
    // =========================================================================
    // [ClientRpc]: 全场广播，所有连接的客户端都会收到并执行
    [ClientRpc]
    void RpcPlayExplosionEffect()
    {
        Debug.Log($"[Client] 全员收到广播：玩家 {netId} 释放了攻击/爆炸特效！");
        // 此处可编写 Instantiate 特效或 Play 音效的代码
    }

    // [TargetRpc]: 精准单播，仅指定的 NetworkConnection 对应的客户端会收到
    [TargetRpc]
    void TargetSendPrivateNotice(NetworkConnection target, string message)
    {
        Debug.Log($"[Client] 收到来自服务端的单播提示: {message}");
    }
}