using System;
using System.Collections.Generic;
using System.Linq;
using Codice.CM.Common;
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
        
        
        
        private readonly ActionDictionary _registeredActionPool = new ActionDictionary();
        
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
                _registeredActionPool.Add(clone); 
            }
        }



        private void Update()
        {
            this.UpdateEffects();
            this.UpdateActions();
            this.DestroyObjects();
        }



        public List<EffectBase> GetRunningEffects(int actionHash)
        {
            return _runningEffects.GetValueOrDefault(actionHash);
        }



        public List<ActionBase> GetRunningActions()
        {
            return _runningActions;
        }

        
        
        public bool HasAction(string actionName, bool searchInRunningActions = false)
        {
            if (_registeredActionPool.TryGetValue(actionName, out ActionBase targetAction) == false)
            {
                return false;
            }
            
            if (searchInRunningActions)
            {
                return _runningActions.Find(a => a.hash == targetAction.hash);
            }
            else
            {
                return targetAction != null;
            }
        }



        public bool HasAction(string actionName, out ActionBase action, bool searchInRunningActions = false)
        {
            if (_registeredActionPool.TryGetValue(actionName, out action) == false)
            {
                return false;
            }
            
            if (searchInRunningActions)
            {
                int targetHash = action.hash;

                return _runningActions.Find(a => a.hash == targetHash);
            }
            else
            {
                return action != null;
            }
        }
        
        

        public ActionBase Trigger(string actionName)
        {
            if (this.HasAction(actionName) == false)
            {
                Debug.LogWarning($"{actionName}이 존재하지 않습니다.");
                return null;
            }

            ActionBase action = _registeredActionPool[actionName];
            this.Trigger(action);
            return action;
        }



        public void Trigger(ActionBase action)
        {
            if (action.Trigger())
            {
                this.AddActionToRunningQueue(action);
            }
            else
            {
                Debug.LogWarning($"{action.name}을 시작할 수 없습니다."); 
            }
        }



        public void Cancel(string actionName)
        {
            if (this.HasAction(actionName, out ActionBase action, true))
            {
                action.Cancel();
            }
            else
            {
                Debug.LogError($"동작 중인 액션 {actionName}를 찾을 수 없습니다.");
            }
        }



        public void ManualRelease(string actionName, string effectName)
        {
            if (string.IsNullOrEmpty(actionName) || string.IsNullOrEmpty(effectName))
            {
                Debug.LogError("Action Name 또는 Effect Name이 비었습니다.");
                return;
            }

            if (this.HasAction(actionName, out ActionBase action, true) == false)
            {
                Debug.LogError($"{actionName}.{effectName}을 찾을 수 없습니다.");
                return;
            }

            foreach (EffectBase effect in this._runningEffects[action.hash])
            {
                if (effect.name == effectName)
                {
                    effect.Release();
                }
            }
        }



        public EffectBase[] ManualApply(string actionName, string effectName)
        {
            if (string.IsNullOrEmpty(actionName) || string.IsNullOrEmpty(effectName))
            {
                Debug.LogError("Action Name 또는 Effect Name이 비었습니다.");
                return Array.Empty<EffectBase>();
            }
            
            if (this.HasAction(actionName, out ActionBase action, true))
            {
                EffectBase[] effects = this._runningEffects[action.hash].Where(effect => effect.name == effectName).ToArray();
                
                foreach (EffectBase effect in effects)
                {
                    effect.Apply();
                }

                return effects;
            }

            Debug.LogError($"{actionName}을 찾지 못했습니다.");
            return Array.Empty<EffectBase>();
        }



        public bool ManualApply(EffectBase effect)
        {
            if (_runningActions.Contains(effect.action) == false)
            {
                return false;
            }
            
            if (_runningEffects.TryGetValue(effect.action.hash, out List<EffectBase> list))
            {
                int idx = list.IndexOf(effect);

                if (idx <= 0)
                {
                    return false;
                }

                list[idx].Apply();
                return true;
            }

            return false;
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

                if (action.CheckFinish())
                {
                    _runningEffects[action.hash].ForEach(e => this.ManageEffectsOnActionEnd(e));
                    _actionQueues.Item2.Enqueue(action);
                }
                else
                {
                    action.Update();
                }
            }

            while (_actionQueues.Item2.Count > 0)
            {
                ActionBase action = _actionQueues.Item2.Dequeue();
                _runningActions.Remove(action);
                action.Reset();
            }
        }


        private void ManageEffectsOnActionEnd(EffectBase effect)
        {
            bool finishEffect = false;

            if (effect.endPolicy == EffectEndPolicy.StopOnActionEnd)
            {
                finishEffect = true;
            }

            //Action에 종속된 Effect가 아니라면 Early Return.
            if (effect.endPolicy == EffectEndPolicy.EffectDurationEnd)
            {
                finishEffect |= effect.elapsedTime > effect.duration;
            }

            if (finishEffect == false)
            {
                return;
            }

            //Action이 종료됐을 때, 자동으로 Release되는 옵션이 켜져있다면. 
            if (effect.autoRelease)
            {
                effect.Release();
            }
            
            effect.OnActionEnd();
            
            this.RemoveEffectFromRunningQueue(effect);
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

                if (effect.isOverDuration)
                {
                    effect.Release();
                    
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
                    Object.Destroy(destroyTarget);
                }
            }
        }


        private void OnDestroy()
        {
            
        }

#endregion
    }
}