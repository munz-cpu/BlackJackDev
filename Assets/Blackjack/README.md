# Simple Blackjack

`Assets/Scenes/main.unity`을 열고 Play를 누르세요. START → HIT 또는 STAND → 중앙 결과 창 → PLAY AGAIN 순서로 진행합니다. 베팅은 없습니다. 딜러는 17 이상에서 멈추며, 에이스는 1 또는 11로 계산합니다.

`Blackjack Canvas`의 `BlackjackController` Inspector에서 **Starting Coins**를 바꾸면 시작 코인 수가 바뀝니다. **Coins Per Win / Coins Per Loss**로 승패마다 변하는 코인 수도 바꿀 수 있습니다. 기본값은 시작 10개, 승리 +1개, 패배 -1개입니다. 무승부는 변동이 없고 코인은 0 아래로 내려가지 않습니다. 코인은 현재 플레이 세션에서 유지되며 게임을 다시 실행하면 Inspector의 시작값으로 초기화됩니다.

화면 전체는 `Blackjack Canvas` 아래의 uGUI입니다. 카드 앞면은 `Assets/Image`의 `ana카드`, `Creal카드`, `Para카드`, `Tapie카드` 시트에서 왼쪽부터 A, 2…K 순서로 연결합니다. 딜러의 숨긴 카드는 `카트겉면.png`를 사용합니다.

왼쪽 카드 더미에서 카드가 손패로 이동하고 앞면으로 뒤집힙니다. 시작, 히트, 딜러 드로우에 같은 연출을 사용합니다. 속도는 `BlackjackController` Inspector의 `Travel Seconds`, `Flip Seconds`에서 바꿀 수 있습니다.

`Assets/sound/카드효과음.mp3`는 카드가 손패에 도착할 때와 딜러의 숨긴 카드를 뒤집을 때 재생됩니다. 음량은 `BlackjackController` Inspector의 `Card Sound Volume`에서 조절합니다.

각 카드의 그림자는 오른쪽 아래로 3×4 UI 단위만큼 약하게 적용됩니다. 승리 또는 블랙잭에는 `성공.mp3`, 패배에는 `미션실패.mp3`가 결과 화면에 맞춰 재생됩니다. 무승부는 결과음이 없습니다. 결과음 크기는 `Result Sound Volume`에서 조절합니다.

수정 위치:

- 카드 규칙과 결과 판정: `BlackjackGame.cs`
- 버튼 동작과 카드 표시: `BlackjackController.cs`
- 색, 글자, 기본 배치: `Editor/BlackjackSceneBuilder.cs`
- 현재 화면의 위치·크기: 씬의 `Blackjack Canvas` 자식 RectTransform
- 카드 스프라이트: `Blackjack Canvas`의 `BlackjackController` Inspector
- 코인 설정: 같은 Inspector의 `Starting Coins`, `Coins Per Win`, `Coins Per Loss`

**Tools > Blackjack > Build Main Scene**은 기본 Canvas를 다시 만듭니다. 씬에서 직접 수정한 배치는 이 메뉴를 실행하면 초기화됩니다.

WebGL 빌드는 `main.unity`을 시작 씬으로 사용하세요. 실제 WebGL 빌드는 아직 실행하지 않았습니다.
