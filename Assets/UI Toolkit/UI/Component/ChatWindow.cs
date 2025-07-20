using System;
using System.Collections.Generic;
using BossSlayingTourney.Core;
using BossSlayingTourney.Network;
using Fusion;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIToolkit
{
    [UxmlElement]
    public partial class ChatWindow : VisualElement
    {
        #region Fields
        private Vector2 _windowSize = new Vector2(300, 600);
        private ScrollView _chatList;
        private TextField _inputField;
        private List<string> _messages = new List<string>();
        private NetworkManager _networkManager;
        private NetworkRunner _networkRunner;
        #endregion

        #region Properties
        [UxmlAttribute]
        public Vector2 WindowSize
        {
            get => _windowSize;
            set
            {
                _windowSize = value;
                UpdateWindowSize();
            }
        }
        #endregion

        #region Events
        /// <summary>
        /// メッセージ送信時に発火するイベント，外部のクラスから登録可能
        /// </summary>
        public event Action<string> OnMessageSent;
        #endregion

        #region Constructor & Lifecycle
        public ChatWindow()
        {
            OnInstantiated();
        }

        private void OnDestroy()
        {
            OnMessageSent -= OnChatMessageSent;
            _networkManager.OnChatHistoryUpdated -= UpdateChatHistory;
        }
        #endregion

        #region Public Methods
        public void Initialize(NetworkManager networkManager, NetworkRunner networkRunner)
        {
            _networkManager = networkManager;
            _networkRunner = networkRunner;

            // チャットメッセージ送信イベントの購読
            OnMessageSent += (message) => OnChatMessageSent(message);
            _networkManager.OnChatHistoryUpdated += UpdateChatHistory;
            UpdateChatHistory(_networkManager.GetChatHistory());
        }

        public void AddMessage(string message)
        {
            _messages.Add(message);

            // メッセージ要素の作成
            var messageElement = new Label(message);
            messageElement.style.paddingTop = 2;
            messageElement.style.paddingBottom = 2;
            messageElement.style.paddingLeft = 5;
            messageElement.style.paddingRight = 5;
            messageElement.style.color = Color.white;
            messageElement.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
            messageElement.style.marginBottom = 2;
            messageElement.style.borderTopLeftRadius = 5;
            messageElement.style.borderTopRightRadius = 5;
            messageElement.style.borderBottomLeftRadius = 5;
            messageElement.style.borderBottomRightRadius = 5;
            messageElement.style.whiteSpace = WhiteSpace.Normal;

            // 新しいメッセージを末尾に追加（下端に表示される）
            _chatList.Add(messageElement);

            // 最下部（新しいメッセージ）までスクロール
            schedule.Execute(() =>
            {
                _chatList.ScrollTo(messageElement);
            });
        }

        public void ClearMessages()
        {
            _messages.Clear();
            _chatList.Clear();
        }

        public List<string> GetMessages()
        {
            return new List<string>(_messages);
        }

        public void UpdateChatHistory(List<string> chatHistory)
        {
            ClearMessages();
            foreach (var message in chatHistory)
            {
                AddMessage(message);
            }
        }

        public void OnChatMessageSent(string message)
        {
            // 自分のメッセージを自分のチャットに表示（名前付き）
            var myName = _networkManager.GetPlayerName(_networkRunner.LocalPlayer);
            var formattedMessage = $"{myName}: {message}";
            AddMessage(formattedMessage);

            // チャット履歴にメッセージを追加（マスタークライアントが管理）
            _networkManager.AddChatMessage(formattedMessage);

            // メッセージをRPCで他のプレイヤーに送信
            if (_networkRunner != null && _networkRunner.IsConnectedToServer)
            {
                BroadcastChatMessageRPC(formattedMessage, _networkRunner.LocalPlayer);
            }
        }
        #endregion

        #region Private Methods - Initialization
        private void OnInstantiated()
        {
            // メインコンテナのスタイル設定
            style.flexDirection = FlexDirection.Column;
            // ウィンドウサイズ
            style.width = WindowSize.x;
            style.height = WindowSize.y;
            // ウィンドウ枠線
            style.borderTopWidth = 1;
            style.borderBottomWidth = 1;
            style.borderLeftWidth = 1;
            style.borderRightWidth = 1;
            style.borderTopColor = Color.gray;
            style.borderBottomColor = Color.gray;
            style.borderLeftColor = Color.gray;
            style.borderRightColor = Color.gray;
            // ウィンドウの背景色
            style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);

            // チャットリストの作成
            _chatList = new ScrollView();
            // ウィンドウサイズからインプットエリアの高さを引いた分をチャットリストに割り当てる(flexGrow == 1)
            _chatList.style.flexGrow = 1;
            // チャットリストの枠とコンテンツとの間の隙間
            _chatList.style.paddingTop = 5;
            _chatList.style.paddingBottom = 5;
            _chatList.style.paddingLeft = 5;
            _chatList.style.paddingRight = 5;

            // スクロールバーを非表示にする
            _chatList.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            _chatList.verticalScrollerVisibility = ScrollerVisibility.Hidden;
            // チャットが下から上に流れるようにする
            _chatList.contentViewport.style.flexDirection = FlexDirection.ColumnReverse;
            Add(_chatList);

            // インプットエリアの作成
            var inputContainer = new VisualElement();
            inputContainer.style.flexDirection = FlexDirection.Row;
            inputContainer.style.paddingTop = 5;
            inputContainer.style.paddingBottom = 5;
            inputContainer.style.paddingLeft = 5;
            inputContainer.style.paddingRight = 5;

            // テキストフィールドの作成
            _inputField = new TextField();
            _inputField.style.flexGrow = 1;
            _inputField.RegisterCallback<KeyDownEvent>(OnInputKeyDown);
            inputContainer.Add(_inputField);

            Add(inputContainer);

            // プレースホルダーテキストの設定
            UpdatePlaceholder();
        }

        private void UpdateWindowSize()
        {
            if (style != null)
            {
                style.width = WindowSize.x;
                style.height = WindowSize.y;
            }
        }

        private void UpdatePlaceholder()
        {
            if (_inputField != null && _inputField.value == string.Empty)
            {
                // プレースホルダーテキストを設定
                _inputField.value = Constants.GetAssetReferenceChatPlaceholder(Settings.Language);
                _inputField.style.color = Color.gray;
            }
        }
        #endregion

        #region Private Methods - Event Handling
        private void OnInputKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                SendMessage();
                evt.StopPropagation();
            }
        }

        private void SendMessage()
        {
            string message = _inputField.value?.Trim();
            if (!string.IsNullOrEmpty(message))
            {
                // 名前付きメッセージはOnMessageSentイベントで外部から設定してもらう
                OnMessageSent?.Invoke(message);
                _inputField.value = string.Empty;
                _inputField.Focus();
            }
        }
        #endregion

        #region Network Methods
        [Rpc(RpcSources.All, RpcTargets.All)]
        private void BroadcastChatMessageRPC(string formattedMessage, PlayerRef sender)
        {
            // 送信者以外のプレイヤーのチャットウィンドウにメッセージを追加
            if (sender != _networkRunner.LocalPlayer)
            {
                AddMessage(formattedMessage);
            }
        }
        #endregion
    }
}
