using System;
using BlockSystem.Abstractions;
using UnityEngine;
using UnityEngine.EventSystems;

namespace View
{
    public class RawImageRayCast : MonoBehaviour, IPointerMoveHandler, IPointerDownHandler, IPointerUpHandler
    {
        public Camera previewCamera; // 渲染 RenderTexture 的相机

        public Camera uiCamera; // UI 相机

        public RectTransform RectTransform { get; private set; }

        public float Width => this.RectTransform.rect.width;

        public float Height => this.RectTransform.rect.height;

        private InteractiveModel _currentInteractiveModel = null; // 当前鼠标悬停的物体

        /// <summary>
        /// 开始拖拽事件, 参数为鼠标位置, 是否为鼠标左键
        /// </summary>
        public event Action<Vector2, bool> OnBeginDragEvent;

        public event Action<Vector2> OnDragEvent;
        public event Action<Vector2> OnEndDragEvent;

        public event Action<Vector2> OnClickNullEvent;

        public const float DragThreshold = 0.1f; // 拖拽的阈值

        public static Vector2 YAxis = new(0, 1);
        public static Vector2 XAxis = new(0.894427191f, 0.447213596f);
        public static Vector2 ZAxis = new(-0.894427191f, 0.447213596f);

        public Vector2 ScreenMousePosition { get; private set; }
        private bool _isDragging;

        private void Start()
        {
            this.RectTransform = this.GetComponent<RectTransform>();
            this.r = 16f;
            this.theta = Mathf.PI / 3f;
            this.phi = Mathf.PI / 4f;
            this.UpdateSphericalCoordinates();
        }

        private void OnDisable()
        {
            // 如果之前有悬停的物体，调用 OnExit
            this._currentInteractiveModel?.OnExit();
        }

        public void ChangePreviewCamera(float deltaR = 0, float deltaTheta = 0, float deltaPhi = 0, float deltaSize = 0)
        {
            this.r += deltaR;
            if (this.r < 1f)
                this.r = 1f;
            this.theta += deltaTheta;
            if (this.theta < 0f)
                this.theta = 0f;
            if (this.theta > Mathf.PI)
                this.theta = Mathf.PI;
            this.phi += deltaPhi;
            if (this.phi < 0f)
                this.phi += 2f * Mathf.PI;
            if (this.phi > 2f * Mathf.PI)
                this.phi -= 2f * Mathf.PI;
            this.size += deltaSize;
            if (this.size < 2.5f)
                this.size = 2.5f;
            if (this.size > 10f)
                this.size = 10f;
            this.UpdateSphericalCoordinates();
        }

        [Header("球坐标系参数")] [SerializeField] private float r = 16f; // 半径
        [SerializeField] private float theta = 0f; // 极角 (0 到 π)
        [SerializeField] private float phi = 0f; // 方位角 (0 到 2π)
        [SerializeField] private float size = 5f; // 视口大小

        private void UpdateSphericalCoordinates()
        {
            // 将球坐标系转换为笛卡尔坐标系
            float x = this.r * Mathf.Sin(this.theta) * Mathf.Cos(this.phi);
            float y = this.r * Mathf.Cos(this.theta);
            float z = this.r * Mathf.Sin(this.theta) * Mathf.Sin(this.phi);

            // 更新摄像机位置
            this.previewCamera.transform.position = new Vector3(x, y, z);

            // 更新摄像机的视口大小
            this.previewCamera.orthographicSize = this.size;

            // 计算摄像机朝向
            Vector3 forward = -this.previewCamera.transform.position.normalized;

            // 计算上方向，避免在极点处发生翻转
            Vector3 up;
            if (Mathf.Abs(this.theta) < 0.001f)
            {
                up = new Vector3(-Mathf.Cos(this.phi), 0, -Mathf.Sin(this.phi));
            }
            else if (Mathf.Abs(this.theta - Mathf.PI) < 0.001f)
            {
                up = new Vector3(Mathf.Cos(this.phi), 0, Mathf.Sin(this.phi));
            }
            else
            {
                // 在其他位置使用世界Y轴作为参考
                up = Vector3.up;
            }

            // 设置摄像机旋转
            this.previewCamera.transform.rotation = Quaternion.LookRotation(forward, up);

            // 更新坐标轴
            XAxis = this.MapToWorldToScreen(new Vector3(1, 0, 0));
            YAxis = this.MapToWorldToScreen(new Vector3(0, 1, 0));
            ZAxis = this.MapToWorldToScreen(new Vector3(0, 0, 1));
        }

