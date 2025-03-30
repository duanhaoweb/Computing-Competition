using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace AI.BlockSystem
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

        protected override async UniTask ExecuteInternalAsync(AIWoodBlock block, CancellationToken cancellationToken)
        {
            var targetPosition = block.Position + this.Transfer.move;

            // 先保存原位置，用于可能的恢复
            var originalPosition = block.Position;

            // 创建并进入移动状态
            var moveState = new MovingState(originalPosition, targetPosition, this._duration);
            await block.SetStateAsync(moveState);
            await block.SetStateAsync(IdleState.Instance);
            /*try
            {
                // 等待移动完成或被取消
                while (block.CurrentState is MovingState currentMoveState && !currentMoveState.IsComplete)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await UniTask.Yield();
                }

                // 如果当前状态是恢复状态，说明发生了碰撞
                if (block.CurrentState is RestoringState)
                {
                    throw new System.OperationCanceledException("Movement was interrupted by collision");
                }

                // 切换回空闲状态
                await block.SetStateAsync(new IdleState());
            }
            catch (System.Exception)
            {
                if (!(block.CurrentState is RestoringState))
                {
                    // 如果不是因为碰撞进入恢复状态，则手动恢复
                    await block.SetStateAsync(new RestoringState(originalPosition, block.Rotation));
                }
                throw;
            }*/
        }

        public sealed override (Vector3 move, Quaternion rotation) Transfer { get; protected set; }

        public override async UniTask UndoAsync(AIWoodBlock block)
        {
            // 使用 MovingState 直接执行反向移动
            var targetPosition = block.Position - this._moveDirection * this._moveDistance;
            var movingState = new MovingState(block.Position, targetPosition, 0.05f);
            await block.SetStateAsync(movingState);

            // 等待移动完成后切换回空闲状态
            while (block.CurrentState is MovingState currentMoveState && !currentMoveState.IsComplete)
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