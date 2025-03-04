using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    // Start is called before the first frame update
    Rigidbody2D rb;
    public float jumpForce;
    int jumpTimes;
    public gameManager gameManager;
    
    void Start()
    {
        rb= GetComponent<Rigidbody2D>();
        jumpTimes= 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space)&&jumpTimes<2) {
        rb.velocity =new Vector2(0,jumpForce);
            jumpTimes++;
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        jumpTimes = 0;
        if (collision.gameObject.tag == "Barrier")
        {
            gameManager.GameOver();
        }
    }
}
