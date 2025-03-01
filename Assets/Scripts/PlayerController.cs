using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private int speed = 5;  // �ƶ��ٶ�
    [SerializeField] private Animator anim;  // ����������
    [SerializeField] private SpriteRenderer playerSprite;  // ��Ҿ�����Ⱦ��
    private Rigidbody rb;  // ����
    private Vector3 movement;  // ��ɫ�ƶ�����

    private const string IS_WALK_PARAM = "IsWalk";  // �����������Ƿ�����
    private bool isWalking = false;  // �ж��Ƿ�������

    // Start is called before the first frame update
    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();  // ��ȡ�������
    }

    // Update is called once per frame
    private void Update()
    {
        HandleMovement();  // ��������ƶ�
        UpdateAnimation();  // ���¶���
    }

    private void HandleMovement()
    {
        // ��ȡ�������
        float horizontal = Input.GetAxisRaw("Horizontal");  // A/D �� ��/�Ҽ�ͷ
        float vertical = Input.GetAxisRaw("Vertical");  // W/S �� ��/�¼�ͷ

        // �����ƶ�����
        movement = new Vector3(horizontal, 0, vertical).normalized;

        // ����а�������
        if (movement.magnitude > 0)
        {
            // ÿ�ΰ���ʱǰ��1.5��λ
            transform.Translate(movement * 3f * speed * Time.deltaTime, Space.World);

            // ��������������ߣ���ʼ����
            if (!isWalking)
            {
                isWalking = true;
            }
        }
        else
        {
            // �ɿ�����ֹͣ����
            if (isWalking)
            {
                isWalking = false;
            }
        }
    }

    private void UpdateAnimation()
    {
        // ����Animator�����߲���
        anim.SetBool(IS_WALK_PARAM, isWalking);

        // ������ƶ�������������뷽�������ҳ���
        if (movement.magnitude > 0)
        {
            playerSprite.flipX = movement.x < 0;  // ����ˮƽ���뷴ת��ҳ�����/�ң�
        }
    }
}
