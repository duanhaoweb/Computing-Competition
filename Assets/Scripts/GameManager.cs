using BlockSystem.Implementation;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using QFramework;
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
        ResKit.Init();
        this.woodBlockController.transform.position = new Vector3(0, -2.8f, 0);
        this.woodBlockController.Disable();
        this.RotateObject();
        this.tutorialManager.StartTutorial();
        this.tutorialManager.OnTutorialComplete += this.OnTutorialComplete;
    }

    private async UniTaskVoid OnTutorialComplete()
    {
        this.tutorialManager.OnTutorialComplete -= this.OnTutorialComplete;
        await this.StopRotation();
        this.woodBlockController.onSuccess.AddListener(this.GameComplete);
        this.woodBlockController.Enable();
    }

    private void GameComplete()
    {
        this.woodBlockController.Disable();
        this.woodBlockController.transform.DOMoveY(-2.8f, 0.5f).onComplete += this.RotateObject;
        this.tutorialManager.ShowGameComplete(Application.Quit);
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

    private async UniTask StopRotation()
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

        Tween tween1 = this.woodBlockController.transform.DOLocalMoveY(0, 0.5f);
        Tween tween2 = this.woodBlockController.transform.DOLocalRotate(new Vector3(0, 0, 0), 0.5f);
        // 等待所有动画完成
        await UniTask.WhenAll(tween1.ToUniTask(), tween2.ToUniTask());
    }
}