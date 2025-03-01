using UnityEngine;
using System.Collections.Generic;

public class PathManager : MonoBehaviour
{
    [Header("�Թ�����")]
    [SerializeField] private Transform player;  // ��Ҷ���
    [SerializeField] private float gridSize = 1f;  // ÿ�� Cube �Ĵ�С
    [SerializeField] private List<GameObject> mazeCubes;  // �����Թ� Cube ���б�

    [Header("�ɼ�����")]
    [SerializeField] private Color visitedColor = Color.white;  // �߹��ĸ��ӵ���ɫ
    [SerializeField] private float visibilityRange = 1.5f;  // ��ҿɼ��ķ�Χ��1.5x1.5��

    private HashSet<Vector3> visitedPositions = new HashSet<Vector3>();  // ��¼������߹���λ��

    void Start()
    {
        // ��ʼ���������е� Cube ����Ϊ���ɼ�
        SetAllCubesInvisible();
    }

    void Update()
    {
        // ��ȡ��ҵĵ�ǰλ��
        Vector3 playerPosition = player.position;

        // ������ҿɼ��ķ�Χ��1.5 x 1.5 ������
        Vector3 minPosition = new Vector3(playerPosition.x - visibilityRange / 2, 0f, playerPosition.z - visibilityRange / 2);
        Vector3 maxPosition = new Vector3(playerPosition.x + visibilityRange / 2, 0f, playerPosition.z + visibilityRange / 2);

        // ���������Թ� Cube������������Ƿ��ڿɼ���Χ��
        foreach (var cube in mazeCubes)
        {
            Vector3 cubePosition = cube.transform.position;

            // ��� Cube �Ƿ�����ҿɼ��ķ�Χ��
            if (cubePosition.x >= minPosition.x && cubePosition.x <= maxPosition.x &&
                cubePosition.z >= minPosition.z && cubePosition.z <= maxPosition.z)
            {
                // ��� Cube ��û�б��߹�������Ϊ�ɼ�����ɫ
                if (!visitedPositions.Contains(cubePosition))
                {
                    visitedPositions.Add(cubePosition);
                    cube.SetActive(true);  // ʹ Cube �ɼ�
                    cube.GetComponent<Renderer>().material.color = visitedColor;  // �޸���ɫ
                }
            }
            else
            {
                // ��� Cube ������ҷ�Χ�ڣ����ֲ��ɼ�
                cube.SetActive(false);
            }
        }
    }

    // �������� Cube ���ɼ�
    private void SetAllCubesInvisible()
    {
        foreach (var cube in mazeCubes)
        {
            cube.SetActive(false);  // ����ÿ�� Cube ���ɼ�
        }
    }
}
