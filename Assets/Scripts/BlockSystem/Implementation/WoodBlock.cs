using System.Threading;
using BlockSystem.Abstractions;
using BlockSystem.States;
using Cysharp.Threading.Tasks;
using Helper;
using QFramework;
using QuickOutline.Scripts;
using UnityEngine;
using View;

namespace BlockSystem.Implementation
{
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
        private IBlockState _currentState;

        /// <summary>
        /// 获取当前状态
        /// </summary>
        public IBlockState CurrentState => this._currentState;

        public Direction Direction => this._hasShadow.Direction;

        public int Index { get; set; }
        public readonly EasyEvent<int> OnClickEvent = new EasyEvent<int>();
        public readonly EasyEvent OnTriggerEnterEvent = new EasyEvent();

        private static readonly int HighLight = Shader.PropertyToID("_HighLight");
        private static readonly int Color = Shader.PropertyToID("_Color");
        private static readonly int AniTime = Shader.PropertyToID("_AniTime");
        private static readonly int SparkleCenter = Shader.PropertyToID("_SparkleCenter");

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

        public async UniTask SetStateAsync(IBlockState newState, CancellationToken c = default)
        {
            if (this._currentState != null)
                await this._currentState.ExitAsync(this, c);

            this._currentState = newState;

            if (this._currentState != null)
                await this._currentState.EnterAsync(this, c);
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


        private ISequence _sequence;

        public void Sparkle(Vector2 center)
        {
            if (this._sequence != null)
            {
                this._sequence.Finish();
                this._sequence = null;
            }

            this._material.SetFloat(AniTime, 0f);
            this._material.SetVector(SparkleCenter, center);
            this._sequence = ActionKit.Sequence().Lerp(0, 6f, 1f, this.SparkleLerp)
                .Callback(this.SparkleReset);
            this._sequence.Start(this);
        }

        private void SparkleReset()
        {
            this._sequence = null;
            this.transform.localScale = Vector3.one;
            this._material.SetFloat(AniTime, 0f);
        }

        private void SparkleLerp(float t)
        {
            float wave = (1 - (t - 1) * (t - 1)) / 5;
            wave = Mathf.Clamp01(wave);
            this.transform.localScale = Vector3.one * (1 + wave);
            this._material.SetFloat(AniTime, t);
        }

        public void Alert()
        {
            AudioKit.PlaySound("不对");
            ActionKit.Lerp(0, 1, 0.2f, this.ColorLerp)
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
                Outline outline = go.AddComponent<Outline>();
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