using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoleManager 
{
    public static RoleManager Instance =new RoleManager();
    public int Step = 1;//步数

    public void Init()
    {

        CreatRole();


    }
    public void CreatRole()
    {
        GameObject obj = Object.Instantiate(Resources.Load("Model/Player")) as GameObject;//从资源路径加载对应的人物模型

        Player player = obj.AddComponent<Player>();//添加人物脚本

        obj.transform.position = new Vector3(-7,3, 6);//人物的位置
    }

}
