using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//��Ϸ��ڽű�
public class GameApp : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //��ʼ����Ƶ������
        AudioManager.Instance.Init();

        //��ʾUI
        UIManager.Instance.ShowUI<CompassUI>("CompassUI");
        
        //����BGM
        AudioManager.Instance.PlayBGM("Start");


    }

}
