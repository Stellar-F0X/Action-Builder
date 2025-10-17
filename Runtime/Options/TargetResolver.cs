using System;
using System.Linq;
using HideIf.Runtime;
using UnityEngine;

namespace ActionBuilder.Runtime
{
    [Serializable]
    public abstract class TargetResolver
    {
        public SpawnAnchorMode spawnAnchorMode;
        public bool spawnOnly;  

        [TagDropdown]
        public string trackingTag;
        public string objectName;



        public virtual Transform GetTrackingTarget(EffectBase effect)
        {
            switch (spawnAnchorMode)
            {
                case SpawnAnchorMode.InternalTransform:
                {
                    return effect.action.controller.transform.FindAllWithTag(trackingTag).FirstOrDefault(transform => transform.name == objectName);
                }

                case SpawnAnchorMode.ExternalTarget:
                {
                    return GameObject.FindGameObjectsWithTag(trackingTag)?.FirstOrDefault(gobj => gobj.name == objectName)?.transform;
                }

                case SpawnAnchorMode.CachedTarget:
                {
                    return effect.action.targets.FirstOrDefault(gobj => gobj.CompareTag(trackingTag) && gobj.name == objectName)?.transform;
                }
            }

            return null;
        }


        public virtual Vector3 GetTrackingPosition(EffectBase effect)
        {
            return this.GetTrackingTarget(effect).position;
        }
    }
}