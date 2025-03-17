using System.Collections;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Newtonsoft.Json;
using System;
using UnityEditor;

public class VNManager : MonoBehaviour
{
    #region Variables
    public GameObject gamePanel;
    public GameObject dialogueBox;
    public TextMeshProUGUI SpeakerName;
    public TextMeshProUGUI SpeakingContent;
    public TypewriterEffect typewriterEffect;
    public ScenesShoot screenShotter;
    
    public Image avaterImage;
    public AudioSource vocalAudio;
    public Image backgroundImage;
    public AudioSource backgroundMusic;
    public Image CharacterImage1;
    public Image CharacterImage2;

    public GameObject choicePanel;
    public Button choiceButton1;
    public Button choiceButton2;

    public GameObject bottomButtons;
    public Button autoButton;
    public Button skipButton;
    public Button saveButton;
    public Button loadButton;
    public Button historyButton;
    public Button settingButton;
    public Button homeButton;
    public Button closeButton;

    private readonly string storyPath = Constants.STORY_PATH;
    private readonly string defaultStoryFileName = Constants.DEFAULT_STORY_FILE_NAME;
    private readonly int defaultStartLine = Constants.DEFAULT_START_LINE;
    private readonly string excelFileExtension = Constants.EXCEL_FILE_EXTENSION;

    private string saveFolderPath;
    private byte[] screenshotData;//保存金额图数据
    private string currentSpeakingContent;//保存对话内容
    
    private List<ExcelReader.ExcelData> storydata;
    private int currentLine;
    private string currentStoryFileName;
    private float currentTypingSpeed = Constants.DEFAULT_TYPING_SECONDS;
    
    private bool isAutoPlay = false;
    private bool isSkip = false;
    private bool isLoad = false;
    private int maxReachedLineIndex = 0;
    private Dictionary<string, int> globalMaxReachedLineIndices = new Dictionary<string, int>();
    private LinkedList<string> historyRecords = new LinkedList<string>();
    public static VNManager Instance { get; private set; }
    #endregion

    #region Lifecycle
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

