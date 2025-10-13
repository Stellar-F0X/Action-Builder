using System;
using UnityEngine;

namespace ActionBuilder.Runtime
{
    [Serializable]
    public abstract class StatModifierBase
    {
        public StatModifierBase() { }

        
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

        [SerializeField]
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


        public virtual float Calculate(float leftValue)
        {
            switch (this._type)
            {
                case StatModifierType.Override: return _operand;

                case StatModifierType.Additive: return leftValue + _operand;

                case StatModifierType.Multiplicative: return leftValue * _operand;
            }

            return leftValue;
        }
    }
}