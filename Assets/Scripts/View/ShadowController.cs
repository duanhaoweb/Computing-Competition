using UnityEngine;

namespace View
{
    public class ShadowController : MonoBehaviour
    {
        [SerializeField] private GameObject shadow;
        public static ShadowController Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }

            Instance = this;
        }

        public GameObject GetShadow()
        {
            return this.shadow;
        }

        private void OnDestroy()
        {
            Instance = null;
        }
    }
}