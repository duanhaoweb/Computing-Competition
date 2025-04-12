using UnityEngine;

namespace View
{
    public class HasShadow : MonoBehaviour
    {
        [SerializeField] private Direction direction;
        public Direction Direction => this.direction;
        private GameObject _shadow;


        private Vector3 GetForward()
        {
            if (this.direction == Direction.XAxis)
                return this.transform.right;
            else if (this.direction == Direction.YAxis)
                return this.transform.up;
            else
                return this.transform.forward;
        }

        private Vector3 GetShadowScale()
        {
            Vector3 forward = this.GetForward();
            float x = Mathf.Abs(Vector3.Dot(forward, Vector3.right));
            float z = Mathf.Abs(Vector3.Dot(forward, Vector3.forward));
            float xScale = Mathf.Lerp(0.105f, 0.505f, x);
            float zScale = Mathf.Lerp(0.105f, 0.505f, z);
            return new Vector3(xScale, 1, zScale);
        }

        private void OnEnable()
        {
            this._shadow = ShadowController.Instance.GetShadow();
            this._shadow.SetActive(true);
            this._shadow.transform.localScale = this.GetShadowScale();
            this._shadow.transform.position = this.transform.position;
        }

        private void Update()
        {
            this._shadow.transform.position = new Vector3(this.transform.position.x, -0.1f, this.transform.position.z);
            this._shadow.transform.localScale = this.GetShadowScale();
        }

        private void OnDisable()
        {
            if (this._shadow == null) return;
            this._shadow.SetActive(false);
        }
    }

    public enum Direction
    {
        XAxis,
        YAxis,
        ZAxis
    }
}