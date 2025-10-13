using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace ActionBuilder.Runtime
{
    [Serializable]
    public class ModifyStatsEffect : EffectBase
    {
        public StatModifierReleaseOption statReleaseOption;
        
        public List<StatModifierSelector> selectors = new List<StatModifierSelector>();

        private StatController _cachedStatController;
        
        

        public override void OnValidateEffect()
        {
            if (this.selectors is null || this.selectors.Count == 0)
            {
                return;
            }
            
            if (base.referencedAction?.usingStatsTemplate == null)
            {
                return;
            }

            StatModifierSelector selector = selectors[0];
            
            //업데이트 순서로 인해 null이 될 수도 있지만 다음 프레임에 바로 잡아지니 오류로 체크하기보단 그냥 early return.
            if (string.IsNullOrEmpty(selector.keyTypeName))
            {
                return;
            }
            
            //selector의 typeName이 null이 아닌데, 타입을 못 구하면 확실히 문제임.
            Type selectorType = Type.GetType(selector.keyTypeName);
            Assert.IsNotNull(selectorType, "select's type name is null");

            if (referencedAction.usingStatsTemplate.keyType == selectorType)
            {
                return;
            }

            selectors.Clear();
        }


        protected override void Apply()
        {
            Assert.IsNotNull(selectors);

            if (base.referencedAction.statController == null)
            {
                _cachedStatController = base.referencedAction.owner.GetComponent<StatController>();
            }
            else
            {
                _cachedStatController = base.referencedAction.statController;
            }

            foreach (StatModifierSelector selector in selectors)
            {
                Stat stat = _cachedStatController.GetStat(selector.statKey);
                stat?.AddModifier(selector.modifier);
            }
        }


        public override void OnActionEnd()
        {
            if (statReleaseOption != StatModifierReleaseOption.OnActionEnd)
            {
                return;
            }
            
            this.ReleaseModifiers();
            this._cachedStatController = null;
        }


        public override void OnRelease()
        {
            if (statReleaseOption != StatModifierReleaseOption.OnEffectRelease)
            {
                return;
            }
            
            this.ReleaseModifiers();
            this._cachedStatController = null;
        }


        private void ReleaseModifiers()
        {
            Assert.IsNotNull(selectors);

            foreach (StatModifierSelector selector in selectors)
            {
                Stat stat = _cachedStatController.GetStat(selector.statKey);
                stat?.RemoveModifier(selector.modifier);
                StatModifierPool.Release(selector.modifier);
            }
        }
    }
}