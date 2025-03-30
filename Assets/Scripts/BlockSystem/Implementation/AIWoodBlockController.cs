using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace AI.BlockSystem
{
    public class AIWoodBlockController : MonoBehaviour
    {
        [SerializeField] private List<AIWoodBlock> woodBlocks;
        [SerializeField] private RawImageRayCast rawImageRayCast;
        [SerializeField] private GameObject plane;

        private AIWoodBlock BaseBlock => this.woodBlocks[0];
        private (Vector3 position, Quaternion rotation) _baseTransform;
        private Vector3[] _relativePosition;
        private Quaternion[] _relativeRotation;
        private int _selectedIndex = -1;

        private AIWoodBlock SelectedWoodBlock =>
            this._selectedIndex == -1 ? null : this.woodBlocks[this._selectedIndex];

        private readonly Stack<(IBlockCommand Command, AIWoodBlock Block)> _operationHistory = new();
        private CancellationTokenSource _dragCts;
        private int _operationId = 0;

        private void Start()
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

        private void Init()
        {
            this._relativePosition = new Vector3[this.woodBlocks.Count];
            this._relativeRotation = new Quaternion[this.woodBlocks.Count];
            this._baseTransform = (this.BaseBlock.Position, this.BaseBlock.Rotation);
            // 保存相对位置和旋转，用于检查是否完成
            foreach (var woodBlock in this.woodBlocks)
            {
                woodBlock.Index = this.woodBlocks.IndexOf(woodBlock);
                woodBlock.OnClickEvent.Register(this.ChangeSelectState);
                this._relativePosition[woodBlock.Index] = Quaternion.Inverse(this.BaseBlock.Rotation) *
                                                          (woodBlock.Position - this.BaseBlock.Position);
                this._relativeRotation[woodBlock.Index] =
                    woodBlock.Rotation * Quaternion.Inverse(this.BaseBlock.Rotation);
            }

            this.rawImageRayCast.OnBeginDragEvent += this.OnStartDrag;
            this.rawImageRayCast.OnEndDragEvent += this.OnEndDrag;
            this._onClickNullHandler = v => this.ChangeSelectState(this._selectedIndex);
            this.rawImageRayCast.OnClickNullEvent += this._onClickNullHandler;
        }

        private void OnDestroy()
        {
            this.Dispose();
        }

        private void Dispose()
        {
            this.rawImageRayCast.OnBeginDragEvent -= this.OnStartDrag;
            this.rawImageRayCast.OnEndDragEvent -= this.OnEndDrag;
            this.rawImageRayCast.OnClickNullEvent -= this._onClickNullHandler;
            foreach (var woodBlock in this.woodBlocks)
            {
                woodBlock.OnClickEvent.UnRegister(this.ChangeSelectState);
            }

            this._operationHistory.Clear();
            this._dragCts?.Cancel();
            this._dragCts?.Dispose();
            this._dragCts = null;
            this._relativePosition = null;
            this._relativeRotation = null;
        }

        private void OnStartDrag(Vector2 screenPosition)
        {
            if (this._selectedIndex == -1) return;
            this._operationId++;
            this._dragCts?.Cancel();
            this._dragCts = new CancellationTokenSource();

            // 开始监听拖拽
            this.HandleDragAsync(this._dragCts.Token).Forget();
        }

        private void OnEndDrag(Vector2 screenPosition)
        {
            this._dragCts?.Cancel();
        }

        private async UniTaskVoid HandleDragAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (this.SelectedWoodBlock is null) break;
                    Vector2 woodBlockScreenPos =
                        this.rawImageRayCast.WorldPointToScreenPoint(this.SelectedWoodBlock.transform.position);
                    Vector2 currentPos = this.rawImageRayCast.ScreenMousePosition;
                    var delta = currentPos - woodBlockScreenPos;
                    if (delta.magnitude < 40f || this.SelectedWoodBlock.CanAcceptCommand == false)
                    {
                        await UniTask.Yield();
                        continue;
                    }

                    var direction = delta.normalized;
                    var dotY = Vector2.Dot(direction, RawImageRayCast.YAxis);
                    var dotX = Vector2.Dot(direction, RawImageRayCast.XAxis);
                    var dotZ = Vector2.Dot(direction, RawImageRayCast.ZAxis);

                    IBlockCommand command;
                    if (Mathf.Abs(dotY) > Mathf.Abs(dotX) && Mathf.Abs(dotY) > Mathf.Abs(dotZ))
                        command = MoveCommand.CreateAxisMove(this._operationId, Vector3.up, dotY > 0 ? 1 : -1);
                    else if (Mathf.Abs(dotX) > Mathf.Abs(dotY) && Mathf.Abs(dotX) > Mathf.Abs(dotZ))
                        command = MoveCommand.CreateAxisMove(this._operationId, Vector3.right, dotX > 0 ? 1 : -1);
                    else
                        command = MoveCommand.CreateAxisMove(this._operationId, Vector3.forward, dotZ > 0 ? 1 : -1);

                    var result = await this.SelectedWoodBlock.ExecuteCommandAsync(command);
                    if (result.Status == CommandResultStatus.Success)
                    {
                        this._operationHistory.Push((command, this.SelectedWoodBlock));
                    }
                    else
                    {
                        Debug.Log($"Drag operation failed: {result.Message}" +
                                  $" | Command: {command.GetType().Name} | Block: {this.SelectedWoodBlock.name}");
                    }

                    await UniTask.Yield();
                }
            }
            catch (System.Exception e)
            {
                // Ignore any exceptions during drag
                Debug.LogError($"Drag operation cancelled: {e.Message}");
            }
        }

        /// <summary>
        /// 切换选中状态. 如果已选中木块不允许接收命令(正忙)，则不切换
        /// 如果选中同一个木块，则取消选中, 否则选中新的木块
        /// </summary>
        public void ChangeSelectState(int index)
        {
            if (this._selectedIndex != -1)
            {
                if (!this.SelectedWoodBlock.CanAcceptCommand)
                    return;
                this.SelectedWoodBlock.IsSelected = false;
            }

            if (this._selectedIndex == index)
            {
                this._selectedIndex = -1;
            }
            else
            {
                this._selectedIndex = index;
                this.SelectedWoodBlock.IsSelected = true;
            }
        }

        public void Select(int index)
        {
            if (this._selectedIndex != -1)
            {
                if (!this.SelectedWoodBlock.CanAcceptCommand)
                    return;
                this.SelectedWoodBlock.IsSelected = false;
            }

            this._selectedIndex = index;
            this.SelectedWoodBlock.IsSelected = true;
        }

        public void Select(AIWoodBlock block)
        {
            if (this._selectedIndex != -1)
            {
                if (!this.SelectedWoodBlock.CanAcceptCommand)
                    return;
                this.SelectedWoodBlock.IsSelected = false;
            }

            this._selectedIndex = block.Index;
            this.SelectedWoodBlock.IsSelected = true;
        }

        public void DeselectAll()
        {
            if (this._selectedIndex != -1)
            {
                this.SelectedWoodBlock.IsSelected = false;
                this._selectedIndex = -1;
            }
        }

        private async void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                this.rawImageRayCast.ChangePreviewCamera();
            }

            if (Input.GetKeyDown(KeyCode.Z))
            {
                await this.UndoOperationAsync();
                return;
            }

            if (Input.GetKeyDown(KeyCode.Return))
            {
                await this.RestoreAsync();
                return;
            }

            if (this._selectedIndex == -1) return;

            // 旋转控制
            if (Input.GetKeyDown(KeyCode.R))
            {
                this._operationId++;
                await this.ExecuteCommandAsync(RotateCommand.CreateAxisRotation(this._operationId, Vector3.right, 1));
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                this._operationId++;
                await this.ExecuteCommandAsync(RotateCommand.CreateAxisRotation(this._operationId, Vector3.up, 1));
            }
            else if (Input.GetKeyDown(KeyCode.T))
            {
                this._operationId++;
                await this.ExecuteCommandAsync(RotateCommand.CreateAxisRotation(this._operationId, Vector3.forward, 1));
            }
        }

        private async UniTask ExecuteCommandAsync(BlockCommandBase command)
        {
            var result = await this.SelectedWoodBlock.ExecuteCommandAsync(command);
            if (result.Status == CommandResultStatus.Success)
            {
                this._operationHistory.Push((command, this.SelectedWoodBlock));
            }
        }

        private async UniTask UndoOperationAsync()
        {
            if (this._operationHistory.Count <= 0 || this.woodBlocks.Any(block => !block.CanAcceptCommand))
            {
                Debug.Log("No operations to undo or blocks are busy.");
                return;
            }

            int operationId = ((BlockCommandBase)this._operationHistory.Peek().Command).OperationId;

            // Group operations by the same operationId
            var operationsToUndo = new List<(IBlockCommand Command, AIWoodBlock Block)>();

            do
            {
                operationsToUndo.Add(this._operationHistory.Pop());
            } while (this._operationHistory.Count > 0 &&
                     ((BlockCommandBase)this._operationHistory.Peek().Command).OperationId == operationId);

            AIWoodBlock block = operationsToUndo[0].Block;
            Vector3 targetPosition = block.Position;
            Quaternion targetRotation = block.Rotation;
            foreach ((IBlockCommand command, AIWoodBlock b) in operationsToUndo)
            {
                // Calculate inverse transformation
                (Vector3 move, Quaternion rotation) transfer = command.Transfer;
                Vector3 inverseMove = -transfer.move;
                Quaternion inverseRotation = Quaternion.Inverse(transfer.rotation);

                // Apply inverse transformation
                targetPosition += inverseMove;
                targetRotation = inverseRotation * targetRotation;
            }

            DirectTransferCommand undoCommand = DirectTransferCommand.Create(targetPosition, targetRotation);
            await block.ExecuteCommandAsync(undoCommand);
        }

        private async UniTask RestoreAsync()
        {
            if (this.woodBlocks.Any(block => !block.CanAcceptCommand))
            {
                return;
            }

            this.DeselectAll();
            this.Select(0);
            var command = DirectTransferCommand.Create(this._baseTransform.position, this._baseTransform.rotation);
            await this.BaseBlock.ExecuteCommandAsync(command);
            this.ChangeSelectState(0);
            for (int i = 1; i < this.woodBlocks.Count; i++)
            {
                command = DirectTransferCommand.Create(
                    this.BaseBlock.Position + (this.BaseBlock.Rotation * this._relativePosition[i]),
                    this._relativeRotation[i] * this.BaseBlock.Rotation);
                this.Select(i);
                CommandResult result = await this.woodBlocks[i].ExecuteCommandAsync(command);
                this.ChangeSelectState(i);
            }
        }

        private bool CheckAllBlocksCorrect()
        {
            for (var i = 0; i < this.woodBlocks.Count; i++)
            {
                var position = this.BaseBlock.Position + (this.BaseBlock.Rotation * this._relativePosition[i]);
                var rotation = this.woodBlocks[i].Rotation * Quaternion.Inverse(this.BaseBlock.Rotation);

                if (i == 4 && Approximately(this.woodBlocks[i].Position, position)) //长条形的木块需要特殊处理
                    return true;

                if (!Approximately(this.woodBlocks[i].Position, position) ||
                    !Approximately(rotation, this._relativeRotation[i]))
                    return false;
            }

            return true;
        }

        private static bool Approximately(Vector3 a, Vector3 b)
        {
            return Vector3.Distance(a, b) < 0.02f;
        }

        private static bool Approximately(Quaternion a, Quaternion b)
        {
            return Quaternion.Angle(a, b) < 0.02f;
        }

        private System.Action<Vector2> _onClickNullHandler;
    }
}