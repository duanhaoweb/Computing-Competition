using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;
using Cysharp.Threading.Tasks;
using QFramework;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private Material blurMaterial;
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private Button nextButton;

    private int _currentStep = 0;

    private readonly string[] _tutorialSteps =
    {
        "欢迎来到游戏!\n试着在指引下将木块拼装为鲁班锁吧!",
        "鼠标左键长按并拖动木块, 即可移动木块\n使用R/T/F键, 可以旋转木块",
        "鼠标右键拖动(或在空白处使用鼠标左键拖动)\n可以旋转视角" +
        "滑动鼠标滚轮, 可以缩放视角",
        "使用Z键, 可以撤回您的操作",
        "您可以点击右上角的按钮再次查看游戏说明"
    };

    private static readonly int Intensity = Shader.PropertyToID("_Intensity");

    public event Func<UniTaskVoid> OnTutorialComplete;

    private void ShowCurrentStep()
    {
        if (this.tutorialText != null)
        {
            this.tutorialText.text = this._tutorialSteps[this._currentStep];
            // 添加文字动画效果
            this.tutorialText.DOFade(0, 0);
            this.tutorialText.DOFade(1, 0.7f);
            this.blurMaterial.SetFloat(Intensity, 0.5f);
            ActionKit.Lerp(0.5f, 1.0f, 0.4f, t => { this.blurMaterial.SetFloat(Intensity, t); }).StartGlobal();
        }
    }

    public void ShowGameComplete(Action exit)
    {
        this.nextButton.onClick.AddListener(() =>
        {
            this.CompleteTutorial();
            exit?.Invoke();
        });
        this.tutorialPanel.SetActive(true);
        this.tutorialText.text = "恭喜您完成了游戏!";
        this.tutorialText.DOFade(0, 0);
        this.tutorialText.DOFade(1, 0.7f);
        this.blurMaterial.SetFloat(Intensity, 0.5f);
        ActionKit.Lerp(0.5f, 1.0f, 0.4f, t => { this.blurMaterial.SetFloat(Intensity, t); }).StartGlobal();
    }

    private void NextStep()
    {
        this._currentStep++;

        if (this._currentStep >= this._tutorialSteps.Length)
        {
            this.CompleteTutorial();
            return;
        }

        this.ShowCurrentStep();
    }

    public void StartTutorial()
    {
        this.tutorialPanel.SetActive(true);
        this._currentStep = 0;
        this.nextButton.onClick.AddListener(this.NextStep);
        this.ShowCurrentStep();
    }

    private void CompleteTutorial()
    {
        if (this.tutorialPanel != null)
        {
            this.tutorialText.DOFade(0, 0.5f);
            ActionKit.Sequence().Lerp(1.0f, 0.0f, 0.4f, t => { this.blurMaterial.SetFloat(Intensity, t); })
                .Callback(() => { this.tutorialPanel.SetActive(false); }).StartGlobal();
        }

        this.nextButton.onClick.RemoveListener(this.NextStep);
        this.OnTutorialComplete?.Invoke();
    }
}