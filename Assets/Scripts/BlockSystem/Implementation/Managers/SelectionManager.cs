using UnityEngine.Events;

namespace BlockSystem.Implementation.Managers
{
    public class SelectionManager
    {
        private readonly WoodBlock[] _woodBlocks;
        private int _selectedIndex = -1;

        public readonly UnityEvent onSelectionChanged;

        public WoodBlock SelectedWoodBlock => this._selectedIndex == -1 ? null : this._woodBlocks[this._selectedIndex];

        public SelectionManager(WoodBlock[] woodBlocks)
        {
            this._woodBlocks = woodBlocks;
            this.onSelectionChanged = new UnityEvent();

            foreach (WoodBlock woodBlock in this._woodBlocks)
            {
                woodBlock.OnClickEvent.Register(this.ChangeSelectState);
            }
        }

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

            this.onSelectionChanged?.Invoke();
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
            this.onSelectionChanged?.Invoke();
        }

        public void Select(WoodBlock block)
        {
            this.Select(block.Index);
        }

        public void DeselectAll()
        {
            if (this._selectedIndex != -1)
            {
                this.SelectedWoodBlock.IsSelected = false;
                this._selectedIndex = -1;
                this.onSelectionChanged?.Invoke();
            }
        }

        public void Dispose()
        {
            foreach (WoodBlock woodBlock in this._woodBlocks)
            {
                woodBlock.OnClickEvent.UnRegister(this.ChangeSelectState);
            }

            this.onSelectionChanged.RemoveAllListeners();
        }
    }
}