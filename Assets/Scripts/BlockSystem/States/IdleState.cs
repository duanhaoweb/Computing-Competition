using System.Threading;
using BlockSystem.Abstractions;
using BlockSystem.Implementation;
using Cysharp.Threading.Tasks;

namespace BlockSystem.States
{
    public class IdleState : IBlockState
    {
        public bool CanAcceptCommand => true;

        public static readonly IdleState Instance = new IdleState();

        private IdleState()
        {
            // 尽量不 new ~
        }

        public UniTask EnterAsync(WoodBlock block, CancellationToken cancellationToken = default)
        {
            return UniTask.CompletedTask;
        }

        public UniTask ExitAsync(WoodBlock block, CancellationToken cancellationToken = default)
        {
            // 空闲状态无需特殊的退出逻辑
            return UniTask.CompletedTask;
        }
    }
}