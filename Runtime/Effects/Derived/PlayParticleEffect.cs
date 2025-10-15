using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ActionBuilder.Runtime
{
    [Serializable]
    public class PlayParticleEffect : EffectBase
    {
        private static Dictionary<GameObject, Queue<GameObject>> _particlePool = new Dictionary<GameObject, Queue<GameObject>>();

        public event Action<PlayParticleEffect> onParticleStart;
        public event Action<PlayParticleEffect> onParticleEnd;

        
        [Header("Particle Settings")]
        public GameObject particlePrefab;
        public bool usePooling = true;

        [TagDropdown]
        public string trackingTag;
        
        public Vector3 particleSize = Vector3.one;
        public Vector3 trackingOffset = Vector3.zero;

        
        private bool _isParticlePlaying;
        
        private float _particleStartTime;
        
        private float _particleDuration;
        
        private Transform _trackingTarget;
        
        private GameObject _currentParticleInstance;
        
        private ParticleSystem _currentParticleSystem;

        
        public bool isParticlePlaying
        {
            get { return _isParticlePlaying; }
        }

        public bool enableTracking
        {
            get;
            private set;
        }
        

        /// <summary> 파티클을 적용합니다 </summary>
        public override void OnApply()
        {
            if (this.particlePrefab == null)
            {
                Debug.LogError("Particle prefab is not assigned!");
            }
            else
            {
                this.PlayParticle();
            }
        }


        /// <summary> 파티클 상태를 업데이트합니다 </summary>
        public override void OnUpdate(float deltaTime)
        {
            if (_isParticlePlaying == false || _currentParticleInstance == null || _currentParticleSystem == null)
            {
                return;
            }

            if (this.enableTracking && _trackingTarget != null)
            {
                this.UpdateTracking();
            }

            this.CheckParticleEnd();
        }




        /// <summary> 파티클을 해제합니다 </summary>
        public override void OnRelease()
        {
            this.StopParticle();
        }


        /// <summary> 액션 종료시 파티클을 정리합니다 </summary>
        public override void OnActionEnd()
        {
            if (_currentParticleSystem == null || _isParticlePlaying == false)
            {
                return;
            }

            if (_currentParticleInstance.TryGetComponent(out ParticleObserver observer) == false)
            {
                observer = _currentParticleInstance.AddComponent<ParticleObserver>();
            }
            
            // 파티클이 끝날 때까지 독립적으로 관리하기 위해 MonoBehaviour 컴포넌트 추가
            observer.Initialize(this, _currentParticleSystem);

            // 현재 Effect에서는 더 이상 관리하지 않음
            _currentParticleInstance = null;
            _currentParticleSystem = null;
            _trackingTarget = null;
            _isParticlePlaying = false;
        }


        /// <summary> 파티클을 수동으로 정지합니다 </summary>
        public void StopParticle()
        {
            if (_currentParticleSystem == null || _isParticlePlaying == false)
            {
                return;
            }

            _currentParticleSystem.Stop();
            this.OnParticleFinished();
        }


        /// <summary> 파티클을 재생합니다 </summary>
        private void PlayParticle()
        {
            // 기존 파티클이 재실행되는 경우 리셋
            if (_currentParticleInstance != null)
            {
                this.ResetParticle();
            }

            _currentParticleInstance = this.GetParticleInstance();

            if (_currentParticleInstance == null)
            {
                return;
            }

            if (_currentParticleInstance.TryGetComponent(out ParticleSystem system))
            {
                _currentParticleSystem = system;
            }
            else
            {
                Debug.LogError("Particle prefab doesn't have ParticleSystem component!");
                this.ReturnToPool(_currentParticleInstance);
                return;
            }

            this.SetupParticle();

            this.enableTracking = string.CompareOrdinal(trackingTag, "Untagged") != 0;

            if (this.enableTracking)
            {
                _trackingTarget = transform.FindWithTag(trackingTag);
            }

            _currentParticleSystem.Play();
            _particleStartTime = elapsedTime;
            _isParticlePlaying = true;

            ParticleSystem.MainModule main = _currentParticleSystem.main;
            _particleDuration = main.duration + main.startLifetime.constantMax;
            onParticleStart?.Invoke(this);
        }


        /// <summary> 파티클을 리셋합니다 </summary>
        private void ResetParticle()
        {
            if (_currentParticleSystem != null)
            {
                _currentParticleSystem.Stop();
                _currentParticleSystem.Clear();
            }

            if (_currentParticleInstance != null)
            {
                this.ReturnToPool(_currentParticleInstance);
            }

            _currentParticleInstance = null;
            _currentParticleSystem = null;
            _trackingTarget = null;
            _isParticlePlaying = false;
        }


        /// <summary> 추적 대상의 위치를 업데이트합니다 </summary>
        private void UpdateTracking()
        {
            if (_trackingTarget == null || _currentParticleInstance == null)
            {
                return;
            }

            Vector3 targetPosition = _trackingTarget.position + trackingOffset;
            _currentParticleInstance.transform.position = targetPosition;
        }


        /// <summary> 파티클 종료를 확인합니다 </summary>
        private void CheckParticleEnd()
        {
            if (_currentParticleSystem.isPlaying)
            {
                return;
            }

            if ((elapsedTime - _particleStartTime) > _particleDuration)
            {
                this.OnParticleFinished();
            }
        }


        /// <summary> 파티클 인스턴스를 가져옵니다 </summary>
        private GameObject GetParticleInstance()
        {
            if (usePooling)
            {
                return this.GetFromPool(particlePrefab);
            }
            else
            {
                return Object.Instantiate(particlePrefab);
            }
        }


        /// <summary> 풀에서 파티클 오브젝트를 가져옵니다 </summary>
        private GameObject GetFromPool(GameObject prefab)
        {
            if (_particlePool.ContainsKey(prefab) == false)
            {
                _particlePool[prefab] = new Queue<GameObject>();
            }

            Queue<GameObject> pool = _particlePool[prefab];

            if (pool.Count == 0)
            {
                return Object.Instantiate(prefab);
            }

            GameObject pooledObject = pool.Dequeue();

            if (pooledObject != null)
            {
                pooledObject.SetActive(true);
                return pooledObject;
            }
            else
            {
                return Object.Instantiate(prefab);
            }
        }


        /// <summary> 파티클 오브젝트를 풀에 반환합니다 </summary>
        private void ReturnToPool(GameObject instance)
        {
            if (instance == null)
            {
                return;
            }

            if (usePooling == false)
            {
                Object.Destroy(instance);
                return;
            }

            instance.SetActive(false);

            if (_particlePool.ContainsKey(particlePrefab) == false)
            {
                _particlePool[particlePrefab] = new Queue<GameObject>();
            }

            _particlePool[particlePrefab].Enqueue(instance);
        }


        /// <summary> 파티클의 초기 설정을 적용합니다 </summary>
        private void SetupParticle()
        {
            _currentParticleInstance.transform.position = transform.position;
            _currentParticleInstance.transform.rotation = transform.rotation;
            _currentParticleInstance.transform.localScale = particleSize;
        }


        /// <summary> 파티클이 종료되었을 때의 처리를 수행합니다 </summary>
        private void OnParticleFinished()
        {
            _isParticlePlaying = false;
            onParticleEnd?.Invoke(this);

            if (_currentParticleInstance != null)
            {
                this.ReturnToPool(_currentParticleInstance);
                _currentParticleInstance = null;
            }

            _currentParticleSystem = null;
            _trackingTarget = null;
        }


        /// <summary> 독립 실행중인 파티클이 종료되었을 때 호출됩니다 </summary>
        internal void OnDetachedParticleFinished(GameObject particleInstance)
        {
            onParticleEnd?.Invoke(this);
            this.ReturnToPool(particleInstance);
        }
    }


    /// <summary> 독립 실행 파티클을 감시하는 MonoBehaviour 컴포넌트 </summary>
    internal class ParticleObserver : MonoBehaviour
    {
        private ParticleSystem _particleSystem;
        
        private PlayParticleEffect _owner;
        
        private bool _hasFinished;

        

        /// <summary> 파티클 와처를 초기화합니다 </summary>
        public void Initialize(PlayParticleEffect owner, ParticleSystem particle)
        {
            _owner = owner;
            _hasFinished = false;
            _particleSystem = particle;

            base.StartCoroutine(this.WatchParticle());
        }


        /// <summary> 파티클이 끝날 때까지 감시합니다 </summary>
        private IEnumerator WatchParticle()
        {
            while (_particleSystem != null && _particleSystem.isPlaying)
            {
                yield return null;
            }

            if (_hasFinished)
            {
                yield break;
            }

            _hasFinished = true;
            _owner?.OnDetachedParticleFinished(gameObject);
                
            Object.Destroy(this);
        }


        private void OnDestroy()
        {
            if (_hasFinished || _owner == null)
            {
                return;
            }

            _hasFinished = true;
            _owner?.OnDetachedParticleFinished(gameObject);
        }
    }
}