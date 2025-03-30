using Cysharp.Threading.Tasks;

namespace AI.BlockSystem
{
    public interface IBlockState
    {
        /// <summary>
        /// 进入该状态时调用
        /// </summary>
        /// <param name="block">状态所属的木块</param>
        UniTask EnterAsync(AIWoodBlock block);

        /// <summary>
        /// 退出该状态时调用
        /// </summary>
        /// <param name="block">状态所属的木块</param>
        UniTask ExitAsync(AIWoodBlock block);

        /// <summary>
        /// 状态是否允许接收新的命令
        /// </summary>
        bool CanAcceptCommand { get; }

        /// <summary>
        /// 状态是否可以被打断（例如碰撞发生时）
        /// </summary>
        bool CanBeInterrupted { get; }
    }
}