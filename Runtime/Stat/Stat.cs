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
        protected float _baseValue;
        protected bool _changedModifiers = true; //최초 한 번만.
        
        [SerializeField, ReadOnly]
        protected float _finalValue;

        [SerializeReference, ReadOnly]
        private List<StatModifierBase> _modifiers = new List<StatModifierBase>();


        public virtual float value
        {
            get
            {
                if (this._changedModifiers)
                {
                    this._changedModifiers = false;
                    this._finalValue = this.ApplyModifiers();
                }

                return this._finalValue;
            }

            set
            {
                this._finalValue = value;
            }
        }


        public virtual float baseValue
        {
            get { return _baseValue; }

            set { _baseValue = value; }
        }



        protected virtual float ApplyModifiers()
        {
            if (_modifiers.Count > 1)
            {
                _modifiers.Sort(this.DetermineModifierPriority);
            }

            float currentValue = this._baseValue;

            for (int index = 0; index < _modifiers.Count; ++index)
            {
                StatModifierBase modifier = _modifiers[index];
                currentValue = modifier.Calculate(currentValue);
            }

            return currentValue;
        }


        public void AddModifiers(StatModifierBase[] modifier)
        {
            _modifiers.AddRange(modifier);
            _changedModifiers = true;
        }


        public void AddModifier(StatModifierBase modifier)
        {
            _modifiers.Add(modifier);
            _changedModifiers = true;
        }


        public bool RemoveModifier(StatModifierBase modifier)
        {
            _changedModifiers = _modifiers.Remove(modifier);
            return _changedModifiers;
        }


        public bool RemoveModifier(string name)
        {
            StatModifierBase modifier = _modifiers?.Find(m => m.name == name);

            if (string.IsNullOrEmpty(modifier?.name) || string.Compare(modifier.name, name) != 0)
            {
                return false;
            }
            else
            {
                return this.RemoveModifier(modifier);
            }
        }


        public IReadOnlyList<StatModifierBase> GetModifiers(Func<float, bool> condition = null)
        {
            if (condition is null)
            {
                return _modifiers;
            }
            else
            {
                return _modifiers.Where(m => condition.Invoke(m.operand)).ToList();
            }
        }


        private int DetermineModifierPriority(StatModifierBase l, StatModifierBase r)
        {
            if (l.priority != r.priority)
            {
                return l.priority > r.priority ? -1 : 1;
            }
            else
            {
                return ((int)l.modifierType).CompareTo((int)r.modifierType);
            }
        }


        public virtual object Clone()
        {
            Stat clonedStat = Activator.CreateInstance(this.GetType()) as Stat;
            Assert.IsNotNull(clonedStat, "Failed to create clone of Stat");

            clonedStat._modifiers = this._modifiers.ToList();
            clonedStat._baseValue = this._baseValue;
            return clonedStat;
        }
    }
}