        private Vector2 MapToWorldToScreen(Vector3 worldVector)
        {
            // 将世界坐标系的点转换为屏幕坐标系的点
            Vector2 screenPoint = this.previewCamera.WorldToScreenPoint(worldVector);

            // 屏幕中心是 (0, 0)，所以需要将屏幕坐标系的点相对于屏幕中心进行偏移
            Vector2 screenCenter = new Vector3(this.previewCamera.targetTexture.width / 2f,
                this.previewCamera.targetTexture.height / 2f);
            Vector2 relativeScreenPoint = screenPoint - screenCenter;
            return relativeScreenPoint;
        }


        public bool IsDragging
        {
            get => this._isDragging;
            set
            {
                if (this._isDragging == value) return;
                this._isDragging = value;
                if (this._isDragging)
                    this.OnBeginDragEvent?.Invoke(this.ScreenMousePosition, Input.GetMouseButton(0));
                else
                    this.OnEndDragEvent?.Invoke(this.ScreenMousePosition);
            }
        }

        private float _lastDownTime;
        private float _lastUpTime;

        public void OnPointerDown(PointerEventData eventData)
        {
            this._lastDownTime = Time.time;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            this._lastUpTime = Time.time;
            if (!(this._lastUpTime - this._lastDownTime < DragThreshold)) return;
            if (eventData.button == PointerEventData.InputButton.Left &&
                this.TryGetInteractiveModel(out InteractiveModel model))
            {
                model.OnClick();
            }
            else
            {
                this.OnClickNullEvent?.Invoke(this.ScreenMousePosition);
            }
        }

        private void Update()
        {
            this.ScreenMousePosition = Input.mousePosition;
            this.IsDragging =
                this._lastDownTime > this._lastUpTime && Time.time - this._lastDownTime > DragThreshold;
            //if (this.IsDragging) this.OnDragEvent?.Invoke(this.ScreenMousePosition);
        }

        // 处理鼠标移动事件
        public void OnPointerMove(PointerEventData eventData)
        {
            if (this.TryGetInteractiveModel(out InteractiveModel model))
            {
                // 如果鼠标悬停的物体发生变化
                if (this._currentInteractiveModel != model)
                {
                    // 如果之前有悬停的物体，调用 OnExit
                    this._currentInteractiveModel?.OnExit();
                    // 更新当前悬停的物体
                    this._currentInteractiveModel = model;
                    // 调用新悬停物体的 OnEnter
                    this._currentInteractiveModel?.OnEnter();
                }
            }
            else
            {
                // 如果鼠标不再悬停在任何物体上
                this._currentInteractiveModel?.OnExit();
                this._currentInteractiveModel = null;
            }
        }

        public bool TryGetInteractiveModel(out InteractiveModel model)
        {
            model = null;

            if (this.ScreenPointToLocalPointInRectangle(this.ScreenMousePosition, out Vector2 clickPos))
            {
                float x = clickPos.x / this.RectTransform.rect.width;
                float y = clickPos.y / this.RectTransform.rect.height;

                Ray ray = this.previewCamera.ViewportPointToRay(new Vector2(x + 0.5f, y + 0.5f));
                if (Physics.Raycast(ray, out RaycastHit hit))
                    if (hit.collider.gameObject.TryGetComponent<InteractiveModel>(out model) ||
                        hit.collider.gameObject.transform.parent.gameObject
                            .TryGetComponent<InteractiveModel>(out model))
                        return true;
            }

            return false;
        }

        public bool ScreenPointToLocalPointInRectangle(Vector2 screenPoint, out Vector2 localPoint)
        {
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(this.RectTransform, screenPoint,
                this.uiCamera,
                out localPoint);
        }

        public Vector3 LocalPointToScreenPoint(Vector2 localPoint)
        {
            // 将局部点转换为世界点
            Vector3 worldPoint = this.RectTransform.TransformPoint(localPoint);
            // 将世界点转换为屏幕点
            return this.uiCamera.WorldToScreenPoint(worldPoint);
        }

        public Vector2 WorldPointToScreenPoint(Vector3 worldPoint)
        {
            // 第一步: 将世界坐标转换为 previewCamera 的视口坐标
            Vector3 viewportPoint = this.previewCamera.WorldToViewportPoint(worldPoint);
            // x + 0.5f, y + 0.5f 的操作反向
            float x = viewportPoint.x - 0.5f;
            float y = viewportPoint.y - 0.5f;

            // 获取 RectTransform 的宽高
            RectTransform rectTransform = this.GetComponent<RectTransform>();
            float width = rectTransform.rect.width;
            float height = rectTransform.rect.height;

            // 第二步: 将视口坐标转换为本地坐标
            Vector2 localPoint = new Vector2(x * width, y * height);

            // 第三步: 将本地坐标转换为屏幕坐标
            return this.LocalPointToScreenPoint(localPoint);
        }
    }
}