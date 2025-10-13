using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace ActionBuilder.Runtime
{
    [Serializable]
    public class ModifyStatsEffect : EffectBase
    {
        public List<StatModifierSelector> modifiers = new List<StatModifierSelector>();


        public override void OnValidateEffect()
        {
            if (modifiers is null || modifiers.Count == 0)
            {
                return;
            }

            if (referencedAction?.usingStatsTemplate == null)
            {
                return;
            }

            StatModifierSelector selector = modifiers[0];
            
            //업데이트 순서로 인해 null이 될 수도 있지만 다음 프레임에 바로 잡아지니 오류로 체크하기보단 그냥 early return.
            if (string.IsNullOrEmpty(selector.typeName))
            {
                return;
            }
            
            //selector의 typeName이 null이 아닌데, 타입을 못 구하면 확실히 문제임.
            Type selectorType = Type.GetType(selector.typeName);
            Assert.IsNotNull(selectorType, "select's type name is null");

            if (referencedAction.usingStatsTemplate.keyType != selectorType)
            {
                modifiers.Clear();
            }
        }
    }
}