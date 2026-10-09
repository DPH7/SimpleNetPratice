using Mirror;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    void Update()
    {
        // 如果不是本地玩家，直接退出，不响应按键输入
        if (!isLocalPlayer) return;

        // 本地玩家移动逻辑
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        transform.Translate(new Vector3(h, 0, v) * (5 * Time.deltaTime));
    }
}