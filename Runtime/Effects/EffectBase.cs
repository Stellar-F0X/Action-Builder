using System;
using UnityEngine;

namespace ActionBuilder.Runtime
{
    [Serializable]
    public abstract class EffectBase
    {
        public event Action<EffectBase> onBeforeApply;
        
        public event Action<EffectBase> onAfterApply;
        
        public event Action<EffectBase> onBeforeRelease;
        
        public event Action<EffectBase> onAfterRelease;
        
        
        
        public string name;

        
        [HideInInspector]
        public bool enable = true;
        public string description;

        
        [Space(3)]
        public EffectDurationData durationData;
        
        
        [Space(3)]
        public EffectFinishOption finishOption;
        
        
#if UNITY_EDITOR
        [SerializeField, HideInInspector]
        internal bool isExpanded = true;
#endif
        
        
        private bool _applied;
        private bool _released;
        private int _appliedCount;
        private float _elapsedTime;

        [SerializeReference, HideInInspector]
        private ActionBase _referencedAction;




        public ActionBase referencedAction
        {
            get { return _referencedAction; }

            internal set { _referencedAction = value; }
        }

        public bool isPlaying
        {
            get { return _applied && _released == false; }
        }

        public bool isApplied
        {
            get { return _applied; }
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
            get { return durationData.duration; }
        }
        
        public int maxApplicationCount
        {
            get { return durationData.maxApplicationCount; }
        }


        
        public void Reset()
        {
            _elapsedTime = 0;
            _applied = false;
            _released = false;
            _appliedCount = 0;
        }
        

        public void Apply()
        {
            _applied = true;
            ++_appliedCount;
            
            onBeforeApply?.Invoke(this);
            this.OnApply();
            onAfterApply?.Invoke(this);
        }


        public void Release()
        {
            _released = false;
            
            onBeforeRelease?.Invoke(this);
            this.OnRelease();
            onAfterRelease?.Invoke(this);
        }
        

        /// <summary> 지정된 델타 시간에 따라 이펙트의 상태를 업데이트하고 이펙트가 완료되었는지 판단합니다. </summary>
        /// <param name="deltaTime">이펙트의 상태를 업데이트하는데 사용되는 시간 증분.</param>
        /// <returns>이펙트가 완료되었으면 true를, 그렇지 않으면 false를 반환합니다.</returns>
        public bool Update(float deltaTime)
        {
            switch (finishOption)
            {
                case EffectFinishOption.CycleCompleted:
                {
                    if (_appliedCount == maxApplicationCount)
                    {
                        return true;
                    }
                    else
                    {
                        break;
                    }
                }

                case EffectFinishOption.DurationEnded:
                {
                    if (_elapsedTime > duration)
                    {
                        return true;
                    }
                    else
                    {
                        break;
                    }
                }

                case EffectFinishOption.CycleOrDuration:
                {
                    if (_elapsedTime > duration)
                    {
                        return true;
                    }

                    if (_appliedCount == maxApplicationCount)
                    {
                        return true;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            
            _elapsedTime += deltaTime;
            this.OnUpdate(deltaTime);
            return false;
        }
        
        
        
        public virtual void OnPause() { }


        public virtual void OnApply() { }

        
        public virtual void OnRelease() { }

        
        public virtual void OnUpdate(float deltaTime) { }
    }
}