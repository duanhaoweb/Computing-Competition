using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class gameManager : MonoBehaviour
{
    //public GameObject Barrier;
    public GameObject Barrier_SetPos;
    public float Barrier_Set_Time;
    float timer;
    float y;
    public GameObject GameOverScene;
    //public static gameManager instance;

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1.0f;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        
        if (timer >= Barrier_Set_Time)
        {
            
            
                System.Random rand = new System.Random();
                GameObject gameObject = new GameObject();
                switch (rand.Next(2))
                {
                    case 0:
                        gameObject = Resources.Load<GameObject>("Prefabs/Monster1");
                    y = 1.04f;
                        break;
                    case 1:
                        gameObject = Resources.Load<GameObject>("Prefabs/Monster2");
                    y = -0.315f;
                    if(rand.Next(4)==0){
                        Instantiate(Resources.Load<GameObject>("Prefabs/Monster3"), new Vector3(20, 6, 0), Barrier_SetPos.transform.rotation);
                    }
                        break;
                    
                        
                       
                    default:
                        break;
                }
                Instantiate(gameObject, new Vector3(20,y,0), Barrier_SetPos.transform.rotation);
                Debug.Log("scsc");
                timer = 0;
            
        }
    }
    public void GameOver()
    {
        Time.timeScale = 0;
        GameOverScene.SetActive(true);
    }
    public void Restart()
    {
        SceneManager.LoadScene("Scene1");
    }
}
