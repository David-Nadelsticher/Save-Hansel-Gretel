using System.Collections;
using BossProject.Core;
using Core.Data;
using Core.Managers;
using Gameplay.Controls.Player.AttackLogic;
using Gameplay.General.UI;
using UnityEngine;


namespace Gameplay.Controls.Player
{
    public class PlayerController : BaseMono

    {
        private static readonly int Move = Animator.StringToHash("Run");
        private static readonly int SpecialAttack = Animator.StringToHash("SpecialAttack");
        private static readonly int BasicAttack = Animator.StringToHash("Attack1");
        private static readonly int ComboAttack = Animator.StringToHash("Attack2");
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int Roll = Animator.StringToHash("Roll");
        private static readonly int RollBool = Animator.StringToHash("RollBool");
        private static readonly int ExitMotionTrigger = Animator.StringToHash("ExitMotionTrigger");
        private static readonly int Hit = Animator.StringToHash("Hit");
        private static readonly int Die = Animator.StringToHash("Die");

        [Header("Movement Settings")] [SerializeField]
        private float maxSpeed = 5;

        [SerializeField] private float brakingFactor = 10f;
        [SerializeField] private float movementForceAmount = 75;
        [SerializeField] private float minSpeedToAnimateMove = 0.1f;
        [SerializeField] private float minSpeedRollExecution = 0.2f;

        [SerializeField] private float currentSpeed;

        private float _normalizeSpeed;
        private float _horizontalMovement;
        private float _verticalMovement;
        private Vector2 _movement = new Vector2();
        private float _velocity = 0.0f;

        private float _targetSpeed;

        [Header("Health Settings")] [SerializeField]
        private int maxHealth = 100;

        [SerializeField] private int maxEnergy = 100;

        [SerializeField] private PlayerHealth _playerHealth;
        [SerializeField] private ResourceBarTracker healthBarUI;
        [SerializeField] private ResourceBarTracker energyBarUI;
        [SerializeField] private float energyRegenInterval = 3f;
        [SerializeField] private int energyRegenAmount = 10;
        private Coroutine _energyRegenCoroutine;

        [Header("Attack Command Settings")] [SerializeField]
        private float attackCooldown = 0.5f; // Time between attacks

        [SerializeField] private AttackDatabaseSO attackDatabaseSo;
        private AttackHandler _attackHandler;
        private float _timeForNextAttack;

        [SerializeField] private Animator animator;
        [SerializeField] private Rigidbody2D rb;
        [Range(0, 1)] [SerializeField] private float smoothTime;
        private bool _facingRight = true;
        private bool _isMoving;

        [Header("Roll Command Settings")] private bool _canRoll;
        [SerializeField] private float rollCooldown = 0.5f;
        [SerializeField] private float rollForce = 1f;
        [SerializeField] private int rollEnergyCost = 10;
        [SerializeField] private float checkRadius = 2f;
        [SerializeField] private Transform attackTransform;
        [SerializeField] private Transform playerLegsTransform;
        [SerializeField] private Transform playerCenterTransform;


        [Header("Debug")] [SerializeField] private bool debugMode = false;

        private bool _stopMovement;
        private bool _canTakeDamage = true;
        private bool _playerDie;

        private void Start()
        {
            if (debugMode) Debug.Log("[PlayerController] Start");
            InitializePlayer();
            StartEnergyRegen();
        }

        private void InitializePlayer()
        {
            if (debugMode) Debug.Log("[PlayerController] InitializePlayer");
            _attackHandler = new AttackHandler(attackDatabaseSo);
            _playerHealth = new PlayerHealth(healthBarUI, energyBarUI, maxHealth, maxHealth, maxEnergy, maxEnergy);
        }

        private void Update()
        {
            if(_playerDie) return;
            CheckInput();
        }


        private void CheckInput()
        {

            //for test if player is dead, press 'K' for kill player

            HandleMovementInput();
            HandleRollInput();
            HandleAttackInput();
        }


        private void HandleMovementInput()
        {
            _horizontalMovement = Input.GetAxis("Horizontal");
            _verticalMovement = Input.GetAxis("Vertical");
            if (_horizontalMovement != 0 || _verticalMovement != 0)
            {
                FlipSprite();
            }

            if (_stopMovement)
            {
                _isMoving = false;
            }

            if (!_isMoving)
            {
                currentSpeed = Mathf.Lerp(currentSpeed, 0f, Time.deltaTime * brakingFactor);
                if (Mathf.Abs(currentSpeed) < 0.01f)
                {
                    currentSpeed = 0f;
                }
            }
            else
            {
                currentSpeed = Mathf.SmoothDamp(currentSpeed, _targetSpeed, ref _velocity, smoothTime);
            }

            animator.SetFloat(Speed, currentSpeed);
            animator.SetBool(Move, _isMoving);
        }

