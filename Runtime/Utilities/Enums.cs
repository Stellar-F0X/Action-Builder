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

    
    public enum EffectEndPolicy
    {
        EffectDurationEnd = 0,
        
        StopOnActionEnd = 1,
    };


    public enum StatModifierType
    {
        /// <summary> 단순히 수치를 더하는 등의 연산. </summary>
        Additive = 0,

        /// <summary> 수치를 곱한다. </summary>
        Multiplicative = 1,

        /// <summary> 값을 덮어씌운다. </summary>
        Override = 2
    }


    /// <summary> 오디오 클립 재생 방식 </summary>
    public enum AudioPlayType
    {
        /// <summary> 모든 클립을 동시에 재생 </summary>
        PlayAll,
        
        /// <summary> 랜덤으로 하나만 재생 </summary>
        RandomSingle,
    }

    
    public enum SpawnAnchorMode
    {
        /// <summary> 스폰 주체의 위치, 회전. </summary>
        InternalTransform,
        
        /// <summary> 마우스 커서나, 그 외 직접 지정된 오브젝트. </summary>
        ExternalTarget,
        
        /// <summary> SearchTargetsEffect를 통해 캐싱된 적들. </summary>
        CachedTarget
    }
}