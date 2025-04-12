using System.Threading;
using BlockSystem.Implementation;
using Cysharp.Threading.Tasks;

namespace BlockSystem.Abstractions
{
    public interface IBlockState
    {
        /// <summary>
        /// 进入该状态时调用
        /// </summary>
        /// <param name="block">状态所属的木块</param>
        /// <param name="cancellationToken">取消令牌</param>
        UniTask EnterAsync(WoodBlock block, CancellationToken cancellationToken = default);

        /// <summary>
        /// 退出该状态时调用
        /// </summary>
        /// <param name="block">状态所属的木块</param>
        /// <param name="cancellationToken">取消令牌</param>
        UniTask ExitAsync(WoodBlock block, CancellationToken cancellationToken = default);

        /// <summary>
        /// 状态是否允许接收新的命令
        /// </summary>
        bool CanAcceptCommand { get; }
    }
}