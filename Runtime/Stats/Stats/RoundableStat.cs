using System;
using UnityEngine.Assertions;

namespace ActionBuilder.Runtime
{
    [Serializable]
    public class RoundableStat : Stat
    {
        public int digits;
        

        public override float value
        {
            get { return base.value; }
            
            set { base.value = (float)Math.Round(value, digits); }
        }
        
        
        public override object Clone()
        {
            RoundableStat roundableStat = base.Clone() as RoundableStat;
            Assert.IsNotNull(roundableStat);

            roundableStat.digits = this.digits;
            return roundableStat;
        }
    }
}
