using DG.Tweening;
using UnityEngine;

namespace BlockSystem.Implementation.Managers
{
    public class Guidance : MonoBehaviour
    {
        [SerializeField] private GameObject guideBlock;
        private BlockStateManager _blockStateManager;

        private void Start()
        {
            this.Hide();
        }

        public void Hide()
        {
            this.guideBlock.SetActive(false);
        }

        public void Init(BlockStateManager blockStateManager)
        {
            this._blockStateManager = blockStateManager;
        }

        public void Hint(GameObject block, PosAndAngle target)
        {
            this.guideBlock.SetActive(true);
            this.guideBlock.transform.position = target.position;
            this.guideBlock.transform.rotation = Quaternion.Euler(target.angle);
            MeshFilter meshFilter = this.guideBlock.GetComponent<MeshFilter>();
            Mesh targetMesh = block.GetComponent<MeshFilter>().sharedMesh;
            if (meshFilter.sharedMesh == targetMesh)
            {
                return;
            }

            this.guideBlock.GetComponent<MeshFilter>().sharedMesh = targetMesh;
            //用Dotween来做一个出现的动画
            this.guideBlock.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            this.guideBlock.transform.DOScale(Vector3.one, 1f).SetEase(Ease.OutBack);
        }
    }
}