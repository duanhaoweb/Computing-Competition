using BlockSystem.Implementation;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BlockSystem.Abstractions
{
    public enum CommandResultStatus
    {
        Success,
        Failed,
        Pending
    }

    public struct CommandResult
    {
        public CommandResultStatus Status;
        public string Message;

        public static CommandResult Success() => new() { Status = CommandResultStatus.Success };

        public static CommandResult Failed(string message = "") =>
            new() { Status = CommandResultStatus.Failed, Message = message };

        public static CommandResult Pending() => new() { Status = CommandResultStatus.Pending };
    }

    public interface IBlockCommand
    {
        /// <summary>
        /// 异步执行命令
        /// </summary>
        /// <param name="block">执行命令的木块</param>
        /// <returns>命令执行结果</returns>
        UniTask<CommandResult> ExecuteAsync(WoodBlock block);

        /// <summary>
        /// 异步撤销命令
        /// </summary>
        /// <param name="block">需要撤销命令的木块</param>
        UniTask UndoAsync(WoodBlock block);

        /// <summary>
        /// 获取命令的转移信息
        /// </summary>
        (Vector3 move, Quaternion rotation) Transfer { get; }
    }
}