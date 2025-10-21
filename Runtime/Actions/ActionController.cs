using System;
using System.Collections.Generic;
using UnityEngine;

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

        
        
        // 매니저들
        private PoolController _poolController = new PoolController();
        
        private ActionHandler _actionHandler;
        
        private EffectHandler _effectHandler;

        
        
        private void Awake()
        {
            for (int index = 0; index < _actions.Count; ++index)
            {
                ActionBase clone = _actions[index].InstantiateSelf<ActionBase>();
                clone.Initialize(this);
                _actionTemplates.Add(clone);
            }

            _actionHandler = new ActionHandler(this, _actionTemplates, _runningActions, _actionQueues);
            _effectHandler = new EffectHandler(this, _runningEffects, _effectQueues);
        }

        
        private void Update()
        {
            _effectHandler.UpdateEffects();
            _actionHandler.UpdateActions(_effectHandler);
            
            _poolController.ProcessDestroyQueue();
        }
        
        
        public List<EffectBase> GetRunningEffects(int actionHash)
        {
            return _effectHandler.GetRunningEffects(actionHash);
        }

        
        public List<ActionBase> GetRunningActions()
        {
            return _runningActions;
        }

        
        public ActionBase GetPooledAction(int hash)
        {
            return _poolController.GetPooledObject<ActionBase>(hash);
        }

        
        public void EnqueueForDestroy(ExecutableBase target)
        {
            _poolController.PushObjectToPool(target);
        }

        
        // Action 제어
        public bool HasAction(string actionName, bool searchInRunningActions = false)
        {
            return _actionHandler.HasAction(actionName, searchInRunningActions);
        }
        

        public bool HasAction(string actionName, out ActionBase action, bool searchInRunningActions = false)
        {
            return _actionHandler.HasAction(actionName, out action, searchInRunningActions);
        }
        

        public ActionBase TriggerAction(string actionName)
        {
            return _actionHandler.TriggerAction(actionName);
        }

        
        public void TriggerAction(ActionBase action)
        {
            _actionHandler.TriggerAction(action);
        }

        
        public void PauseAction(string actionName)
        {
            _actionHandler.PauseAction(actionName);
        }

        
        public void ResumeAction(string actionName)
        {
            _actionHandler.ResumeAction(actionName);
        }

        
        public void CancelAction(string actionName, bool withEffects = true)
        {
            _actionHandler.CancelAction(actionName, withEffects);
        }

        
        // Effect 제어
        public void StopAllEffects()
        {
            _effectHandler.StopAllEffects();
        }

        
        public void CancelEffectsByActionType(string actionName)
        {
            _effectHandler.CancelEffectsByActionType(actionName, _actionHandler);
        }

        
        public void ManualReleaseEffects(string actionName, string effectName)
        {
            _effectHandler.ManualReleaseEffects(actionName, effectName, _actionHandler);
        }

        
        public EffectBase[] ManualApplyEffects(string actionName, string effectName)
        {
            return _effectHandler.ManualApplyEffects(actionName, effectName, _actionHandler);
        }
        

        public bool ManualApplyEffects(EffectBase effect)
        {
            return _effectHandler.ManualApplyEffects(effect);
        }

        
        // Queue 관리
        public void RegisterActionToRunningQueue(ActionBase action)
        {
            _actionHandler.RegisterActionToRunningQueue(action);
        }

        
        public void UnregisterActionFromRunningQueue(ActionBase action)
        {
            _actionHandler.UnregisterActionFromRunningQueue(action);
        }
        

        public void RegisterEffectToRunningQueue(EffectBase effect)
        {
            _effectHandler.RegisterEffectToRunningQueue(effect);
        }

        
        public void UnregisterEffectFromRunningQueue(EffectBase effect)
        {
            _effectHandler.UnregisterEffectFromRunningQueue(effect);
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
            _poolController.ClearPool();
            _effectQueues.Item1.Clear();
            _effectQueues.Item2.Clear();
            _actionQueues.Item1.Clear();
            _actionQueues.Item2.Clear();
        }
    }
}