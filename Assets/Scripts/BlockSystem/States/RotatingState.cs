using System.Threading;
using BlockSystem.Abstractions;
using BlockSystem.Implementation;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace BlockSystem.States
{
    public class RotatingState : IBlockState
    {
        private readonly Quaternion _startRotation;
        private readonly Quaternion _targetRotation;
        private readonly float _duration;

        public bool CanAcceptCommand => false; // 旋转过程中不接受新命令

        public bool IsComplete { get; private set; }

        public RotatingState(Quaternion startRotation, Quaternion targetRotation, float duration = 0.2f)
        {
            this._startRotation = startRotation;
            this._targetRotation = targetRotation;
            this._duration = duration;
            this.IsComplete = false;
        }

        /// <summary>
        /// 使用旋转轴和角度创建旋转状态
        /// </summary>
        public static RotatingState CreateFromAxisAngle(Vector3 axis, float angle, Quaternion currentRotation,
            float duration = 0.2f)
        {
            Quaternion rotation = Quaternion.AngleAxis(angle, axis);
            Quaternion targetRotation = rotation * currentRotation;
            return new RotatingState(currentRotation, targetRotation, duration);
        }

        public async UniTask EnterAsync(WoodBlock block, CancellationToken cancellationToken = default)
        {
            var tween = block.transform.DORotateQuaternion(this._targetRotation, this._duration);
            await tween.ToUniTask(cancellationToken: cancellationToken);
            this.IsComplete = true;
        }

        public UniTask ExitAsync(WoodBlock block, CancellationToken cancellationToken = default)
        {
            return UniTask.CompletedTask;
        }
    }
}