// Removed unnecessary using directive
using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class StateController : MonoBehaviour
{
    public State CurrentState { get; private set; }

    private MainController _mainController;
    private FieldController _fieldController;
    private PlayerController _playerController;
    private UIDocument _overAllUi;
    private UIDocument _titleUi;
    private UIDocument _fieldUi;
    private UIDocument _battleUi;
    private UIDocument _resultUi;

    private VisualElement _overAllroot;
    private VisualElement _titleRoot;
    private VisualElement _fieldRoot;
    private VisualElement _battleRoot;
    private VisualElement _resultRoot;

    private Color _blackoutColor = new Color(0f, 0f, 0f, 0.8f);

    private VisualElement _colorEffectPanel;

    public void Initialize(MainController mainController, FieldController fieldController, PlayerController playerController, UIDocument overAllUi, UIDocument titleUi, UIDocument fieldUi, UIDocument battleUi, UIDocument resultUi)
    {
        _mainController = mainController;
        _fieldController = fieldController;
        _playerController = playerController;
        _overAllUi = overAllUi;
        _titleUi = titleUi;
        _fieldUi = fieldUi;
        _battleUi = battleUi;
        _resultUi = resultUi;
    }

    private void Start()
    {
        _overAllroot = _overAllUi.rootVisualElement;
        _titleRoot = _titleUi.rootVisualElement;
        _fieldRoot = _fieldUi.rootVisualElement;
        _battleRoot = _battleUi.rootVisualElement;
        _resultRoot = _resultUi.rootVisualElement;

        _overAllroot.style.display = DisplayStyle.Flex;
        _titleRoot.style.display = DisplayStyle.None;
        _fieldRoot.style.display = DisplayStyle.None;
        _battleRoot.style.display = DisplayStyle.None;
        _resultRoot.style.display = DisplayStyle.None;

        _colorEffectPanel = _overAllroot.Q<VisualElement>("ColorEffectPanel");

        ChangeState(State.Title);
    }

    public void ChangeState(State state)
    {
        CurrentState = state;

        _titleRoot.style.display = DisplayStyle.None;
        _fieldRoot.style.display = DisplayStyle.None;
        _battleRoot.style.display = DisplayStyle.None;
        _resultRoot.style.display = DisplayStyle.None;

        switch (state)
        {
            case State.Title:
                SwitchTitleState();
                break;
            case State.Field:
                SwitchFieldState().Forget();
                break;
            case State.Battle:
                SwitchBattleState();
                break;
            case State.Result:
                SwitchResultState();
                break;
        }
    }

    private void SwitchTitleState()
    {
        _titleRoot.style.display = DisplayStyle.Flex;
        BlackoutField();
    }

    private async UniTask SwitchFieldState()
    {
        _fieldRoot.style.display = DisplayStyle.Flex;
        RevealField();

        if (_mainController.TurnCount == 0)
        {
            await _mainController.InitializeGame();
            // ステータスボックスの表示が不完全なまま見えないように先に中身を更新しておく
            await _fieldController.UpdateStatusBoxesAsync();
            _fieldController.DisplayStatusBoxes();
        }
        await _fieldController.UpdateStatusBoxesAsync();

        await UniTask.Delay(TimeSpan.FromSeconds(1f));
        await _mainController.StartNewTurnAsync();
    }

    private async void SwitchBattleState()
    {
        BlackoutField();
        await UniTask.Delay(TimeSpan.FromSeconds(1f));
        _battleRoot.style.display = DisplayStyle.Flex;

        // スライドイン演出
        // 画面右端から中央へ移動（X座標を調整）
        var width = _battleRoot.resolvedStyle.width;
        if (width == 0) width = 1920; // fallback（エディタで未レイアウト時）
        _battleRoot.style.translate = new StyleTranslate(new Translate(width, 0, 0));
        float duration = 0.4f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float x = Mathf.Lerp(width, 0, t);
            _battleRoot.style.translate = new StyleTranslate(new Translate(x, 0, 0));
            await UniTask.DelayFrame(1);
            elapsed += Time.deltaTime;
        }
        _battleRoot.style.translate = new StyleTranslate(new Translate(0, 0, 0));
    }

    private void SwitchResultState()
    {
        _resultRoot.style.display = DisplayStyle.Flex;
        BlackoutField();
    }


    public void BlackoutField()
    {
        _colorEffectPanel.style.backgroundColor = new StyleColor(_blackoutColor);
    }

    public void RevealField()
    {
        _colorEffectPanel.style.backgroundColor = new Color(0, 0, 0, 0);
    }
}
