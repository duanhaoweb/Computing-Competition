using System.Threading;
using BlockSystem.Abstractions;
using BlockSystem.Implementation;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace BlockSystem.States
{
    public class MovingState : IBlockState
    {
        private readonly Vector3 _startPosition;
        private readonly Vector3 _targetPosition;
        private readonly float _duration;

        public bool CanAcceptCommand => false; // 移动过程中不接受新命令

        public bool IsComplete { get; private set; }

        public MovingState(Vector3 startPosition, Vector3 targetPosition, float duration = 0.15f)
        {
            this._startPosition = startPosition;
            this._targetPosition = targetPosition;
            this._duration = duration;
            this.IsComplete = false;
        }

        public async UniTask EnterAsync(WoodBlock block, CancellationToken cancellationToken)
        {
            var tween = block.transform.DOMove(this._targetPosition, this._duration);
            await tween.ToUniTask(cancellationToken: cancellationToken);
            this.IsComplete = true;
        }

        public UniTask ExitAsync(WoodBlock block, CancellationToken cancellationToken = default)
        {
            return UniTask.CompletedTask;
        }
    }
}