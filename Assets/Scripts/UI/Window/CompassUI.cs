using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

// ս��ҳ��
public class CompassUI : UIBase
{

    private Text Step_TxT;
    private Image CompassImg;

    private void Awake()
    {


        // �� UI Ԫ��
        Step_TxT = FindText("Compass/Step");

        CompassImg = FindImage("Compass");


    }


    private void Start()
    {
        // ��ʼ�� UI ��ʾ
        UpdateStep();
    }

  

    // ��������������ʾ
    public void UpdateStep()
    {


        Step_TxT.text = RoleManager.Instance.Step.ToString();
    }




    // ���߷������� Text
    private Text FindText(string path)
    {
        return transform.Find(path)?.GetComponent<Text>();
    }

    // ���߷������� Image
    private Image FindImage(string path)
    {
        return transform.Find(path)?.GetComponent<Image>();
    }


}
