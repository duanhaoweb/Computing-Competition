using System.Linq;
using UnityEngine;
using QuickOutline.Scripts;
using QFramework;
using DG.Tweening;
using Cysharp.Threading.Tasks;

namespace AI.BlockSystem
{
    [RequireComponent(typeof(InteractiveModel))]
    [RequireComponent(typeof(Renderer))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(HasShadow))]
    public class AIWoodBlock : MonoBehaviour
    {
        private InteractiveModel _interactiveModel;
        private Rigidbody _rb;
        private Material _material;
        private HasShadow _hasShadow;
        private IBlockState _currentState;

        /// <summary>
        /// 获取当前状态
        /// </summary>
        public IBlockState CurrentState => this._currentState;

        public int Index { get; set; }
        public readonly EasyEvent<int> OnClickEvent = new EasyEvent<int>();
        public readonly EasyEvent OnTriggerEnterEvent = new EasyEvent();

        private static readonly int HighLight = Shader.PropertyToID("_HighLight");
        private static readonly int Color = Shader.PropertyToID("_Color");

        private bool _isSelected;

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

        /// <summary>
        /// 当前状态是否允许接受命令
        /// </summary>
        public bool CanAcceptCommand => this._currentState?.CanAcceptCommand ?? true;

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

            // 设置初始状态为空闲状态
            this.SetStateAsync(IdleState.Instance).Forget();
        }

        private void OnDestroy()
        {
            this._interactiveModel.onClick.RemoveListener(this.OnClick);
            this._interactiveModel.onExit.RemoveListener(this.OnExit);
            this._interactiveModel.onEnter.RemoveListener(this.OnEnter);
        }

        public async UniTask SetStateAsync(IBlockState newState)
        {
            if (this._currentState != null)
                await this._currentState.ExitAsync(this);

            this._currentState = newState;

            if (this._currentState != null)
                await this._currentState.EnterAsync(this);
        }

        /// <summary>
        /// 异步执行命令
        /// </summary>
        public async UniTask<CommandResult> ExecuteCommandAsync(IBlockCommand command)
        {
            // 如果当前状态不允许接收命令，返回失败
            if (!this.CanAcceptCommand)
                return CommandResult.Failed("Current state does not accept commands");

            return await command.ExecuteAsync(this);
        }

        private void OnTriggerEnter(Collider other)
        {
            this.OnTriggerEnterEvent.Trigger();
        }

        public Vector3 Position => this.transform.position;
        public Quaternion Rotation => this.transform.rotation;

        private void OnClick()
        {
            if (this.CanAcceptCommand)
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

        public void Alert()
        {
            ActionKit.Sequence()
                .Lerp(0, 1, 0.2f, this.ColorLerp)
                .Start(this);
        }

        private void ColorLerp(float t)
        {
            this._material.SetColor(Color, UnityEngine.Color.Lerp(UnityEngine.Color.red, UnityEngine.Color.white, t));
        }

        private static readonly Color OutlineColor = ColorHelper.HexToColor("#5ee7df");
        private static readonly Color OutlineSubColor = ColorHelper.HexToColor("#b490ca");

        // ReSharper disable Unity.PerformanceAnalysis
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
                outline.OutlineColor = OutlineColor;
                outline.OutlineSubColor = OutlineSubColor;
                outline.OutlineWidth = 9f;
            }
        }

        private static void RemoveOutline(GameObject go)
        {
            if (go.TryGetComponent(out Outline o))
                o.enabled = false;
        }
    }
}