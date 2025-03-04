using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//游戏入口脚本
public class GameApp : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //初始化音频管理器
        AudioManager.Instance.Init();

        //显示UI
        UIManager.Instance.ShowUI<CompassUI>("CompassUI");

        //播放BGM
        AudioManager.Instance.PlayBGM("Start");


    }

}
