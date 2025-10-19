using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace ActionBuilder.Runtime
{
    public class PlayParticleEffect : EffectBase
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
        private ParticleSystem _particle;



        public override void OnApply()
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
            _particle = instantiated.GetComponent<ParticleSystem>();

            if (_particle == null)
            {
                return;
            }

            _particle.transform.localScale = particleOffset.size;

            this.UpdateParticleTransform();

            if (_particle.main.playOnAwake == false)
            {
                _particle.Play();
            }

            onParticleStarted?.Invoke(_particle);
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
            if (_particle.isStopped == false)
            {
                _particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            onParticleEnded?.Invoke(_particle);
            _particle.gameObject.SetActive(false);
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

        
        private void OnDestroy()
        {
            this.Release();
            
            Object.Destroy(_particle.gameObject);
        }
    }
}