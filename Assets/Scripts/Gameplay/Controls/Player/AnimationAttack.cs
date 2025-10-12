using UnityEngine;

namespace Gameplay.Controls.Player
{
    public class AnimationAttack : MonoBehaviour
    {
        [SerializeField]private PlayerController _playerController;
        
        public void OnSpecialAttackAnimationEvent()
        {
            if (_playerController == null)
            {
                Debug.LogError("PlayerController is not assigned.");
                return;
            }

            // Trigger the special attack logic
            _playerController.HandleSpecialAttackLogic();
        }
        public void OnBasicAttackAnimationEvent()
        {
            if (_playerController == null)
            {
                Debug.LogError("PlayerController is not assigned.");
                return;
            }

            // Trigger the special attack logic
            _playerController.HandleBasicAttackLogic();
        }
        
    }
}