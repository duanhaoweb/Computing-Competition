using DG.Tweening;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private WoodBlockController woodBlockController;

    [SerializeField] private float rotationDuration = 10f; // X 轴旋转一周所需的时间

    private Tweener _rotationTweener; // 新增旋转动画引用
    private Tweener _positionTweener; // 新增位置动画引用

    [SerializeField] private TutorialManager tutorialManager;


    private void Start()
    {
        this.woodBlockController.Disable();
        this.woodBlockController.transform.position = new Vector3(0, -2.8f, 0);
        this.RotateObject();
        this.tutorialManager.OnTutorialComplete += this.OnTutorialComplete;
    }

    private void OnTutorialComplete()
    {
        this.StopRotation();
        this.woodBlockController.Enable();
    }

    private void OnDestroy()
    {
        this.tutorialManager.OnTutorialComplete -= this.OnTutorialComplete;
    }

    private void RotateObject()
    {
        // 仅保留Y轴无限旋转（末影水晶主要特征）
        this._rotationTweener = this.woodBlockController.transform.DOLocalRotate(new Vector3(0, 360, 360),
                this.rotationDuration, RotateMode.LocalAxisAdd)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);

        // 添加上下浮动效果（可选）
        this._positionTweener = this.woodBlockController.transform.DOMoveY(3f, 3f)
            .SetEase(Ease.InOutQuad)
            .SetLoops(-1, LoopType.Yoyo)
            .SetRelative(true);
    }

    public void StopRotation()
    {
        // 停止旋转动画
        if (this._rotationTweener != null && this._rotationTweener.IsActive())
        {
            this._rotationTweener.Kill();
            this._rotationTweener = null;
        }

        // 停止位置动画
        if (this._positionTweener != null && this._positionTweener.IsActive())
        {
            this._positionTweener.Kill();
            this._positionTweener = null;
        }

        // 重置到初始状态（可选）
        this.woodBlockController.transform.DOLocalMoveY(0, 0.5f);
        this.woodBlockController.transform.DOLocalRotate(Vector3.zero, 0.5f);
    }
}