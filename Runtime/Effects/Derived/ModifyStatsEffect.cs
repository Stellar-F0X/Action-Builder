using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace ActionBuilder.Runtime
{
    public class ModifyStatsEffect : EffectBase
    {
        public List<StatModifierSelector> selectors = new List<StatModifierSelector>();

        private StatController _cachedStatController;



        protected override void OnValidateEffect()
        {
            if (this.selectors is null || this.selectors.Count == 0)
            {
                return;
            }
            
            if (base.action?.usingStatsTemplate == null)
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

            if (action.usingStatsTemplate.keyType != selectorType)
            {
                selectors.Clear();
                return;
            }

            if (selectors.Count == 1)
            {
                return;
            }

            int lastIdx = selectors.Count - 1;

            //유니티는 리스트를 새로 만들때 index - 1번째 아이템을 그냥 복사해서 만들어서 별도로 초기화 해줘야 됨.
            if (selectors[lastIdx - 1].modifier == selectors[lastIdx].modifier)
            {
                selectors[lastIdx] = null;
            }
        }


        public override void Apply()
        {
            Assert.IsNotNull(selectors);

            if (base.action.statController == null)
            {
                _cachedStatController = base.action.controller.GetComponent<StatController>();
            }
            else
            {
                _cachedStatController = base.action.statController;
            }

            foreach (StatModifierSelector selector in selectors)
            {
                Stat stat = _cachedStatController.GetStat(selector.statKey);
                stat?.AddModifier(selector.modifier);
            }
        }


        public override void OnRelease()
        {
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