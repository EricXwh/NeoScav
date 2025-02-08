using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintCanvasOnEnable : MonoBehaviour
{
    private void OnEnable()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            // 计算从摄像机到当前 Hint Canvas 的方向
            Vector3 direction = transform.position - cam.transform.position;
            // 设置 Hint Canvas 的旋转使其面向摄像机，保持摄像机的 up 方向
            transform.rotation = Quaternion.LookRotation(direction, cam.transform.up);
        }
    }
}
