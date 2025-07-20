using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using R3;
using BossSlayingTourney.Core;
using BossSlayingTourney.Skills;

namespace BossSlayingTourney.Game.Battle
{
    public class BattleUIController
    {
        #region UI Elements
        private VisualElement _root;
        private VisualElement _battleElement;
        private VisualElement _rewardElement;
        private VisualElement _actionElement;
        private VisualElement _commanndView;
        private VisualElement _skillView;
        private VisualElement _skillScrollContainer;

        private VisualElement _currentActionerArrowLeft;
        private VisualElement _currentActionerArrowRight;
        private Label _entityNameLeft;
        private Label _entityNameRight;

        private VisualElement _entityImageLeft;
        private VisualElement _entityImageRight;

        private VisualElement _leftConditionImage;
        private VisualElement _rightConditionImage;

        private Button _closeSkillScrollViewButton;
        private List<(Button, int)> _skillButtons = new();

        private Label _healthLabelLeft;
        private Label _manaLabelLeft;
        private Label _healthLabelRight;
        private Label _manaLabelRight;
        #endregion

        #region Events
        public readonly Subject<Unit> OnAttackClicked = new();
        public readonly Subject<Unit> OnSkillScrollOpenClicked = new();
        public readonly Subject<Unit> OnSkillScrollCloseClicked = new();
        public readonly Subject<string> OnSkillUsed = new();
        #endregion

        #region Properties
        public VisualElement EntityImageLeft => _entityImageLeft;
        public VisualElement EntityImageRight => _entityImageRight;
        public Label HealthLabelLeft => _healthLabelLeft;
        public Label HealthLabelRight => _healthLabelRight;
        public Label ManaLabelLeft => _manaLabelLeft;
        public Label ManaLabelRight => _manaLabelRight;
        #endregion

        public void Initialize(VisualElement root)
        {
            _root = root;
            InitializeUIElements();
            SetupEventHandlers();
        }

        private void InitializeUIElements()
        {
            _battleElement = _root.Q<VisualElement>("BattleElement");
            _rewardElement = _root.Q<VisualElement>("RewardElement");

            _actionElement = _root.Q<VisualElement>("ActionElement");
            _commanndView = _root.Q<VisualElement>("CommandView");

            _skillView = _root.Q<VisualElement>("SkillView");
            _skillScrollContainer = _root.Q<VisualElement>("unity-content-container");

            _closeSkillScrollViewButton = _root.Q<Button>("Button-CloseSkillScroll");

            var rootLeftElement = _root.Q<VisualElement>("Element-Left");
            _entityImageLeft = rootLeftElement.Q<VisualElement>("Image-Entity");
            _entityNameLeft = rootLeftElement.Q<Label>("Label-EntityName");
            _currentActionerArrowLeft = rootLeftElement.Q<VisualElement>("Image-Arrow");

            _healthLabelLeft = rootLeftElement.Q<VisualElement>("Element-HitPoint").Q<Label>("Label");
            _manaLabelLeft = rootLeftElement.Q<VisualElement>("Element-ManaPoint").Q<Label>("Label");
            _leftConditionImage = rootLeftElement.Q<VisualElement>("Element-Condition").Q<VisualElement>("Icon");

            var rootRightElement = _root.Q<VisualElement>("Element-Right");
            _entityImageRight = rootRightElement.Q<VisualElement>("Image-Entity");
            _entityNameRight = rootRightElement.Q<Label>("Label-EntityName");
            _currentActionerArrowRight = rootRightElement.Q<VisualElement>("Image-Arrow");

            _healthLabelRight = rootRightElement.Q<VisualElement>("Element-HitPoint").Q<Label>("Label");
            _manaLabelRight = rootRightElement.Q<VisualElement>("Element-ManaPoint").Q<Label>("Label");
            _rightConditionImage = rootRightElement.Q<VisualElement>("Element-Condition").Q<VisualElement>("Icon");
        }

        private void SetupEventHandlers()
        {
            _root.Q<Button>("Button-Attack").clicked += () => OnAttackClicked.OnNext(Unit.Default);
            _root.Q<Button>("Button-Skill").clicked += () => OnSkillScrollOpenClicked.OnNext(Unit.Default);
            _closeSkillScrollViewButton.clicked += () => OnSkillScrollCloseClicked.OnNext(Unit.Default);
        }

