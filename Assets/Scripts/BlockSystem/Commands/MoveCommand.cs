using System.Threading;
using BlockSystem.Abstractions;
using BlockSystem.Implementation;
using BlockSystem.States;
using Cysharp.Threading.Tasks;
using QFramework;
using UnityEngine;

namespace BlockSystem.Commands
{
    public class MoveCommand : BlockCommandBase
    {
        private readonly Vector3 _moveDirection;
        private readonly float _moveDistance;
        private readonly float _duration;

        /// <summary>
        /// 创建移动命令
        /// </summary>
        /// <param name="operationId">操作标记</param>
        /// <param name="direction">移动方向（应为单位向量）</param>
        /// <param name="distance">移动距离</param>
        /// <param name="duration">移动动画持续时间（秒）</param>
        private MoveCommand(int operationId, Vector3 direction, float distance = 0.5f, float duration = 0.15f) : base(
            operationId)
        {
            this._moveDirection = direction.normalized;
            this._moveDistance = distance;
            this._duration = duration;
            this.Transfer = (this._moveDistance * this._moveDirection, Quaternion.identity);
        }

        /// <summary>
        /// 创建基于轴向的移动命令
        /// </summary>
        /// <param name="operationId">操作标记</param>
        /// <param name="axis">移动轴向（例如 Vector3.right 表示 X 轴）</param>
        /// <param name="value">移动方向（1 或 -1）</param>
        /// <param name="distance">移动距离</param>
        /// <param name="duration">移动动画持续时间（秒）</param>
        public static MoveCommand CreateAxisMove(int operationId, Vector3 axis, int value, float distance = 0.5f,
            float duration = 0.15f)
        {
            return new MoveCommand(operationId, axis * Mathf.Sign(value), distance, duration);
        }

        protected override async UniTask<CommandResult> ExecuteInternalAsync(WoodBlock block,
            CancellationToken cancellationToken)
        {
            Vector3 targetPosition = block.Position + this.Transfer.move;

            // 先保存原位置，用于可能的恢复
            Vector3 originalPosition = block.Position;

            // 创建并进入移动状态
            MovingState moveState = new MovingState(originalPosition, targetPosition, this._duration);
            CommandResult result = CommandResult.Pending();
            AudioKit.PlaySound("移动");
            await block.SetStateAsync(moveState, cancellationToken);

            // 如果移动被取消，恢复到原始位置
            if (cancellationToken.IsCancellationRequested)
            {
                await block.SetStateAsync(new RestoringState(originalPosition, block.Rotation));
                await UniTask.Delay(300); // 等待一小段时间
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
            // 使用 MovingState 直接执行反向移动
            Vector3 targetPosition = block.Position - this._moveDirection * this._moveDistance;
            MovingState movingState = new MovingState(block.Position, targetPosition, 0.05f);
            await block.SetStateAsync(movingState);
            // 确保回到空闲状态
            if (!(block.CurrentState is IdleState))
            {
                await block.SetStateAsync(IdleState.Instance);
            }
        }
    }
}