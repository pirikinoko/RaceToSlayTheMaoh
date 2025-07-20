using UnityEngine.UIElements;
using VContainer;
using Fusion;
using System.Collections.Generic;
using UIToolkit;
using BossSlayingTourney.Network;

namespace BossSlayingTourney.Game.Controllers
{

    public class MatchRoomController : NetworkBehaviour
    {
        private MainController _mainController;
        private NetworkManager _networkManager;

        private Button _buttonStartGame;
        private Button _buttonLeaveRoom;

        private Label _roomNameLabel;
        private ListView _playerListView;
        private List<string> _playerNames = new();

        private ChatWindow _chatWindow;

        private class ClassNames
        {
            public const string PlayerNameLabel = "player-name-label";
        }

        [Inject]
        public void Construct(MainController mainController)
        {
            _mainController = mainController;
            _networkManager = NetworkManager.Instance;
        }

        private void Start()
        {
            InitializeUI();
            SubscribeToNetworkEvents();
        }

        private void SubscribeToNetworkEvents()
        {
            if (_networkManager != null)
            {
                _networkManager.OnPlayerListChanged += UpdatePlayerList;
                // 初期プレイヤーリストの設定
                UpdatePlayerList(_networkManager.GetCurrentPlayerNames());
            }
        }

        private void InitializeUI()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            _buttonStartGame = root.Q<Button>("Button-Start");
            _buttonLeaveRoom = root.Q<Button>("Button-Leave");

            _roomNameLabel = root.Q<Label>("Label-RoomName");
            _playerListView = root.Q<ListView>("ListView-PlayerList");

            // ListViewの初期設定
            _playerListView.itemsSource = _playerNames;
            _playerListView.makeItem = () => new Label();
            _playerListView.bindItem = (element, index) =>
            {
                var label = element as Label;
                label.text = _playerNames[index];
                label.AddToClassList(ClassNames.PlayerNameLabel);
            };

            _buttonStartGame.clicked += () =>
            {
                if (HasStateAuthority)
                {
                    _networkManager.DisableJoin();
                }
                StartGameRPC();
            };
            _buttonLeaveRoom.clicked += LeaveRoom;

            _chatWindow = root.Q<ChatWindow>("ChatWindow");
            _chatWindow.Initialize(_networkManager, Runner);
        }

        private void OnDestroy()
        {
            // イベントの購読解除
            if (_networkManager != null)
            {
                _networkManager.OnPlayerListChanged -= UpdatePlayerList;
            }
        }

        private void UpdatePlayerList(List<string> playerNames)
        {
            // 新しいリストで置き換える（Clearを使わない）
            _playerNames = new List<string>(playerNames);
            _playerListView.itemsSource = _playerNames;
            _playerListView?.Rebuild();
        }

        public void OnEnterRoom(string roomName, List<string> playerNames)
        {
            _roomNameLabel.text = roomName;

            // プレイヤーリストの更新
            _playerNames = playerNames;
            _playerListView.itemsSource = _playerNames;
            _playerListView.Rebuild();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void StartGameRPC()
        {
            _mainController.StartGame();
        }

        private void LeaveRoom()
        {
            if (Runner != null)
            {
                Runner.Shutdown();
                Destroy(gameObject);
            }
        }
    }
}