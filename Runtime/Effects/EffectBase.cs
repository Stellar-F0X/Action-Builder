using System;
using System.Collections;
using UnityEngine;

namespace ActionBuilder.Runtime
{
    public abstract class EffectBase : ExecutableBase
    {
        public event Action<EffectBase> onBeforeApply;

        public event Action<EffectBase> onAfterApply;

        public event Action<EffectBase> onBeforeRelease;

        public event Action<EffectBase> onAfterRelease;


        [HideInInspector]
        public bool enable = true;
        public bool autoRelease = true;


        [HideIf(nameof(autoRelease), false)]
        public EffectEndPolicy endPolicy;


        [Space(3)]
        public ApplyPolicy applyPolicy;


        [Space(3)]
        public EffectDurationData executionData;


#if UNITY_EDITOR
        [SerializeField, HideInInspector]
        internal bool isExpanded = true;
#endif
        
        private bool _released;
        private int _currentApplyCount;
        private float _lastApplyTime;


        [SerializeReference, HideInInspector]
        private ActionBase _action;
        private Coroutine _autoApplyCoroutine;




#region Properties

        public ActionBase action
        {
            get { return _action; }

            internal set { _action = value; }
        }

        public bool isReleased
        {
            get { return _released; }
        }

        public float duration
        {
            get { return executionData.duration; }
        }

        public int targetApplyCount
        {
            get { return executionData.applyCount; }
        }

        public int currentApplyCount
        {
            get { return _currentApplyCount; }
        }

        public float applyInterval
        {
            get { return executionData.applyInterval; }
        }

        public virtual bool isOverDuration
        {
            get { return _elapsedTime > duration; }
        }

        public virtual bool canApply
        {
            get { return this.CanApplyEffect(); }
        }

#endregion

        
#region Internal Properties

        protected internal virtual MinMax durationLimit
        {
            get { return new MinMax(float.MinValue, float.MaxValue); }
        }

#endregion



        public void Reset()
        {
            // 실행 중인 코루틴 정리
            if (_autoApplyCoroutine != null && _action?.controller != null)
            {
                _action.controller.StopCoroutine(_autoApplyCoroutine);
            }

            _released = false;
            _elapsedTime = 0;
            _lastApplyTime = 0;
            _currentApplyCount = 0;
            _autoApplyCoroutine = null;

            this.OnReset();
        }



        public virtual void ManualApply()
        {
            if (applyPolicy == ApplyPolicy.Auto)
            {
                Debug.LogWarning($"[{this.name}] ManualApply is not allowed when applyPolicy is Auto");
                return;
            }

            if (this.enable == false)
            {
                return;
            }

            if (_currentApplyCount == targetApplyCount)
            {
                return;
            }

            // 아직 적용 주기에 따른 쿨타임이 끝나지 않았다면 종료.
            if (this.canApply == false)
            {
                return;
            }

            _lastApplyTime = _elapsedTime;
            _currentApplyCount++;

            this.onBeforeApply?.Invoke(this);
            this.OnApply();
            this.onAfterApply?.Invoke(this);
        }



        protected virtual IEnumerator AutoApply()
        {
            bool isValidInterval = !(float.IsNaN(applyInterval) || Mathf.Approximately(applyInterval, 0f));

            for (int index = 0; index < targetApplyCount && _released == false; index++)
            {
                _lastApplyTime = _elapsedTime;
                _currentApplyCount++;

                this.onBeforeApply?.Invoke(this);
                this.OnApply();
                this.onAfterApply?.Invoke(this);

                if (isValidInterval)
                {
                    yield return new WaitForSeconds(applyInterval);
                }
            }

            _autoApplyCoroutine = null;
        }



        public void Release(bool forceRelease = false)
        {
            //이미 릴리즈 된 상태라면 굳이 다시 릴리즈를 할 필요가 없다.
            if (_released)
            {
                return;
            }

            //강제로 수행하거나, 활성화되어 있고, 재생 시간 초과라는 종료 조건이 성립해야 됨.
            if (forceRelease || (this.enable && isOverDuration))
            {
                _released = true;

                this.onBeforeRelease?.Invoke(this);
                this.OnRelease();
                this.onAfterRelease?.Invoke(this);
            }
        }



        public void Update()
        {
            if (this.enable == false || this.isOverDuration)
            {
                return;
            }

            _elapsedTime += Time.deltaTime;

            this.StartAutoApplyLoop();
            this.OnUpdate(Time.deltaTime);
        }



        private void OnValidate()
        {
            this.OnValidateEffect();
        }



        public virtual void Cancel()
        {
            if (_autoApplyCoroutine != null && _action?.controller != null)
            {
                _action.controller.StopCoroutine(_autoApplyCoroutine);
                _autoApplyCoroutine = null;
            }

            if (this.autoRelease)
            {
                this.Release(forceRelease: true);
            }

            this.OnCanceled();
            this.controller?.UnregisterEffectFromRunningQueue(this);
        }
        
        
        
        private void StartAutoApplyLoop()
        {
            if (applyPolicy != ApplyPolicy.Auto || _autoApplyCoroutine is not null)
            {
                return;
            }

            _autoApplyCoroutine = controller.StartCoroutine(this.AutoApply());
        }



        private bool CanApplyEffect()
        {
            //적용이 목표로 설정한 적용 수보다 많으면 안되므로 False를 반환.
            if (currentApplyCount >= targetApplyCount)
            {
                return false;
            }

            //첫 적용 이전이라면 시작한 적도 없는데, 다음 적용을 위한 쿨타임이 있으면 안되므로 즉각 True 반환. 
            if (currentApplyCount == 0)
            {
                return true;
            }

            //첫 적용 이후라면 빈도 시간에 따라 True를 반환.
            if ((_lastApplyTime + executionData.applyInterval) < _elapsedTime)
            {
                return true;
            }
            else
            {
                return false;
            }
        }




#region Action Event Callbacks

        public virtual void OnActionCancel() { }


        public virtual void OnActionPause() { }


        public virtual void OnActionResume() { }


        public virtual void OnActionEnd() { }

#endregion


        protected virtual void OnCanceled() { }


        protected virtual void OnValidateEffect() { }


        protected virtual void OnReset() { }


        protected virtual void OnApply() { }


        protected virtual void OnUpdate(float deltaTime) { }


        protected virtual void OnRelease() { }
    }
}