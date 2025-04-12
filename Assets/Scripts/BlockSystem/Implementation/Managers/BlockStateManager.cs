using System.Linq;
using BlockSystem.Commands;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BlockSystem.Implementation.Managers
{
    public class BlockStateManager
    {
        private readonly WoodBlock[] _woodBlocks;
        private readonly (Vector3 position, Quaternion rotation) _baseTransform;
        private readonly Vector3[] _relativePosition;
        private readonly Quaternion[] _relativeRotation;
        private readonly TransformsConfig _shufflePositionsConfig;

        private WoodBlock BaseBlock => this._woodBlocks[0];

        public BlockStateManager(WoodBlock[] woodBlocks, TransformsConfig shuffleConfig)
        {
            this._woodBlocks = woodBlocks;
            this._shufflePositionsConfig = shuffleConfig;
            this._baseTransform = (this.BaseBlock.Position, this.BaseBlock.Rotation);

            this._relativePosition = new Vector3[this._woodBlocks.Length];
            this._relativeRotation = new Quaternion[this._woodBlocks.Length];

            this.InitializeRelativeTransforms();
        }

        private void InitializeRelativeTransforms()
        {
            for (int i = 0; i < this._woodBlocks.Length; i++)
            {
                this._woodBlocks[i].Index = i;
                this._relativePosition[i] = Quaternion.Inverse(this.BaseBlock.Rotation) *
                                            (this._woodBlocks[i].Position - this.BaseBlock.Position);
                this._relativeRotation[i] = this._woodBlocks[i].Rotation * Quaternion.Inverse(this.BaseBlock.Rotation);
            }
        }

        public async UniTask RestoreAsync(CommandManager commandManager, SelectionManager selectionManager)
        {
            if (this._woodBlocks.Any(block => !block.CanAcceptCommand))
            {
                return;
            }

            selectionManager.DeselectAll();
            selectionManager.Select(0);

            DirectTransferCommand command =
                DirectTransferCommand.Create(this._baseTransform.position, this._baseTransform.rotation);
            await commandManager.ExecuteCommandAsync(command, this.BaseBlock);
            selectionManager.ChangeSelectState(0);

            for (int i = 1; i < this._woodBlocks.Length; i++)
            {
                command = DirectTransferCommand.Create(
                    this.BaseBlock.Position + (this.BaseBlock.Rotation * this._relativePosition[i]),
                    this._relativeRotation[i] * this.BaseBlock.Rotation);

                selectionManager.Select(i);
                await commandManager.ExecuteCommandAsync(command, this._woodBlocks[i]);
                selectionManager.ChangeSelectState(i);
            }
        }

        public async UniTask ShuffleBlocksAsync(CommandManager commandManager, SelectionManager selectionManager)
        {
            if (this._woodBlocks.Any(block => !block.CanAcceptCommand))
            {
                return;
            }

            selectionManager.DeselectAll();
            for (int i = 0; i < this._woodBlocks.Length; i++)
            {
                selectionManager.Select(i);
                DirectTransferCommand command = DirectTransferCommand.Create(
                    this._shufflePositionsConfig.transforms[i].position,
                    Quaternion.Euler(this._shufflePositionsConfig.transforms[i].angle));
                await commandManager.ExecuteCommandAsync(command, this._woodBlocks[i]);
                selectionManager.ChangeSelectState(i);
            }
        }

        public bool CheckAllBlocksCorrect(out GameObject incorrectBlock, out PosAndAngle correctPosAndAngle)
        {
            incorrectBlock = null;
            correctPosAndAngle = PosAndAngle.Zero;

            if (!Approximately(this.BaseBlock.Position, this._baseTransform.position) ||
                !Approximately(this.BaseBlock.Rotation, this._baseTransform.rotation))
            {
                incorrectBlock = this.BaseBlock.gameObject;
                correctPosAndAngle =
                    new PosAndAngle(this._baseTransform.position, this._baseTransform.rotation.eulerAngles);
                return false;
            }

            for (int i = 1; i < this._woodBlocks.Length; i++)
            {
                Vector3 position = this.BaseBlock.Position + (this.BaseBlock.Rotation * this._relativePosition[i]);
                Quaternion rotation = this._woodBlocks[i].Rotation * Quaternion.Inverse(this.BaseBlock.Rotation);

                if (i == (this._woodBlocks.Length - 1) && Approximately(this._woodBlocks[i].Position, position))
                {
                    return true;
                }

                if (!Approximately(this._woodBlocks[i].Position, position) ||
                    !Approximately(rotation, this._relativeRotation[i]))
                {
                    incorrectBlock = this._woodBlocks[i].gameObject;
                    correctPosAndAngle = new PosAndAngle(position, this._relativeRotation[i].eulerAngles);
                    return false;
                }
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
    }
}