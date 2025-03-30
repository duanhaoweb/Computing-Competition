using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace AI.BlockSystem
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

        protected override async UniTask ExecuteInternalAsync(AIWoodBlock block, CancellationToken cancellationToken)
        {
            // 计算目标旋转
            var targetRotation = this.Transfer.rotation * block.Rotation;

            // 创建并进入旋转状态
            var rotateState = new RotatingState(block.Rotation, targetRotation, this._duration);
            await block.SetStateAsync(rotateState);

            try
            {
                // 等待旋转完成或被取消
                while (block.CurrentState is RotatingState currentRotateState && !currentRotateState.IsComplete)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await UniTask.Yield();
                }

                // 如果当前状态是恢复状态，说明发生了碰撞
                if (block.CurrentState is RestoringState)
                {
                    throw new System.OperationCanceledException("Rotation was interrupted by collision");
                }

                // 切换回空闲状态
                await block.SetStateAsync(IdleState.Instance);
            }
            catch (System.Exception)
            {
                if (!(block.CurrentState is RestoringState))
                {
                    // 如果不是因为碰撞进入恢复状态，则手动恢复
                    await block.SetStateAsync(new RestoringState(block.Position, block.Rotation));
                }

                throw;
            }
        }

        public sealed override (Vector3 move, Quaternion rotation) Transfer { get; protected set; }

        public override async UniTask UndoAsync(AIWoodBlock block)
        {
            // 使用 RotatingState 直接执行反向旋转，不通过命令系统
            var targetRotation = Quaternion.AngleAxis(-this._rotationAngle, this._rotationAxis) * block.Rotation;
            var rotateState = new RotatingState(block.Rotation, targetRotation, this._duration);
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