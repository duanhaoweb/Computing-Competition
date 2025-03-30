using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AI.BlockSystem
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

        protected override UniTask ExecuteInternalAsync(AIWoodBlock block, CancellationToken cancellationToken)
        {
            // 进入恢复状态
            var restoringState = new RestoringState(this._targetPosition, this._targetRotation, false);
            return block.SetStateAsync(restoringState);
        }

        public static DirectTransferCommand Create(Vector3 targetPosition, Quaternion targetRotation)
        {
            var command = new DirectTransferCommand(-1, targetPosition, targetRotation);
            return command;
        }
    }
}