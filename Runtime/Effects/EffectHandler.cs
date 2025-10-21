using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ActionBuilder.Runtime
{
    public class EffectHandler
    {
        public EffectHandler(ActionController controller, Dictionary<int, List<EffectBase>> runningEffects, ValueTuple<Queue<EffectBase>, Queue<EffectBase>> queues
        )
        {
            _effectQueues = queues;
            _controller = controller;
            _runningEffects = runningEffects;
        }

        private readonly ActionController _controller;

        private readonly Dictionary<int, List<EffectBase>> _runningEffects;

        private readonly ValueTuple<Queue<EffectBase>, Queue<EffectBase>> _effectQueues;



        public List<EffectBase> GetRunningEffects(int actionHash)
        {
            return _runningEffects.GetValueOrDefault(actionHash);
        }


        public void StopAllEffects()
        {
            foreach (List<EffectBase> effectList in _runningEffects.Values)
            {
                effectList.ForEach(e => e.Cancel());
            }
        }


        public void CancelEffectsByActionType(string actionName, ActionHandler actionHandler)
        {
            if (actionHandler.HasAction(actionName, out ActionBase action, true) == false)
            {
                Debug.LogError($"동작 중인 액션 {actionName}를 찾을 수 없습니다.");
                return;
            }

            if (_runningEffects.TryGetValue(action.hash, out List<EffectBase> list))
            {
                list.ForEach(e => e.Cancel());
            }
        }


        public void ManualReleaseEffects(string actionName, string effectName, ActionHandler actionHandler)
        {
            if (string.IsNullOrEmpty(actionName) || string.IsNullOrEmpty(effectName))
            {
                Debug.LogError("Action Name 또는 Effect Name이 비었습니다.");
                return;
            }

            if (actionHandler.HasAction(actionName, out ActionBase action, true) == false)
            {
                Debug.LogError($"{actionName}.{effectName}을 찾을 수 없습니다.");
                return;
            }

            foreach (EffectBase effect in _runningEffects.GetValueOrDefault(action.hash))
            {
                if (effect.name == effectName)
                {
                    effect.Release();
                }
            }
        }


        public EffectBase[] ManualApplyEffects(string actionName, string effectName, ActionHandler actionHandler)
        {
            if (string.IsNullOrEmpty(actionName) || string.IsNullOrEmpty(effectName))
            {
                Debug.LogError("Action Name 또는 Effect Name이 비었습니다.");
                return Array.Empty<EffectBase>();
            }

            if (actionHandler.HasAction(actionName, out ActionBase action, true))
            {
                EffectBase[] effects = _runningEffects[action.hash].Where(effect => effect.name == effectName) .ToArray();

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
            if (_runningEffects.TryGetValue(effect.action.hash, out List<EffectBase> list) == false)
            {
                return false;
            }

            int idx = list.IndexOf(effect);

            if (idx < 0)
            {
                return false;
            }

            list[idx].ManualApply();
            return true;

        }


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


        public void HandleActionEnd(ActionBase action)
        {
            if (_runningEffects.TryGetValue(action.hash, out var list))
            {
                list.ForEach(this.ManageEffectsOnActionEnd);
            }
        }


        public void UpdateEffects()
        {
            // Effect 등록 처리
            while (_effectQueues.Item1.Count > 0)
            {
                EffectBase effect = _effectQueues.Item1.Dequeue();
                _runningEffects[effect.action.hash].Add(effect);
                effect.action.activeEffectCount++;
            }

            // 실행 중인 Effect 업데이트
            foreach (List<EffectBase> effects in _runningEffects.Values)
            {
                this.UpdateEffectsList(effects);
            }

            // Effect 제거 처리
            while (_effectQueues.Item2.Count > 0)
            {
                EffectBase effect = _effectQueues.Item2.Dequeue();
                _runningEffects[effect.action.hash].Remove(effect);
                effect.action.activeEffectCount--;
                _controller.EnqueueForDestroy(effect);
            }
        }


        private void UpdateEffectsList(List<EffectBase> effects)
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


        private void ManageEffectsOnActionEnd(EffectBase effect)
        {
            bool finishEffect = effect.endPolicy == EffectEndPolicy.StopOnActionEnd;

            if (effect.endPolicy == EffectEndPolicy.EffectDurationEnd)
            {
                finishEffect |= effect.elapsedTime > effect.duration;
            }

            if (finishEffect == false)
            {
                return;
            }

            if (effect.autoRelease)
            {
                effect.Release();
            }

            effect.OnActionEnd();
            this.UnregisterEffectFromRunningQueue(effect);
        }
    }
}