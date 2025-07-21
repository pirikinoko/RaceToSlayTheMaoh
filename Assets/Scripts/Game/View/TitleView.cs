using BossSlayingTourney.Core;

using R3;
using UnityEngine;
using UnityEngine.UIElements;

namespace BossSlayingTourney.Game.View
{
    public class TitleView
    {
        #region Events
        public readonly Subject<Unit> OnLocalGameRequested = new();
        public readonly Subject<Unit> OnMatchmakingRequested = new();
        public readonly Subject<Unit> OnStopMatchmakingRequested = new();
        public readonly Subject<int> OnPlayerCountChangeRequested = new();
        public readonly Subject<(bool useRoomName, string roomName)> OnMatchmakingStartRequested = new();
        #endregion

        #region UI Elements
        private Button _buttonStartLocal;
        private Button _buttonStartMatchMaking;
        private Toggle _roomNameInputToggle;
        private TextField _roomNameInputField;
        private Label _playerCountLabel;
        #endregion

        #region Properties
        private TitleTextData _titleTextData;
        private UIDocument _uiDocument;
        #endregion

        public void Initialize(UIDocument uiDocument, TitleTextData titleTextData)
        {
            _uiDocument = uiDocument;
            _titleTextData = titleTextData;
            SetupUI();
        }

        private void SetupUI()
        {
            var root = _uiDocument.rootVisualElement;

            _buttonStartLocal = root.Q<Button>("Button-StartLocal");
            _buttonStartMatchMaking = root.Q<Button>("Button-StartMatchmaking");
            _roomNameInputToggle = root.Q<Toggle>("Toggle-RoomNameInput");
            _roomNameInputField = root.Q<TextField>("InputField-RoomName");

            // イベント登録
            _buttonStartLocal.clicked += () => OnLocalGameRequested.OnNext(Unit.Default);
            _buttonStartMatchMaking.clicked += OnMatchmakingButtonClicked;
            _roomNameInputToggle.RegisterValueChangedCallback(OnRoomNameToggleChanged);

            root.Q<Button>("Button-ArrowLeft").clicked += () => OnPlayerCountChangeRequested.OnNext(-1);
            root.Q<Button>("Button-ArrowRight").clicked += () => OnPlayerCountChangeRequested.OnNext(1);

            UpdateUI();
        }

        private void OnMatchmakingButtonClicked()
        {
            bool useRoomName = _roomNameInputToggle.value;
            string roomName = useRoomName ? _roomNameInputField.value : "";
            OnMatchmakingStartRequested.OnNext((useRoomName, roomName));
        }

        public void UpdateLocalPlayButtonText(string text)
        {
            if (_buttonStartLocal != null)
                _buttonStartLocal.text = text;
        }

        public void UpdateMatchmakingButtonText(string text)
        {
            if (_buttonStartMatchMaking != null)
                _buttonStartMatchMaking.text = text;
        }

        public void SetMatchingState()
        {
            _buttonStartMatchMaking.clicked -= OnMatchmakingButtonClicked;
            _buttonStartMatchMaking.clicked += () => OnStopMatchmakingRequested.OnNext(Unit.Default);
            _roomNameInputToggle.style.display = DisplayStyle.None;

            UpdateMatchmakingButtonText(Constants.GetSentenceForMatchingButton(Settings.Language));
        }

        public void SetPreMatchmakingState()
        {
            _buttonStartMatchMaking.clicked -= () => OnStopMatchmakingRequested.OnNext(Unit.Default);
            _buttonStartMatchMaking.clicked += OnMatchmakingButtonClicked;
            _roomNameInputToggle.style.display = DisplayStyle.Flex;

            UpdateMatchmakingButtonText(Constants.GetSentenceForOnlinePlayButton(Settings.Language));
        }

        public void UpdatePlayerCountDisplay(int playerCount)
        {
            string text = Constants.GetSentenceForLocalPlayButton(Settings.Language, playerCount);
            UpdateLocalPlayButtonText(text);
        }

        private void OnRoomNameToggleChanged(ChangeEvent<bool> evt)
        {
            if (evt.newValue)
            {
                _roomNameInputField.style.display = DisplayStyle.Flex;
            }
            else
            {
                _roomNameInputField.style.display = DisplayStyle.None;
            }
        }

        private void UpdateUI()
        {
            if (_titleTextData != null)
            {
                UpdateLocalPlayButtonText(_titleTextData.LocalPlayButtonText);
                UpdateMatchmakingButtonText(_titleTextData.MatchmakingButtonText);
            }
        }

        public void Dispose()
        {
            OnLocalGameRequested?.Dispose();
            OnMatchmakingRequested?.Dispose();
            OnStopMatchmakingRequested?.Dispose();
            OnPlayerCountChangeRequested?.Dispose();
            OnMatchmakingStartRequested?.Dispose();
        }
    }
}
