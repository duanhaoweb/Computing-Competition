using System.Collections.Generic;
using BlockSystem.Data;
using BlockSystem.Implementation;
using Cysharp.Threading.Tasks;

namespace BlockSystem.Abstractions
{
    /// <summary>
    /// 木块操作处理器接口，用于管理全局操作
    /// </summary>
    public interface IBlockOperationHandler
    {
        /// <summary>
        /// 当前操作ID，用于标识连续的操作（如拖拽过程中的多次移动）
        /// </summary>
        int CurrentOperationId { get; }

        /// <summary>
        /// 执行操作并记录到历史
        /// </summary>
        UniTask<CommandResult> ExecuteOperationAsync(WoodBlock block, BlockOperationData data);

        /// <summary>
        /// 撤销上一步操作
        /// </summary>
        UniTask UndoLastOperationAsync();

        /// <summary>
        /// 撤销指定操作ID的所有操作
        /// </summary>
        UniTask UndoOperationsAsync(int operationId);

        /// <summary>
        /// 获取操作历史
        /// </summary>
        IReadOnlyList<(BlockOperationData Data, WoodBlock Block)> GetOperationHistory();

        /// <summary>
        /// 清空操作历史
        /// </summary>
        void ClearHistory();

        /// <summary>
        /// 开始一个新的操作序列（例如开始拖拽时调用）
        /// </summary>
        void BeginNewOperation();
    }
}