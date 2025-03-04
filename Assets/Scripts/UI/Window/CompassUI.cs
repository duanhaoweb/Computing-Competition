using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

// 战斗页面
public class CompassUI : UIBase
{

    private Text Step_TxT;
    private Image CompassImg;

    private void Awake()
    {


        // 绑定 UI 元素
        Step_TxT = FindText("Compass/Step");

        CompassImg = FindImage("Compass");


    }


    private void Start()
    {
        // 初始化 UI 显示
        UpdateStep();
    }

  

    // 更新力量提升显示
    public void UpdateStep()
    {


        Step_TxT.text = RoleManager.Instance.Step.ToString();
    }




    // 工具方法：绑定 Text
    private Text FindText(string path)
    {
        return transform.Find(path)?.GetComponent<Text>();
    }

    // 工具方法：绑定 Image
    private Image FindImage(string path)
    {
        return transform.Find(path)?.GetComponent<Image>();
    }


}
