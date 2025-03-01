using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform player;  // 玩家对象的Transform
    [SerializeField] private Vector3 offset = new Vector3(0, 3, -6);  // 摄像机相对玩家的偏移量
    [SerializeField] private float followSpeed = 5f;  // 跟随速度

    // Update is called once per frame
    private void Update()
    {
        // 确保摄像机始终保持在玩家的前方
        Vector3 desiredPosition = player.position + offset;

        // 平滑地移动摄像机到目标位置
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

    }
}
