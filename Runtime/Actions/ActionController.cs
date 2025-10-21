using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace ActionBuilder.Runtime
{
    [DefaultExecutionOrder(-2)]
    public class ActionController : MonoBehaviour
    {
        [SerializeField]
        private List<ActionBase> _actions = new List<ActionBase>();

        private readonly ActionDictionary _actionTemplates = new ActionDictionary();
        
        

        private readonly List<ActionBase> _runningActions = new List<ActionBase>();

        private readonly Dictionary<int, List<EffectBase>> _runningEffects = new Dictionary<int, List<EffectBase>>();



        private readonly ValueTuple<Queue<EffectBase>, Queue<EffectBase>> _effectQueues = new(new Queue<EffectBase>(), new Queue<EffectBase>());

        private readonly ValueTuple<Queue<ActionBase>, Queue<ActionBase>> _actionQueues = new(new Queue<ActionBase>(), new Queue<ActionBase>());



        private readonly List<IPoolable> _objectPool = new List<IPoolable>();

        private readonly Queue<ExecutableBase> _disableQueue = new Queue<ExecutableBase>();



        private void Awake()
        {
            Assert.IsNotNull(_actionTemplates);
            
            Assert.IsNotNull(_actions);
            
            
            for (int index = 0; index < _actions.Count; ++index)
            {
                ActionBase clone = Object.Instantiate(_actions[index]);
                clone.Initialize(this);
                _actionTemplates.Add(clone);
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
            if (_actionTemplates.TryGetValue(actionName, out ActionBase targetAction) == false)
            {
                return false;
            }

            if (searchInRunningActions)
            {
                return _runningActions.Any(a => a.hash == targetAction.hash);
            }
            else
            {
                return targetAction != null;
            }
        }



        public bool HasAction(string actionName, out ActionBase action, bool searchInRunningActions = false)
        {
            if (_actionTemplates.TryGetValue(actionName, out action) == false)
            {
                return false;
            }

            if (searchInRunningActions)
            {
                int targetHash = action.hash;

                return _runningActions.Any(a => a.hash == targetHash);
            }
            else
            {
                return action != null;
            }
        }



        public ActionBase TriggerAction(string actionName)
        {
            if (this.HasAction(actionName, out ActionBase original, false) == false)
            {
                Debug.LogWarning($"{actionName}이 존재하지 않습니다.");
                return null;
            }

            ActionBase action = _objectPool.Where(a => a.isReadyInPool)
                                           .OfType<ActionBase>()
                                           .FirstOrDefault(act => act.hash == original.hash);

            if (action != null)
            {
                action.OnGetFromPool();
                this.TriggerAction(action);
                return action;
            }

            if (original != null)
            {
                action = Object.Instantiate(original);
                action.Initialize(this);
                this.TriggerAction(action);
                return action;
            }

            Debug.LogWarning($"{actionName}이 존재하지 않습니다.");
            return null;
        }



        public void TriggerAction(ActionBase action)
        {
            if (action.Trigger())
            {
                this.RegisterActionToRunningQueue(action);
            }
            else
            {
                Debug.LogWarning($"{((Object)action).name}을 시작할 수 없습니다.");
            }
        }



        public void PauseAction(string actionName)
        {
            if (this.HasAction(actionName, out ActionBase action, true))
            {
                action.Pause();
            }
            else
            {
                Debug.LogError($"동작 중인 액션 {actionName}를 찾을 수 없습니다.");
            }
        }



        public void ResumeAction(string actionName)
        {
            if (this.HasAction(actionName, out ActionBase action, true))
            {
                action.Resume();
            }
            else
            {
                Debug.LogError($"동작 중인 액션 {actionName}를 찾을 수 없습니다.");
            }
        }



        public void CancelAction(string actionName, bool withEffects = true)
        {
            if (this.HasAction(actionName, out ActionBase action, true))
            {
                action.Cancel(withEffects);
            }
            else
            {
                Debug.LogError($"동작 중인 액션 {actionName}를 찾을 수 없습니다.");
            }
        }



        public void StopAllEffects()
        {
            foreach (List<EffectBase> effectList in _runningEffects.Values)
            {
                effectList.ForEach(e => e.Cancel());
            }
        }



        public void CancelEffectsByActionType(string actionName)
        {
            if (this.HasAction(actionName, out ActionBase action, true) == false)
            {
                Debug.LogError($"동작 중인 액션 {actionName}를 찾을 수 없습니다.");
                return;
            }

            if (_runningEffects.TryGetValue(action.hash, out List<EffectBase> list))
            {
                list.ForEach(e => e.Cancel());
            }
        }



        public void ManualReleaseEffects(string actionName, string effectName)
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

            foreach (EffectBase effect in this._runningEffects.GetValueOrDefault(action.hash))
            {
                if (effect.name == effectName)
                {
                    effect.Release();
                }
            }
        }



        public EffectBase[] ManualApplyEffects(string actionName, string effectName)
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
                    effect.ManualApply();
                }

                return effects;
            }

            Debug.LogError($"{actionName}을 찾지 못했습니다.");
            return Array.Empty<EffectBase>();
        }



        public bool ManualApplyEffects(EffectBase effect)
        {
            if (_runningActions.Contains(effect.action) == false)
            {
                return false;
            }

            if (_runningEffects.TryGetValue(effect.action.hash, out List<EffectBase> list))
            {
                int idx = list.IndexOf(effect);

                if (idx < 0)
                {
                    return false;
                }

                list[idx].ManualApply();
                return true;
            }

            return false;
        }



