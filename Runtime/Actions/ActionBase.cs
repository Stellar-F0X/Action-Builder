using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

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
        protected IdentifyData _identifyData;

        [Space(5)]
        public ActionDurationData durationData;

        /// <summary>
        /// 현재 상황에 따라 액션을 끝낸거나 연장할 수 도 있다.
        /// 몇 가지 추가 기능을 수행할 수 있다.
        /// </summary>
        [SerializeReference, SubclassSelector]
        public ConditionBase finishResolver;

        [SerializeReference]
        public StatSet usingStatsTemplate;


        /// <summary> Action 동작 경과 시간 </summary>
        private float _elapsedTime;


        /// <summary> 마지막으로 실행이 종료되었던 시간 </summary>
        private float _lastQuitTime;


        /// <summary> 현재 동작 중인 상태 </summary>
        private ActionState _currentState;


        [SerializeReference, HideInInspector]
        protected List<EffectBase> _effects;


        /// <summary> Runtime only </summary>
        protected List<EventChannelBase> _channels;



#region Properties

        public virtual bool isOnCooldown
        {
            get { return _lastQuitTime > float.Epsilon && _lastQuitTime + durationData.cooldownTime > Time.time; }
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


        public ActionController controller
        {
            get;
            set;
        }


        public List<GameObject> targets
        {
            get;
            set;
        }


        public Transform transform
        {
            get { return this.controller.transform; }
        }


        public GameObject gameObject
        {
            get { return this.controller.gameObject; }
        }


        public Sprite icon
        {
            get { return _identifyData.icon; }
        }


        public string actionName
        {
            get { return _identifyData.name; }
        }


        public string description
        {
            get { return _identifyData.description; }
        }


        public int hash
        {
            get { return _identifyData.hash; }
        }


        public float duration
        {
            get { return durationData.duration; }
        }


        public string tag
        {
            get { return _identifyData.tag; }
        }


        internal List<EffectBase> internalEffects
        {
            get { return _effects; }
        }

#endregion



        internal void OnCreate()
        {
            _identifyData = new IdentifyData(this.name);
            _channels = new List<EventChannelBase>();
            _effects = new List<EffectBase>();
        }



        private void OnValidate()
        {
            this.OnValidateAction();
        }



        internal virtual void Initialize(ActionController actionOwner)
        {
            this._channels = new List<EventChannelBase>();
            this.controller = actionOwner;
            this.name = name.Replace("(Clone)", "");

            if (actionOwner.TryGetComponent(out StatController foundStatController))
            {
                this.statController = foundStatController;
            }

            if (statController == null)
            {
                Debug.LogError("StatController is not found in ActionController's GameObject");
            }

            this.OnInitialized();
        }


        public virtual void Reset()
        {
            _lastQuitTime = 0;
            _elapsedTime = 0;
            _currentState = ActionState.Idle;
        }


        public virtual bool Trigger()
        {
            if (isOnCooldown || isActive)
            {
                return false;
            }

            this._elapsedTime = 0f;
            this._currentState = ActionState.Playing;

            this.OnStart();
            this.onStarted?.Invoke(this);
            return true;
        }



        public virtual void Pause()
        {
            if (_currentState != ActionState.Playing)
            {
                return;
            }

            this._currentState = ActionState.Paused;
            this.OnPause();
            this.onPaused?.Invoke(this);

            for (int index = 0; index < _effects.Count; ++index)
            {
                _effects[index].OnActionPause();
            }
        }



        public virtual void Resume()
        {
            if (_currentState != ActionState.Paused)
            {
                return;
            }

            this._currentState = ActionState.Playing;
            this.OnResume();
            this.onResumed?.Invoke(this);

            for (int index = 0; index < _effects.Count; ++index)
            {
                _effects[index].OnActionResume();
            }
        }



        public virtual void Cancel()
        {
            if (isActive == false)
            {
                return;
            }

            this._currentState = ActionState.Cancelled;

            this.OnCancel();
            this.onCancelled?.Invoke(this);
        }



        public virtual void ClearAllEvents()
        {
            this.onStarted = null;
            this.onEnded = null;
            this.onPaused = null;
            this.onResumed = null;
            this.onCancelled = null;
        }



        public virtual void Update()
        {
            if (_currentState != ActionState.Playing)
            {
                return;
            }

            float deltaTime = Time.deltaTime;
            this._elapsedTime += deltaTime;
            this.OnUpdate(deltaTime);

            foreach (EffectBase effect in _effects)
            {
                this.CloneAndQueueEffect(effect);
            }
        }


        private void CloneAndQueueEffect(EffectBase effect)
        {
            if (effect.executionData.delay >= _elapsedTime)
            {
                return;
            }

            EffectBase clonedEffect = Object.Instantiate(effect);

            clonedEffect.name = clonedEffect.name.Replace("(Clone)", "");
            clonedEffect.effectName = clonedEffect.name;
            clonedEffect.action = this;

            controller.AddEffectToRunningQueue(clonedEffect);
        }



        /// <summary> 조건에 따라 완료 여부를 확인하고, 액션이 끝났다면 종료 로직을 수행한다. </summary>
        /// <returns> 액션의 종료 여부. </returns>
        public virtual bool CheckFinish()
        {
            bool finish = false;

            switch (this.durationData.durationType)
            {
                case ActionDuration.Duration: finish = _elapsedTime > duration && (finishResolver is null || finishResolver.CanExecute(this)); break;

                //instant는 finish Resolver가 없거나, 조건이 참이라면 그냥 끝낸다. 
                case ActionDuration.Instant: finish =  finishResolver is null || finishResolver.CanExecute(this); break;

                //반면, infinite는 Resolver가 있고, 조건에 맞아야지만 끝낸다. 
                //이게 아니면 무조건 Cancel 함수 호출로만 Action을 종료할 수 있다.
                case ActionDuration.Infinite: finish =  finishResolver is not null && finishResolver.CanExecute(this); break;
            }

            if (finish)
            {
                this._lastQuitTime = Time.time;
                this._currentState = ActionState.Finished;

                this.OnEnd();

                Assert.IsNotNull(_effects);
                this.onEnded?.Invoke(this);
            }
            
            return finish;
        }


        public override string ToString()
        {
            return $"{typeof(ActionBase)} {actionName} (State: {_currentState}, Cooldown: {isOnCooldown}, Elapsed: {_elapsedTime:F2}s)";
        }



        protected virtual void OnUpdate(float deltaTime) { }


        protected virtual void OnStart() { }


        protected virtual void OnPause() { }


        protected virtual void OnResume() { }


        protected virtual void OnCancel() { }


        protected virtual void OnEnd() { }


        protected virtual void OnValidateAction() { }


        protected virtual void OnInitialized() { }
    }
}