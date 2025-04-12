using System.Threading;
using BlockSystem.Abstractions;
using BlockSystem.Implementation;
using BlockSystem.States;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BlockSystem.Commands
{
    public abstract class BlockCommandBase : IBlockCommand
    {
        protected Vector3 OriginalPosition { get; private set; }
        protected Quaternion OriginalRotation { get; private set; }
        protected bool WasInterrupted { get; private set; }

        public readonly int OperationId;

        public abstract (Vector3 move, Quaternion rotation) Transfer { get; protected set; }

        public BlockCommandBase(int operationId)
        {
            this.OperationId = operationId;
        }

        public async UniTask<CommandResult> ExecuteAsync(WoodBlock block)
        {
            // 保存初始状态，用于可能的恢复
            this.OriginalPosition = block.Position;
            this.OriginalRotation = block.Rotation;
            this.WasInterrupted = false;

            // 创建取消令牌源
            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken collisionToken = cts.Token;

            try
            {
                // 注册碰撞检测
                void OnCollision()
                {
                    if (!cts.IsCancellationRequested)
                    {
                        cts.Cancel();
                    }
                }

                block.OnTriggerEnterEvent.Register(OnCollision);
                try
                {
                    // 执行具体的命令逻辑
                    CommandResult r = await this.ExecuteInternalAsync(block, collisionToken);
                    return r;
                }
                finally
                {
                    // 清理碰撞检测事件
                    block.OnTriggerEnterEvent.UnRegister(OnCollision);
                }
            }
            finally
            {
                // 释放取消令牌源
                cts.Dispose();
            }
        }

        public virtual UniTask UndoAsync(WoodBlock block)
        {
            // 默认的撤销操作是恢复到原始位置，但不显示警告效果
            return block.SetStateAsync(new RestoringState(
                this.OriginalPosition,
                this.OriginalRotation,
                alertOnEnter: false));
        }

        /// <summary>
        /// 执行具体的命令逻辑 (模板方法)
        /// </summary>
        /// <param name="block">操作的木块</param>
        /// <param name="cancellationToken">如果发生碰撞，此令牌将被取消</param>
        protected abstract UniTask<CommandResult> ExecuteInternalAsync(WoodBlock block,
            CancellationToken cancellationToken);
    }
}