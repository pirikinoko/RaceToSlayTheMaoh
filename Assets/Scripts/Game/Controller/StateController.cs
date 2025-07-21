﻿using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements; // Label を使うために追加
using VContainer;
using BossSlayingTourney.Core;

namespace BossSlayingTourney.Game.Controllers
{
    public class StateController : MonoBehaviour
    {
        public State CurrentState { get; private set; }

        private MainController _mainController;
        private FieldController _fieldController;
        private CameraController _cameraController;

        [SerializeField] private UIDocument _overAllUi;
        [SerializeField] private UIDocument _titleUi;
        [SerializeField] private UIDocument _fieldUi;
        [SerializeField] private UIDocument _battleUi;
        [SerializeField] private UIDocument _resultUi;

        private VisualElement _overAllroot;
        private VisualElement _titleRoot;
        private VisualElement _fieldRoot;
        private VisualElement _battleRoot;
        private VisualElement _resultRoot;

        private Color _blackoutColor = new Color(0f, 0f, 0f, 0.8f);

        private VisualElement _colorEffectPanel;

        [Inject]
        public void Construct(MainController mainController, FieldController fieldController, CameraController cameraController)
        {
            _mainController = mainController;
            _fieldController = fieldController;
            _cameraController = cameraController;
        }

        private void Start()
        {
            _titleRoot = _titleUi.rootVisualElement;
            _fieldRoot = _fieldUi.rootVisualElement;
            _battleRoot = _battleUi.rootVisualElement;
            _resultRoot = _resultUi.rootVisualElement;

            _overAllroot.style.display = DisplayStyle.Flex;
            _titleRoot.style.display = DisplayStyle.None;
            _fieldRoot.style.display = DisplayStyle.None;
            _battleRoot.style.display = DisplayStyle.None;
            _resultRoot.style.display = DisplayStyle.None;

            _colorEffectPanel = _overAllroot.Q<VisualElement>("ColorEffectPanel");

            ChangeState(State.Title);
        }

        public void ChangeState(State state)
        {
            CurrentState = state;

            _titleRoot.style.display = DisplayStyle.None;
            _fieldRoot.style.display = DisplayStyle.None;
            _battleRoot.style.display = DisplayStyle.None;
            _resultRoot.style.display = DisplayStyle.None;

            switch (state)
            {
                case State.Title:
                    SwitchTitleState();
                    break;
                case State.Field:
                    SwitchFieldState().Forget();
                    break;
                case State.Battle:
                    SwitchBattleStateAsync().Forget();
                    break;
                case State.Result:
                    SwitchResultState().Forget();
                    break;
            }
        }

        private void SwitchTitleState()
        {
            _titleRoot.style.display = DisplayStyle.Flex;
            BlackoutField();
        }

        private async UniTask SwitchFieldState()
        {
            _fieldRoot.style.display = DisplayStyle.Flex;
            RevealField();

            if (_mainController.TurnCount == 0)
            {
                await _mainController.InitializeGame();
                // ステータスボックスの表示が不完全なまま見えないように先に中身を更新しておく
                await _fieldController.UpdateStatusBoxesAsync();
                _fieldController.DisplayStatusBoxes();
            }
            else
            {
                await _fieldController.UpdateStatusBoxesAsync();
            }

            await UniTask.Delay(TimeSpan.FromSeconds(Constants.DelayBeforeNewTurnSeconds));
            _mainController.NewTurnProcess();
        }

        private async UniTask SwitchBattleStateAsync()
        {
            BlackoutField();
            await UniTask.Delay(TimeSpan.FromSeconds(1f));
            _battleRoot.style.display = DisplayStyle.Flex;

            await SlideInBattleUI();
            _battleRoot.style.translate = new StyleTranslate(new Translate(0, 0, 0));
        }

        private async UniTask SlideInBattleUI()
        {
            // スライドイン演出
            // 画面右端から中央へ移動（X座標を調整）
            var width = _battleRoot.resolvedStyle.width;
            if (width == 0) width = 1920;
            _battleRoot.style.translate = new StyleTranslate(new Translate(width, 0, 0));
            float duration = 0.4f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float x = Mathf.Lerp(width, 0, t);
                _battleRoot.style.translate = new StyleTranslate(new Translate(x, 0, 0));
                await UniTask.DelayFrame(1);
                elapsed += Time.deltaTime;
            }
        }

        private async UniTask SwitchResultState()
        {
            Entity winner = _mainController.WinnerEntity;

            RevealField();

            // 勝者にズームイン
            await _cameraController.ZoomInAsync(winner.transform.position);

            // リザルト画面のUIを準備
            var resultMessageLabel = _resultRoot.Q<Label>("ResultMessageLabel");
            resultMessageLabel.text = Constants.GetResultMessageWin(Settings.Language, winner.BaseParameter.Name);
            resultMessageLabel.style.display = DisplayStyle.None;

            var turnCountMessageLabel = _resultRoot.Q<Label>("TurnCountMessageLabel");
            turnCountMessageLabel.text = Constants.GetTurnCountMessage(Settings.Language, _mainController.TurnCount);
            turnCountMessageLabel.style.display = DisplayStyle.None;

            var backToTitleButton = _resultRoot.Q<Button>("BackToTitleButton");
            backToTitleButton.style.display = DisplayStyle.None;

            _resultRoot.style.display = DisplayStyle.Flex;

            // 暗転アニメーション
            var resultElements = _resultRoot.Q<VisualElement>("ResultElements");

            Color initialColor = resultElements.style.backgroundColor.value;
            resultElements.style.backgroundColor = new StyleColor(new Color(initialColor.r, initialColor.g, initialColor.b, 0));

            await DOTween.To(
                () => resultElements.style.backgroundColor.value,
                x => resultElements.style.backgroundColor = x,
                new Color(initialColor.r, initialColor.g, initialColor.b, Constants.ResultFadeAlpha),
                Constants.ResultFadeDuration
            ).AsyncWaitForCompletion();

            // 暗転後にメッセージやボタンを表示
            resultMessageLabel.style.display = DisplayStyle.Flex;
            turnCountMessageLabel.style.display = DisplayStyle.Flex;
            backToTitleButton.style.display = DisplayStyle.Flex;

            backToTitleButton.text = Constants.GetBackToTitleButtonText(Settings.Language);
            backToTitleButton.style.display = DisplayStyle.Flex;
            backToTitleButton.RegisterCallback<ClickEvent>(e =>
            {
                // タイトル画面に戻る処理
                _mainController.ResetGame();
            });
        }

        public void BlackoutField()
        {
            _colorEffectPanel.style.backgroundColor = new StyleColor(_blackoutColor);
        }

        public void RevealField()
        {
            _colorEffectPanel.style.backgroundColor = new Color(0, 0, 0, 0);
        }
    }
}
