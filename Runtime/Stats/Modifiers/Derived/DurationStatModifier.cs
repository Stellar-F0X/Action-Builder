using System;
using System.Timers;
using UnityEngine;

namespace ActionBuilder.Runtime
{
    [Serializable]
    public class DurationStatModifier : StatModifierBase
    {
        public DurationStatModifier() : base() { }

        public DurationStatModifier(string name, float operand, float duration, int priority = 0, StatModifierType modifierType = StatModifierType.Override) : base(name, operand, priority, modifierType)
        {
            this.duration = duration;
        }
        
        [Min(0)]
        public float duration;

        [NonSerialized]
        private Timer _durationTimer;

        
        public override void OnStatModifierAttached()
        {
            base.OnStatModifierAttached();
            this.StartDurationTimer();
        }
        

        public override void OnStatModifierDetached()
        {
            base.OnStatModifierDetached();
            this.StopDurationTimer();
        }
        

        protected override void OnReset()
        {
            base.OnReset();
            this.duration = 0f;
            this.StopDurationTimer();
        }

        
        private void StartDurationTimer()
        {
            if (this.duration <= 0f || base.basedStat == null)
            {
                return;
            }

            this.StopDurationTimer();

            _durationTimer = new Timer(duration * 1000);
            _durationTimer.Elapsed += this.OnDurationExpired;
            _durationTimer.AutoReset = false;
            _durationTimer.Start();
        }

        
        private void StopDurationTimer()
        {
            if (_durationTimer == null)
            {
                return;
            }

            _durationTimer.Stop();
            _durationTimer.Elapsed -= this.OnDurationExpired;
            _durationTimer.Dispose();
            _durationTimer = null;
        }
        

        private void OnDurationExpired(object sender, ElapsedEventArgs e)
        {
            basedStat?.RemoveModifierThreadSafe(this);
        }
    }
}