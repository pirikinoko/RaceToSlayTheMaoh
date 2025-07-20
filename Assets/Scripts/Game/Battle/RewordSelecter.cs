using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using R3;
using BossSlayingTourney.Core;
using BossSlayingTourney.Game.Controllers;
using BossSlayingTourney.Skills;

namespace BossSlayingTourney.Game.Battle
{
    public class RewardSelecter
    {
        #region Events
        public Observable<Unit> OnStatusRewardSelected => _onStatusRewardSelected;
        public Observable<int> OnSkillRewardSelected => _onSkillRewardSelected;

        private Subject<Unit> _onStatusRewardSelected = new();
        private Subject<int> _onSkillRewardSelected = new();
        #endregion

        #region Fields
        private VisualElement _rewardElement;
        private Entity _winnerEntity;
        private Entity _loserEntity;
        private UserController _userController;

        private Reward _currentReward;
        private List<Skill> _currentRewardSkills;
        #endregion

        #region Constructor
        public RewardSelecter(VisualElement rewardElement, UserController userController)
        {
            _rewardElement = rewardElement;
            _userController = userController;
        }
        #endregion

        #region Public Methods
        public void SetupRewards(Entity winnerEntity, Entity loserEntity)
        {
            _winnerEntity = winnerEntity;
            _loserEntity = loserEntity;

            // スキル報酬の準備
            GenerateRewardSkills();

            // UI作成
            CreateRewardUI();
        }

        public List<int> GetAvailableRewardChoices()
        {
            var rewardChoices = new List<int> { 0 }; // ステータス報酬は常に選択可能

            for (int i = 0; i < _currentRewardSkills.Count; i++)
            {
                if (_winnerEntity.SyncedSkills.Find(s => s.Name == _currentRewardSkills[i].Name) == null)
                {
                    rewardChoices.Add(i + 1); // スキルは1から開始
                }
            }

            return rewardChoices;
        }

        public void ShowRewardView()
        {
            _rewardElement.style.display = DisplayStyle.Flex;
        }

        public void HideRewardView()
        {
            _rewardElement.style.display = DisplayStyle.None;
        }

        public string ExecuteStatusReward()
        {
            return _currentReward.Execute(_winnerEntity);
        }

        public string ExecuteSkillReward(int skillIndex)
        {
            var skill = _currentRewardSkills[skillIndex];
            _winnerEntity.SyncedSkills.Add(skill);
            return string.Format(Constants.GetSkillGetSentence(Settings.Language), _winnerEntity.name, skill.Name);
        }

        public void ClearRewards()
        {
            _rewardElement.Clear();
            _currentReward = null;
            _currentRewardSkills?.Clear();
        }
        #endregion

        #region Private Methods
        private void GenerateRewardSkills()
        {
            List<Skill> loserSkills = new List<Skill>(_loserEntity.SyncedSkills);
            _currentRewardSkills = GetRandomTwoElementsFromList(loserSkills);
        }

        private void CreateRewardUI()
        {
            _rewardElement.Clear();
            var rewardButtons = new List<Button>();

            // ステータス報酬ボタン
            CreateStatusRewardButton(rewardButtons);

            // スキル報酬ボタン
            CreateSkillRewardButtons(rewardButtons);

            // ボタンをUIに追加
            AddButtonsToUI(rewardButtons);
        }

        private void CreateStatusRewardButton(List<Button> rewardButtons)
        {
            _currentReward = new Reward();
            var statusRewardButton = new Button(() => _onStatusRewardSelected.OnNext(Unit.Default));
            statusRewardButton.AddToClassList(BattleController.ClassNames.RewardButton);
            statusRewardButton.Add(CreateLabel(BattleController.ClassNames.RewardTitle, Constants.GetStatusRewardTitle(Settings.Language)));
            statusRewardButton.Add(CreateLabel(BattleController.ClassNames.RewardDescription, _currentReward.Description));
            rewardButtons.Add(statusRewardButton);
        }

        private void CreateSkillRewardButtons(List<Button> rewardButtons)
        {
            for (int i = 0; i < _currentRewardSkills.Count; i++)
            {
                int index = i;
                var skillRewardButton = new Button(() => _onSkillRewardSelected.OnNext(index));
                skillRewardButton.AddToClassList(BattleController.ClassNames.RewardButton);

                var titleLabel = CreateLabel(BattleController.ClassNames.RewardTitle, _currentRewardSkills[index].Name);
                skillRewardButton.Add(titleLabel);

                var descriptionLabel = CreateLabel(BattleController.ClassNames.RewardDescription, _currentRewardSkills[index].Description);
                skillRewardButton.Add(descriptionLabel);

                // 既に持っているスキルの場合は無効化
                if (_winnerEntity.SyncedSkills.Find(s => s.Name == _currentRewardSkills[index].Name) != null)
                {
                    skillRewardButton.SetEnabled(false);
                    descriptionLabel.text = Constants.GetSentenceWhenAlreadyHoldingTheSkill(Settings.Language);
                }

                rewardButtons.Add(skillRewardButton);
            }
        }

        private void AddButtonsToUI(List<Button> rewardButtons)
        {
            foreach (var button in rewardButtons)
            {
                _rewardElement.Add(button);

                // 無効なボタンはスキップ
                if (!button.enabledSelf)
                {
                    continue;
                }

                // 勝者が自分の場合のみボタンを有効化
                button.SetEnabled(_winnerEntity == _userController.MyEntity);
            }
        }

        private Label CreateLabel(string className, string text)
        {
            var label = new Label();
            label.AddToClassList(className);
            label.text = text;
            return label;
        }

        private List<T> GetRandomTwoElementsFromList<T>(List<T> list)
        {
            List<T> result = new List<T>();

            if (list.Count > 0)
            {
                int index1 = UnityEngine.Random.Range(0, list.Count);
                result.Add(list[index1]);
                list.RemoveAt(index1);
            }

            if (list.Count > 0)
            {
                int index2 = UnityEngine.Random.Range(0, list.Count);
                result.Add(list[index2]);
            }

            return result;
        }
        #endregion

        #region Cleanup
        public void Dispose()
        {
            _onStatusRewardSelected?.Dispose();
            _onSkillRewardSelected?.Dispose();
        }
        #endregion
    }
}