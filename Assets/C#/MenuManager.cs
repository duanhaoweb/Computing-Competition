using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public GameObject menuPanel;
    public Button startButton;
    public Button continueButton;
    public Button loadButton;
    public Button settingButton;
    public Button quitButton;

    private bool hasStarted = false;
    public static MenuManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else 
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {

        MenuButtonsAddListener();
    }

    void MenuButtonsAddListener() 
    {
        startButton.onClick.AddListener(StartGame);
        continueButton.onClick.AddListener(ContinueGame);
        loadButton.onClick.AddListener(LoadGame);
    }

    private void StartGame()
    {
        hasStarted = true;
        VNManager.Instance.StartGame();
        menuPanel.SetActive(false);
        VNManager.Instance.gamePanel.SetActive(true);
    }

    private void ContinueGame() 
    {
        if (hasStarted) 
        {
            menuPanel.SetActive(false);
            VNManager.Instance.gamePanel.SetActive(true);
        }
    }

    private void LoadGame() 
    {
        VNManager.Instance.ShowLoadPanel(ShowGamePanel);
    }

    private void ShowGamePanel() 
    {
        menuPanel.SetActive(false);
        VNManager.Instance.gameObject.SetActive(true);
    }

    private void ShowSettingPanel() 
    {
        SettingManager.Instance.ShowSettingPanel();
    }

    private void QuitGame() 
    {
        Application.Quit();
    }
}
