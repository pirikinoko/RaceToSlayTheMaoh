using UnityEngine;
using UnityEngine.UIElements;
using Cysharp.Threading.Tasks;
using VContainer;
using Fusion;

public class MatchRoomController : MonoBehaviour
{
    private MainController _mainController;
    private StateController _stateContoller;
    private NetworkManager _networkManager;
    private TitleTextData _titleTextData;

    private Button _buttonStartGame;
    private Button _buttonLeaveRoom;

    private Label _roomNameLabel;
    private Label[] _playerNameLabels = new Label[Constants.MaxPlayerCount];
    private TextField _chatInputField;

    private bool _whileMatching = false;

    [Inject]
    public void Construct(MainController mainController, StateController stateController, TitleTextData titleTextData)
    {
        _mainController = mainController;
        _stateContoller = stateController;
        _titleTextData = titleTextData;
        _networkManager = NetworkManager.Instance;
    }

    private void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        _buttonStartLocal = root.Q<Button>("Button-Start-Local");
        _buttonStartLocal.clicked += StartLocalGame;
        _buttonStartMatchMaking = root.Q<Button>("Button-Start-Matchmaking");
        _buttonStartMatchMaking.clicked += StartMatchmaking;
        _roomNameLabel = root.Q<Toggle>("Toggle-Room-Name-Input");
        _roomNameInputField = root.Q<TextField>("Input-Room-Name");
        _roomNameLabel.RegisterValueChangedCallback(OnRoomNameToggleChanged);
        root.Q<Button>("ArrowLeft").clicked += () => ChangePlayerCount(_mainController.PlayerCount - 1);
        root.Q<Button>("ArrowRight").clicked += () => ChangePlayerCount(_mainController.PlayerCount + 1);

        _titleTextData.LocalPlayButtonText = Constants.GetSentenceForLocalPlayButton(Settings.Language, _mainController.PlayerCount);
        _titleTextData.MatchmakingButtonText = Constants.GetSentenceForOnlinePlayButton(Settings.Language);
        _buttonStartLocal.text = _titleTextData.LocalPlayButtonText;
        _buttonStartMatchMaking.text = _titleTextData.MatchmakingButtonText;
    }

    private void Update()
    {
        // ランダムマッチの処理
        if (_whileMatching && _roomNameLabel.value == false)
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

    private void StartMatchmaking()
    {
        if (_networkManager.GetNetworkRunner() != null && _networkManager.GetNetworkRunner().IsRunning)
        {
            return;
        }
        MatchmakingProcessAsync().Forget();
    }

    private async UniTask MatchmakingProcessAsync()
    {
        _whileMatching = true;
        _titleTextData.MatchmakingButtonText = Constants.GetSentenceForMatchingButton(Settings.Language);
        _buttonStartMatchMaking.clicked += StopMatchmaking;
        _roomNameLabel.style.display = DisplayStyle.None;

        StartGameResult result = null;
        if (_roomNameLabel.value == true)
        {
            result = await _networkManager.JoinOrCreateRoomByNameAsync(_roomNameInputField.value);
            Debug.Log($"Matchmaking result: {result}");
        }
        else
        {
            result = await _networkManager.JoinOrCreateOldestRoomAsync();
            Debug.Log($"Matchmaking result: {result}");
        }

        if (!result.Ok)
        {
            Debug.LogError($"Failed to start matchmaking: {result.ErrorMessage}");

            _whileMatching = false;
            SetUiPreMatchmakingState();
        }
    }

    private void StopMatchmaking()
    {
        _whileMatching = false;
        SetUiPreMatchmakingState();

        if (_networkManager.GetNetworkRunner() != null && _networkManager.GetNetworkRunner().IsRunning)
        {
            _networkManager.GetNetworkRunner().Disconnect(_networkManager.GetNetworkRunner().LocalPlayer);
        }
    }

    private void SetUiPreMatchmakingState()
    {
        _titleTextData.MatchmakingButtonText = Constants.GetSentenceForOnlinePlayButton(Settings.Language);
        _buttonStartMatchMaking.clicked -= StopMatchmaking;
        _buttonStartMatchMaking.clicked += StartMatchmaking;
        _roomNameLabel.style.display = DisplayStyle.Flex;
    }

    private void ChangePlayerCount(int playerCount)
    {
        if (playerCount < 1) playerCount = 1;
        if (playerCount > Constants.MaxPlayerCount) playerCount = Constants.MaxPlayerCount;

        _titleTextData.LocalPlayButtonText = Constants.GetSentenceForLocalPlayButton(Settings.Language, playerCount);
        _mainController.PlayerCount = playerCount;
    }

    private void OnRoomNameToggleChanged(ChangeEvent<bool> evt)
    {
        if (evt.newValue == true)
        {
            _roomNameInputField.style.display = DisplayStyle.Flex;
        }
        else
        {
            _roomNameInputField.style.display = DisplayStyle.None;
        }
    }
}