using System;
using Gameplay.Controls.Player;
using UnityEngine;
namespace Gameplay.Controls.Collectable
{
    [Serializable]
    public class LifeCollectable : CollectableEffect
    {
        [SerializeField] private int amount;

        public LifeCollectable(int amount)
        {
            this.amount = amount;
        }

        public override CollectableType Type => CollectableType.Life;

        public override void ApplyReward(PlayerController player)
        {
            player.AddLife(amount);
        }
    }
}
