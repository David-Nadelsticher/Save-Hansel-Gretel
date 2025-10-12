using System;
using System.Collections;
using BossProject.Core;
using Core.Data;
using Core.Managers;
using DG.Tweening;
using Gameplay.Controls.Player;
using Gameplay.General.Utils;
using Gameplay.Providers.Pool;
using UnityEngine;
using Object = System.Object;

namespace Gameplay.Controls.Collectable
{
    public class CollectableObject : BaseMono, IPoolable
    {
        [SerializeField] private float riseHeight = 2f; // Height to rise
        [SerializeField] private float riseDuration = 0.3f; // Fast rise time
        [SerializeField] private float fallDuration = 1.5f; // Slow fall time
        [SerializeField] private int energyAmount = 100;
        [SerializeField] private int healthAmount = 100;
        [SerializeField] private AttackType attackType;
        [SerializeField] private CollectableType collectableType;
        [SerializeField] private string collectableName;
        [SerializeField] private Transform transformToAnimate;
        [SerializeField] private float delayBeforeReturn = 0.5f;
        
        
        private Vector3 _startPosition;
        private Vector3 _transformToAnimateStartPosition;
        private Coroutine _returnToPoolCoroutine;
        private CollectableEffect _effect;
        private bool _isCollected;


        public void InitializeEffect(CollectableEffect effect)
        {
            _effect = effect;
        }

        public void InitializeEffect(CollectableType collectableType)
        {
            switch (collectableType)
            {
                case CollectableType.SpecialAttack:
                    _effect = new SpecialAttackCollectable(attackType);
                    Debug.Log("Initialize Special Attack");
                    break;
                case CollectableType.Life:
                    _effect = new LifeCollectable(healthAmount);
                    Debug.Log("Initialize Health");
                    break;
                default:
                    _effect = new EnergyCollectable(energyAmount);
                    Debug.Log("Initialize Energy");
                    break;
            }
        }

        private void Start()
        {
            InitializeEffect(collectableType);
            InitializeStartPosition();
        }

        public void OnCollect(PlayerController player)
        {
            PlayCollectableSound();
            _effect.ApplyReward(player);
        }

        private void PlayCollectableSound()
        {
            AudioManager.Instance.PlaySoundByAudioType(GameSoundsSo.AudioType.CollectableCollect);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            //  Debug.Log("Collision Enter");
            if (_isCollected) return;

            if (collision.CompareTag("Player")) // Make sure the player has the correct tag
            {
                _isCollected = true;

                var player = collision.transform.gameObject;
                PlayerController playerController = player.GetComponentInParent<PlayerController>();
                if (playerController)
                {
                    EventManager.Instance.InvokeEvent(EventNames.OnPlayerCollect, collectableName);
                    OnCollect(playerController);
                }

                AnimateCollectable();
            }
        }

        private void AnimateCollectable()
        {
            Debug.Log("Animating collectable");
            Debug.Log(collectableType.ToString());
            AnimateRing();
        }

        private void AnimateRing()
        {
            // Move up quickly
            transformToAnimate.DOMoveY( /*transformToAnimate.position.y*/
                    _transformToAnimateStartPosition.y + riseHeight, riseDuration)
                .SetEase(Ease.OutQuad) // Eases out for a natural feel
                .OnComplete(() =>
                {
                    // Float down slowly like a balloon
                    transformToAnimate.DOMoveY(_transformToAnimateStartPosition.y, fallDuration)
                        .SetEase(Ease.InOutSine).OnComplete(() =>
                        {
                            // Return to the pool after the animation completes
                            _returnToPoolCoroutine = StartCoroutine(ReturnToPoolAfterDelay(delayBeforeReturn));
                            Debug.Log("Collectable returned to pool after delay");
                        });
                });
        }

        private IEnumerator ReturnToPoolAfterDelay(float delay)
        {
            // Wait for the specified delay before returning to the pool
            yield return new WaitForSeconds(delay);
            _returnToPoolCoroutine = null;
            CollectableObjectPool.Instance.ReturnToPool(collectableName, this);
        }

        public void ReturnCollectableToPool()
        {
            _isCollected = true;

            if (_returnToPoolCoroutine != null)
            {
                StopCoroutine(_returnToPoolCoroutine);
            }

            _returnToPoolCoroutine = StartCoroutine(ReturnToPoolAfterDelay(1f));
        }

        public void HandlePlayerReachedCheckpoint(Object obj)
        {
            Checkpoint.CheckpointData data = obj as Checkpoint.CheckpointData;
            if (data == null) return;

            if ((data.Position != Vector2.zero) && (data.Position.x > transform.position.x))
            {
                Debug.Log(
                    $"Collectable {collectableName} returned to pool because player at checkpoint position: {data.Position}");
                ReturnCollectableToPool();
            }
        }


        public void InitializeStartPosition()
        {
            _startPosition = transform.position;
            if (transformToAnimate != null) _transformToAnimateStartPosition = transformToAnimate.position;
        }

        private void OnDisable()
        {
            if (transformToAnimate != null)
                DOTween.Kill(transformToAnimate);

            DOTween.Kill(transform);
            if (_returnToPoolCoroutine != null)
            {
                StopCoroutine(_returnToPoolCoroutine);
                _returnToPoolCoroutine = null;
            }

            EventManager.Instance.RemoveListener(EventNames.OnCheckpointReached, HandlePlayerReachedCheckpoint);
        }

        private void OnEnable()
        {
            EventManager.Instance.AddListener(EventNames.OnCheckpointReached, HandlePlayerReachedCheckpoint);
        }


        public void Reset()
        {
            // Reset the collectable to its initial state
            _isCollected = false;
            InitializeStartPosition();
        }
    }
}