using System;

namespace ActionBuilder.Runtime
{
    [Serializable]
    public class DurationCompleted : ConditionBase
    {
        public override bool CanExecute(ActionBase controller)
        {
            return true;
        }
    }
}