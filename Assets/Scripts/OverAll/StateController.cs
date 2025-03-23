using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class StateController : MonoBehaviour
{
    public State CurrentState { get; private set; }

    [SerializeField]
    private MainContrller _mainController;

    [SerializeField]
    private UIDocument _overAllUi;

    [SerializeField]
    private UIDocument _titleUi;

    [SerializeField]
    private UIDocument _fieldUi;

    [SerializeField]
    private UIDocument _battleUi;

    [SerializeField]
    private UIDocument _resultUi;

    private VisualElement _overAllroot;
    private VisualElement _titleRoot;
    private VisualElement _fieldRoot;
    private VisualElement _battleRoot;
    private VisualElement _resultRoot;

    private Color _blackoutColor = new Color(0f, 0f, 0f, 0.8f);

    private VisualElement _colorEffectPanel;

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
        }
        _mainController.StartNewTurn().Forget();
    }

    private void SwitchBattleState()
    {
        _battleRoot.style.display = DisplayStyle.Flex;
        BlackoutField();
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