#region Add / Remove Action

        public void RegisterActionToRunningQueue(ActionBase action)
        {
            _actionQueues.Item1.Enqueue(action);
        }



        public void UnregisterActionFromRunningQueue(ActionBase action)
        {
            _actionQueues.Item2.Enqueue(action);
        }

#endregion



#region Add / Remove Effect

        public void RegisterEffectToRunningQueue(EffectBase effect)
        {
            if (_runningEffects.ContainsKey(effect.action.hash) == false)
            {
                _runningEffects.Add(effect.action.hash, new List<EffectBase>());
            }

            _effectQueues.Item1.Enqueue(effect);
        }


        public void UnregisterEffectFromRunningQueue(EffectBase effect)
        {
            if (_runningEffects.ContainsKey(effect.action.hash))
            {
                _effectQueues.Item2.Enqueue(effect);
            }
            else
            {
                Debug.LogError($"Running Queue에 Effect (:{effect.name})이 등록되어있지 않습니다.");
            }
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
                    continue;
                }

                if (action.CheckFinish() && _runningEffects.TryGetValue(action.hash, out var list))
                {
                    list.ForEach(this.ManageEffectsOnActionEnd);
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
                _disableQueue.Enqueue(action);
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

            this.UnregisterEffectFromRunningQueue(effect);
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
                effect.action.activeEffectCount++;
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
                effect.action.activeEffectCount--;
                _disableQueue.Enqueue(effect);
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

                if (effect.isOverDuration)
                {
                    if (effect.autoRelease)
                    {
                        effect.Release();
                    }

                    this.UnregisterEffectFromRunningQueue(effect);
                    continue;
                }

                if (effect.enable)
                {
                    effect.Update();
                }
            }
        }

#endregion



#region Destory Objects

        private void DestroyObjects()
        {
            while (_disableQueue.Count > 0)
            {
                ExecutableBase destroyTarget = _disableQueue.Dequeue();

                if (destroyTarget == null)
                {
                    Debug.LogWarning("이미 파괴된 오브젝트가 들어와 있습니다.");
                    continue;
                }
                
                if (destroyTarget is IPoolable poolable)
                {
                    poolable.OnBackToPool();
                    _objectPool.Add(poolable);
                }
                else
                {
                    Object.Destroy(destroyTarget);
                }
            }
        }



        private void OnDestroy()
        {
            foreach (ActionBase action in _runningActions)
            {
                action.Cancel(withEffects: true);
            }

            _runningActions.Clear();
            _actionTemplates.Clear();
            _runningEffects.Clear();
            _disableQueue.Clear();
            _effectQueues.Item1.Clear();
            _effectQueues.Item2.Clear();
            _actionQueues.Item1.Clear();
            _actionQueues.Item2.Clear();
        }

#endregion
    }
}