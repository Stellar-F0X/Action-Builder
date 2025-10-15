using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ActionBuilder.Runtime
{
    [DefaultExecutionOrder(-2)]
    public class ActionController : MonoBehaviour
    {
        /// <summary> Action이 컨트롤러에 등록될 때 발생 </summary>
                public event Action<ActionBase> onActionRegistered;
        
                /// <summary> Action이 컨트롤러에서 제거될 때 발생 </summary>
                public event Action<ActionBase> onActionUnregistered;
        
                /// <summary> Action이 시작될 때 발생 </summary>
                public event Action<ActionBase> onActionStarted;
        
                /// <summary> Action이 종료될 때 발생 </summary>
                public event Action<ActionBase> onActionEnded;
        
                /// <summary> Action이 일시정지될 때 발생 </summary>
                public event Action<ActionBase> onActionPaused;
        
                /// <summary> Action이 재개될 때 발생 </summary>
                public event Action<ActionBase> onActionResumed;
        
                /// <summary> Action이 취소될 때 발생 </summary>
                public event Action<ActionBase> onActionCancelled;
        
                /// <summary> Effect가 Action에 등록될 때 발생 </summary>
                public event Action<ActionBase, EffectBase> onEffectRegistered;
        
                /// <summary> Effect가 Action에서 제거될 때 발생 </summary>
                public event Action<ActionBase, EffectBase> onEffectUnregistered;
        
                /// <summary> 런타임 Action 인스턴스들의 Dictionary (statKey: actionName string) </summary>
                private readonly Dictionary<string, ActionBase> _actionInstances = new Dictionary<string, ActionBase>();
        
                /// <summary> 런타임 Action 인스턴스들의 Dictionary (statKey: actionName hash) </summary>
                private readonly Dictionary<int, ActionBase> _actionInstancesByHash = new Dictionary<int, ActionBase>();
        
                /// <summary> 현재 실행 중인 Action들의 리스트 </summary>
                private readonly List<ActionBase> _activeActions = new List<ActionBase>();
        
                /// <summary> 실행 완료되어 제거 대기 중인 Action들의 리스트 </summary>
                private readonly List<ActionBase> _finishedActions = new List<ActionBase>();
        
                /// <summary> 이벤트 채널 Dictionary </summary>
                private readonly Dictionary<UGUID, EventChannelBase> _eventChannels = new Dictionary<UGUID, EventChannelBase>();
        
                public bool debug;
                
                /// <summary> 등록된 모든 Action들의 SO 원본 리스트 (Inspector 설정용) </summary>
                [SerializeField]
                private List<ActionBase> _actionTemplates = new List<ActionBase>();
        
        
        #region Properties
        
                /// <summary> 등록된 모든 Action 인스턴스들 (읽기 전용) </summary>
                public IReadOnlyDictionary<string, ActionBase> actionInstances
                {
                    get { return _actionInstances; }
                }
        
        
                /// <summary> 등록된 모든 Action 인스턴스들 해시 Dictionary (읽기 전용) </summary>
                public IReadOnlyDictionary<int, ActionBase> actionInstancesByHash
                {
                    get { return _actionInstancesByHash; }
                }
        
        
                /// <summary> 현재 실행 중인 Action들 (읽기 전용) </summary>
                public IReadOnlyList<ActionBase> activeActions
                {
                    get { return _activeActions; }
                }
        
        
                /// <summary> 등록된 Action 개수 </summary>
                public int actionCount
                {
                    get { return _actionInstances.Count; }
                }
        
        
                /// <summary> 실행 중인 Action 개수 </summary>
                public int activeActionCount
                {
                    get { return _activeActions.Count; }
                }
        
        #endregion
        
        
                private void Awake()
                {
                    this._actionInstances.Clear();
                    this._actionInstancesByHash.Clear();
                    this._activeActions.Clear();
                    this._finishedActions.Clear();
                    this._eventChannels.Clear();
                }
        
        
                private void Start()
                {
                    if (_actionTemplates is null || _actionTemplates.Count == 0)
                    {
                        return;
                    }
        
                    foreach (ActionBase template in _actionTemplates)
                    {
                        if (template == null)
                        {
                            continue;
                        }
        
                        this.RegisterAction(template);
                    }
                }
        
        
                private void Update()
                {
                    // 활성 Action들 실행 (역순으로 순회하여 실행 중 리스트 변경에 안전)
                    for (int i = _activeActions.Count - 1; i >= 0; --i)
                    {
                        ActionBase action = _activeActions[i];
        
                        if (action == null)
                        {
                            _activeActions.RemoveAt(i);
                            continue;
                        }
        
                        action.Execute(Time.deltaTime);
        
                        // 비활성 상태가 되면 활성 리스트에서 제거
                        if (action.isActive == false)
                        {
                            _activeActions.RemoveAt(i);
                        }
                    }
        
                    // 완료된 Action들 정리
                    if (_finishedActions.Count <= 0)
                    {
                        return;
                    }
        
                    _finishedActions.Clear();
                }
        
        
                private void OnDestroy()
                {
                    // 모든 Action 취소
                    this.CancelAllActions();
        
                    // 모든 Action 이벤트 정리
                    foreach (ActionBase action in _actionInstances.Values)
                    {
                        action.onStarted -= this.HandleActionStarted;
                        action.onEnded -= this.HandleActionEnded;
                        action.onPaused -= this.HandleActionPaused;
                        action.onResumed -= this.HandleActionResumed;
                        action.onCancelled -= this.HandleActionCancelled;
        
                        action.ClearAllEvents();
                        Object.DestroyImmediate(action);
                    }
        
                    // 컨트롤러 이벤트 정리
                    this.ClearAllControllerEvents();
        
                    // 리스트 정리
                    _actionInstances.Clear();
                    _actionInstancesByHash.Clear();
                    _activeActions.Clear();
                    _finishedActions.Clear();
                    _eventChannels.Clear();
                }
        
        
                /// <summary> 컨트롤러의 모든 이벤트 정리 </summary>
                public void ClearAllControllerEvents()
                {
                    this.onActionRegistered = null;
                    this.onActionUnregistered = null;
                    this.onActionStarted = null;
                    this.onActionEnded = null;
                    this.onActionPaused = null;
                    this.onActionResumed = null;
                    this.onActionCancelled = null;
                    this.onEffectRegistered = null;
                    this.onEffectUnregistered = null;
                }
        
        
        #region Action Management
        
                /// <summary> Action을 컨트롤러에 등록 (SO를 인스턴스로 복제) </summary>
                /// <param name="actionTemplate">등록할 Action SO 템플릿</param>
                /// <returns>등록 성공 여부</returns>
                public bool RegisterAction(ActionBase actionTemplate)
                {
                    if (actionTemplate == null)
                    {
                        Debug.LogError($"[ActionController] Action template is null");
                        return false;
                    }
        
                    string actionName = actionTemplate.actionName;
        
                    if (_actionInstances.ContainsKey(actionName))
                    {
                        Debug.LogWarning($"[ActionController] Action '{actionName}' is already registered");
                        return false;
                    }
        
                    // SO를 런타임 인스턴스로 복제
                    ActionBase actionInstance = Object.Instantiate(actionTemplate);
                    actionInstance.name = actionName;
        
                    // Action 초기화 및 이벤트 구독
                    actionInstance.Initialize(this);
                    actionInstance.InitializeChannels(_eventChannels);
        
                    actionInstance.onStarted += this.HandleActionStarted;
                    actionInstance.onEnded += this.HandleActionEnded;
                    actionInstance.onPaused += this.HandleActionPaused;
                    actionInstance.onResumed += this.HandleActionResumed;
                    actionInstance.onCancelled += this.HandleActionCancelled;
        
                    // Dictionary에 등록
                    _actionInstances[actionName] = actionInstance;
                    _actionInstancesByHash[actionTemplate.hash] = actionInstance;
        
                    // 이벤트 발생
                    this.onActionRegistered?.Invoke(actionInstance);
        
                    if (debug)
                    {
                        Debug.Log($"[ActionController] Action '{actionName}' registered successfully");
                    }
                    
                    return true;
                }
        
        
                /// <summary> Action을 컨트롤러에 등록 (문자열 이름으로) </summary>
                /// <param name="actionName">등록할 Action 이름</param>
                /// <returns>등록 성공 여부</returns>
                public bool RegisterAction(string actionName)
                {
                    foreach (ActionBase template in _actionTemplates)
                    {
                        if (template != null && template.actionName == actionName)
                        {
                            return this.RegisterAction(template);
                        }
                    }
        
                    Debug.LogError($"[ActionController] Action template '{actionName}' not found in templates list");
                    return false;
                }
        
        
                /// <summary> Action을 컨트롤러에 등록 (해시로) </summary>
                /// <param name="actionHash">등록할 Action 해시</param>
                /// <returns>등록 성공 여부</returns>
                public bool RegisterAction(int actionHash)
                {
                    foreach (ActionBase template in _actionTemplates)
                    {
                        if (template != null && template.hash == actionHash)
                        {
                            return this.RegisterAction(template);
                        }
                    }
        
                    Debug.LogError($"[ActionController] Action template with hash '{actionHash}' not found in templates list");
                    return false;
                }
        
        
                /// <summary> Action을 컨트롤러에서 제거 </summary>
                /// <param name="actionTemplate">제거할 Action 템플릿</param>
                /// <returns>제거 성공 여부</returns>
                public bool UnregisterAction(ActionBase actionTemplate)
                {
                    if (actionTemplate == null)
                    {
                        Debug.LogError($"[ActionController] Action template is null");
                        return false;
                    }
        
                    string actionName = actionTemplate.actionName;
                    int actionHash = actionTemplate.hash;
        
                    if (_actionInstances.TryGetValue(actionName, out ActionBase actionInstance) == false)
                    {
                        Debug.LogWarning($"[ActionController] Action '{actionName}' not found for unregistration");
                        return false;
                    }
        
                    if (actionInstance == null)
                    {
                        _actionInstances.Remove(actionName);
                        _actionInstancesByHash.Remove(actionHash);
                        return false;
                    }
        
                    // 실행 중이라면 취소
                    if (actionInstance.isActive)
                    {
                        actionInstance.Cancel();
                    }
        
                    // 이벤트 구독 해제
                    actionInstance.onStarted -= this.HandleActionStarted;
                    actionInstance.onEnded -= this.HandleActionEnded;
                    actionInstance.onPaused -= this.HandleActionPaused;
                    actionInstance.onResumed -= this.HandleActionResumed;
                    actionInstance.onCancelled -= this.HandleActionCancelled;
        
                    // Dictionary에서 제거
                    _actionInstances.Remove(actionName);
                    _actionInstancesByHash.Remove(actionHash);
        
                    // 인스턴스 파괴
                    Object.DestroyImmediate(actionInstance);
        
                    // 이벤트 발생
                    this.onActionUnregistered?.Invoke(actionInstance);
        
                    return true;
                }
        
        
                /// <summary> Action을 컨트롤러에서 제거 (문자열 이름으로) </summary>
                /// <param name="actionName">제거할 Action 이름</param>
                /// <returns>제거 성공 여부</returns>
                public bool UnregisterAction(string actionName)
                {
                    if (_actionInstances.TryGetValue(actionName, out ActionBase actionInstance))
                    {
                        return this.UnregisterAction(actionInstance);
                    }
        
                    Debug.LogWarning($"[ActionController] Action '{actionName}' not found for unregistration");
                    return false;
                }
        
        
                /// <summary> Action을 컨트롤러에서 제거 (해시로) </summary>
                /// <param name="actionHash">제거할 Action 해시</param>
                /// <returns>제거 성공 여부</returns>
                public bool UnregisterAction(int actionHash)
                {
                    if (_actionInstancesByHash.TryGetValue(actionHash, out ActionBase actionInstance))
                    {
                        return this.UnregisterAction(actionInstance);
                    }
        
                    Debug.LogWarning($"[ActionController] Action with hash '{actionHash}' not found for unregistration");
                    return false;
                }
        
        
                /// <summary> 등록된 Action 가져오기 </summary>
                /// <param name="actionName">Action 이름</param>
                /// <returns>Action 인스턴스 또는 null</returns>
                public ActionBase GetAction(string actionName)
                {
                    return _actionInstances.GetValueOrDefault(actionName);
                }
        
        
                /// <summary> 등록된 Action 가져오기 (해시로) </summary>
                /// <param name="actionHash">Action 해시</param>
                /// <returns>Action 인스턴스 또는 null</returns>
                public ActionBase GetAction(int actionHash)
                {
                    return _actionInstancesByHash.GetValueOrDefault(actionHash);
                }
        
        
                /// <summary> 등록된 Action 존재 여부 확인 </summary>
                /// <param name="actionName">Action 이름</param>
                /// <returns>존재 여부</returns>
                public bool HasAction(string actionName)
                {
                    return _actionInstances.ContainsKey(actionName);
                }
        
        
                /// <summary> 등록된 Action 존재 여부 확인 (해시로) </summary>
                /// <param name="actionHash">Action 해시</param>
                /// <returns>존재 여부</returns>
                public bool HasAction(int actionHash)
                {
                    return _actionInstancesByHash.ContainsKey(actionHash);
                }
        
        #endregion
        
        
        #region Action Execution
        
                /// <summary> Action 실행 </summary>
                /// <param name="actionName">실행할 Action 이름</param>
                /// <param name="force">강제 실행 여부</param>
                /// <returns>실행 성공 여부</returns>
                public bool TriggerAction(string actionName, bool force = false)
                {
                    ActionBase action = this.GetAction(actionName);
        
                    if (action == null)
                    {
                        Debug.LogError($"[ActionController] Action '{actionName}' not found for execution");
                        return false;
                    }
        
                    bool success = action.Trigger(force);
        
                    if (success && _activeActions.Contains(action) == false)
                    {
                        _activeActions.Add(action);
                    }
        
                    return success;
                }
        
        
                /// <summary> Action 실행 (해시로) </summary>
                /// <param name="actionHash">실행할 Action 해시</param>
                /// <param name="force">강제 실행 여부</param>
                /// <returns>실행 성공 여부</returns>
                public bool TriggerAction(int actionHash, bool force = false)
                {
                    ActionBase action = this.GetAction(actionHash);
        
                    if (action == null)
                    {
                        Debug.LogError($"[ActionController] Action with hash '{actionHash}' not found for execution");
                        return false;
                    }
        
                    bool success = action.Trigger(force);
        
                    if (success && _activeActions.Contains(action) == false)
                    {
                        _activeActions.Add(action);
                    }
        
                    return success;
                }
        
        
                /// <summary> Action 일시정지 </summary>
                /// <param name="actionName">일시정지할 Action 이름</param>
                public void PauseAction(string actionName)
                {
                    ActionBase action = this.GetAction(actionName);
                    Debug.Assert(action, "Not found action");
                    action?.Pause();
                }
        
        
                /// <summary> Action 일시정지 (해시로) </summary>
                /// <param name="actionHash">일시정지할 Action 해시</param>
                public void PauseAction(int actionHash)
                {
                    ActionBase action = this.GetAction(actionHash);
                    Debug.Assert(action, "Not found action");
                    action?.Pause();
                }
        
        
                /// <summary> Action 재개 </summary>
                /// <param name="actionName">재개할 Action 이름</param>
                public void ResumeAction(string actionName)
                {
                    ActionBase action = this.GetAction(actionName);
                    Debug.Assert(action, "Not found action");
                    action?.Resume();
                }
        
        
                /// <summary> Action 재개 (해시로) </summary>
                /// <param name="actionHash">재개할 Action 해시</param>
                public void ResumeAction(int actionHash)
                {
                    ActionBase action = this.GetAction(actionHash);
                    Debug.Assert(action, "Not found action");
                    action?.Resume();
                }
        
        
                /// <summary> Action 취소 </summary>
                /// <param name="actionName">취소할 Action 이름</param>
                public void CancelAction(string actionName)
                {
                    ActionBase action = this.GetAction(actionName);
                    Debug.Assert(action, "Not found action");
                    action?.Cancel();
                }
        
        
                /// <summary> Action 취소 (해시로) </summary>
                /// <param name="actionHash">취소할 Action 해시</param>
                public void CancelAction(int actionHash)
                {
                    ActionBase action = this.GetAction(actionHash);
                    Debug.Assert(action, "Not found action");
                    action?.Cancel();
                }
        
        
                /// <summary> 모든 활성 Action 일시정지 </summary>
                public void PauseAllActions()
                {
                    if (_activeActions is null || _activeActions.Count == 0)
                    {
                        return;
                    }
        
                    foreach (ActionBase action in _activeActions)
                    {
                        action.Pause();
                    }
                }
        
        
                /// <summary> 모든 일시정지된 Action 재개 </summary>
                public void ResumeAllActions()
                {
                    if (_activeActions is null || _activeActions.Count == 0)
                    {
                        return;
                    }
        
                    foreach (ActionBase action in _activeActions)
                    {
                        if (action.currentState != ActionState.Paused)
                        {
                            continue;
                        }
        
                        action.Resume();
                    }
                }
        
        
                /// <summary> 모든 활성 Action 취소 </summary>
                public void CancelAllActions()
                {
                    if (_activeActions is null || _activeActions.Count == 0)
                    {
                        return;
                    }
        
                    foreach (ActionBase action in _activeActions)
                    {
                        action.Cancel();
                    }
                }
        
        #endregion
        
        
        #region Effect Management
        
                /// <summary> Action에 Effect 동적 등록 </summary>
                /// <param name="actionName">대상 Action 이름</param>
                /// <param name="effect">등록할 Effect</param>
                /// <returns>등록 성공 여부</returns>
                public bool RegisterEffect(string actionName, EffectBase effect)
                {
                    ActionBase action = this.GetAction(actionName);
        
                    if (action == null)
                    {
                        Debug.LogError($"[ActionController] Action '{actionName}' not found for effect registration");
                        return false;
                    }
        
                    if (effect == null)
                    {
                        Debug.LogError($"[ActionController] Effect is null");
                        return false;
                    }
        
                    action.internalEffects.Add(effect);
                    effect.referencedAction = action;
        
                    this.onEffectRegistered?.Invoke(action, effect);
        
                    if (debug)
                    {
                        Debug.Log($"[ActionController] Effect '{effect.effectName}' registered to Action '{actionName}'");
                    }
                    
                    return true;
                }
        
        
                /// <summary> Action에 Effect 동적 등록 (해시로) </summary>
                /// <param name="actionHash">대상 Action 해시</param>
                /// <param name="effect">등록할 Effect</param>
                /// <returns>등록 성공 여부</returns>
                public bool RegisterEffect(int actionHash, EffectBase effect)
                {
                    ActionBase action = this.GetAction(actionHash);
        
                    if (action == null)
                    {
                        Debug.LogError($"[ActionController] Action with hash '{actionHash}' not found for effect registration");
                        return false;
                    }
        
                    if (effect == null)
                    {
                        Debug.LogError($"[ActionController] Effect is null");
                        return false;
                    }
        
                    action.internalEffects.Add(effect);
                    effect.referencedAction = action;
        
                    this.onEffectRegistered?.Invoke(action, effect);
        
                    if (debug)
                    {
                        Debug.Log($"[ActionController] Effect '{effect.effectName}' registered to Action '{action.actionName}'");
                    }
                    
                    return true;
                }
        
        
                /// <summary> Action에서 Effect 제거 </summary>
                /// <param name="actionName">대상 Action 이름</param>
                /// <param name="effect">제거할 Effect</param>
                /// <returns>제거 성공 여부</returns>
                public bool UnregisterEffect(string actionName, EffectBase effect)
                {
                    ActionBase action = this.GetAction(actionName);
        
                    if (action == null)
                    {
                        Debug.LogError($"[ActionController] Action '{actionName}' not found for effect unregistration");
                        return false;
                    }
        
                    if (effect == null)
                    {
                        Debug.LogError($"[ActionController] Effect is null");
                        return false;
                    }
        
                    bool removed = action.internalEffects.Remove(effect);
        
                    if (removed)
                    {
                        effect.referencedAction = null;
                        this.onEffectUnregistered?.Invoke(action, effect);
                        Debug.Log($"[ActionController] Effect '{effect.effectName}' unregistered from Action '{actionName}'");
                    }
                    else
                    {
                        Debug.LogWarning($"[ActionController] Effect '{effect.effectName}' not found in Action '{actionName}'");
                    }
        
                    return removed;
                }
        
        
                /// <summary> Action에서 Effect 제거 (해시로) </summary>
                /// <param name="actionHash">대상 Action 해시</param>
                /// <param name="effect">제거할 Effect</param>
                /// <returns>제거 성공 여부</returns>
                public bool UnregisterEffect(int actionHash, EffectBase effect)
                {
                    ActionBase action = this.GetAction(actionHash);
        
                    if (action == null)
                    {
                        Debug.LogError($"[ActionController] Action with hash '{actionHash}' not found for effect unregistration");
                        return false;
                    }
        
                    if (effect == null)
                    {
                        Debug.LogError($"[ActionController] Effect is null");
                        return false;
                    }
        
                    bool removed = action.internalEffects.Remove(effect);
        
                    if (removed)
                    {
                        effect.referencedAction = null;
                        this.onEffectUnregistered?.Invoke(action, effect);
                        Debug.Log($"[ActionController] Effect '{effect.effectName}' unregistered from Action '{action.actionName}'");
                    }
                    else
                    {
                        Debug.LogWarning($"[ActionController] Effect '{effect.effectName}' not found in Action '{action.actionName}'");
                    }
        
                    return removed;
                }
        
        
                /// <summary> Action의 모든 Effect 제거 </summary>
                /// <param name="actionName">대상 Action 이름</param>
                /// <returns>제거된 Effect 개수</returns>
                public int ClearAllEffects(string actionName)
                {
                    ActionBase action = this.GetAction(actionName);
        
                    if (action == null)
                    {
                        Debug.LogError($"[ActionController] Action '{actionName}' not found for clearing effects");
                        return 0;
                    }
        
                    int removedCount = action.internalEffects.Count;
        
                    foreach (EffectBase effect in action.internalEffects)
                    {
                        effect.referencedAction = null;
                        this.onEffectUnregistered?.Invoke(action, effect);
                    }
        
                    action.internalEffects.Clear();
        
                    Debug.Log($"[ActionController] {removedCount} effects cleared from Action '{actionName}'");
                    return removedCount;
                }
        
        
                /// <summary> Action의 모든 Effect 제거 (해시로) </summary>
                /// <param name="actionHash">대상 Action 해시</param>
                /// <returns>제거된 Effect 개수</returns>
                public int ClearAllEffects(int actionHash)
                {
                    ActionBase action = this.GetAction(actionHash);
        
                    if (action == null)
                    {
                        Debug.LogError($"[ActionController] Action with hash '{actionHash}' not found for clearing effects");
                        return 0;
                    }
        
                    int removedCount = action.internalEffects.Count;
        
                    foreach (EffectBase effect in action.internalEffects)
                    {
                        effect.referencedAction = null;
                        this.onEffectUnregistered?.Invoke(action, effect);
                    }
        
                    action.internalEffects.Clear();
        
                    if (debug)
                    {
                        Debug.Log($"[ActionController] {removedCount} effects cleared from Action '{action.actionName}'");
                    }
                    
                    return removedCount;
                }
        
        #endregion
        
        
        #region Action Event Handlers
        
                private void HandleActionStarted(ActionBase action)
                {
                    this.onActionStarted?.Invoke(action);
                }
        
        
                private void HandleActionEnded(ActionBase action)
                {
                    if (_finishedActions.Contains(action) == false)
                    {
                        _finishedActions.Add(action);
                    }
        
                    this.onActionEnded?.Invoke(action);
                }
        
        
                private void HandleActionPaused(ActionBase action)
                {
                    this.onActionPaused?.Invoke(action);
                }
        
        
                private void HandleActionResumed(ActionBase action)
                {
                    this.onActionResumed?.Invoke(action);
                }
        
        
                private void HandleActionCancelled(ActionBase action)
                {
                    this.onActionCancelled?.Invoke(action);
                }
        
        #endregion
    }
}