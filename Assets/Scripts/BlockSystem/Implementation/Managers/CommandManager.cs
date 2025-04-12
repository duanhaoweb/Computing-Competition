using System.Collections.Generic;
using BlockSystem.Abstractions;
using BlockSystem.Commands;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace BlockSystem.Implementation.Managers
{
    public class CommandManager
    {
        private readonly Stack<(IBlockCommand Command, WoodBlock Block)> _operationHistory = new();
        public readonly UnityEvent onOperationCompleted = new();

        public async UniTask<CommandResult> ExecuteCommandAsync(IBlockCommand command, WoodBlock block)
        {
            CommandResult result = await block.ExecuteCommandAsync(command);
            if (result.Status == CommandResultStatus.Success && command is not DirectTransferCommand)
            {
                this._operationHistory.Push((command, block));
                this.onOperationCompleted?.Invoke();
            }

            return result;
        }

        public async UniTask UndoOperationAsync()
        {
            if (this._operationHistory.Count <= 0)
            {
                Debug.Log("No operations to undo.");
                return;
            }

            int operationId = ((BlockCommandBase)this._operationHistory.Peek().Command).OperationId;
            var operationsToUndo = new List<(IBlockCommand Command, WoodBlock Block)>();

            do
            {
                operationsToUndo.Add(this._operationHistory.Pop());
            } while (this._operationHistory.Count > 0 &&
                     ((BlockCommandBase)this._operationHistory.Peek().Command).OperationId == operationId);

            WoodBlock block = operationsToUndo[0].Block;
            if (!block.CanAcceptCommand)
            {
                Debug.Log("Block is busy, cannot undo.");
                return;
            }

            Vector3 targetPosition = block.Position;
            Quaternion targetRotation = block.Rotation;
            foreach ((IBlockCommand command, WoodBlock _) in operationsToUndo)
            {
                (Vector3 move, Quaternion rotation) transfer = command.Transfer;
                Vector3 inverseMove = -transfer.move;
                Quaternion inverseRotation = Quaternion.Inverse(transfer.rotation);

                targetPosition += inverseMove;
                targetRotation = inverseRotation * targetRotation;
            }

            DirectTransferCommand undoCommand = DirectTransferCommand.Create(targetPosition, targetRotation);
            await block.ExecuteCommandAsync(undoCommand);
        }

        public void Dispose()
        {
            this._operationHistory.Clear();
            this.onOperationCompleted.RemoveAllListeners();
        }
    }
}