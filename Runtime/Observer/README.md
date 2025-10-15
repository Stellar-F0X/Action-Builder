MonoObserver는 Effect가 Action의 라이프사이클에 종속되지 않게 Effect 생성 이후 라이프 사이클을 관찰하는 객체이다.

EffectBase.cs에 라이프사이클에 연결될 것인지 여부를 저장하는 필드가 있다.
```C#
public bool linkActionLifetime
```

만약 이게 켜져있다면, MonoObserver가 해당 이펙트의 IsPlaying 프로퍼티를 업데이트하며 False가 됐을 때, 자동으로 끈다.

또한 MonoObserver는 ActionBase의 executionData와 EffectBase의 executionData에 따라 기능을 호출하는 기능이 있다.

linkActionLifetime가 켜져있다해도, MonoObserver를 통해서 Effect에 접근할 수 있다. 

예를 들어 ApplyPolicy가 Manual이라면, 유저가 ActionController - Action - Effect를 통해 ManualApply를 호출할 수 있다.

마찬가지로 Auto일때도 호출은 가능하나, MonoObserver가 생성될 당시의 EffectBase에 설정된 정보를 토대로 자동으로 호출한다.