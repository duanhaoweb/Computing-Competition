using System;
using QFramework;
using UnityEngine;

[Serializable]
public class UnitOperation
{
    public static readonly float MoveStep = 0.5f; // 移动步长
    public static readonly float RotationStep = 90f; // 旋转步长

    public int id;
    public int index;
    public OperationType type;
    public int value;
    public bool success;

    public UnitOperation(int id, OperationType type, int value, int index)
    {
        this.id = id;
        this.type = type;
        this.value = value;
        this.index = index;
    }

    public bool IsMoveOperation => this.type == OperationType.MoveX || this.type == OperationType.MoveY ||
                                   this.type == OperationType.MoveZ;

    public bool IsRotateOperation => this.type == OperationType.RotateX || this.type == OperationType.RotateY ||
                                     this.type == OperationType.RotateZ;

    public Vector3 GetTargetPosition(Vector3 currentPosition)
    {
        switch (this.type)
        {
            case OperationType.MoveX:
                return new Vector3(currentPosition.x + this.value * MoveStep, currentPosition.y, currentPosition.z);
            case OperationType.MoveY:
                return new Vector3(currentPosition.x, currentPosition.y + this.value * MoveStep, currentPosition.z);
            case OperationType.MoveZ:
                return new Vector3(currentPosition.x, currentPosition.y, currentPosition.z + this.value * MoveStep);
            default:
                return currentPosition;
        }
    }

    public Quaternion GetTargetQuaternion(Quaternion currentRotation)
    {
        switch (this.type)
        {
            //使用四元数旋转, 用欧拉角会有万向锁问题(不能使用Quaternion.Euler)
            case OperationType.RotateX:
                return Quaternion.AngleAxis(this.value * RotationStep, Vector3.right) * currentRotation;
            case OperationType.RotateY:
                return Quaternion.AngleAxis(this.value * RotationStep, Vector3.up) * currentRotation;
            case OperationType.RotateZ:
                return Quaternion.AngleAxis(this.value * RotationStep, Vector3.forward) * currentRotation;
            default:
                return currentRotation;
        }
    }
}

public enum OperationType
{
    MoveX,
    MoveY,
    MoveZ,
    RotateX,
    RotateY,
    RotateZ
}