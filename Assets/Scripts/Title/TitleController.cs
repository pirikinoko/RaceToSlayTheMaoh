using UnityEngine;
using UnityEngine.UIElements;

public class TitleController : MonoBehaviour
{
    [SerializeField]
    private MainController _mainController;

    [SerializeField]
    private StateController _stateContoller;

    private Button _buttonStartLocal;
    private Button _buttonStartOnline;

    private void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        _buttonStartLocal = root.Q<Button>("Button-Start-Local");
        _buttonStartLocal.clicked += StartLocalGame;
        _buttonStartOnline = root.Q<Button>("Button-Start-Online");
        _buttonStartOnline.clicked += StartOnlineGame;
        root.Q<Button>("ArrowLeft").clicked += DecreasePlayerCount;
        root.Q<Button>("ArrowRight").clicked += IncreasePlayerCount;

        _buttonStartLocal.text = Constants.GetSentenceForLocalPlayButton(Settings.Language, _mainController.PlayerCount);
    }

    private void StartLocalGame()
    {
        _mainController.GameMode = GameMode.Local;
        _stateContoller.ChangeState(State.Field);
    }

    private void StartOnlineGame()
    {
        _mainController.GameMode = GameMode.Online;
        _stateContoller.ChangeState(State.Field);
    }

    private void DecreasePlayerCount()
    {
        int playerCount = _mainController.PlayerCount - 1;
        if (playerCount < 1) playerCount = 1;
        _buttonStartLocal.text = Constants.GetSentenceForLocalPlayButton(Settings.Language, playerCount);
        _mainController.PlayerCount = playerCount;
    }

    private void IncreasePlayerCount()
    {
        int playerCount = _mainController.PlayerCount + 1;
        if (playerCount > Constants.MaxPlayerCount) playerCount = Constants.MaxPlayerCount;
        _buttonStartLocal.text = Constants.GetSentenceForLocalPlayButton(Settings.Language, playerCount);
        _mainController.PlayerCount = playerCount;
    }
}