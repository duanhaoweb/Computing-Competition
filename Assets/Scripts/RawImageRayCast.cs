using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class RawImageRayCast : MonoBehaviour, IPointerMoveHandler, IPointerDownHandler, IPointerUpHandler
{
    public Camera previewCamera; // 渲染 RenderTexture 的相机

    // 渲染 RenderTexture 的相机的位置和旋转, 第一个是位置, 第二个是旋转
    public int CurrentPreviewCameraIndex { get; private set; }

    public readonly (Vector3, Vector3)[] PreviewCameraTransforms = new[]
    {
        (new Vector3(-10, 8, -10), new Vector3(30, 45, 0)),
        (new Vector3(-10, 8, 10), new Vector3(30, 135, 0)),
        (new Vector3(10, 8, 10), new Vector3(30, -135, 0)),
        (new Vector3(10, 8, -10), new Vector3(30, -45, 0))
    };

    public Camera uiCamera; // UI 相机

    public RectTransform RectTransform { get; private set; }

    private InteractiveModel _currentInteractiveModel = null; // 当前鼠标悬停的物体

    public event Action<Vector2> OnBeginDragEvent;
    public event Action<Vector2> OnDragEvent;
    public event Action<Vector2> OnEndDragEvent;

    public static readonly Vector2 YAxis = new(0, 1);
    public static Vector2 XAxis = new(0.894427191f, 0.447213596f);
    public static Vector2 ZAxis = new(-0.894427191f, 0.447213596f);

    public Vector2 ScreenMousePosition { get; private set; }
    private bool _isDragging;

    private void Start()
    {
        this.RectTransform = this.GetComponent<RectTransform>();
        this.CurrentPreviewCameraIndex = 0;
        this.previewCamera.transform.position = this.PreviewCameraTransforms[this.CurrentPreviewCameraIndex].Item1;
        this.previewCamera.transform.rotation =
            Quaternion.Euler(this.PreviewCameraTransforms[this.CurrentPreviewCameraIndex].Item2);
    }

    public void ChangePreviewCamera()
    {
        this.CurrentPreviewCameraIndex = (this.CurrentPreviewCameraIndex + 1) % this.PreviewCameraTransforms.Length;
        this.StartCoroutine(this.RotatePreviewCamera());
    }

    //旋转协程
    private IEnumerator RotatePreviewCamera()
    {
        // 旋转角度
        var angle = 0f;
        // 旋转速度
        var speed = 400f;
        // 计算旋转轴
        var axis = Vector3.up;
        while (angle < 90f)
        {
            var deltaAngle = Time.deltaTime * speed;
            angle += deltaAngle;
            this.previewCamera.transform.RotateAround(Vector3.zero, axis, deltaAngle);
            yield return null;
        }

        //交换X轴和Z轴, 并取反
        (XAxis, ZAxis) = (ZAxis, -XAxis);
        this.previewCamera.transform.position = this.PreviewCameraTransforms[this.CurrentPreviewCameraIndex].Item1;
        this.previewCamera.transform.rotation =
            Quaternion.Euler(this.PreviewCameraTransforms[this.CurrentPreviewCameraIndex].Item2);
    }

    public bool IsDragging
    {
        get => this._isDragging;
        set
        {
            if (this._isDragging == value) return;
            this._isDragging = value;
            if (this._isDragging)
                this.OnBeginDragEvent?.Invoke(this.ScreenMousePosition);
            else
                this.OnEndDragEvent?.Invoke(this.ScreenMousePosition);
        }
    }

    private float _lastDownTime;
    private float _lastUpTime;

    public void OnPointerDown(PointerEventData eventData)
    {
        this._lastDownTime = Time.time;
        this.ScreenMousePosition = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        this._lastUpTime = Time.time;
        if (this._lastUpTime - this._lastDownTime < 0.2f && this.TryGetInteractiveModel(out var model)) model.OnClick();
    }

    private void Update()
    {
        this.IsDragging =
            this._lastDownTime > this._lastUpTime && Time.time - this._lastDownTime > 0.2f; // 0.2秒内按下且未抬起, 认为是拖拽
        if (this.IsDragging) this.OnDragEvent?.Invoke(this.ScreenMousePosition);
    }

    // 处理鼠标移动事件
    public void OnPointerMove(PointerEventData eventData)
    {
        this.ScreenMousePosition = eventData.position;
        if (this.TryGetInteractiveModel(out var model))
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


    private bool TryGetInteractiveModel(out InteractiveModel model)
    {
        model = null;

        if (this.ScreenPointToLocalPointInRectangle(this.ScreenMousePosition, out var clickPos))
        {
            var x = clickPos.x / this.RectTransform.rect.width;
            var y = clickPos.y / this.RectTransform.rect.height;

            var ray = this.previewCamera.ViewportPointToRay(new Vector2(x + 0.5f, y + 0.5f));
            if (Physics.Raycast(ray, out var hit))
                if (hit.collider.gameObject.TryGetComponent<InteractiveModel>(out model) ||
                    hit.collider.gameObject.transform.parent.gameObject.TryGetComponent<InteractiveModel>(out model))
                    return true;
        }

        return false;
    }

    public bool ScreenPointToLocalPointInRectangle(Vector2 screenPoint, out Vector2 localPoint)
    {
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(this.RectTransform, screenPoint, this.uiCamera,
            out localPoint);
    }

    public Vector3 LocalPointToScreenPoint(Vector2 localPoint)
    {
        // 将局部点转换为世界点
        var worldPoint = this.RectTransform.TransformPoint(localPoint);
        // 将世界点转换为屏幕点
        return this.uiCamera.WorldToScreenPoint(worldPoint);
    }

    public Vector2 WorldPointToScreenPoint(Vector3 worldPoint)
    {
        // 第一步: 将世界坐标转换为 previewCamera 的视口坐标
        var viewportPoint = this.previewCamera.WorldToViewportPoint(worldPoint);
        // x + 0.5f, y + 0.5f 的操作反向
        var x = viewportPoint.x - 0.5f;
        var y = viewportPoint.y - 0.5f;

        // 获取 RectTransform 的宽高
        var rectTransform = this.GetComponent<RectTransform>();
        var width = rectTransform.rect.width;
        var height = rectTransform.rect.height;

        // 第二步: 将视口坐标转换为本地坐标
        var localPoint = new Vector2(x * width, y * height);

        // 第三步: 将本地坐标转换为屏幕坐标
        return this.LocalPointToScreenPoint(localPoint);
    }
}