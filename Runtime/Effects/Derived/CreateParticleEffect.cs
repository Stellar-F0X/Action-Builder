using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace ActionBuilder.Runtime
{
    public class CreateParticleEffect : EffectBase
    {
        public event Action<ParticleSystem> onParticleStarted;
        public event Action<ParticleSystem> onParticleEnded;

        [Header("Particle Settings")]
        public GameObject particlePrefab;

        [SerializeReference, SubclassSelector]
        public TargetResolver targetResolver = new DefaultTargetResolver();

        public TransformOffset particleOffset = new TransformOffset()
        {
                positionOffset = Vector3.zero,
                rotationOffset = Vector3.zero,
                size = Vector3.one
        };

        private Transform _trackingTransform;
        private List<ParticleSystem> _particles = new List<ParticleSystem>();



        protected override void OnApply()
        {
            if (particlePrefab == null)
            {
                return;
            }

            if (targetResolver is not null)
            {
                _trackingTransform = targetResolver.GetTrackingTarget(this);
            }

            GameObject instantiated = Object.Instantiate(particlePrefab);
            _particles.Add(instantiated.GetComponent<ParticleSystem>());

            if (_particles == null)
            {
                return;
            }

            ParticleSystem activeParticle = _particles.Last();
            activeParticle.transform.localScale = particleOffset.size;

            this.UpdateParticleTransform();

            if (activeParticle.main.playOnAwake == false)
            {
                activeParticle.Play();
            }

            onParticleStarted?.Invoke(activeParticle);
        }


        protected override void OnUpdate(float deltaTime)
        {
            if (targetResolver.spawnOnly)
            {
                return;
            }

            this.UpdateParticleTransform();
        }


        protected override void OnRelease()
        {
            ParticleSystem activeParticle = _particles.Last();

            if (activeParticle.isStopped == false)
            {
                activeParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            onParticleEnded?.Invoke(activeParticle);
            activeParticle.gameObject.SetActive(false);
        }


        private void UpdateParticleTransform()
        {
            if (targetResolver is null || _particles.Count == 0)
            {
                return;
            }

            Transform particleTrans = _particles.Last().transform;

            particleTrans.position = _trackingTransform.position + particleOffset.positionOffset;
            particleTrans.rotation = _trackingTransform.rotation * Quaternion.Euler(particleOffset.rotationOffset);
        }


        private void OnDestroy()
        {
            Assert.IsNotNull(_particles, $"{typeof(CreateParticleEffect)}: particles is null");

            foreach (ParticleSystem particle in _particles.Where(p => p != null))
            {
                Singleton<MonoObserver>.Instance.Register(particle.gameObject, () => !particle.IsAlive(false), Object.Destroy);
            }
        }
    }
}