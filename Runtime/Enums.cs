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
    
    public enum EffectFinishOption
    {
        // Effect가 설정된 적용 횟수만큼 적용된다면 완료되는 Option.
        // 단, 이 Option은 지속 시간(=Duration)이 끝나도 완료됨.
        // 타격을 입힌다던가, 치료를 해주는 Effect에 적합Option
        CycleCompleted,
        // 지속 시간이 끝나면 완료되는 Option.
        // Effect가 설정된 적용 횟수만큼 적용되도, 지속 시간이 남았다면 완료가 안됨.
        // 처음 한번 적용되고, 일정 시간동안 지속되는 Buff나 Debuff Effect에 적합한 Option.
        DurationEnded,
        
        // Effect가 설정된 적용 횟수만큼 적용되거나, 지속 시간이 끝나면 완료되는 Option.
        // CycleCompleted와 DurationEnded의 조합.
        CycleOrDuration
    }

    public enum ActionState
    {
        Idle,
        Playing,
        Paused,
        Finished,
        Cancelled
    }
}