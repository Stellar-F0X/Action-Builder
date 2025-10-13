using System;

namespace ActionBuilder.Runtime
{
    [Serializable]
    public abstract class ConditionBase
    {
        public abstract bool CanExecute(ActionBase controller); 
    }
}