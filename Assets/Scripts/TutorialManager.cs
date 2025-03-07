using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private Button nextButton;

    private int _currentStep = 0;

    private readonly string[] _tutorialSteps =
    {
        "欢迎来到游戏!\n点击鼠标左键或按下空格键来旋转木块。",
        "使用A/D键或方向键来移动木块。",
        "目标是将木块完美放置到合适的位置。\n准备好了吗?"
    };

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
            this.tutorialText.DOFade(1, 0.5f);
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
            this.tutorialPanel.SetActive(false);
        }

        OnTutorialComplete?.Invoke();
    }
}