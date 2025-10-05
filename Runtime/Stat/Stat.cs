using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace StatController.Runtime
{
    [Serializable]
    public class Stat : ICloneable
    {
        [SerializeField]
        protected float _value;

        [SerializeReference, ReadOnly]
        private List<IStatModifier> _modifiers = new List<IStatModifier>();


        public virtual float value
        {
            get { return _value; }
            set { _value = value; }
        }

        public IReadOnlyList<IStatModifier> modifiers
        {
            get { return _modifiers; }
        }


        public void AddModifiers(IStatModifier[] modifier)
        {
            _modifiers.AddRange(modifier);
        }


        public void AddModifier(IStatModifier modifier)
        {
            _modifiers.Add(modifier);
        }


        public bool RemoveModifier(IStatModifier modifier)
        {
            return _modifiers.Remove(modifier);
        }


        public bool RemoveModifier(string identity)
        {
            return _modifiers.Remove(_modifiers.Find(m => string.Compare(m.identity, identity) == 0));
        }


        protected virtual void ApplyModifiers()
        {
            _modifiers.Sort(this.DetermineModifierPriority);

            float currentValue = this.value;

            for (int i = 0; i < _modifiers.Count; ++i)
            {
                currentValue = _modifiers[i].Calculate(currentValue);
            }

            this.value = currentValue;
        }


        protected virtual int DetermineModifierPriority(IStatModifier l, IStatModifier r)
        {
            if (l.priority != r.priority)
            {
                return l.priority > r.priority ? -1 : 1;
            }
            else
            {
                return this.TypeOrder(l.modifierType).CompareTo(this.TypeOrder(r.modifierType));
            }
        }


        private int TypeOrder(StatModifierType t)
        {
            switch (t)
            {
                case StatModifierType.Multiplicative: return 1;

                case StatModifierType.Override: return 2;

                case StatModifierType.Additive: return 0;

                default: return 0;
            }
        }


        public virtual object Clone()
        {
            Stat clonedStat = Activator.CreateInstance(this.GetType()) as Stat;
            Assert.IsNotNull(clonedStat, "Failed to create clone of Stat instance");

            clonedStat._modifiers = this._modifiers.ToList();
            clonedStat._value = this._value;

            return clonedStat;
        }
    }
}