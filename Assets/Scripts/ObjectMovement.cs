using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


public class ObjectMovement : MonoBehaviour
{
    float moveSpeed=10;
    float startPos=24;
    float endPos=-24;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position=new Vector2(transform.position.x-moveSpeed*Time.deltaTime, transform.position.y);
        if(transform.position.x <= endPos) {
            if (gameObject.tag == "Barrier")
            {
                
                Destroy(gameObject);
            }
            transform.position = new Vector2(startPos,transform.position.y);
            
        }
    }
}
