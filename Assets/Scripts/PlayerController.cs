using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private int speed = 5;  // 移动速度
    [SerializeField] private Animator anim;  // 动画控制器
    [SerializeField] private SpriteRenderer playerSprite;  // 玩家精灵渲染器
    private Rigidbody rb;  // 刚体
    private Vector3 movement;  // 角色移动方向

    private const string IS_WALK_PARAM = "IsWalk";  // 动画参数：是否行走
    private bool isWalking = false;  // 判断是否在行走

    // Start is called before the first frame update
    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();  // 获取刚体组件
    }

    // Update is called once per frame
    private void Update()
    {
        HandleMovement();  // 控制玩家移动
        UpdateAnimation();  // 更新动画
    }

    private void HandleMovement()
    {
        // 获取玩家输入
        float horizontal = Input.GetAxisRaw("Horizontal");  // A/D 或 左/右箭头
        float vertical = Input.GetAxisRaw("Vertical");  // W/S 或 上/下箭头

        // 计算移动方向
        movement = new Vector3(horizontal, 0, vertical).normalized;

        // 如果有按键输入
        if (movement.magnitude > 0)
        {
            // 每次按键时前进1.5单位
            transform.Translate(movement * 3f * speed * Time.deltaTime, Space.World);

            // 如果不是正在行走，开始行走
            if (!isWalking)
            {
                isWalking = true;
            }
        }
        else
        {
            // 松开按键停止行走
            if (isWalking)
            {
                isWalking = false;
            }
        }
    }

    private void UpdateAnimation()
    {
        // 更新Animator的行走参数
        anim.SetBool(IS_WALK_PARAM, isWalking);

        // 如果有移动方向，则根据输入方向控制玩家朝向
        if (movement.magnitude > 0)
        {
            playerSprite.flipX = movement.x < 0;  // 根据水平输入反转玩家朝向（左/右）
        }
    }
}