        private void AddForceIfUnderMaxSpeed()
        {
            if (currentSpeed < maxSpeed)
            {
                rb.AddForce(_movement * movementForceAmount, ForceMode2D.Force);
            }
        }

        private void UpdateSpeedPhysics()
        {
            if (_stopMovement) return;
            _movement = new Vector2(_horizontalMovement, _verticalMovement).normalized;
            float flatVelocity = rb.linearVelocity.magnitude;
            _targetSpeed = flatVelocity / maxSpeed;
            _isMoving = flatVelocity >= minSpeedToAnimateMove;
            AddForceIfUnderMaxSpeed();
        }

        void FixedUpdate()
        {
            UpdateSpeedPhysics();
        }

        private void HandleRollInput()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (debugMode) Debug.Log("[PlayerController] Roll input detected");
                float flatVelocity = rb.linearVelocity.magnitude;
                _canRoll = flatVelocity >= minSpeedRollExecution && _playerHealth.CouldExecuteAction(rollEnergyCost);
                if (_canRoll)
                {
                    if (debugMode) Debug.Log("[PlayerController] Roll executed");
                    _playerHealth.DecreaseEnergy(rollEnergyCost);
                    animator.SetTrigger(ExitMotionTrigger);
                    animator.SetTrigger(Roll);
                    rb.AddForce(_movement * rollForce, ForceMode2D.Impulse);
                }
            }
        }


        private void HandleAttackInput()
        {
            if ((Input.GetKeyDown(KeyCode.Z)) && (Time.time >= _timeForNextAttack))
            {
                if (debugMode) Debug.Log("[PlayerController] Basic attack input");
                PerformBasicAttack();
                _timeForNextAttack = Time.time + attackCooldown;
            }

            if ((Input.GetKeyDown(KeyCode.X)) && (Time.time >= _timeForNextAttack))
            {
                if (debugMode) Debug.Log("[PlayerController] Special attack input");


                PerformSpecialAttack();
                _timeForNextAttack = Time.time + attackCooldown;
            }
        }

        private void PerformSpecialAttack()
        {
            if (_attackHandler.CanUseSpecialAttack(_playerHealth.CurrentEnergy))
            {
                if (debugMode) Debug.Log("[PlayerController] Performing special attack");
                _playerHealth.DecreaseEnergy(_attackHandler.GetSpecialAttackEnergyCost());
                PlayAttackSound();
                animator.SetTrigger(ExitMotionTrigger);
                animator.SetTrigger(SpecialAttack);
                _stopMovement = true;
                _canTakeDamage = false; // Prevent player from taking damage during Special attack
            }
            else
            {
                if (debugMode) Debug.Log("[PlayerController] Not enough energy for special attack");
                Debug.Log("Not enough currentEnergy for special attack.");
            }
        }

        private void PlayAttackSound()
        {
            AudioManager.Instance.PlaySoundByAudioType(GameSoundsSo.AudioType.PlayerAttack);
        }

        public void HandleSpecialAttackLogic()
        {
            //check using raycast if player is in range of enemy
            //if yes, then use _attackHandler for perform attack for each enemy
            //if no, then do nothing
            Collider2D[] hits =
                Physics2D.OverlapCircleAll(attackTransform.position, checkRadius, LayerMask.GetMask("Enemy"));
            foreach (var hit in hits)
            {
                _attackHandler.PerformSpecialAttack(hit, attackTransform.position);
            }

            EventManager.Instance.InvokeEvent(EventNames.OnPlayerSpecialAttack, hits.Length);
            _canTakeDamage = true; // Allow player to take damage after Special attack
            _stopMovement = false; // Allow movement after Special attack
        }

        public void HandleBasicAttackLogic()
        {
            //check using raycast if player is in range of enemy
            //if yes, then use _attackHandler for perform attack for each enemy
            //if no, then do nothing
            Collider2D[] hits =
                Physics2D.OverlapCircleAll(attackTransform.position, checkRadius, LayerMask.GetMask("Enemy"));
            foreach (var hit in hits)
            {
                _attackHandler.PerformNormalAttack(hit, attackTransform.position);
            }

            _canTakeDamage = true; // Allow player to take damage after Basic attack
            _stopMovement = false; // Allow movement after basic attack
        }

        private void PerformBasicAttack()
        {
            PlayAttackSound();
            if (debugMode) Debug.Log("[PlayerController] Performing basic attack");
            _canTakeDamage = false; // Prevent player from taking damage during Basic attack
            _stopMovement = true;
            animator.SetTrigger(ExitMotionTrigger);
            animator.SetTrigger(BasicAttack);
        }

        private void FlipSprite()
        {
            // Flip the player sprite if moving in the opposite direction
            if ((_horizontalMovement > 0 && !_facingRight) || (_horizontalMovement < 0 && _facingRight))
            {
                _facingRight = !_facingRight; // Toggle facing direction
                Vector3 localScale = transform.localScale;
                localScale.x *= -1; // Flip the x-axis scale
                transform.localScale = localScale;
            }
        }

        private void OnEnable()
        {
            EventManager.Instance.AddListener(EventNames.OnTimeOver, HandleTimeOver);
        }
        private void OnDisable()
        {
            EventManager.Instance.RemoveListener(EventNames.OnTimeOver, HandleTimeOver);
        }

        private void HandleTimeOver(object obj)
        {
            HandlePlayerDeath();
        }


        public void TakeDamage(int amount)
        {
            if (!_canTakeDamage) return;
            if (debugMode) Debug.Log($"[PlayerController] TakeDamage: {amount}");
            //Debug.Log($"Player took {amount} damage!");
            _stopMovement = false;
            _playerHealth.DecreaseHealth(amount);
            if (_playerHealth.CurrentHealth <= 0)
            {
                HandlePlayerDeath();
                return;
            }

            animator.SetTrigger(ExitMotionTrigger);
            animator.SetTrigger(Hit);
            EventManager.Instance.InvokeEvent(EventNames.OnPlayerTakingDamage, amount);
        }

        private void HandlePlayerDeath()
        {
            _playerDie = true;
            if (debugMode) Debug.Log("[PlayerController] Player died");
            if (animator != null)
            {
                animator.Rebind();
                //animator.SetTrigger(ExitMotionTrigger);
                animator.SetTrigger(Die);
            }
            _canTakeDamage = false; // Prevent further damage
            _stopMovement = true;
            DisablePhysics();
            PlayDeathSound();
            EventManager.Instance.InvokeEvent(EventNames.OnPlayerDeath,false);
            
        }

        private void PlayDeathSound()
        {
            AudioManager.Instance.PlaySoundByAudioType(GameSoundsSo.AudioType.PlayerDie);
        }

        private void DisablePhysics()
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic; // Disable physics interactions
                rb.simulated = false; // Disable Rigidbody2D simulation
            }
        }

        public void SetSpecialAttack(AttackType attackType)
        {
            _attackHandler.SetSpecialAttack(attackType);
        }

        public void AddEnergy(int amount)
        {
            if (debugMode) Debug.Log($"[PlayerController] AddEnergy: {amount}");
            _playerHealth.IncreaseEnergy(amount);
        }

        public void AddLife(int amount)
        {
            if (debugMode) Debug.Log($"[PlayerController] AddLife: {amount}");
            _playerHealth.IncreaseHealth(amount);
        }


        public Transform GetPlayerLegsTransform()
        {
            return playerLegsTransform;
        }

        public Transform GetPlayerCenterTransform()
        {
            return playerCenterTransform;
        }

        private IEnumerator EnergyRegenRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(energyRegenInterval);
                if (_playerHealth != null && !_playerHealth.IsDead() &&
                    _playerHealth.CurrentEnergy < _playerHealth.MaxEnergy && !_isMoving)
                {
                    _playerHealth.IncreaseEnergy(energyRegenAmount);
                }
            }
        }

        private void StartEnergyRegen()
        {
            if (_energyRegenCoroutine == null)
            {
                _energyRegenCoroutine = StartCoroutine(EnergyRegenRoutine());
            }
        }


#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // Make sure the attackTransform exists
            if (attackTransform != null)
            {
                // By default, color is blue (no enemies detected)
                Color gizmoColor = Color.cyan;

                // Detect enemies in attack range
                Collider2D[] hits = Physics2D.OverlapCircleAll(
                    attackTransform.position,
                    checkRadius,
                    LayerMask.GetMask("Enemy")
                );

                // If at least one enemy is found, color will be red
                if (hits.Length > 0)
                    gizmoColor = Color.red;

                // Set Gizmo color
                Gizmos.color = gizmoColor;
                // Draw attack range circle
                Gizmos.DrawWireSphere(attackTransform.position, checkRadius);
            }
        }
#endif
    }
}