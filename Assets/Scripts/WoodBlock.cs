using DG.Tweening;
using QuickOutline.Scripts;
using QFramework;
using UnityEngine;

[RequireComponent(typeof(InteractiveModel))]
[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(HasShadow))]
public class WoodBlock : MonoBehaviour
{
    private InteractiveModel _interactiveModel;
    private Rigidbody _rb;
    private Material _material;
    private HasShadow _hasShadow;

    private bool _isSelected;
    private bool _isRestoring; // 是否正在恢复, 恢复过程不相应任何碰撞

    public bool IsSelected
    {
        get => this._isSelected;
        set
        {
            if (this._isSelected == value) return;
            this._isSelected = value;
            if (this._isSelected)
            {
                AddOutline(this.gameObject);
                this._hasShadow.enabled = true;
            }
            else
            {
                RemoveOutline(this.gameObject);
                this._hasShadow.enabled = false;
            }
        }
    }

    public int Index { get; set; }
    public readonly EasyEvent<int> OnClickEvent = new EasyEvent<int>();
    private readonly EasyEvent _onHit = new EasyEvent();
    private Tween _currentTween;
    private static readonly int HighLight = Shader.PropertyToID("_HighLight");
    private static readonly int Color = Shader.PropertyToID("_Color");
    public bool IsAnimating => this._currentTween != null;

    private void Start()
    {
        this._hasShadow = this.GetComponent<HasShadow>();
        this._hasShadow.enabled = false;
        this._interactiveModel = this.GetComponent<InteractiveModel>();
        this._material = this.GetComponent<Renderer>().material;
        this._rb = this.GetComponent<Rigidbody>();
        this._interactiveModel.onClick.AddListener(this.OnClick);
        this._interactiveModel.onExit.AddListener(this.OnExit);
        this._interactiveModel.onEnter.AddListener(this.OnEnter);
        this._onHit.Register(this.Alarm);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (this._currentTween == null || this._isRestoring) return;
        this._onHit.Trigger();
    }

    public Tween DirectMove(Vector3 target)
    {
        return this.transform.DOMove(target, 0.2f);
    }

    public Tween DirectRotate(Quaternion target)
    {
        return this.transform.DORotateQuaternion(target, 0.2f);
    }

    public void TryMove(UnitOperation move)
    {
        if (this._currentTween != null) return;

        this._originalPos = this.transform.position;
        this._originalRot = this.transform.rotation;
        var target = move.GetTargetPosition(this._originalPos);
        this._onHit.Register(this.Restore);
        this._currentTween = this.transform.DOMove(target, 0.15f).OnComplete(() =>
        {
            this._onHit.UnRegister(this.Restore);
            this._currentTween = null;
            move.success = true;
        });
    }

    private Vector3 _originalPos;
    private Quaternion _originalRot;


    public void TryRotate(UnitOperation rotate)
    {
        if (this._currentTween != null) return;

        this._originalPos = this.transform.position;
        this._originalRot = this.transform.rotation;
        var target = rotate.GetTargetQuaternion(this._originalRot);
        // 旋转
        this._onHit.Register(this.Restore);
        this._currentTween =
            this.transform.DORotateQuaternion(target, 0.2f).OnComplete(() =>
            {
                this._onHit.UnRegister(this.Restore);
                this._currentTween = null;
                rotate.success = true;
            });
    }

    private void Restore()
    {
        this._onHit.UnRegister(this.Restore);
        this._isRestoring = true;
        this._currentTween?.Kill();

        //恢复后再加0.2秒的停滞时间
        this._currentTween = this.transform.DOMove(this._originalPos, 0.3f);
        this.transform.DORotateQuaternion(this._originalRot, 0.3f);
        this._currentTween.OnComplete(() =>
        {
            this._currentTween = null;
            if (this.IsSelected) this._isRestoring = false;
        });
    }

    public void Select(bool isSelect)
    {
        this.IsSelected = isSelect;
    }

    private void OnClick()
    {
        this.OnClickEvent.Trigger(this.Index);
    }

    private void OnEnter()
    {
        this._material.SetFloat(HighLight, 1f);
    }

    private void OnExit()
    {
        this._material.SetFloat(HighLight, 0f);
    }

    private void ColorLerp(float t)
    {
        this._material.SetColor(Color,
            UnityEngine.Color.Lerp(UnityEngine.Color.red, UnityEngine.Color.white, t));
    }

    private void Alarm()
    {
        ActionKit.Sequence()
            .Lerp(0, 1, 0.2f, this.ColorLerp)
            .Start(this);
    }

    private static void AddOutline(GameObject go)
    {
        if (go.TryGetComponent(out Outline o))
        {
            o.enabled = true;
        }
        else
        {
            var outline = go.AddComponent<Outline>();
            outline.OutlineMode = Outline.Mode.OutlineAll;
            outline.OutlineColor = ColorHelper.HexToColor("#5ee7df");
            outline.OutlineSubColor = ColorHelper.HexToColor("#b490ca");
            outline.OutlineWidth = 9f;
        }
    }

    private static void RemoveOutline(GameObject go)
    {
        if (go.TryGetComponent(out Outline o)) o.enabled = false;
    }

    private void OnDestroy()
    {
        this._interactiveModel.onClick.RemoveListener(this.OnClick);
        this._interactiveModel.onExit.RemoveListener(this.OnExit);
        this._interactiveModel.onEnter.RemoveListener(this.OnEnter);
        this._onHit.UnRegister(this.Restore);
        this._onHit.UnRegister(this.Alarm);
        this._currentTween?.Kill();
    }
}