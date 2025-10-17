using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace ActionBuilder.Runtime
{
    public class PlayParticleEffect : EffectBase
    {
        public event Action<ParticleSystem> onParticleStart;
        public event Action<ParticleSystem> onParticleEnd;

        [Header("Particle Settings")]
        public GameObject particlePrefab;

        public ParticleSystemStopBehavior stopBehavior;

        [SerializeReference, SubclassSelector]
        public TargetResolver targetResolver;

        public TransformOffset particleOffset;

        private Transform _trackingTransform;
        private ParticleSystem _particle;


        public override void OnApply()
        {
            if (targetResolver is not null)
            {
                _trackingTransform = targetResolver.GetTrackingTarget(this);
            }


            GameObject instantiated = Object.Instantiate(particlePrefab);
            Assert.IsNotNull(instantiated);

            _particle = instantiated.GetComponent<ParticleSystem>();
            Assert.IsNotNull(_particle);

            _particle.transform.localScale = particleOffset.sizeOffset;

            this.UpdateParticleTransform();

            if (_particle.main.playOnAwake == false)
            {
                _particle.Play();
            }

            onParticleStart?.Invoke(_particle);
        }


        public override void OnUpdate(float deltaTime)
        {
            if (targetResolver.spawnOnly)
            {
                return;
            }

            this.UpdateParticleTransform();
        }


        public override void OnRelease()
        {
            if (_particle.isStopped)
            {
                onParticleEnd?.Invoke(_particle);
                return;
            }

            _particle.Stop(true, stopBehavior);

            if (stopBehavior != ParticleSystemStopBehavior.StopEmitting)
            {
                return;
            }

            Singleton<MonoObserver>.Instance.Register(() => _particle.isStopped, _particle, this.OnParticleStopped);
        }



        private void OnParticleStopped()
        {
            this.onParticleEnd?.Invoke(_particle);
            Object.DestroyImmediate(_particle.gameObject);
        }

        

        private void UpdateParticleTransform()
        {
            if (targetResolver is null)
            {
                return;
            }

            Transform particleTrans = _particle.transform;

            particleTrans.position = _trackingTransform.position + particleOffset.positionOffset;
            particleTrans.rotation = _trackingTransform.rotation * Quaternion.Euler(particleOffset.rotationOffset);
        }
    }
}