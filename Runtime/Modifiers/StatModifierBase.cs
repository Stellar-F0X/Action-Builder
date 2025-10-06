using System;
using UnityEngine;

namespace StatController.Runtime
{
    [Serializable]
    public abstract class StatModifierBase
    {
        protected StatModifierBase(string name, float rightOperand, int priority, StatModifierType modifierType)
        {
            this.name = name;
            this.priority = priority;
            this.modifierType = modifierType;
            this._rightOperand = rightOperand;
        }
        
        public string name;
        public int priority;

        public StatModifierType modifierType;
     
        [SerializeField, ReadOnly]
        protected float _rightOperand;
        protected Stat _basedStat;

        
        public float rightOperand
        {
            get { return _rightOperand; }
        }
        
        
        public abstract float Calculate(float leftValue);
    }
}