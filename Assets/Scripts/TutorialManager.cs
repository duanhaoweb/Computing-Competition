using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;
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
        "欢迎来到游戏!\n点击鼠标左键或按下空格键来旋转木块",
        "使用A/D键或方向键来移动木块",
        "目标是将木块完美放置到合适的位置 \n准备好了吗?"
    };

    private static readonly int Intensity = Shader.PropertyToID("_Intensity");

    public event Action OnTutorialComplete;

    private void Start()
    {
        if (this.nextButton != null) this.nextButton.onClick.AddListener(this.NextStep);
        this.ShowCurrentStep();
    }

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

    private void CompleteTutorial()
    {
        if (this.tutorialPanel != null)
        {
            this.tutorialText.DOFade(0, 0.5f);
            ActionKit.Sequence().Lerp(1.0f, 0.0f, 0.4f, t => { this.blurMaterial.SetFloat(Intensity, t); })
                .Callback(() => { this.tutorialPanel.SetActive(false); }).StartGlobal();
        }

        this.OnTutorialComplete?.Invoke();
    }
}