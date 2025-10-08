using System;
using UnityEngine;

namespace StatController.Runtime
{
    [Serializable]
    public abstract class StatModifierBase
    {
        protected StatModifierBase(string name, float operand, int priority, StatModifierType type)
        {
            this._name = name;
            this._type = type;
            this._priority = priority;
            this._operand = operand;
        }

        [SerializeField]
        protected string _name;
        
        [SerializeField]
        protected int _priority;
        
        [SerializeField]
        protected StatModifierType _type;
     
        [SerializeField, ReadOnly]
        protected float _operand;
        protected Stat _basedStat;

        
        public float operand
        {
            get { return _operand; }
        }

        public string name
        {
            get { return _name; }
        }

        public int priority
        {
            get { return _priority; }
        }

        public StatModifierType modifierType
        {
            get { return _type; }
        }
        
        
        public abstract float Calculate(float leftValue);
    }
}