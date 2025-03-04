using UnityEngine;
using System.Collections.Generic;

public class PathManager : MonoBehaviour
{
    [Header("迷宫设置")]
    [SerializeField] private Transform player;  // 玩家对象
    [SerializeField] private float gridSize = 1f;  // 每个 Cube 的大小
    [SerializeField] private List<GameObject> mazeCubes;  // 所有迷宫 Cube 的列表

    [Header("可见设置")]
    [SerializeField] private Color visitedColor = Color.white;  // 走过的格子的颜色
    [SerializeField] private float visibilityRange = 1.5f;  // 玩家可见的范围（1.5x1.5）

    private HashSet<Vector3> visitedPositions = new HashSet<Vector3>();  // 记录玩家已走过的位置

    void Start()
    {
        // 初始化：将所有的 Cube 设置为不可见
        SetAllCubesInvisible();
    }

    void Update()
    {
        // 获取玩家的当前位置
        Vector3 playerPosition = player.position;

        // 计算玩家可见的范围：1.5 x 1.5 的区域
        Vector3 minPosition = new Vector3(playerPosition.x - visibilityRange / 2, 0f, playerPosition.z - visibilityRange / 2);
        Vector3 maxPosition = new Vector3(playerPosition.x + visibilityRange / 2, 0f, playerPosition.z + visibilityRange / 2);

        // 遍历所有迷宫 Cube，并检查它们是否在可见范围内
        foreach (var cube in mazeCubes)
        {
            Vector3 cubePosition = cube.transform.position;

            // 检查 Cube 是否在玩家可见的范围内
            if (cubePosition.x >= minPosition.x && cubePosition.x <= maxPosition.x &&
                cubePosition.z >= minPosition.z && cubePosition.z <= maxPosition.z)
            {
                // 如果 Cube 还没有被走过，设置为可见并变色
                if (!visitedPositions.Contains(cubePosition))
                {
                    visitedPositions.Add(cubePosition);
                    cube.SetActive(true);  // 使 Cube 可见
                    cube.GetComponent<Renderer>().material.color = visitedColor;  // 修改颜色
                }
            }
            else
            {
                // 如果 Cube 不在玩家范围内，保持不可见
                cube.SetActive(false);
            }
        }
    }

    // 设置所有 Cube 不可见
    private void SetAllCubesInvisible()
    {
        foreach (var cube in mazeCubes)
        {
            cube.SetActive(false);  // 设置每个 Cube 不可见
        }
    }
}
