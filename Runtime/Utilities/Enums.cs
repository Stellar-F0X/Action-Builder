using System;
using UnityEngine;

namespace ActionBuilder.Runtime
{
    public enum AnimationEventTiming
    {
        Start,
        Cancel,
        Pause,
        End,
        InTime
    };


    public enum ActionDuration
    {
        Instant,
        Infinite,
        Duration
    }


    public enum SignalReceiveType
    {
        None,
        Once,
        Range
    }


    /// <summary> Options determining when an effect should be considered finished </summary>
    public enum EffectFinishOption
    {
        /// <summary> 지속 시간이 끝나면 완료되는 Option, 설정된 적용 횟수만큼 적용되도, 지속 시간이 남았다면 완료가 안됨. </summary>
        [Tooltip("Effect is finished when duration ends. Even if apply count is met, effect won't finish until duration expires.")]
        DurationEnded = 0,

        /// <summary> Effect가 설정된 적용 횟수만큼 적용된다면 완료되는 Option, 이 Option은 지속 시간이 끝나도 완료됨. </summary>
        [Tooltip("Effect is finished when apply count is met. Effect will finish when apply count is met even if duration remains.")]
        ApplyCompleted = 1,
    }


    public enum ActionState
    {
        Idle,
        Playing,
        Paused,
        Finished,
        Cancelled
    }


    public enum ApplyPolicy
    {
        Auto,
        Manual
    }


    /// <summary> 차징 타입을 정의하는 열거형 </summary>
    public enum ChargeType
    {
        /// <summary>토글 방식: 한 번 누르면 시작, 다시 누르면 중단</summary>
        Toggle
    }

    /// <summary> 차징 완료 시 동작을 정의하는 열거형 </summary>
    public enum ChargeCompletionBehavior
    {
        /// <summary>차징이 완료되면 취소됨</summary>
        Cancel,

        /// <summary>차징이 완료되면 시전됨</summary>
        Execute
    }


    public enum StatModifierReleaseOption
    {
        /// <summary> 해제하지 않음. </summary>
        None,

        /// <summary> Effect가 release될 때 해제 </summary>
        OnEffectRelease,

        /// <summary> Action이 End될 때 해제 </summary>
        OnActionEnd
    }


    public enum StatModifierType
    {
        /// <summary> 단순히 수치를 더하는 등의 연산. </summary>
        Additive = 0,

        /// <summary> 수치를 곱한다. </summary>
        Multiplicative = 1,

        /// <summary> 값을 덮어씌운다. </summary>
        Override = 2
    }


    [Flags]
    public enum SoundPlayMode
    {
        None = 0,

        /// <summary>
        /// Effect가 Apply될 때 재생
        /// </summary>
        PlayOnApply = 1 << 0,

        /// <summary>
        /// Action이 시작될 때 재생
        /// </summary>
        PlayOnActionStart = 1 << 1,

        /// <summary>
        /// Action이 끝날 때 재생
        /// </summary>
        PlayOnActionEnd = 1 << 2,
    }


    /// <summary> 오디오 클립 재생 방식 </summary>
    public enum AudioPlayType
    {
        /// <summary> 모든 클립을 동시에 재생 </summary>
        PlayAll,
        
        /// <summary> 랜덤으로 하나만 재생 </summary>
        RandomSingle,
    }
}