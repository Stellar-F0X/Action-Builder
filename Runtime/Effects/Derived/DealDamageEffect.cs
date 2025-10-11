namespace ActionBuilder.Runtime
{
    public class DealDamageEffect : EffectBase
    {
        //Effect는 수동으로, 그리고 타이밍에 맞춰서 실행될 수 있어야 됨. 
        //3타 같은건 Action으로 구현해야될 것 같음, 3타를 이루는 각 타격.
        //1타씩 때릴때마다 적의 디버프 큐? 같은거에 해당 액션에 대한 효과를 삽입하고. 
        //때릴 때마다 이를 확인한 뒤, 추가적인 효과를 입힐 수 있으면 입히는 식으로 구현해야 됨.
        
        //따라서 적용된 효과가 있을 때, 어떤 효과를 더 입힐건진 유저가 구현하게 만들고,
        //여기서 체크할 수 있는 옵션은 효과가 있을 때 기본 데미지를 입힐건지, 아니면 효과 데미지만 입히고 기본 데미지는 입히지 않을건지 정도가 될 것 같음.
        //치명타 정책 같은건 따로 Policy로 빼는게 맞을 듯.
        
        public virtual void TrySearchForTarget() { }
    }
}