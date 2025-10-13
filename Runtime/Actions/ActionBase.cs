using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ActionBuilder.Runtime
{
    public abstract class ActionBase : ScriptableObject
    {
        public event Action<ActionBase> onStarted;
        public event Action<ActionBase> onEnded;
        public event Action<ActionBase> onPaused;
        public event Action<ActionBase> onResumed;
        public event Action<ActionBase> onCancelled;


        [SerializeField, Space(-15)]
        protected ActionData _actionData;

        [Space(5)]
        public ActionDurationData durationData;

        /// <summary>
        /// 현재 상황에 따라 액션을 끝낸거나 연장할 수 도 있다.
        /// 몇 가지 추가 기능을 수행할 수 있다.
        /// </summary>
        [SerializeReference, SubclassSelector]
        public ConditionBase finishResolver;

        [Space(5)]
        public StatSet usingStatsTemplate;


        /// <summary> 정규화된 경과 퍼센테이지 </summary>
        private float _progress;
        
        /// <summary> Action 동작 경과 시간 </summary>
        private float _elapsedTime;

        /// <summary> 마지막으로 실행이 종료되었던 시간 </summary>
        private float _lastQuitTime;

        /// <summary> 현재 동작 중인 상태 </summary>
        private ActionState _currentState;

        /// <summary> Runtime only </summary>
        protected List<EventChannelBase> _channels;

        

#region Properties

        public virtual float progress
        {
            get { return Mathf.InverseLerp(0, durationData.duration, _elapsedTime); }
        }
        
        
        public virtual bool isOnCooldown
        {
            get { return _lastQuitTime + durationData.cooldownTime > Time.time; }
        }
        

        public ActionState currentState
        {
            get { return _currentState; }
        }


        public bool isActive
        {
            get { return _currentState is ActionState.Playing or ActionState.Paused; }
        }


        public StatController statController
        {
            get;
            set;
        }


        public ActionController owner
        {
            get;
            set;
        }


        public GameObject target
        {
            get;
            set;
        }


        public Sprite icon
        {
            get { return _actionData.icon; }
        }


        public string actionName
        {
            get { return _actionData.name; }
        }


        public string description
        {
            get { return _actionData.description; }
        }


        public int hash
        {
            get { return _actionData.hash; }
        }


        public float duration
        {
            get { return durationData.duration; }
        }


        public IReadOnlyList<EffectBase> effects
        {
            get { return _actionData.effects; }
        }


        internal List<EffectBase> internalEffects
        {
            get { return _actionData.effects; }
        }

#endregion



        internal void OnCreate()
        {
            _actionData = new ActionData(this.name);
        }

        
        private void OnValidate()
        {
            this.OnValidateAction();

            if (effects is null || effects.Count == 0)
            {
                return;
            }
            
            for (int index = 0; index < effects.Count; ++index)
            {
                effects[index].OnValidateEffect();
            }
        }
        

        internal void Initialize(ActionController actionOwner)
        {
            this.owner = actionOwner;
            this._channels = new List<EventChannelBase>();


            if (actionOwner.TryGetComponent(out StatController foundStatController))
            {
                this.statController = foundStatController;
            }
            else if (this.usingStatsTemplate != null)
            {
                Debug.LogWarning("StatController not found, Stats Template will not be applied.");
            }


            this.internalEffects.ForEach(e => e.referencedAction = this);


            if (usingStatsTemplate == null)
            {
                return;
            }

            if (statController == null)
            {
                Debug.LogError("StatController is not found in ActionController's GameObject");
                return;
            }

            if (usingStatsTemplate.keyType != statController.keyType)
            {
                Debug.LogError("Using Stats Template Key Type is not same as StatController Key Type");
            }
        }


        internal void InitializeChannels(Dictionary<UGUID, EventChannelBase> allChannels) { }
        



#region Control Methods

        public bool Trigger(bool force = false)
        {
            if (isOnCooldown && force == false)
            {
                return false;
            }

            if (isActive && force)
            {
                this.FinishAction();
            }

            this._elapsedTime = 0f;
            this._currentState = ActionState.Playing;

            this.OnStart();
            this.onStarted?.Invoke(this);
            return true;
        }



        public void Pause()
        {
            if (_currentState != ActionState.Playing)
            {
                return;
            }

            this._currentState = ActionState.Paused;
            this.OnPause();
            this.onPaused?.Invoke(this);
        }



        public void Resume()
        {
            if (_currentState != ActionState.Paused)
            {
                return;
            }

            this._currentState = ActionState.Playing;
            this.OnResume();
            this.onResumed?.Invoke(this);
        }



        public void Cancel()
        {
            if (isActive == false)
            {
                return;
            }

            this._currentState = ActionState.Cancelled;

            this.OnCancel();
            this.onCancelled?.Invoke(this);

            this.FinishAction();
        }



        public void ClearAllEvents()
        {
            this.onStarted = null;
            this.onEnded = null;
            this.onPaused = null;
            this.onResumed = null;
            this.onCancelled = null;
        }



        public virtual void Execute(float deltaTime)
        {
            if (_currentState != ActionState.Playing)
            {
                return;
            }

            this._elapsedTime += deltaTime;

            this.OnUpdate(deltaTime);
            this.UpdateEffects(deltaTime);

            switch (this.durationData.durationType)
            {
                case ActionDuration.Duration:
                {
                    if (_elapsedTime > duration && (finishResolver is null || finishResolver.CanExecute(this)))
                    {
                        this.FinishAction();
                    }

                    break;
                }

                case ActionDuration.Instant:
                {
                    //instant는 finish Resolvar가 없거나, 조건이 참이라면 그냥 끝낸다. 
                    if (finishResolver is null || finishResolver.CanExecute(this))
                    {
                        this.FinishAction();
                    }

                    break;
                }

                case ActionDuration.Infinite:
                {
                    //반면, infinite는 Resolvar가 있고, 조건에 맞아야지만 끝낸다. 
                    //이게 아니면 무조건 Cancel 함수 호출로만 Action을 종료할 수 있다.
                    if (finishResolver is not null && finishResolver.CanExecute(this))
                    {
                        this.FinishAction();
                    }

                    break;
                }
            }
        }



        private void UpdateEffects(float deltaTime)
        {
            if (effects is null || effects.Count == 0)
            {
                return;
            }

            for (int i = 0; i < effects.Count; ++i)
            {
                EffectBase currentEffect = effects[i];

                if (currentEffect.enable == false)
                {
                    continue;
                }

                if (currentEffect.Update(deltaTime))
                {
                    currentEffect.Release();
                }
            }
        }



        private void ResetEffects()
        {
            if (effects is null || effects.Count == 0)
            {
                return;
            }

            for (int i = 0; i < effects.Count; ++i)
            {
                EffectBase currentEffect = effects[i];

                if (currentEffect.enable)
                {
                    currentEffect.Reset();
                }
            }
        }



        private void FinishAction()
        {
            this._lastQuitTime = Time.time;
            this._currentState = ActionState.Finished;

            this.OnEnd();
            this.ResetEffects();
            this.onEnded?.Invoke(this);
        }



        public override string ToString()
        {
            return $"{typeof(ActionBase)} {actionName} (State: {_currentState}, Cooldown: {isOnCooldown}, Elapsed: {_elapsedTime:F2}s)";
        }

#endregion



        protected virtual void OnUpdate(float deltaTime) { }


        protected virtual void OnStart() { }


        protected virtual void OnPause() { }


        protected virtual void OnResume() { }


        protected virtual void OnCancel() { }


        protected virtual void OnEnd() { }
        
        
        protected virtual void OnValidateAction() { }
    }
}