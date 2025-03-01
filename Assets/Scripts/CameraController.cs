using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform player;  // ��Ҷ����Transform
    [SerializeField] private Vector3 offset = new Vector3(0, 3, -6);  // ����������ҵ�ƫ����
    [SerializeField] private float followSpeed = 5f;  // �����ٶ�

    // Update is called once per frame
    private void Update()
    {
        // ȷ�������ʼ�ձ�������ҵ�ǰ��
        Vector3 desiredPosition = player.position + offset;

        // ƽ�����ƶ��������Ŀ��λ��
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

    }
}
