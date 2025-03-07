using System;
using System.Collections.Generic;
using System.Text;
using QFramework;
using UnityEngine;

public class WoodBlockController : MonoBehaviour
{
    public List<WoodBlock> woodBlocks;
    [SerializeField] private RawImageRayCast rawImageRayCast;
    [SerializeField] private GameObject plane;
    private WoodBlock BaseBlock => this.woodBlocks[0]; //基准木块
    private Vector3[] _relativePosition; // 复原时相对基准木块的位置
    private Quaternion[] _relativeRotation; // 复原时相对基准木块的旋转
    private int _selectedIndex = -1;
    private WoodBlock SelectedWoodBlock => this._selectedIndex == -1 ? null : this.woodBlocks[this._selectedIndex];
    private readonly Stack<UnitOperation> _operationHistory = new(); // 操作历史记录栈


    private void Start()
    {
        this._relativePosition = new Vector3[this.woodBlocks.Count];
        this._relativeRotation = new Quaternion[this.woodBlocks.Count];
        this.rawImageRayCast.OnBeginDragEvent += this.OnStartDrag;
        this.rawImageRayCast.OnEndDragEvent += this.OnEndDrag;

        this._onClickNullHandler = (v) => this.Select(this._selectedIndex);
        this.rawImageRayCast.OnClickNullEvent += this._onClickNullHandler;
        foreach (var woodBlock in this.woodBlocks)
        {
            woodBlock.Index = this.woodBlocks.IndexOf(woodBlock);
            woodBlock.OnClickEvent.Register(this.Select);
            this._relativePosition[woodBlock.Index] = woodBlock.transform.position - this.BaseBlock.transform.position;
            this._relativeRotation[woodBlock.Index] =
                woodBlock.transform.rotation * Quaternion.Inverse(this.BaseBlock.transform.rotation);
        }
    }

    public void Disable()
    {
        this.rawImageRayCast.enabled = false;
        this.plane.SetActive(false);
    }

    public void Enable()
    {
        this.rawImageRayCast.enabled = true;
        this.plane.SetActive(true);
    }

    private Action<Vector2> _onClickNullHandler; //仅用于处理点击空白处的事件

    protected void OnDestroy()
    {
        this.rawImageRayCast.OnBeginDragEvent -= this.OnStartDrag;
        this.rawImageRayCast.OnEndDragEvent -= this.OnEndDrag;
        this.rawImageRayCast.OnClickNullEvent -= this._onClickNullHandler;
        foreach (var woodBlock in this.woodBlocks) woodBlock.OnClickEvent.UnRegister(this.Select);
        this._operationHistory.Clear();
    }

    private void OnStartDrag(Vector2 screenPosition)
    {
        if (this._selectedIndex == -1) return;
        this._operationId++;
        this.rawImageRayCast.OnDragEvent += this.OnDrag;
    }

    private void OnEndDrag(Vector2 screenPosition)
    {
        this.rawImageRayCast.OnDragEvent -= this.OnDrag;
    }

    private void OnDrag(Vector2 screenPosition)
    {
        if (this._selectedIndex == -1) return;
        var p = this.rawImageRayCast.WorldPointToScreenPoint(this.SelectedWoodBlock.transform.position);
        if ((screenPosition - p).magnitude < 40f) return;
        var direction = (screenPosition - p).normalized;
        var dotY = Vector2.Dot(direction, RawImageRayCast.YAxis);
        var dotX = Vector2.Dot(direction, RawImageRayCast.XAxis);
        var dotZ = Vector2.Dot(direction, RawImageRayCast.ZAxis);
        if (Mathf.Abs(dotY) > Mathf.Abs(dotX) && Mathf.Abs(dotY) > Mathf.Abs(dotZ))
            this.PerformOperation(OperationType.MoveY, dotY > 0 ? 1 : -1);
        else if (Mathf.Abs(dotX) > Mathf.Abs(dotY) && Mathf.Abs(dotX) > Mathf.Abs(dotZ))
            this.PerformOperation(OperationType.MoveX, dotX > 0 ? 1 : -1);
        else
            this.PerformOperation(OperationType.MoveZ, dotZ > 0 ? 1 : -1);
    }

