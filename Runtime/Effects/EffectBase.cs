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
        public bool linkActionLifetime = false;
        
        [Space(3)]
        public ApplyPolicy applyPolicy;

        [Space(3)]
        public ExecutionData executionData;


#if UNITY_EDITOR
        [SerializeField, HideInInspector]
        internal bool isExpanded = true;
#endif


        private bool _applied;

        private bool _released;

        private int _appliedCount;

        private float _elapsedTime;

        private float _lastApplyTime;


        [SerializeReference, HideInInspector]
        private ActionBase _referencedAction;



#region Properties

        public ActionBase referencedAction
        {
            get { return _referencedAction; }

            internal set { _referencedAction = value; }
        }

        public StatController statController
        {
            get { return _referencedAction.statController; }
        }

        public Transform transform
        {
            get { return _referencedAction.owner.transform; }
        }

        public GameObject gameObject
        {
            get { return _referencedAction.owner.gameObject; }
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

#endregion



        public void Reset()
        {
            _elapsedTime = 0;
            _applied = false;
            _released = false;
            _appliedCount = 0;
            _lastApplyTime = 0;

            this.OnReset();
        }



        public void ManuelApply()
        {
            if (this.applyPolicy == ApplyPolicy.Auto)
            {
                return;
            }

            this.Apply();
        }



        protected virtual void Apply()
        {
            if (this.enable == false)
            {
                return;
            }

            // 아직 적용 주기에 따른 쿨타임이 끝나지 않았다면 종료.
            if (_applied && _lastApplyTime + executionData.applyInterval > _elapsedTime)
            {
                return;
            }

            if (_appliedCount == applyCount)
            {
                return;
            }

            _lastApplyTime = _elapsedTime;
            _appliedCount++;
            _applied = true;

            this.onBeforeApply?.Invoke(this);
            this.OnApply();
            this.onAfterApply?.Invoke(this);
        }



        public void Release(bool forceRelease = false)
        {
            if (forceRelease == false)
            {
                //아직 적용된 횟수가 설정된 적용 횟수에 도달하지 않았다면 해제하지 않음.
                if (_appliedCount < applyCount)
                {
                    return;
                }

                //적용횟수에 도달하여 이미 해제된 상태라면 중복 해제를 막음.
                if (_released)
                {
                    return;
                }
            }

            _released = true;

            this.onBeforeRelease?.Invoke(this);
            this.OnRelease();
            this.onAfterRelease?.Invoke(this);
        }



        /// <summary> 지정된 델타 시간에 따라 이펙트의 상태를 업데이트하고 이펙트가 완료되었는지 판단합니다. </summary>
        /// <param name="deltaTime">이펙트의 상태를 업데이트하는데 사용되는 시간 증분.</param>
        /// <returns>이펙트가 완료되었으면 true를, 그렇지 않으면 false를 반환합니다.</returns>
        public bool TryUpdate(float deltaTime)
        {
            if (this.enable == false)
            {
                return false;
            }

            _elapsedTime += deltaTime;

            if (applyPolicy == ApplyPolicy.Auto)
            {
                this.Apply();
            }

            if (this.CompletedApply())
            {
                return true;
            }
            else
            {
                this.OnUpdate(deltaTime);
                return false;
            }
        }
        
        
        public virtual bool CompletedApply()
        {
            return _appliedCount == applyCount;
        }


        public virtual void OnReset() { }


        public virtual void OnValidateEffect() { }


        public virtual void OnApply() { }


        public virtual void OnUpdate(float deltaTime) { }


        public virtual void OnRelease() { }

        
        public virtual void OnActionPause() { }


        public virtual void OnActionResume() { }


        public virtual void OnActionStart() { }


        public virtual void OnActionEnd() { }
    }
}