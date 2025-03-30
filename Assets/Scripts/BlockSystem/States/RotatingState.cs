using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace AI.BlockSystem
{
    public class RotatingState : IBlockState
    {
        private readonly Quaternion _startRotation;
        private readonly Quaternion _targetRotation;
        private readonly float _duration;

        public bool CanAcceptCommand => false; // 旋转过程中不接受新命令
        public bool CanBeInterrupted => true; // 旋转过程中可以被碰撞打断
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
            var rotation = Quaternion.AngleAxis(angle, axis);
            var targetRotation = rotation * currentRotation;
            return new RotatingState(currentRotation, targetRotation, duration);
        }

        public async UniTask EnterAsync(AIWoodBlock block)
        {
            var tween = block.transform.DORotateQuaternion(this._targetRotation, this._duration);
            await tween.ToUniTask();
            this.IsComplete = true;
        }

        public UniTask ExitAsync(AIWoodBlock block)
        {
            return UniTask.CompletedTask;
        }
    }
}