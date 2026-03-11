<p align="center">
  <img width="1536" height="350" alt="Myplanet banner2" src="https://github.com/user-attachments/assets/7fd7a636-c3b9-44a7-ab8a-876d220f08c9" />
</p>

# MyPlanet

다양한 선택지를 통해 매 순간마다 새로운 전략으로 플레이하는 로그라이트 뱀서류 게임

---

## 프로젝트 소개

* 개발 인원 : 개발자 3인, 기획자 3인 (협업 프로젝트)
* 개발 기간 : 2025.11.07 ~ 2026.01.06 (8주)
* 빌드 : Android (Google PlayStore 등록)
* 개발 환경 : Unity 6.x

---

### 게임 설명

* 몰려오는 적들을 잡아 레벨업하고, 원하는 선택지를 골라 자신이 원하는 플레이 전략을 맞춰나갑니다.
* 모든 웨이브를 버티면서 마지막 보스를 잡아 스테이지를 클리어합니다.
* 메인 로비에서는 더 좋은 전략을 세우기 위해 행성과 타워를 강화하거나 도감에서 등장 확률을 조작할 수 있습니다.

### 장르

* 캐주얼
* 로그라이크
* 뱀서라이크

---

## 게임 스크린샷 및 GIF

|  인게임 전투 화면  | 증강 선택지 화면  |  도감 기능  |  타워 강화  |
|  --------------------------------- | ----------------------------------- | --------------------------------- | --------------------------------- |
|  ![](https://github.com/user-attachments/assets/52b3ecf3-1aa7-4fda-ae0a-75db9394975e) |  ![](https://github.com/user-attachments/assets/104af4ac-9b34-4394-8480-36e883fc0979)  | ![](https://github.com/user-attachments/assets/2adfd9eb-5e1f-4bc1-b533-93097babd33e) | ![](https://github.com/user-attachments/assets/86a7001b-c66f-486d-9b41-5bdd462a6eea) |

---

## 프로젝트 스트립트 폴더 구조

```text
Assets/
└── Scripts/
    ├── Ability/                # 버프 시스템
    ├── AsyncRaid/              # 비동기 레이드 관련 로직
    ├── BalanceTest/            # 밸런스 테스트용 기능
    ├── DataTable/              # 데이터 테이블
    │   ├── GatchaAndItems/     # 가챠 및 아이템 데이터
    │   └── Tower/              # 타워 데이터 테이블
    ├── Effect/                 # 이펙트 관련 처리
    ├── Enemy/
    │   ├── Movement/           # 적 이동 로직
    │   ├── Pattern/            # 적 패턴 로직
    │   └── Test/               # 적 테스트용 코드
    ├── Firebase/               # Firebase 관련 시스템
    ├── Interface/              # 인터페이스 정의
    ├── Item/                   # 드롭 아이템
    ├── Managers/               # 매니저 클래스
    ├── Planet/                 # 행성 및 타워 작동 관련
    ├── Title/                  # 타이틀 씬 관련
    ├── TowerSystem/            # 각 타워 시스템
    │   ├── PriortyStrategy/    # 타겟 우선순위 전략
    │   ├── TargetingSystem/    # 타겟팅 시스템
    │   └── TowerData/
    │       ├── AmplifierTowerData/   # 증폭 타워 데이터 및 SO
    │       ├── AttackTowerData/      # 공격 타워 데이터 및 SO
    │       ├── PriorityStrategyData/ # 우선순위 전략 데이터
    │       └── RangeData/            # 사거리 데이터
    ├── UI/
    │   └── UIAnimator/         # UI 애니메이션 처리
    └── Upgrades/               # 증강 시스템
```

---

## 담당 기능 및 시스템

###  타워 및 행성 관련 시스템

| 구현 기능 | 관련 링크 |
| --- | --- |
| 행성 이동 기능 | [PlayerMove](Assets/Scripts/PlayerMove.cs) |
| 타워 버프 시스템 | [Ability](Assets/Scripts/Ability) |

### 전투 시스템

| 구현 기능 | 관련 링크 |
| --- | --- |
| 비동기 레이드 전투 구현 | [AsyncRaid](Assets/Scripts/AsyncRaid) |
| 퀘이사 아이템 구현 | [PowerUpItem](Assets/Scripts/TowerSystem/PowerUpItemControlUI.cs) |

###  게임 외부 시스템

| 구현 기능 | 관련 링크 |
| --- | --- |
| 타워 강화 시스템 구현 | [LobbyTowerUpgrade](Assets/Scripts/Upgrades/LobbyTowerUpgrade.cs) |
| 상점 시스템 구현 | [StoreUI](Assets/Scripts/UI/StoreUI.cs) |
| 로그인 기능 구현 | [AuthManager](Assets/Scripts/Firebase/AuthManager.cs) |
| 터치 관련 매니져 구현 | [TouchManager](Assets/Scripts/Managers/TouchManager.cs)  |
| 밸런스 테스트 툴 제작 | [BalanceWindow](Assets/Editor/BalanceWindow.cs) |
| UI 배치 | Unity Scene |

---

## 기술 스택 / 개발 환경

프로젝트 개발 환경 및 사용 기술

| Category        | Technology           |
| --------------- | -------------------- |
| Engine          | Unity                |
| Language        | C#                   |
| IDE             | Visual Studio        |
| Version Control | Git / GitHub         |
| Other Tool      | Firebase             |





