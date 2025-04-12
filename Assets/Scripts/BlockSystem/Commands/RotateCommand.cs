using System.Threading;
using BlockSystem.Abstractions;
using BlockSystem.Implementation;
using BlockSystem.States;
using Cysharp.Threading.Tasks;
using QFramework;
using UnityEngine;

namespace BlockSystem.Commands
{
    public class RotateCommand : BlockCommandBase
    {
        private readonly Vector3 _rotationAxis;
        private readonly float _rotationAngle;
        private readonly float _duration;

        /// <summary>
        /// 创建旋转命令
        /// </summary>
        /// <param name="operationId">操作标记</param>
        /// <param name="axis">旋转轴</param>
        /// <param name="angle">旋转角度（度）</param>
        /// <param name="duration">旋转动画持续时间（秒）</param>
        private RotateCommand(int operationId, Vector3 axis, float angle = 90f, float duration = 0.2f) : base(
            operationId)
        {
            this._rotationAxis = axis.normalized;
            this._rotationAngle = angle;
            this._duration = duration;
            this.Transfer = (Vector3.zero, Quaternion.AngleAxis(this._rotationAngle, this._rotationAxis));
        }

        /// <summary>
        /// 创建基于世界坐标轴的旋转命令
        /// </summary>
        /// <param name="operationId">操作标记</param>
        /// <param name="axis">世界坐标轴（Vector3.right/up/forward）</param>
        /// <param name="value">旋转方向（1 或 -1）</param>
        /// <param name="angle">旋转角度（度）</param>
        /// <param name="duration">旋转动画持续时间（秒）</param>
        public static RotateCommand CreateAxisRotation(int operationId, Vector3 axis, int value, float angle = 90f,
            float duration = 0.2f)
        {
            return new RotateCommand(operationId, axis, angle * Mathf.Sign(value), duration);
        }

        protected override async UniTask<CommandResult> ExecuteInternalAsync(WoodBlock block,
            CancellationToken cancellationToken)
        {
            // 计算目标旋转
            Quaternion targetRotation = this.Transfer.rotation * block.Rotation;

            // 先保存原旋转，用于可能的恢复
            Quaternion originalRotation = block.Rotation;

            // 创建并进入旋转状态
            RotatingState rotateState = new RotatingState(block.Rotation, targetRotation, this._duration);
            CommandResult result = CommandResult.Pending();
            AudioKit.PlaySound("移动");
            await block.SetStateAsync(rotateState, cancellationToken);
            // 如果旋转被取消，恢复到原始状态
            if (cancellationToken.IsCancellationRequested)
            {
                await block.SetStateAsync(new RestoringState(block.Position, originalRotation));
                result.Status = CommandResultStatus.Failed;
            }
            else
            {
                result.Status = CommandResultStatus.Success;
            }

            // 确保回到空闲状态, 其实RestoringState会自动切换到IdleState~
            if (!(block.CurrentState is IdleState))
            {
                await block.SetStateAsync(IdleState.Instance);
            }

            return result;
        }

        public sealed override (Vector3 move, Quaternion rotation) Transfer { get; protected set; }

        public override async UniTask UndoAsync(WoodBlock block)
        {
            // 使用 RotatingState 直接执行反向旋转，不通过命令系统
            Quaternion targetRotation = Quaternion.AngleAxis(-this._rotationAngle, this._rotationAxis) * block.Rotation;
            RotatingState rotateState = new RotatingState(block.Rotation, targetRotation, this._duration);
            await block.SetStateAsync(rotateState);

            // 等待旋转完成后切换回空闲状态
            while (block.CurrentState is RotatingState currentRotateState && !currentRotateState.IsComplete)
            {
                await UniTask.Yield();
            }

            // 确保回到空闲状态
            if (!(block.CurrentState is IdleState))
            {
                await block.SetStateAsync(IdleState.Instance);
            }
        }
    }
}