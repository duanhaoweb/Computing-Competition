using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Unity.VisualScripting;
using Newtonsoft.Json;

public class SaveLoadManager : MonoBehaviour
{
    public GameObject saveLoadPanel;
    public TextMeshProUGUI panelTitle;
    public Button[] saveLoadButtons;
    public Button prevPageButton;
    public Button nextPageButton;
    public Button returnButton;

    private bool isSave;
    private int currentPage = Constants.DEFAULT_START_LINE;
    private readonly int slotsPerpage = Constants.SLOTS_PER_PAGE;
    private readonly int totalSlots = Constants.TOTAL_SLOTS;
    private System.Action<int> currentAction;
    private System.Action menuAction;

    public static SaveLoadManager Instance { get; private set; }
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
        prevPageButton.onClick.AddListener(PrevPage);
        nextPageButton.onClick.AddListener(NextPage);
        returnButton.onClick.AddListener(GoBack);
        saveLoadPanel.SetActive(false);
    }

    public void ShowSavePanel(System.Action<int> action) 
    {
        isSave = true;
        panelTitle.text = Constants.SAVE_GAME;
        currentAction = action;
        UpdateUI();
        saveLoadPanel.SetActive(true);
    }

    public void ShowLoadPanel(System.Action<int> action,System.Action menuAction)
    {
        isSave = false;
        panelTitle.text = Constants.LOAD_GAME;
        currentAction = action;
        this.menuAction = menuAction;
        UpdateUI();
        saveLoadPanel.SetActive(true);
    }

    private void UpdateUI()
    {
        for (int i = 0; i < slotsPerpage; ++i) 
        {
            int slotIndex = currentPage*slotsPerpage + i;
            if (slotIndex < totalSlots)
            {
                UpdateSaveLoadButtons(saveLoadButtons[i], slotIndex);
                LoadStorylineAndScreenshots(saveLoadButtons[i], slotIndex); ;
            }
            else 
            {
                saveLoadButtons[i].gameObject.SetActive(false);
            }
        }
    }
    private void UpdateSaveLoadButtons(Button button,int index)
    {
        button.gameObject.SetActive(true);
        button.interactable = true;

        var savePath = GenerateDataPath(index);
        var fileExists = File.Exists(savePath);

        if (!isSave && !fileExists)
        {
            button.interactable = false;
        }

        var textComponents = button.GetComponentsInChildren<TextMeshProUGUI>();
        textComponents[0].text = null;
        textComponents[1].text = (index+1)+Constants.COLON +Constants.EMPLY_SLOT;
        button.GetComponentInChildren<RawImage>().texture = null;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => OnButtonClick(button, index));



    }

    private void OnButtonClick(Button button ,int index)
    {
        menuAction?.Invoke();
        currentAction?.Invoke(index);
        if (isSave)
        {
            LoadStorylineAndScreenshots(button, index);
        }
        else 
        {
            GoBack();
        }
    }
    private void PrevPage() 
    {
        if (currentPage > 0) 
        {
            currentPage--;
           UpdateUI();
        }
    }

    private void NextPage() 
    {
        if ((currentPage + 1) * slotsPerpage < totalSlots) 
        {
            currentPage++;
            UpdateUI();
        }
    }

    private void GoBack() 
    {
        saveLoadPanel.SetActive(false);
    }

    private void LoadStorylineAndScreenshots(Button button,int index) 
    {
        var savePath = GenerateDataPath(index);
        if (File.Exists(savePath)) 
        {
            string json = File.ReadAllText(savePath);
            var saveData = JsonConvert.DeserializeObject<VNManager.SaveData>(json);
            if (saveData.savedScreenshotData != null) 
            {
                Texture2D screenshot = new Texture2D(2, 2);
                screenshot.LoadImage(saveData.savedScreenshotData);
                button.GetComponentInChildren<RawImage>().texture = screenshot;
            }
            if (saveData.saveSpeakingcontent != null) 
            {
                var textComponents = button.GetComponentsInChildren<TextMeshProUGUI>();//GetComponentInChildren仅可获得一个，加上s可以获取多个
                textComponents[0].text = saveData.saveSpeakingcontent;
                textComponents[1].text = File.GetLastWriteTime(savePath).ToString("G");
            }
        }
    }

    private string GenerateDataPath(int index) 
    {
        return Path.Combine(Application.persistentDataPath, Constants.SAVE_FILE_PATH, index + Constants.SAVE_FILE_EXTENSION);
    }
}
