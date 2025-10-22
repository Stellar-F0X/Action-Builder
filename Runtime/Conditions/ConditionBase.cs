using System;

namespace ActionBuilder.Runtime
{
    [Serializable]
    public abstract class ConditionBase
    {
        public virtual void Reset() { }
        
        public abstract bool CanExecute(ActionBase controller); 
    }
}