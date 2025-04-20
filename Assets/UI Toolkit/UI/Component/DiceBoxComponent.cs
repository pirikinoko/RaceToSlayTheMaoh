using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIToolkit
{
    public class DiceBoxComponent : VisualElement
    {
        public bool IsRolling;
        public Button StopButton;
        private Label _numberLabel;
        private int _currentNumber;


        public DiceBoxComponent()
        {
            // UI要素の初期化
            _numberLabel = new Label("0");
            StopButton = new Button(() => StopRolling()) { text = "Stop" };

            Add(_numberLabel);
            Add(StopButton);

            StartRolling();
        }

        public async void StartRolling()
        {
            // サイコロの数字をランダムに切り替え続ける処理
            IsRolling = true;

            while (IsRolling)
            {
                _currentNumber = UnityEngine.Random.Range(1, 7);
                _numberLabel.text = _currentNumber.ToString();
                await UniTask.Delay(100);
            }
        }

        private async void StopRolling()
        {
            // サイコロの数字を止める処理
            // 数字の切り替えを徐々に遅くし、最終的にランダムな数字を生成
            float delay = 0.1f;
            for (int i = 0; i < 10; i++)
            {
                _currentNumber = UnityEngine.Random.Range(1, 7);
                _numberLabel.text = _currentNumber.ToString();
                await UniTask.Delay(TimeSpan.FromSeconds(delay));
                delay += 0.05f;
            }

            _currentNumber = UnityEngine.Random.Range(1, 7);
            _numberLabel.text = _currentNumber.ToString();

            IsRolling = false;
            Debug.Log($"Final number: {_currentNumber}");
        }

        public int GetCurrentNumber()
        {
            // 現在のサイコロの数字を取得
            return _currentNumber;
        }
    }
}
