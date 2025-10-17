using System;
using UnityEngine;

namespace ActionBuilder.Runtime
{
    public abstract class EffectBase : ScriptableObject
    {
        public event Action<EffectBase> onBeforeApply;

        public event Action<EffectBase> onAfterApply;

        public event Action<EffectBase> onBeforeRelease;

        public event Action<EffectBase> onAfterRelease;


        public string effectName;


        [HideInInspector]
        public bool enable = true;
        public string description;
        public bool autoRelease;
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

        private float _elapsedTime;

        private float _lastApplyTime;


        [SerializeReference, HideInInspector]
        private ActionBase _action;




#region Properties

        public ActionBase action
        {
            get { return _action; }

            internal set { _action = value; }
        }

        public Transform transform
        {
            get { return _action.controller.transform; }
        }

        public GameObject gameObject
        {
            get { return _action.controller.gameObject; }
        }

        public bool isApplyComplete
        {
            get { return _currentApplyCount == applyCount; }
        }

        public bool isReleased
        {
            get { return _released; }
        }

        public float elapsedTime
        {
            get { return _elapsedTime; }
        }

        public float duration
        {
            get { return executionData.duration; }
        }

        public int applyCount
        {
            get { return executionData.applyCount; }
        }

        public float applyInterval
        {
            get { return executionData.applyInterval; }
        }
        
        public virtual bool hasFinished
        {
            get { return _elapsedTime > duration; }
        }

        public virtual bool canApply
        {
            get { return _currentApplyCount == 0 || (_lastApplyTime + executionData.applyInterval) < _elapsedTime; }
        }

        public virtual MinMax durationLimit
        {
            get { return new MinMax(float.MinValue, float.MaxValue); }
        }

#endregion



        public void Reset()
        {
            _released = false;
            _elapsedTime = 0;
            _lastApplyTime = 0;
            _currentApplyCount = 0;

            this.OnReset();
        }



        public virtual void Apply()
        {
            if (this.enable == false)
            {
                return;
            }

            if (this.isApplyComplete)
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



        public void Release(bool forceRelease = false)
        {
            if (this.enable == false)
            {
                return;
            }
            
            if (forceRelease == false && (_released || hasFinished == false))
            {
                return;
            }

            _released = true;

            this.onBeforeRelease?.Invoke(this);
            this.OnRelease();
            this.onAfterRelease?.Invoke(this);
        }



        public void Update()
        {
            if (this.enable == false || this.hasFinished)
            {
                return;
            }

            _elapsedTime += Time.deltaTime;

            if (applyPolicy == ApplyPolicy.Auto)
            {
                this.Apply();
            }

            this.OnUpdate(Time.deltaTime);
        }



        public void OnValidate()
        {
            this.OnValidateEffect();
        }



        protected virtual void OnValidateEffect() { }


        public virtual void OnReset() { }


        public virtual void OnApply() { }


        public virtual void OnUpdate(float deltaTime) { }


        public virtual void OnRelease() { }


        public virtual void OnActionPause() { }


        public virtual void OnActionResume() { }


        public virtual void OnActionStart() { }


        public virtual void OnActionEnd() { }
    }
}