# Unity project context

- Checked: 2026-09-29. Git metadata was unavailable in this project folder.
- Root: `BlackJack` Unity project. Unity 6000.6.3f1.
- Packages: URP 17.6, uGUI 2.6, Input System 1.20, Unity Test Framework 1.8. Main scene uses a 2D URP camera, but gameplay visuals use a Screen Space Overlay Canvas.
- First-party runtime code: `Assets/Blackjack`. Plain C# rules in `BlackjackGame`; `BlackjackController` owns simple start, hit, stand, result, and restart UI. Editor-only scene authoring in `Assets/Blackjack/Editor`.
- Startup: `Assets/Scenes/main.unity`, enabled as the sole Build Settings scene. It contains a camera, 2D light, `Blackjack Canvas`, and `EventSystem`.
- Input: Input System UI module. The scene UI is uGUI and scales from 960×600.
- Tests: Unity Test Framework is installed; no first-party test assembly was present during onboarding. Editor compilation and a Play Mode button flow were checked for this feature. A WebGL build was not run.
- Assets: `Assets/Image` contains four 13-sprite card sheets (A through K from left to right) and a card back. The Canvas displays those sprites.
- Tooling: a connected Unity Editor MCP was available for compile, scene, and Play Mode checks.
- Editing: change scene RectTransforms for layout, `BlackjackController` Inspector for card sprites, and `BlackjackGame` for rules. The `Tools/Blackjack/Build Main Scene` menu regenerates the base Canvas.
