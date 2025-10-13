using System;
using UnityEngine;

namespace ActionBuilder.Runtime
{
    public class ChargeableAction : ActionBase
    {
        public event Action<ChargeableAction, float> onChargeStarted;
        
        public event Action<ChargeableAction, float> onChargeUpdated;
        
        public event Action<ChargeableAction, float> onChargeCompleted;
        
        public event Action<ChargeableAction> onChargeCancelled;
        
        public event Action<ChargeableAction> onChargeReleased;


        [Header("Charge Configuration"), SerializeField]
        private ChargeData _chargeData;


        private bool _isCharging;
        
        private bool _chargeCompleted;
        
        private float _chargeStartTime;
        
        private float _currentChargeTime;
        




#region Properties

        public ChargeData chargeData
        {
            get { return _chargeData; }
        }


        public bool isCharging
        {
            get { return _isCharging; }
        }


        public float currentChargeTime
        {
            get { return _currentChargeTime; }
        }


        public float chargeStartTime
        {
            get { return _chargeStartTime; }
        }


        public float chargeProgress
        {
            get { return _isCharging ? Mathf.Clamp01(_currentChargeTime / _chargeData.maxChargeTime) : 0; }
        }


        public bool isMinChargeReached
        {
            get { return _currentChargeTime >= _chargeData.minChargeTime; }
        }


        public bool isMaxChargeReached
        {
            get { return _currentChargeTime >= _chargeData.maxChargeTime; }
        }


        public bool isChargeCompleted
        {
            get { return _chargeCompleted; }
        }


        public float chargeLevel
        {
            get { return this.CalculateChargeProgress(); }
        }


        public override float progress
        {
            get { return chargeProgress; }
        }

#endregion



#region Charge Control Methods

        public bool StartCharge()
        {
            if (_isCharging)
            {
                return false;
            }

            if (isOnCooldown)
            {
                return false;
            }

            this._isCharging = true;
            this._chargeStartTime = Time.time;
            this._currentChargeTime = 0f;
            this._chargeCompleted = false;

            if (this.Trigger() == false)
            {
                this._isCharging = false;
                return false;
            }
            else
            {
                this.OnChargeStart();
                this.onChargeStarted?.Invoke(this, _currentChargeTime);
                return true;
            }
        }



        public void StopCharge()
        {
            if (_isCharging == false)
            {
                return;
            }

            float finalChargeTime = _currentChargeTime;
            bool wasCompleted = _chargeCompleted;

            this._isCharging = false;
            this._chargeCompleted = false;

            this.OnChargeStop(finalChargeTime, wasCompleted);

            if (wasCompleted)
            {
                switch (_chargeData.completionBehavior)
                {
                    case ChargeCompletionBehavior.Execute: this.ExecuteChargedAction(finalChargeTime); break;

                    case ChargeCompletionBehavior.Cancel: this.Cancel(); break;
                }
            }
            else if (this.isMinChargeReached)
            {
                this.ExecuteChargedAction(finalChargeTime);
            }
            else
            {
                this.Cancel();
            }

            this.onChargeReleased?.Invoke(this);
        }



        public bool ToggleCharge()
        {
            if (_chargeData.chargeType != ChargeType.Toggle)
            {
                Debug.LogWarning("ToggleCharge can only be used with Toggle charge type");
                return false;
            }

            if (_isCharging)
            {
                this.StopCharge();
                return false;
            }
            else
            {
                return this.StartCharge();
            }
        }
        
        
        
        private float CalculateChargeProgress()
        {
            if (_isCharging)
            {
                float tempRange = _chargeData.maxChargeTime - _chargeData.minChargeTime;
                float tempDiff = _currentChargeTime - _chargeData.minChargeTime;
                return Mathf.Clamp01(tempDiff / tempRange);
            }
            else
            {
                return 0f;
            }
        }



        public void ResetChargeState()
        {
            this._isCharging = false;
            this._currentChargeTime = 0f;
            this._chargeCompleted = false;
        }



        public void ClearChargeEvents()
        {
            this.onChargeStarted = null;
            this.onChargeUpdated = null;
            this.onChargeCompleted = null;
            this.onChargeCancelled = null;
            this.onChargeReleased = null;
        }

#endregion



#region Override Methods

        protected override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);

            if (_isCharging == false)
            {
                return;
            }

            this._currentChargeTime += deltaTime;

            this.onChargeUpdated?.Invoke(this, _currentChargeTime);
            this.OnChargeUpdate(_currentChargeTime, deltaTime);

            if (_chargeCompleted || this.isMaxChargeReached == false)
            {
                return;
            }

            this._chargeCompleted = true;
            this.OnChargeComplete(_currentChargeTime);
            this.onChargeCompleted?.Invoke(this, _currentChargeTime);

            switch (_chargeData.completionBehavior)
            {
                case ChargeCompletionBehavior.Execute: this.StopCharge(); break;

                case ChargeCompletionBehavior.Cancel: this.Cancel(); break;
            }
        }



        protected override void OnCancel()
        {
            base.OnCancel();

            if (_isCharging)
            {
                this._isCharging = false;
                this._chargeCompleted = false;
                this.OnChargeCancelled();
                this.onChargeCancelled?.Invoke(this);
            }
        }



        protected override void OnEnd()
        {
            base.OnEnd();

            if (_isCharging)
            {
                this._isCharging = false;
                this._chargeCompleted = false;
            }
        }



        public override string ToString()
        {
            return
                $"{typeof(ChargeableAction)} {actionName} (Charging: {_isCharging}, ChargeTime: {_currentChargeTime:F2}s, Progress: {chargeProgress:P0}, State: {currentState})";
        }

#endregion



#region Virtual Methods

        protected virtual void OnChargeStart() { }


        protected virtual void OnChargeUpdate(float currentChargeTime, float deltaTime) { }


        protected virtual void OnChargeComplete(float finalChargeTime) { }


        protected virtual void OnChargeStop(float finalChargeTime, bool wasCompleted) { }


        protected virtual void OnChargeCancelled() { }


        protected virtual void ExecuteChargedAction(float chargeTime) { }

#endregion
    }
}