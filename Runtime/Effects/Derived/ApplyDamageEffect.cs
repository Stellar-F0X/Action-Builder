using System;

namespace ActionBuilder.Runtime
{
    //근접 공격의 데미지는 콜라이더에 닿는 즉시 전달되어야 함. 이거 염두하셈. 
    [Serializable]
    public class ApplyDamageEffect : EffectBase
    {
        public int a;
    }
}