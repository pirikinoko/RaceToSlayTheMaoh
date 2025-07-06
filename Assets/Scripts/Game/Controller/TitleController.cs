using UnityEngine;
using UnityEngine.UIElements;
using Cysharp.Threading.Tasks;
using VContainer;
using Fusion;

public class TitleController : MonoBehaviour
{
    private MainController _mainController;
    private StateController _stateContoller;
    private NetworkManager _networkManager;
    private TitleTextData _titleTextData;

    private Button _buttonStartLocal;
    private Button _buttonStartMatchMaking;

    private Toggle _roomNameInputToggle;
    private TextField _roomNameInputField;

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

        _buttonStartLocal = root.Q<Button>("Button-StartLocal");
        _buttonStartLocal.clicked += StartLocalGame;
        _buttonStartMatchMaking = root.Q<Button>("Button-StartMatchmaking");
        _buttonStartMatchMaking.clicked += StartMatchmaking;
        _roomNameInputToggle = root.Q<Toggle>("Toggle-RoomNameInput");
        _roomNameInputField = root.Q<TextField>("InputField-RoomName");
        _roomNameInputToggle.RegisterValueChangedCallback(OnRoomNameToggleChanged);
        root.Q<Button>("Button-ArrowLeft").clicked += () => ChangePlayerCount(_mainController.PlayerCount - 1);
        root.Q<Button>("Button-ArrowRight").clicked += () => ChangePlayerCount(_mainController.PlayerCount + 1);

        _titleTextData.LocalPlayButtonText = Constants.GetSentenceForLocalPlayButton(Settings.Language, _mainController.PlayerCount);
        _titleTextData.MatchmakingButtonText = Constants.GetSentenceForOnlinePlayButton(Settings.Language);
        _buttonStartLocal.text = _titleTextData.LocalPlayButtonText;
        _buttonStartMatchMaking.text = _titleTextData.MatchmakingButtonText;
    }

    private void Update()
    {
        // ランダムマッチの処理
        if (_whileMatching && _roomNameInputToggle.value == false)
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
        _roomNameInputToggle.style.display = DisplayStyle.None;

        StartGameResult result = null;
        if (_roomNameInputToggle.value == true)
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
        _roomNameInputToggle.style.display = DisplayStyle.Flex;
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