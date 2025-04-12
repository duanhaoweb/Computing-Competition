using UnityEngine;

namespace BlockSystem.Data
{
    /// <summary>
    /// 块操作类型，对应不同的操作命令
    /// </summary>
    public enum BlockOperationType
    {
        MoveX,
        MoveY,
        MoveZ,
        RotateX,
        RotateY,
        RotateZ
    }

    /// <summary>
    /// 存储木块操作的数据结构，基于原有 UnitOperation 设计
    /// </summary>
    public class BlockOperationData
    {
        public static readonly float MoveStep = 0.5f; // 移动步长
        public static readonly float RotationStep = 90f; // 旋转步长

        public int Id { get; }
        public BlockOperationType Type { get; }
        public int Value { get; }

        public BlockOperationData(int id, BlockOperationType type, int value)
        {
            this.Id = id;
            this.Type = type;
            this.Value = value;
        }

        public bool IsMoveOperation =>
            this.Type == BlockOperationType.MoveX ||
            this.Type == BlockOperationType.MoveY ||
            this.Type == BlockOperationType.MoveZ;

        public bool IsRotateOperation =>
            this.Type == BlockOperationType.RotateX ||
            this.Type == BlockOperationType.RotateY ||
            this.Type == BlockOperationType.RotateZ;

        public Vector3 GetMoveDirection()
        {
            return this.Type switch
            {
                BlockOperationType.MoveX => Vector3.right * this.Value,
                BlockOperationType.MoveY => Vector3.up * this.Value,
                BlockOperationType.MoveZ => Vector3.forward * this.Value,
                _ => Vector3.zero
            };
        }

        public Vector3 GetRotationAxis()
        {
            return this.Type switch
            {
                BlockOperationType.RotateX => Vector3.right,
                BlockOperationType.RotateY => Vector3.up,
                BlockOperationType.RotateZ => Vector3.forward,
                _ => Vector3.zero
            };
        }

        /// <summary>
        /// 创建移动操作数据
        /// </summary>
        public static BlockOperationData CreateMoveOperation(int id, Vector3 axis, int value)
        {
            BlockOperationType type = axis.x != 0 ? BlockOperationType.MoveX :
                axis.y != 0 ? BlockOperationType.MoveY :
                BlockOperationType.MoveZ;
            return new BlockOperationData(id, type, value);
        }

        /// <summary>
        /// 创建旋转操作数据
        /// </summary>
        public static BlockOperationData CreateRotateOperation(int id, Vector3 axis, int value)
        {
            BlockOperationType type = axis.x != 0 ? BlockOperationType.RotateX :
                axis.y != 0 ? BlockOperationType.RotateY :
                BlockOperationType.RotateZ;
            return new BlockOperationData(id, type, value);
        }
    }
}