using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HistoryManager : MonoBehaviour
{
    public Transform historyContent;
    public GameObject historyItemPrefab;
    public GameObject historyScrollView;
    public Button closeButton;

    private LinkedList<string> historyRecords;//保存历史记录

    public static HistoryManager Instance { get; private set; }

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
        historyScrollView.SetActive(false);
        closeButton.onClick.AddListener(CloseHistory);
    }

    public void ShowHistory(LinkedList<string> records) 
    {
        foreach (Transform child in historyContent) 
        {
            Destroy(child.gameObject);
        }
        historyRecords = records;
        LinkedListNode<string> currentNode = historyRecords.Last;
        while (currentNode != null) 
        {
            AddHistoryItem(currentNode.Value);
            currentNode = currentNode.Previous;
        }

        historyContent.GetComponent<RectTransform>().localPosition = Vector3.zero;
        historyScrollView.SetActive(true);//显示历史记录界面
    }
    public void CloseHistory() 
    {
        historyScrollView.SetActive(false);
    }

    private void AddHistoryItem(string text) 
    {
        GameObject historyitem = Instantiate(historyItemPrefab,historyContent);
        historyitem.GetComponentInChildren<TextMeshProUGUI>().text = text;
        historyitem.transform.SetAsFirstSibling();
    }
}
