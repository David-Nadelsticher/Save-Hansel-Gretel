using System;
using Gameplay.Controls.Player;
using UnityEngine;
using UnityEngine.Serialization;

namespace Gameplay.Controls.Collectable
{
    [Serializable] public class EnergyCollectable : CollectableEffect
    {
        [SerializeField] private int amount;

        public EnergyCollectable(int amount)
        {
            this.amount = amount;
        }

        public override CollectableType Type => CollectableType.Energy;

        public override void ApplyReward(PlayerController player)
        {
            player.AddEnergy(amount);
        }
    }
}