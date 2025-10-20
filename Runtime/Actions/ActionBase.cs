using System;
using System.Collections.Generic;
using System.Linq;
using HideIf.Runtime;
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


        [SerializeReference]
        public StatSet usingStatsTemplate;


        [SerializeReference, HideInInspector]
        protected List<EffectBase> _effects;


        /// <summary> Action 동작 경과 시간 </summary>
        private float _elapsedTime;


        /// <summary> 마지막으로 실행이 종료되었던 시간 </summary>
        private float _lastQuitTime;


        private int _tickedCount;


        /// <summary> 현재 동작 중인 상태 </summary>
        private ActionState _currentState;





#region Properties

        public virtual bool isOnCooldown
        {
            get { return _lastQuitTime > float.Epsilon && (_lastQuitTime + durationData.cooldownTime) > Time.time; }
        }


        public ActionState currentState
        {
            get { return _currentState; }
        }


        public bool isActive
        {
            get { return _currentState is ActionState.Playing or ActionState.Paused; }
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
            
            private set { _identifyData.name = value; }
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


        internal List<EffectBase> internalEffectSO
        {
            get { return _effects; }
        }

        protected List<EffectBase> _runtimeEffects;

#endregion



        internal void OnCreate()
        {
            _identifyData = new IdentifyData(this.name);
            _effects = new List<EffectBase>();
        }



        private void OnValidate()
        {
            this.OnValidateAction();
        }



        internal virtual void Initialize(ActionController actionOwner)
        {
            this.controller = actionOwner;
            
            this.name = name.Replace("(Clone)", "");
            this.actionName = name;

            _runtimeEffects = new List<EffectBase>();
            
            this.OnInitialized();
        }


        public virtual void Reset()
        {
            _elapsedTime = 0;
            _tickedCount = 0;
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
            this.controller.GetRunningEffects(hash)?.ForEach(e => e.OnActionPause());
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
            this.controller.GetRunningEffects(hash)?.ForEach(e => e.OnActionResume());
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
            this.controller.GetRunningEffects(hash)?.ForEach(e => e.OnActionCancel());
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

            _tickedCount++;

            float deltaTime = Time.deltaTime;
            this._elapsedTime += deltaTime;
            this.OnUpdate(deltaTime);

            if ((controller.GetRunningEffects(hash)?.Count ?? 0) == internalEffectSO.Count)
            {
                return;
            }

            foreach (EffectBase effect in _effects)
            {
                this.InstantiateAndQueueEffect(effect);
            }
        }


        private void InstantiateAndQueueEffect(EffectBase effect)
        {
            //경과 시간이 시작 딜레이보다 짧으면 아직 실행되면 안되므로 종료.
            if (effect.executionData.delay > _elapsedTime)
            {
                return;
            }
            
            //effect duration이 0이라도 한 번은 재생시켜야되므로 한 번은 적용 후, 딜레이 + 재생시간이 duration보다 짧으면 종료.
            if (effect.applyCount > 0 && (effect.executionData.delay + effect.duration) < _elapsedTime)
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
                case ActionDuration.Duration: finish = _elapsedTime > duration; break;

                case ActionDuration.Instant: finish = _tickedCount > 0; break;

                case ActionDuration.Infinite: finish = currentState is ActionState.Cancelled; break;
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