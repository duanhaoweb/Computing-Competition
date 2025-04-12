using System.Threading;
using BlockSystem.Abstractions;
using BlockSystem.Commands;
using Cysharp.Threading.Tasks;
using QFramework;
using UnityEngine;
using View;
using Debug = UnityEngine.Debug;


namespace BlockSystem.Implementation.Managers
{
    public class InputHandler
    {
        private readonly RawImageRayCast _rawImageRayCast;
        private readonly SelectionManager _selectionManager;
        private readonly CommandManager _commandManager;
        private readonly System.Action<Vector2> _onClickNullHandler;
        private CancellationTokenSource _dragCts;
        private int _operationId = 0;

        public InputHandler(RawImageRayCast rawImageRayCast, SelectionManager selectionManager,
            CommandManager commandManager)
        {
            this._rawImageRayCast = rawImageRayCast;
            this._selectionManager = selectionManager;
            this._commandManager = commandManager;

            this._onClickNullHandler = v =>
                this._selectionManager.ChangeSelectState(this._selectionManager.SelectedWoodBlock?.Index ?? -1);

            // 注册事件
            this._rawImageRayCast.OnBeginDragEvent += this.OnStartDrag;
            this._rawImageRayCast.OnEndDragEvent += this.OnEndDrag;
            this._rawImageRayCast.OnClickNullEvent += this._onClickNullHandler;
            ActionKit.OnUpdate.Register(this.ProcessInput);
        }

        private void OnStartDrag(Vector2 screenPosition, bool isLiftMouse)
        {
            this._dragCts?.Cancel();
            this._dragCts = new CancellationTokenSource();
            if (!isLiftMouse || !this._rawImageRayCast.TryGetInteractiveModel(out InteractiveModel m))
            {
                //认为不是在拖动木块, 而是在操作屏幕
                this.HandleViewAngleAsync(this._dragCts.Token).Forget();
                return;
            }

            this._operationId++;
            this._rawImageRayCast.ScreenPointToLocalPointInRectangle(screenPosition, out Vector2 localPoint);
            localPoint.x += this._rawImageRayCast.Width / 2;
            localPoint.y = this._rawImageRayCast.Height / 2 - localPoint.y;
            WoodBlock block = m.gameObject.GetComponent<WoodBlock>();
            this._selectionManager.Select(block);
            block.Sparkle(localPoint);
            this.HandleDragAsync(this._dragCts.Token).Forget();
        }

        private void OnEndDrag(Vector2 screenPosition)
        {
            this._dragCts?.Cancel();
        }

        private async UniTaskVoid HandleViewAngleAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    float mouseY = Input.GetAxisRaw("Mouse Y");
                    if (mouseY != 0f)
                    {
                        this._rawImageRayCast.ChangePreviewCamera(deltaTheta: 0.12f * mouseY);
                    }

                    float mouseX = Input.GetAxisRaw("Mouse X");
                    if (mouseX != 0f)
                    {
                        this._rawImageRayCast.ChangePreviewCamera(deltaPhi: -0.2f * mouseX);
                    }

                    await UniTask.Yield();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"View angle operation cancelled: {e.Message}");
            }
        }

        private async UniTaskVoid HandleDragAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (this._selectionManager.SelectedWoodBlock is null) break;
                    Vector2 woodBlockScreenPos =
                        this._rawImageRayCast.WorldPointToScreenPoint(this._selectionManager.SelectedWoodBlock.transform
                            .position);
                    Vector2 currentPos = this._rawImageRayCast.ScreenMousePosition;
                    Vector2 delta = currentPos - woodBlockScreenPos;
                    if (delta.magnitude < 40f || this._selectionManager.SelectedWoodBlock.CanAcceptCommand == false)
                    {
                        await UniTask.Yield();
                        continue;
                    }

                    Vector2 direction = delta.normalized;
                    float dotY = Vector2.Dot(direction, RawImageRayCast.YAxis);
                    float dotX = Vector2.Dot(direction, RawImageRayCast.XAxis);
                    float dotZ = Vector2.Dot(direction, RawImageRayCast.ZAxis);

                    IBlockCommand command;
                    if (Mathf.Abs(dotY) > Mathf.Abs(dotX) && Mathf.Abs(dotY) > Mathf.Abs(dotZ))
                        command = MoveCommand.CreateAxisMove(this._operationId, Vector3.up, dotY > 0 ? 1 : -1);
                    else if (Mathf.Abs(dotX) > Mathf.Abs(dotY) && Mathf.Abs(dotX) > Mathf.Abs(dotZ))
                        command = MoveCommand.CreateAxisMove(this._operationId, Vector3.right, dotX > 0 ? 1 : -1);
                    else
                        command = MoveCommand.CreateAxisMove(this._operationId, Vector3.forward, dotZ > 0 ? 1 : -1);

                    await this._commandManager.ExecuteCommandAsync(command, this._selectionManager.SelectedWoodBlock);
                    await UniTask.Yield();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Drag operation cancelled: {e.Message}");
            }
        }

        private void ProcessInput()
        {
            if (Input.GetAxis("Mouse ScrollWheel") > 0f)
            {
                this._rawImageRayCast.ChangePreviewCamera(deltaSize: -30f * Time.deltaTime);
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
            {
                this._rawImageRayCast.ChangePreviewCamera(deltaSize: 30f * Time.deltaTime);
            }

            if (Input.GetKeyDown(KeyCode.Z))
            {
                this._commandManager.UndoOperationAsync().Forget();
                return;
            }

            if (this._selectionManager.SelectedWoodBlock is null) return;

            if (Input.GetKeyDown(KeyCode.R))
            {
                this._operationId++;
                this._commandManager.ExecuteCommandAsync(
                    RotateCommand.CreateAxisRotation(this._operationId, Vector3.right, 1),
                    this._selectionManager.SelectedWoodBlock).Forget();
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                this._operationId++;
                this._commandManager.ExecuteCommandAsync(
                    RotateCommand.CreateAxisRotation(this._operationId, Vector3.up, 1),
                    this._selectionManager.SelectedWoodBlock).Forget();
            }
            else if (Input.GetKeyDown(KeyCode.T))
            {
                this._operationId++;
                this._commandManager.ExecuteCommandAsync(
                    RotateCommand.CreateAxisRotation(this._operationId, Vector3.forward, 1),
                    this._selectionManager.SelectedWoodBlock).Forget();
            }
        }

        public void Dispose()
        {
            ActionKit.OnUpdate.UnRegister(this.ProcessInput);
            this._rawImageRayCast.OnBeginDragEvent -= this.OnStartDrag;
            this._rawImageRayCast.OnEndDragEvent -= this.OnEndDrag;
            this._rawImageRayCast.OnClickNullEvent -= this._onClickNullHandler;
            this._dragCts?.Cancel();
            this._dragCts?.Dispose();
            this._dragCts = null;
        }
    }
}