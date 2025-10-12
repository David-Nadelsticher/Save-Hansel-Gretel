using System;
using Gameplay.Controls.Player;

namespace Gameplay.Controls.Collectable
{
    [Serializable]
    public abstract class CollectableEffect
    {
        public abstract CollectableType Type { get; }

        public abstract void ApplyReward(PlayerController player);
    }
}