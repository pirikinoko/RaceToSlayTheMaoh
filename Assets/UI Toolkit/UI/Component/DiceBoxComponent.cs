using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIToolkit
{
    public class DiceBoxComponent : VisualElement
    {
        public bool IsRolling;
        public Button StopButton;
        private VisualElement _numberBox;
        private Label _numberLabel;
        private int _currentNumber;

        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _cancellationToken;

        public new class UxmlFactory : UxmlFactory<DiceBoxComponent, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits { }

        public class ClassNames
        {
            public const string DiceBox = "dice-box-component";
            public const string NumberBoxContainer = "number-box-container";
            public const string NumberLabel = "number-label";
            public const string StopButton = "stop-button";
        }

        public DiceBoxComponent()
        {
            // UI要素の初期化
            _numberBox = new VisualElement();
            _numberLabel = new Label("0");
            StopButton = new Button(() => Rpc_StopRolling(UnityEngine.Random.Range(1, Constants.MaxDiceValue + 1))) { text = "Stop" };

            Add(_numberBox);
            _numberBox.Add(_numberLabel);

            Add(StopButton);
            AddToClassList(ClassNames.DiceBox);

            _numberLabel.AddToClassList(ClassNames.NumberLabel);
            StopButton.AddToClassList(ClassNames.StopButton);
            _numberBox.AddToClassList(ClassNames.NumberBoxContainer);

            StartRolling();
        }

        public async void StartRolling()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;

            // サイコロの数字をランダムに切り替え続ける処理
            IsRolling = true;
            while (!_cancellationToken.IsCancellationRequested)
            {
                _currentNumber++;
                if (_currentNumber > Constants.MaxDiceValue)
                {
                    _currentNumber = 1;
                }
                _numberLabel.text = _currentNumber.ToString();
                await UniTask.Delay(TimeSpan.FromSeconds(Constants.DiceRollUpdateInterval), cancellationToken: _cancellationToken);
            }
        }

        private async UniTask BlinkNumberLabel()
        {
            for (int i = 0; i < Constants.DiceHighlightBlinkCount; i++)
            {
                _numberLabel.style.opacity = (i % 2 == 0) ? 0.2f : 1f;
                await UniTask.Delay(TimeSpan.FromSeconds(Constants.DiceHighlightBlinkInterval));
            }
            _numberLabel.style.opacity = 1f;
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void Rpc_StopRolling(int result)
        {
            StopRolling(result);
        }

        public async void StopRolling(int result)
        {
            // StartRolling()をキャンセル
            _cancellationTokenSource.Cancel();

            StopButton.style.display = DisplayStyle.None;
            _currentNumber = result;
            // サイコロの数字を止める処理
            // 数字の切り替えを徐々に遅くし、最終的にランダムな数字を生成
            float delay = Constants.DiceRollUpdateInterval;
            for (int i = 0; i < 8; i++)
            {
                _currentNumber++;
                if (_currentNumber > Constants.MaxDiceValue)
                {
                    _currentNumber = 1;
                }
                _numberLabel.text = _currentNumber.ToString();
                await UniTask.Delay(TimeSpan.FromSeconds(delay));
                delay += 0.08f;
            }

            _numberLabel.text = _currentNumber.ToString();

            // 結果ハイライト（点滅）
            await BlinkNumberLabel();

            IsRolling = false;
        }

        public int GetCurrentNumber()
        {
            // 現在のサイコロの数字を取得
            return _currentNumber;
        }

    }
}