        public void SetEntityInfo(Entity leftEntity, Entity rightEntity)
        {
            // 左側のエンティティのUIの設定
            _entityNameLeft.text = leftEntity.name;
            var texture = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<Sprite>(leftEntity.FieldSpriteAssetReference).WaitForCompletion().texture;
            _entityImageLeft.style.backgroundImage = texture;

            _healthLabelLeft.text = leftEntity.Hp.ToString();
            _manaLabelLeft.text = leftEntity.Mp.ToString();
            _leftConditionImage.style.display = DisplayStyle.None;

            // 右側のエンティティのUIの設定
            _entityNameRight.text = rightEntity.name;
            var rightTexture = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<Sprite>(rightEntity.BattleSpriteAssetReference).WaitForCompletion().texture;
            _entityImageRight.style.backgroundImage = rightTexture;

            _healthLabelRight.text = rightEntity.Hp.ToString();
            _manaLabelRight.text = rightEntity.Mp.ToString();
            _rightConditionImage.style.display = DisplayStyle.None;
        }

        public void SetSkillButtons(Entity entity)
        {
            _skillButtons.Clear();
            _skillScrollContainer.Clear();

            foreach (var skill in entity.SyncedSkills)
            {
                var skillButton = new Button(() =>
                {
                    OnSkillUsed.OnNext(skill.Name);
                    CloseSkillScroll();
                })
                {
                    text = skill.Name
                };
                skillButton.AddToClassList("skillbutton");
                _skillScrollContainer.Add(skillButton);
                _skillButtons.Add((skillButton, skill.ManaCost));
            }
        }

        public void ToggleSkillButtonClickable(Entity entity)
        {
            foreach (var (button, manaCost) in _skillButtons)
            {
                button.SetEnabled(entity.Mp >= manaCost);
            }
        }

        public void SwitchArrowVisibility(Entity currentTurnEntity, Entity leftEntity)
        {
            if (currentTurnEntity == leftEntity)
            {
                _currentActionerArrowLeft.style.visibility = Visibility.Visible;
                _currentActionerArrowRight.style.visibility = Visibility.Hidden;
            }
            else
            {
                _currentActionerArrowLeft.style.visibility = Visibility.Hidden;
                _currentActionerArrowRight.style.visibility = Visibility.Visible;
            }
        }

        public void SetConditionImage(Entity leftEntity, Entity rightEntity)
        {
            var leftCondition = leftEntity.AbnormalConditionType;
            var rightCondition = rightEntity.AbnormalConditionType;

            _leftConditionImage.style.display = leftCondition == Condition.None ? DisplayStyle.None : DisplayStyle.Flex;
            _rightConditionImage.style.display = rightCondition == Condition.None ? DisplayStyle.None : DisplayStyle.Flex;

            SetConditionImageForEntity(_leftConditionImage, leftCondition);
            SetConditionImageForEntity(_rightConditionImage, rightCondition);
        }

        private void SetConditionImageForEntity(VisualElement conditionImage, Condition condition)
        {
            switch (condition)
            {
                case Condition.Poison:
                    conditionImage.style.backgroundImage = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<Sprite>(Constants.AssetReferencePoisonCondition).WaitForCompletion().texture;
                    break;
                case Condition.Regen:
                    conditionImage.style.backgroundImage = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<Sprite>(Constants.AssetReferenceRegenCondition).WaitForCompletion().texture;
                    break;
                case Condition.Stun:
                    conditionImage.style.backgroundImage = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<Sprite>(Constants.AssetReferenceStunCondition).WaitForCompletion().texture;
                    break;
                case Condition.Fire:
                    conditionImage.style.backgroundImage = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<Sprite>(Constants.AssetReferenceFireCondition).WaitForCompletion().texture;
                    break;
            }
        }

        public void DisplayBattleElement() => _battleElement.style.display = DisplayStyle.Flex;
        public void HideBattleElement() => _battleElement.style.display = DisplayStyle.None;

        public void OpenCommandView() => _commanndView.style.visibility = Visibility.Visible;
        public void CloseCommandView() => _commanndView.style.visibility = Visibility.Hidden;

        public void OpenSkillScroll()
        {
            _skillView.style.display = DisplayStyle.Flex;
            _closeSkillScrollViewButton.style.display = DisplayStyle.Flex;
            _commanndView.style.display = DisplayStyle.None;
        }

        public void CloseSkillScroll()
        {
            _skillView.style.display = DisplayStyle.None;
            _commanndView.style.display = DisplayStyle.Flex;
        }

        public void ResetActionElementPosition()
        {
            var parent = _actionElement.parent;
            var index = parent.IndexOf(_actionElement);

            // 一度アクションエレメントを削除
            parent.RemoveAt(index);
            // アクションエレメントを元の位置に戻す
            parent.Insert(0, _actionElement);
        }

        public void Dispose()
        {
            OnAttackClicked?.Dispose();
            OnSkillScrollOpenClicked?.Dispose();
            OnSkillScrollCloseClicked?.Dispose();
            OnSkillUsed?.Dispose();
        }
    }
}
