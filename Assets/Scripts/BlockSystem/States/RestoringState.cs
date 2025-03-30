using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace AI.BlockSystem
{
    public class RestoringState : IBlockState
    {
        private readonly Vector3 _targetPosition;
        private readonly Quaternion _targetRotation;
        private readonly bool _alertOnEnter;

        public RestoringState(Vector3 targetPosition, Quaternion targetRotation, bool alertOnEnter = true)
        {
            this._targetPosition = targetPosition;
            this._targetRotation = targetRotation;
            this._alertOnEnter = alertOnEnter;
        }

        public bool CanAcceptCommand => false; // 恢复过程中不接受新命令
        public bool CanBeInterrupted => false; // 恢复过程不能被打断

        public async UniTask EnterAsync(AIWoodBlock block)
        {
            // 如果需要，触发警告效果
            if (this._alertOnEnter)
            {
                block.Alert();
            }

            Tween moveTween = block.transform.DOMove(this._targetPosition, 0.3f);
            Tween rotateTween = block.transform.DORotateQuaternion(this._targetRotation, 0.3f);

            // 执行恢复动画
            await UniTask.WhenAll(moveTween.ToUniTask(), rotateTween.ToUniTask());

            // 恢复完成后，自动切换回空闲状态
            await block.SetStateAsync(IdleState.Instance);
        }

        public UniTask ExitAsync(AIWoodBlock block)
        {
            // 恢复状态的退出不需要特殊处理
            return UniTask.CompletedTask;
        }
    }
}