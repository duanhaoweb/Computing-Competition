using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoleManager 
{
    public static RoleManager Instance =new RoleManager();
    public int Step = 1;//����

    public void Init()
    {

        CreatRole();


    }
    public void CreatRole()
    {
        GameObject obj = Object.Instantiate(Resources.Load("Model/Player")) as GameObject;//����Դ·�����ض�Ӧ������ģ��

        Player player = obj.AddComponent<Player>();//�������ű�

        obj.transform.position = new Vector3(-7,3, 6);//�����λ��
    }

}
