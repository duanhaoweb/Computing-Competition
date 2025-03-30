using Cysharp.Threading.Tasks;

namespace AI.BlockSystem
{
    public class IdleState : IBlockState
    {
        public bool CanAcceptCommand => true;
        public bool CanBeInterrupted => false; // 空闲状态不需要被打断

        public static readonly IdleState Instance = new IdleState();

        private IdleState()
        {
            // 尽量不 new ~
        }

        public UniTask EnterAsync(AIWoodBlock block)
        {
            // 空闲状态无需特殊的进入逻辑
            return UniTask.CompletedTask;
        }

        public UniTask ExitAsync(AIWoodBlock block)
        {
            // 空闲状态无需特殊的退出逻辑
            return UniTask.CompletedTask;
        }
    }
}