using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ActionBuilder.Runtime
{
    public class ActionHandler
    {
        public ActionHandler(ActionController controller, ActionDictionary templates, List<ActionBase> runningActions, ValueTuple<Queue<ActionBase>, Queue<ActionBase>> queues)
        {
            _controller = controller;
            _actionTemplates = templates;
            _runningActions = runningActions;
            _actionQueues = queues;
        }
        
        
        private readonly ActionController _controller;
        
        private readonly ActionDictionary _actionTemplates;
        
        private readonly List<ActionBase> _runningActions;
        
        private readonly ValueTuple<Queue<ActionBase>, Queue<ActionBase>> _actionQueues;


        
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

            return targetAction != null;
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

            return action != null;
        }
        

        public ActionBase TriggerAction(string actionName)
        {
            if (this.HasAction(actionName, out ActionBase original, false) == false)
            {
                Debug.LogWarning($"{actionName}이 존재하지 않습니다.");
                return null;
            }

            ActionBase action = _controller.GetPooledAction(original.hash);

            if (action != null)
            {
                action.OnGetFromPool();
                this.TriggerAction(action);
                return action;
            }

            if (original != null)
            {
                action = original.InstantiateSelf<ActionBase>();
                action.Initialize(_controller);
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
        

        public void RegisterActionToRunningQueue(ActionBase action)
        {
            _actionQueues.Item1.Enqueue(action);
        }

        
        public void UnregisterActionFromRunningQueue(ActionBase action)
        {
            _actionQueues.Item2.Enqueue(action);
        }
        

        public void UpdateActions(EffectHandler effectHandler)
        {
            // 액션 등록 처리
            while (_actionQueues.Item1.Count > 0)
            {
                _runningActions.Add(_actionQueues.Item1.Dequeue());
            }

            // 실행 중인 액션 업데이트
            int runningActionsCount = _runningActions.Count;

            for (int i = 0; i < runningActionsCount; ++i)
            {
                ActionBase action = _runningActions[i];

                if (action.isActive == false)
                {
                    continue;
                }

                if (action.CheckFinish())
                {
                    effectHandler.HandleActionEnd(action);
                    _actionQueues.Item2.Enqueue(action);
                }
                else
                {
                    action.Update();
                }
            }

            // 액션 제거 처리
            while (_actionQueues.Item2.Count > 0)
            {
                ActionBase action = _actionQueues.Item2.Dequeue();
                _runningActions.Remove(action);
                _controller.EnqueueForDestroy(action);
                action.Reset();
            }
        }
    }
}