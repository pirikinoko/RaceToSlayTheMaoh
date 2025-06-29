using UnityEngine;
using UnityEngine.UIElements;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;

public class TitleController : MonoBehaviour
{
    [SerializeField]
    private MainController _mainController;

    [SerializeField]
    private StateController _stateContoller;

    [SerializeField]
    private NetworkManager _networkManager;

    private Button _buttonStartLocal;
    private Button _buttonStartMatchMaking;

    private bool _whileMatching = false;
    private void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        _buttonStartLocal = root.Q<Button>("Button-Start-Local");
        _buttonStartLocal.clicked += StartLocalGame;
        _buttonStartMatchMaking = root.Q<Button>("Button-Start-Online");
        _buttonStartMatchMaking.clicked += StartMatchMaking;
        root.Q<Button>("ArrowLeft").clicked += DecreasePlayerCount;
        root.Q<Button>("ArrowRight").clicked += IncreasePlayerCount;

        _buttonStartLocal.text = Constants.GetSentenceForLocalPlayButton(Settings.Language, _mainController.PlayerCount);
        _buttonStartMatchMaking.text = Constants.GetSentenceForOnlinePlayButton(Settings.Language);
    }

    private void Update()
    {
        if (_whileMatching)
        {
            bool isMatchComplete = _networkManager.GetNetworkRunner().SessionInfo.PlayerCount ==
            _networkManager.GetNetworkRunner().SessionInfo.MaxPlayers;
            if (isMatchComplete)
            {
                StartOnlineGame();
                _whileMatching = false;
            }
        }
    }

    private void StartLocalGame()
    {
        _mainController.GameMode = GameMode.Local;
        _networkManager.CraeateLocalGameAsync().Forget();
        _stateContoller.ChangeState(State.Field);
    }

    private void StartOnlineGame()
    {
        _mainController.GameMode = GameMode.Online;
        _stateContoller.ChangeState(State.Field);
    }

    private void StartMatchMaking()
    {
        if (_networkManager.GetNetworkRunner() != null && _networkManager.GetNetworkRunner().IsRunning)
        {
            return;
        }
        _networkManager.JoinOrCreateOldestRoomAsync().Forget();
        _whileMatching = true;
        _buttonStartMatchMaking.text = Constants.GetSentenceForMatchingButton(Settings.Language);
        _buttonStartMatchMaking.clicked -= StartMatchMaking;
        _buttonStartMatchMaking.clicked += StopMatching;
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

    private void StopMatching()
    {
        _whileMatching = false;
        _buttonStartMatchMaking.text = Constants.GetSentenceForOnlinePlayButton(Settings.Language);

        _buttonStartMatchMaking.clicked -= StopMatching;
        _buttonStartMatchMaking.clicked += StartMatchMaking;

        if (_networkManager.GetNetworkRunner() != null && _networkManager.GetNetworkRunner().IsRunning)
        {
            _networkManager.GetNetworkRunner().Disconnect(_networkManager.GetNetworkRunner().LocalPlayer);
        }
    }
}