    public void Select(int index)
    {
        Debug.Log("Select " + index);
        if (this._selectedIndex != -1)
        {
            if (this.SelectedWoodBlock.IsAnimating) return; // 正在移动或旋转时不响应点击
            this.SelectedWoodBlock.Select(false);
        }

        if (this._selectedIndex == index)
        {
            this._selectedIndex = -1;
        }
        else
        {
            this._selectedIndex = index;
            this.SelectedWoodBlock.Select(true);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            /*if (this.CheckAllBlocksCorrect())
            {
                Debug.Log("所有木块位置和旋转都正确");
            }
            else
            {
                Debug.Log("所有木块位置和旋转都不正确");
            }*/
            this.rawImageRayCast.ChangePreviewCamera();
        if (Input.GetKeyDown(KeyCode.Z)) // 按下 Z 键撤回操作
            this.UndoOperation();

        if (Input.GetKeyDown(KeyCode.Return)) this.Restore();
        if (this._selectedIndex == -1) return;
        // 移动控制
        /*if (Input.GetKey(KeyCode.W)) this.PerformOperation(new UnitOperation(OperationType.MoveY, 1, this._selectedIndex));
        if (Input.GetKey(KeyCode.S)) this.PerformOperation(new UnitOperation(OperationType.MoveY, -1, this._selectedIndex));
        if (Input.GetKey(KeyCode.A)) this.PerformOperation(new UnitOperation(OperationType.MoveX, -1, this._selectedIndex));
        if (Input.GetKey(KeyCode.E)) this.PerformOperation(new UnitOperation(OperationType.MoveX, 1, this._selectedIndex));
        if (Input.GetKey(KeyCode.Q)) this.PerformOperation(new UnitOperation(OperationType.MoveZ, 1, this._selectedIndex));
        if (Input.GetKey(KeyCode.D)) this.PerformOperation(new UnitOperation(OperationType.MoveZ, -1, this._selectedIndex));*/

        // 旋转控制, 由于 PerformOperation的 if(this.SelectedWoodBlock.IsAnimating) return; 操作不一定会成功, _operationId就会不连续, 但是不影响撤销操作, 只要每次id不同就可
        if (Input.GetKeyDown(KeyCode.R))
        {
            this._operationId++;
            this.PerformOperation(OperationType.RotateX);
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            this._operationId++;
            this.PerformOperation(OperationType.RotateY);
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            this._operationId++;
            this.PerformOperation(OperationType.RotateZ);
        }
    }

    private int _operationId = 0; // 操作id, 用于区分不同的操作, 每次拖拽操作时都会递增, 每次旋转操作时也会递增, 用于实现撤销操作

    private void PerformOperation(OperationType type, int value = 1)
    {
        if (this.SelectedWoodBlock.IsAnimating) return;
        var operation = new UnitOperation(this._operationId, type, value, this._selectedIndex);
        this._operationHistory.Push(operation); // 记录操作
        if (operation.IsMoveOperation)
            this.SelectedWoodBlock.TryMove(operation);
        else
            this.SelectedWoodBlock.TryRotate(operation);
    }

    private void UndoOperation()
    {
        if (this._operationHistory.Count > 0 &&
            (this._selectedIndex == -1 || this.SelectedWoodBlock.IsAnimating == false))
        {
            var operation = this._operationHistory.Peek();
            var block = this.woodBlocks[operation.index];
            if (operation.IsRotateOperation)
            {
                operation = this._operationHistory.Pop();
                // 执行反向操作
                var r = new UnitOperation(-1, operation.type, -operation.value, operation.index);
                block.TryRotate(r);
                return;
            }

            var originalPosition = block.transform.position;
            do
            {
                // 执行反向操作
                operation = this._operationHistory.Pop();
                if (!operation.success) continue;
                operation.value = -operation.value;
                originalPosition = operation.GetTargetPosition(originalPosition);
                operation.value = -operation.value;
            } while (this._operationHistory.Count > 0 && this._operationHistory.Peek().id == operation.id);

            block.DirectMove(originalPosition);
        }
    }

    private bool CheckAllBlocksCorrect()
    {
        for (var i = 0; i < this.woodBlocks.Count; i++)
        {
            var position = this.BaseBlock.transform.position + this._relativePosition[i];
            var rotation = this.woodBlocks[i].transform.rotation *
                           Quaternion.Inverse(this.BaseBlock.transform.rotation);
            // 检查位置和旋转是否正确, 需要注意精度问题
            if (i == 4 && Approximately(this.woodBlocks[i].transform.position, position)) //长条形的木块需要特殊处理
                return true;
            if (!Approximately(this.woodBlocks[i].transform.position, position) ||
                !Approximately(rotation, this._relativeRotation[i])) return false;
        }

        return true;
    }

    // 序列化历史操作栈
    public string SerializeOperationHistory()
    {
        var sb = new StringBuilder();
        foreach (var operation in this._operationHistory)
        {
            sb.Append(JsonUtility.ToJson(operation));
            sb.Append(",");
        }

        return sb.ToString();
    }

    // 反序列化历史操作栈
    public List<UnitOperation> DeserializeOperationHistory(string json)
    {
        var operationJsons = json.Split(',');
        var operations = new List<UnitOperation>();
        foreach (var operationJson in operationJsons)
        {
            if (string.IsNullOrEmpty(operationJson)) continue;
            var operation = JsonUtility.FromJson<UnitOperation>(operationJson);
            operations.Add(operation);
        }

        return operations;
    }

    private void Restore()
    {
        this.woodBlocks[0].DirectMove(new Vector3(-0.5f, 0, 0));
        this.woodBlocks[1].DirectMove(new Vector3(0, 0, -0.5f));
        this.woodBlocks[2].DirectMove(new Vector3(0, -0.5f, 0));
        this.woodBlocks[3].DirectMove(new Vector3(0.5f, 0, 0));
        this.woodBlocks[4].DirectMove(new Vector3(0, 0.5f, 0));
        this.woodBlocks[5].DirectMove(new Vector3(0, 0, 0.5f));
        foreach (var woodBlock in this.woodBlocks) woodBlock.DirectRotate(Quaternion.identity);
    }

    private static bool Approximately(Vector3 a, Vector3 b)
    {
        return Vector3.Distance(a, b) < 0.02f;
    }

    private static bool Approximately(Quaternion a, Quaternion b)
    {
        return Quaternion.Angle(a, b) < 0.02f;
    }
}