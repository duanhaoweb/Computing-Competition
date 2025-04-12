using System.Threading;
using BlockSystem.Abstractions;
using BlockSystem.Implementation;
using BlockSystem.States;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BlockSystem.Commands
{
    public class DirectTransferCommand : BlockCommandBase
    {
        private readonly Vector3 _targetPosition;
        private readonly Quaternion _targetRotation;

        private DirectTransferCommand(int operationId, Vector3 targetPosition, Quaternion targetRotation) : base(
            operationId)
        {
            this._targetPosition = targetPosition;
            this._targetRotation = targetRotation;
        }

        public override (Vector3 move, Quaternion rotation) Transfer { get; protected set; }

        protected override async UniTask<CommandResult> ExecuteInternalAsync(WoodBlock block,
            CancellationToken cancellationToken)
        {
            // 进入恢复状态
            RestoringState restoringState = new RestoringState(this._targetPosition, this._targetRotation, false);
            await block.SetStateAsync(restoringState);
            return CommandResult.Success();
        }

        public static DirectTransferCommand Create(Vector3 targetPosition, Quaternion targetRotation)
        {
            DirectTransferCommand command = new DirectTransferCommand(-1, targetPosition, targetRotation);
            return command;
        }
    }
}