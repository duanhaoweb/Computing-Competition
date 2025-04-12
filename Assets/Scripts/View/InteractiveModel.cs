using UnityEngine;
using UnityEngine.Events;

namespace View
{
    public class InteractiveModel : MonoBehaviour
    {
        public UnityEvent onClick;
        public UnityEvent onEnter;
        public UnityEvent onExit;

        public void OnClick()
        {
            this.onClick.Invoke();
        }

        public void OnEnter()
        {
            this.onEnter.Invoke();
        }

        public void OnExit()
        {
            this.onExit.Invoke();
        }
    }
}