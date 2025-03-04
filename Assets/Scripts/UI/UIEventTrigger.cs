using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

//事件监听
public class UIEventTrigger : MonoBehaviour, IPointerClickHandler
{


    public Action<GameObject, PointerEventData> OnClick;
    public static UIEventTrigger Get (GameObject obj)
    {
        UIEventTrigger trigger = obj.GetComponent<UIEventTrigger>();
        if (trigger != null)
        {
            trigger =obj.GetComponent<UIEventTrigger>();

        }
        return trigger;
    }
    public void OnPointerClick(PointerEventData eventData)
    {

        if (OnClick != null)
        {
            OnClick(gameObject, eventData);
        }
        else
        {
            Debug.LogWarning("OnClick delegate is null. No action invoked.");
        }
    }

}
