using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ActionBuilder.Runtime
{
    [DefaultExecutionOrder(-2)]
    public class ActionController : MonoBehaviour
    {
        [SerializeField]
        private bool _debug;
        
        [SerializeField]
        private List<ActionBase> _actions = new List<ActionBase>();
        
        
        
        private readonly ActionDictionary _registeredActions = new ActionDictionary();
        
        private readonly List<ActionBase> _runningActions = new List<ActionBase>();

        private readonly Dictionary<int, List<EffectBase>> _runningEffects = new Dictionary<int, List<EffectBase>>();
        
        private readonly Queue<ScriptableObject> _destroyQueue = new Queue<ScriptableObject>();
        
        

        private readonly ValueTuple<Queue<EffectBase>, Queue<EffectBase>> _effectQueues = new(new Queue<EffectBase>(), new Queue<EffectBase>());

        private readonly ValueTuple<Queue<ActionBase>, Queue<ActionBase>> _actionQueues = new(new Queue<ActionBase>(), new Queue<ActionBase>());




        private void Awake()
        {
            for (int index = 0; index < _actions.Count; ++index)
            {
                ActionBase clone = Object.Instantiate(_actions[index]);
                clone.Initialize(this);
                
                _registeredActions.Add(clone);
            }
        }



        private void Update()
        {
            this.UpdateActions();
            this.UpdateEffects();
            this.DestroyObjects();
        }



#region Add / Remove Action

        public void AddActionToRunningQueue(ActionBase action)
        {
            _actionQueues.Item1.Enqueue(action);
        }



        public void RemoveActionFromRunningQueue(ActionBase action)
        {
            _actionQueues.Item2.Enqueue(action);
        }

#endregion



#region Add / Remove Effect

        public EffectBase AddEffectToRunningQueue(EffectBase effect)
        {
            _effectQueues.Item1.Enqueue(effect);

            if (_runningEffects.ContainsKey(effect.action.hash))
            {
                return effect;
            }

            _runningEffects.Add(effect.action.hash, new List<EffectBase>());
            return effect;
        }


        public void RemoveEffectFromRunningQueue(EffectBase effect)
        {
            _effectQueues.Item2.Enqueue(effect);

            if (_runningEffects.ContainsKey(effect.action.hash))
            {
                return;
            }

            Debug.LogError($"Running Queue에 Effect (:{effect.name})이 등록되어있지 않습니다.");
        }

#endregion


        
#region Update Action

        private void UpdateActions()
        {
            while (_actionQueues.Item1.Count > 0)
            {
                _runningActions.Add(_actionQueues.Item1.Dequeue());
            }


            int runningActionsCount = _runningActions.Count;

            for (int i = 0; i < runningActionsCount; ++i)
            {
                ActionBase action = _runningActions[i];

                if (action.isActive == false)
                {
                    return;
                }

                action.Update();

                if (action.IsFinish() == false)
                {
                    continue;
                }

                action.FinishAction();

                foreach (EffectBase effects in _runningEffects[action.hash])
                {
                    this.ManageEffectsOnActionEnd(effects);
                }
            }

            while (_actionQueues.Item2.Count > 0)
            {
                ActionBase action = _actionQueues.Item2.Dequeue();
                _runningActions.Remove(action);
                _destroyQueue.Enqueue(action);
            }
        }


        private void ManageEffectsOnActionEnd(EffectBase effects)
        {
            bool finishEffect = false;

            if (effects.endPolicy == EffectEndPolicy.StopOnActionEnd)
            {
                finishEffect = true;
            }

            //Action에 종속된 Effect가 아니라면 Early Return.
            if (effects.endPolicy == EffectEndPolicy.EffectDurationEnd)
            {
                finishEffect |= effects.elapsedTime < effects.duration;
            }

            if (finishEffect == false)
            {
                return;
            }

            //Action이 종료됐을 때, 자동으로 Release되는 옵션이 켜져있다면. 
            if (effects.autoRelease)
            {
                effects.Release();
            }

            effects.OnActionEnd();

            _destroyQueue.Enqueue(effects);
        }

#endregion



#region Update Effects

        private void UpdateEffects()
        {
            //add
            while (_effectQueues.Item1.Count > 0)
            {
                EffectBase effect = _effectQueues.Item1.Dequeue();
                _runningEffects[effect.action.hash].Add(effect);
            }


            //update
            foreach (List<EffectBase> effects in _runningEffects.Values)
            {
                this.UpdateEffects(effects);
            }


            //remove
            while (_effectQueues.Item2.Count > 0)
            {
                EffectBase effect = _effectQueues.Item2.Dequeue();
                _runningEffects[effect.action.hash].Remove(effect);
                _destroyQueue.Enqueue(effect);
            }
        }



        private void UpdateEffects(List<EffectBase> effects)
        {
            int effectLength = effects.Count;

            for (int index = 0; index < effectLength; ++index)
            {
                EffectBase effect = effects[index];
                
                if (effect.isReleased)
                {
                    continue;
                }

                if (effect.enable)
                {
                    effect.Update();
                }

                if (effect.hasFinished)
                {
                    this.RemoveEffectFromRunningQueue(effect);
                }
            }
        }

#endregion


        
#region Clean Up Objects
        
        
        private void DestroyObjects()
        {
            while (_destroyQueue.TryDequeue(out ScriptableObject destroyTarget))
            {
                if (destroyTarget is IPoolable poolable)
                {
                    poolable.ReturnToPool();
                }
                else
                {
                    Object.DestroyImmediate(destroyTarget);
                }
            }
        }


        private void OnDestroy()
        {
            
        }

#endregion
    }
}