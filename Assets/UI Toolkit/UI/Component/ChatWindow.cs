using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIToolkit
{
    [UxmlElement]
    public partial class ChatWindow : VisualElement
    {
        [UxmlAttribute]
        public Vector2 WindowSize { get; set; } = new Vector2(300, 600);
        [UxmlAttribute]
        public string PlaceholderText { get; set; } = "メッセージを入力...";

        private ScrollView _chatList;
        private TextField _inputField;
        private List<string> _messages = new List<string>();

        /// <summary>
        /// メッセージ送信時に発火するイベント，外部のクラスから登録可能
        /// /// </summary>
        public event Action<string> OnMessageSent;

        public ChatWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
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
                AddMessage(message);
                OnMessageSent?.Invoke(message);
                _inputField.value = string.Empty;
                _inputField.Focus();
            }
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

        private void UpdatePlaceholder()
        {
            if (_inputField != null && !string.IsNullOrEmpty(PlaceholderText))
            {
                _inputField.value = PlaceholderText;
            }
        }
    }
}