    // Start is called before the first frame update
    void Start()
    {
        InitializeSaveFilePath();
        bottomButtonsAddListener();
    }
    // Update is called once per frame
    void Update()
    {
        if (!MenuManager.Instance.menuPanel.activeSelf &&
            !SaveLoadManager.Instance.saveLoadPanel.activeSelf &&
            !HistoryManager.Instance.historyScrollView.activeSelf &&
            !SettingManager.Instance.settingPanel.activeSelf && 
            gamePanel.activeSelf)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)) 
            {
                if (!dialogueBox.activeSelf)
                {
                    OpenUI();
                }
                else if (!IsHittingBottomButtons())
                { 
                        DisplayNextLine();
                }
            }
            if (Input.GetKeyDown(KeyCode.Escape)) 
            {
                if (dialogueBox.activeSelf)
                {
                    CloseUI();
                }
                else 
                {
                    OpenUI();
                }
            }
            if (Input.GetKeyDown(KeyCode.Escape)) 
            {
                if (dialogueBox.activeSelf)
                {
                    CloseUI();
                }
                else 
                {
                    OpenUI();
                }
            }
            if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl)) 
            {
                Debug.Log("按下Ctrl键");
                CtrlSkip();
            }
        }
    }

    #endregion

    #region Initialization
    void InitializeSaveFilePath() 
    {
        saveFolderPath = Path.Combine(Application.persistentDataPath,Constants.SAVE_FILE_PATH);
        if (!Directory.Exists(saveFolderPath)) 
        {
            Directory.CreateDirectory(saveFolderPath);
        }
    }

    void bottomButtonsAddListener()
    {
        //添加监听器
        autoButton.onClick.AddListener(OnAutoButtonClick);
        skipButton.onClick.AddListener(OnSkipButtonClick);
        saveButton.onClick.AddListener(OnSaveButtonClick);
        loadButton.onClick.AddListener(OnLoadButtonClick);
        homeButton.onClick.AddListener(OnHomeButtonClick);
        closeButton.onClick.AddListener(OnCloseButtonClick);
        historyButton.onClick.AddListener(OnHistoryButtonClick);
        settingButton.onClick.AddListener(OnSettingButtonClick);
    }

    public void StartGame()
    {
        InitializeAndLoadStory(defaultStoryFileName,defaultStartLine);
    }

    void InitializeAndLoadStory(string fileName,int lineNumber)
    {
        Initialize(lineNumber);
        LoadStoryFromFile(fileName);
        if (isLoad) 
        {
            RecoverLastBackgroundAndAction();
            isLoad = false;
        }
        DisplayNextLine();
    }
    void Initialize(int line)
    {
        currentLine = line;
        
        avaterImage.gameObject.SetActive(false);
        backgroundImage.gameObject.SetActive(false);

        CharacterImage1.gameObject.SetActive(false);
        CharacterImage2.gameObject.SetActive(false);
        
        choicePanel.SetActive(false);
    }
    void LoadStoryFromFile(string fileName)
    {
        currentStoryFileName = fileName;
        var path = storyPath + fileName + excelFileExtension;
        storydata = ExcelReader.ReadExcel(path);
        if (storydata == null || storydata.Count == 0)
        {
            Debug.LogError(Constants.NO_DATA_FOUND);
        }
        if (globalMaxReachedLineIndices.ContainsKey(currentStoryFileName))
        {
            maxReachedLineIndex = globalMaxReachedLineIndices[currentStoryFileName];
        }
        else
        {
            maxReachedLineIndex = 0;
            globalMaxReachedLineIndices[currentStoryFileName] = maxReachedLineIndex;
        }
    }
    #endregion

    #region Display
    void DisplayThisLine()
    {
        var data = storydata[currentLine];
        SpeakerName.text = data.Speaker;
        currentSpeakingContent = data.Content;
        typewriterEffect.StartTyping(currentSpeakingContent, currentTypingSpeed);

        RecordHistory(SpeakerName.text, currentSpeakingContent);
        if (NotNullNorEmpty(data.avaterImageFileName))
        {
            UpdateAvatarImage(data.avaterImageFileName);
        }
        else
        {
            avaterImage.gameObject.SetActive(false);
        }
        if (NotNullNorEmpty(data.vocalAudioFileName))
        {
            PlayVocalAudio(data.vocalAudioFileName);
        }
        if (NotNullNorEmpty(data.backgroundImageFileName))
        {
            UpdateBackgroundImage(data.backgroundImageFileName);
        }
        if (NotNullNorEmpty(data.backgroundMusicFileName))
        {
            PlayBackgroundMusic(data.backgroundMusicFileName);
        }
        if (NotNullNorEmpty(data.character1Action))
        {
            UpdateCharacterImage(data.character1Action, data.character1ImageFileName, CharacterImage1, data.coordinateX1);
        }
        if (NotNullNorEmpty(data.character2Action))
        {
            UpdateCharacterImage(data.character2Action, data.character2ImageFileName, CharacterImage2, data.coordinateX2);
        }
        currentLine++;
    }

    void DisplayNextLine()
    {
        if (currentLine > maxReachedLineIndex)
        {
            maxReachedLineIndex = currentLine;
            globalMaxReachedLineIndices[currentStoryFileName] = maxReachedLineIndex;
        }
        if (currentLine >= storydata.Count - 1)
        {
            if (isAutoPlay)
            {
                isAutoPlay = false;
                UpdateButtonImage(Constants.AUTO_OFF, autoButton);
            }
            if (storydata[currentLine].Speaker == Constants.END_OF_STORY)
            {
                Debug.Log(Constants.END_OF_STORY);
            }
            if (storydata[currentLine].Speaker == Constants.CHOICE)
            {
                ShowChoices();
            }
            return;
        }
        if (currentLine >= storydata.Count)
        {
            Debug.Log("end of story");
            return;
        }
        if (typewriterEffect.IsTyping())
        {
            typewriterEffect.CompleteLine();
        }
        else
        {
            DisplayThisLine();
        }
    }

    void RecordHistory(string speaker, string content) 
    {
        string historyRecord = speaker + Constants.COLON + content;
        if (historyRecords.Count >= Constants.MAX_LENGTH) 
        {
            historyRecords.RemoveFirst();
        }
        historyRecords.AddLast(historyRecord);
    }
    void RecoverLastBackgroundAndAction() 
    {
        var data = storydata[currentLine];
        if (NotNullNorEmpty(data.lastBackgroundImage)) 
        {
            UpdateBackgroundImage(data.lastBackgroundImage);
        }
        if (NotNullNorEmpty(data.lastBackgroundMusic)) 
        {
            PlayBackgroundMusic(data.lastBackgroundMusic);
        }
        if (data.character1Action != Constants.APPEAR_AT
            && NotNullNorEmpty(data.character1ImageFileName)) 
        {
            UpdateCharacterImage(Constants.APPEAR_AT, data.character1ImageFileName,
                CharacterImage1, data.lastCoordinateX1);
        }
        if (data.character2Action != Constants.APPEAR_AT
            && NotNullNorEmpty(data.character2ImageFileName))
        {
            UpdateCharacterImage(Constants.APPEAR_AT, data.character2ImageFileName,
                CharacterImage2, data.lastCoordinateX2);
        }
    }

    bool NotNullNorEmpty(string str)
    {
        return !string.IsNullOrEmpty(str);
    }

    #endregion

    #region Choices
    void ShowChoices()
    {
        var data = storydata[currentLine];
        choiceButton1.onClick.RemoveAllListeners();
        choiceButton2.onClick.RemoveAllListeners();
        choicePanel.SetActive(true);
        choiceButton1.GetComponentInChildren<TextMeshProUGUI>().text = data.Content;
        choiceButton1.onClick.AddListener(() => InitializeAndLoadStory(data.avaterImageFileName,defaultStartLine));
        choiceButton2.GetComponentInChildren<TextMeshProUGUI>().text = data.vocalAudioFileName;
        choiceButton2.onClick.AddListener(() => InitializeAndLoadStory(data.backgroundImageFileName, defaultStartLine));
    }
    #endregion

    #region Audio
    void PlayVocalAudio(string audioFileName)
    {
        string audioPath = Constants.VOCAL_PATH + audioFileName;
        PlayAudio(audioPath, vocalAudio, false);
    }

    void PlayBackgroundMusic(string musicFileName)
    {
        string musicPath = Constants.MUSIC_PATH + musicFileName;
        PlayAudio(musicPath, backgroundMusic, true);
    }

    void PlayAudio(string audioPath, AudioSource audioSource, bool isloop)
    {
        AudioClip audioClip = Resources.Load<AudioClip>(audioPath);
        if (audioClip != null)
        {
            audioSource.clip = audioClip;
            audioSource.Play();
            audioSource.loop = isloop;
        }
        else
        {
            if (audioSource == vocalAudio)
            {
                Debug.LogError(Constants.AUDIO_LOAD_FAILED + audioPath);
            }
            else if (audioSource == backgroundMusic)
            {
                Debug.LogError(Constants.AUDIO_LOAD_FAILED + audioPath);
            }
        }
    }
    #endregion

    #region Image
    void UpdateButtonImage(string imageFileName, Button button)
    {
        string imagePath = Constants.BUTTON_PATH + imageFileName;
        UpdateImage(imagePath, button.image);
    }

    void UpdateAvatarImage(string imageFileName)
    {
        var imagePath = Constants.AVATAR_PATH + imageFileName;
        UpdateImage(imagePath, avaterImage);
    }


    void UpdateBackgroundImage(string imageFileName)
    {
        string imagePath = Constants.BACKGROUND_PATH + imageFileName;
        UpdateImage(imagePath, backgroundImage);
    }


    void UpdateCharacterImage(string action, string imagefileName, Image characterImage, string x)
    {
        // 根据action执行对应的动画和操作
        if (action.StartsWith(Constants.APPEAR_AT))//解析apperat（x，y）动作并在（x，y）显示角色立绘
        {
            string imagePath = Constants.CHARACTER_PATH + imagefileName;
            if (NotNullNorEmpty(x))
            {
                UpdateImage(imagePath, characterImage);
                var newPosition = new Vector2(float.Parse(x), characterImage.rectTransform.anchoredPosition.y);
                characterImage.rectTransform.anchoredPosition = newPosition;
                characterImage.DOFade(1, (isLoad ? 0 : Constants.DURATION_TINE)).From(0);
            }
            else
            {
                Debug.LogError(Constants.COORDINATE_MISSING);
            }
        }
        else if (action == Constants.DISAPPEAR)//隐藏角色立绘，添加消失的动画效果
        {
            characterImage.DOFade(0, Constants.DURATION_TINE).OnComplete(() => characterImage.gameObject.SetActive(false));
        }
        else if (action.StartsWith(Constants.MOVE_TO)) //解析moveTo（x，y）动作并移动角色立绘到（x，y）位置
        {
            if (NotNullNorEmpty(x))
            {
                characterImage.rectTransform.DOAnchorPosX(float.Parse(x), Constants.DURATION_TINE);
            }
            else
            {
                Debug.LogError(Constants.COORDINATE_MISSING);
            }
        }
    }

    void UpdateImage(string imagePath, Image image)
    {
        Sprite sprite = Resources.Load<Sprite>(imagePath);
        if (sprite != null)
        {
            image.sprite = sprite;
            image.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError(Constants.IMAGE_LOAF_FAILED + imagePath);
        }
    }

    #endregion

    #region Button
    #region Button
    bool IsHittingBottomButtons()
    {
        return RectTransformUtility.RectangleContainsScreenPoint(
                bottomButtons.GetComponent<RectTransform>(),
                Input.mousePosition,
                Camera.main
            );
    }
    #endregion

    #region Auto
    void OnAutoButtonClick()
    {
        isAutoPlay = !isAutoPlay;
        UpdateButtonImage((isAutoPlay ? Constants.AUTO_ON : Constants.AUTO_OFF), autoButton);
        if (isAutoPlay)
        {
            StartCoroutine(StartAutoPlay());
        }
    }
    private IEnumerator StartAutoPlay()
    {
        while (isAutoPlay)
        {
            if (!typewriterEffect.IsTyping())
            {
                DisplayNextLine();
            }
            yield return new WaitForSeconds(Constants.DEFAULT_AUTO_WAITING_SECONDS);
        }
    }
    #endregion

    #region Skip
    void StartSkip()
    {
        isSkip = true;
        UpdateButtonImage(Constants.SKIP_ON, skipButton);
        currentTypingSpeed = Constants.SKIP_MODE_TYPING_SPEED;
        StartCoroutine(SkipToMaxReachedLine());
    }

    bool CanSkip()
    {
        return currentLine < maxReachedLineIndex;
    }
    void OnSkipButtonClick()
    {
        if (!isSkip && CanSkip())
        {
            StartSkip();
        }
        else if (isSkip)
        {
            StopCoroutine(SkipToMaxReachedLine());
            EndSkip();
        }
    }
    private IEnumerator SkipToMaxReachedLine()
    {
        while (isSkip)
        {
            if (CanSkip())
            {
                DisplayThisLine();
            }
            else
            {
                EndSkip();
            }
            yield return new WaitForSeconds(Constants.DEFAULT_SKIP_WAITING_SECONDS);
        }
    }

    void EndSkip()
    {
        isSkip = false;
        currentTypingSpeed = Constants.DEFAULT_TYPING_SECONDS;
        UpdateButtonImage(Constants.SKIP_OFF, skipButton);
    }

    void CtrlSkip() 
    {
        currentTypingSpeed = Constants.SKIP_MODE_TYPING_SPEED;
        StartCoroutine(SkipWhilePressingCtrl());
    }

    private IEnumerator SkipWhilePressingCtrl() 
    {
        while (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) 
        {
            DisplayNextLine();
            yield return new WaitForSeconds(Constants.DEFAULT_SKIP_WAITING_SECONDS);
        } 
    }
    #endregion

    #region Save
    void OnSaveButtonClick()
    {
        CloseUI();
        Texture2D screenshot = screenShotter.CaptureScreenshot();
        screenshotData = screenshot.EncodeToPNG();
        SaveLoadManager.Instance.ShowSavePanel(SaveGame);
        OpenUI();
    }

    void SaveGame(int slotIndex) 
    {
        var saveData = new SaveData 
        {
            saveStoryfileName = currentStoryFileName,
            savedLine = currentLine,
            saveSpeakingcontent = currentSpeakingContent,
            savedScreenshotData = screenshotData,
            saveHistoryRecords = historyRecords,
        };
        string savePath = Path.Combine(saveFolderPath, slotIndex + Constants.SAVE_FILE_EXTENSION);
        string json = JsonConvert.SerializeObject(saveData,Formatting.Indented);
        File.WriteAllText(savePath, json);
    }
    public class SaveData 
    {
        public string saveStoryfileName;
        public int savedLine;
        public string saveSpeakingcontent;
        public byte[] savedScreenshotData;
        public LinkedList<string> saveHistoryRecords;
    }

    #endregion

    #region Load
    void OnLoadButtonClick()
    {
        ShowLoadPanel(null);
    }

    public void ShowLoadPanel(Action action)
    {
        SaveLoadManager.Instance.ShowLoadPanel(LoadGame,action);
    } 

    void LoadGame(int slotIndex) 
    {
        string savePath = Path.Combine(saveFolderPath,slotIndex + Constants.SAVE_FILE_EXTENSION);
        if (File.Exists(savePath))
        {
            isLoad = true;
            string json = File.ReadAllText(savePath);
            var saveData = JsonConvert.DeserializeObject<SaveData>(json);
            historyRecords = saveData.saveHistoryRecords;
            historyRecords.RemoveLast();
            var lineNumber = saveData.savedLine - 1;
            InitializeAndLoadStory(saveData.saveStoryfileName,lineNumber);
        }
    }
    #endregion

    #region Setting
    void OnSettingButtonClick()
    {
        SettingManager.Instance.ShowSettingPanel();
        
    }
    #endregion
    #region Home
    void OnHomeButtonClick()
    {
        gamePanel.SetActive(false);
        MenuManager.Instance.menuPanel.SetActive(true);
    }
    #endregion

    #region Close
    void OnCloseButtonClick()
    {
        CloseUI();
    }
    void OpenUI()
    {
        dialogueBox.SetActive(true);
        bottomButtons.SetActive(true);
    }

    void CloseUI() 
    {
        dialogueBox.SetActive(false);
        bottomButtons.SetActive(false);
    }
    #endregion

    #region History
    void OnHistoryButtonClick() 
    {
        HistoryManager.Instance.ShowHistory(historyRecords);
    }

    #endregion
    #endregion
}
