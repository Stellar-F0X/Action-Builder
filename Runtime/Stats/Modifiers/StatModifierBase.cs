using System;
using UnityEngine;

namespace ActionBuilder.Runtime
{
    [Serializable]
    public abstract class StatModifierBase
    {
        protected StatModifierBase() { }

        
        protected StatModifierBase(string name, float operand, int priority, StatModifierType modifierType)
        {
            this.name = name;
            this.modifierType = modifierType;
            this.priority = priority;
            this.operand = operand;
        }
        
        public string name;
        
        public int priority;
        
        public StatModifierType modifierType;
        
        public float operand;


        internal Stat basedStat
        {
            set;
            get;
        }


        public void Reset()
        {
            name = string.Empty;
            priority = 0;
            modifierType = default;
            operand = 0f;
            basedStat = null;
            
            this.OnReset();
        }


        public virtual float Calculate(float leftValue)
        {
            switch (this.modifierType)
            {
                case StatModifierType.Override: return operand;

                case StatModifierType.Additive: return leftValue + operand;

                case StatModifierType.Multiplicative: return leftValue * operand;
            }

            return leftValue;
        }
        
        
        protected virtual void OnReset () { }
        
        
        public virtual void OnStatModifierAttached() { }
        
        
        public virtual void OnStatModifierDetached() { }
    }
}