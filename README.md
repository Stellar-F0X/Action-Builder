## Third-Party Notice

This project includes the following open-source component:

- **[Unity-SerializeReferenceExtensions v1.6.1](https://github.com/mackysoft/Unity-SerializeReferenceExtensions/releases/tag/1.6.1)**  
  Copyright (c) 2021 Hiroya Aramaki <br>
Licensed under the [MIT License](https://github.com/mackysoft/Unity-SerializeReferenceExtensions/blob/master/LICENSE)


- **[Unity_HideIf](https://github.com/Baste-RainGames/Unity_HideIf?tab=MIT-1-ov-file)**  
  Copyright (c) 2017 Baste Nesse Buanes <br>
Licensed under the [MIT License](https://github.com/Baste-RainGames/Unity_HideIf/blob/master/LICENSE)

<br>

# Action-Builder

Unity Engine 기반의 액션 및 이펙트 관리 시스템입니다. <br>
액션과 그로부터 발동되는 동작을 관리할 수 있습니다. <br>

- 스킬
- 공격
- 동작 

Action System은 게임 캐릭터의 다양한 동작(액션)과 그 동작으로부터 발생하는 효과(이펙트)를 체계적으로 관리하기 위한 에디터 시스템입니다.<br>
대부분의 액션은 엔티티의 스탯을 기반으로 동작하며, 이를 쉽게 설정하고 제어할 수 있도록 설계했습니다.

<br>

# 주요 기능
## Action (액션)

스킬, 공격, 회피, 점프 등 게임 내 모든 동작을 정의합니다.

| 기능 | 설명 |
|------|------|
| **Duration 타입** | Instant(즉시), Duration(지속 시간), Infinite(무한) |
| **쿨다운 시스템** | 액션 재사용 대기 시간 관리 |
| **상태 관리** | Idle, Playing, Paused, Cancelled, Finished |
| **이벤트 시스템** | 시작, 종료, 일시정지, 재개, 취소 이벤트 제공 |

<br>

## Effect (이펙트)

액션으로부터 발동되는 효과들을 정의합니다.

| 기능 | 설명 |
|------|------|
| **Apply Policy** | Auto(자동 적용), Manual(수동 적용) |
| **적용 횟수 및 간격** | 이펙트를 여러 번 반복 적용 가능 |
| **Duration 및 Delay** | 효과의 지속 시간과 발동 지연 설정 |
| **End Policy** | 액션 종료 시 또는 이펙트 duration 종료 시 정리 |
| **Auto Release** | 종료 시 자동으로 리소스 해제 |

<br>

# Installation

```
https://github.com/Stellar-F0X/Stat-Controller.git
```