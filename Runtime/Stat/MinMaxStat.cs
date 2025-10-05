using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace StatController.Runtime
{
    [Serializable]
    public class MinMaxStat : Stat
    {
        public float min;
        public float max;


        public override float value
        {
            get { return _value; }
            set { _value = Mathf.Clamp(value, min, max); }
        }


        public override object Clone()
        {
            MinMaxStat minMaxStat = base.Clone() as MinMaxStat;
            Assert.IsNotNull(minMaxStat);

            minMaxStat.max = this.max;
            minMaxStat.min = this.min;
            return minMaxStat;
        }
    }
}