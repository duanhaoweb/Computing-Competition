using UnityEngine;
using Cysharp.Threading.Tasks;
using BlockSystem.Implementation.Managers;
using UnityEngine.Events;
using View;

namespace BlockSystem.Implementation
{
    [RequireComponent(typeof(RawImageRayCast))]
    public class WoodBlockController : MonoBehaviour
    {
        [SerializeField] private WoodBlock[] woodBlocks;
        [SerializeField] private RawImageRayCast rawImageRayCast;
        [SerializeField] private GameObject plane;
        [SerializeField] private TransformsConfig shufflePositionsConfig;

        private SelectionManager _selectionManager;
        private InputHandler _inputHandler;
        private CommandManager _commandManager;
        private BlockStateManager _blockStateManager;
        [SerializeField] private Guidance guidance;

        public UnityEvent onSuccess;

        public void Disable()
        {
            this.rawImageRayCast.enabled = false;
            this.plane.SetActive(false);
        }

        public void Enable()
        {
            this.rawImageRayCast.enabled = true;
            this.plane.SetActive(true);
            this.Init();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                this._blockStateManager.RestoreAsync(this._commandManager, this._selectionManager).Forget();
            }
        }

        private void Init()
        {
            this._selectionManager = new SelectionManager(this.woodBlocks);
            this._commandManager = new CommandManager();
            this._blockStateManager = new BlockStateManager(this.woodBlocks, this.shufflePositionsConfig);
            this._inputHandler = new InputHandler(this.rawImageRayCast, this._selectionManager, this._commandManager);
            this.guidance.Init(this._blockStateManager);

            // 初始打乱木块顺序
            this._blockStateManager.ShuffleBlocksAsync(this._commandManager, this._selectionManager).ContinueWith(
                () =>
                {
                    this.CheckSuccess();
                    this._commandManager.onOperationCompleted.AddListener(this.CheckSuccess);
                }).Forget();
        }

        private void CheckSuccess()
        {
            if (this._blockStateManager.CheckAllBlocksCorrect(out GameObject block, out PosAndAngle target))
            {
                this._selectionManager.DeselectAll();
                this.guidance.Hide();
                this.onSuccess?.Invoke();
            }
            else
            {
                this.guidance.Hint(block, target);
            }
        }

        private void OnDestroy()
        {
            this.Dispose();
        }

        private void Dispose()
        {
            this._selectionManager.Dispose();
            this._inputHandler.Dispose();
            this._commandManager.Dispose();
        }
    }
}