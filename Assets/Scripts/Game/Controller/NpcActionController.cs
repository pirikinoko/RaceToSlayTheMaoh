using Cysharp.Threading.Tasks;
using System.Linq;
using UIToolkit;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;
using BossSlayingTourney.Core;
using BossSlayingTourney.Game.Battle;
using BossSlayingTourney.Game.Field;
using BossSlayingTourney.Skills;

namespace BossSlayingTourney.Game.Controllers
{
    public static class NpcActionController
    {
        public static async UniTask ActAsync(BattleController battleController, Entity acter, Entity target)
        {
            // 攻撃かスキルを使用するかをランダムで決定
            int action = Random.Range(0, 2);

            switch (action)
            {
                case 0:
                    Attack(battleController);
                    break;
                case 1:
                    string skillName = DecideASkillToUse(acter, target);

                    //　適切なスキルが存在しない場合は通常攻撃をする
                    if (skillName == "None")
                    {
                        Attack(battleController);
                        return;
                    }

                    if (HasEnoughManaPoint(acter, skillName))
                    {
                        UseSkill(battleController, skillName);
                    }
                    // マナポイントが足りなければスキルは使用せず通常攻撃をする
                    else
                    {
                        Attack(battleController);
                    }
                    break;
            }
        }

        public static void Attack(BattleController battleController)
        {
            battleController.Attack();
        }

        public static void UseSkill(BattleController battleController, string skillName)
        {
            battleController.UseSkill(skillName);
        }

        public static string DecideASkillToUse(Entity acter, Entity target)
        {
            var HealSkills = acter.SyncedSkills.FindAll(s => SkillList.GetSkillEffectType(s.Name) == SkillList.SkillEffectType.Heal);
            var DamageSkills = acter.SyncedSkills.FindAll(s => SkillList.GetSkillEffectType(s.Name) == SkillList.SkillEffectType.Damage);
            var BuffSkills = acter.SyncedSkills.FindAll(s => SkillList.GetSkillEffectType(s.Name) == SkillList.SkillEffectType.Buff);

            var hasChanceOfDeath = acter.Hp < target.AttackPower * 1.5f;

            // バフスキルがあれば1/3の確率でバフスキルを返す
            if (acter.AbnormalConditionType != Condition.Regen && BuffSkills.Count > 0 && Random.Range(0, 3) == 0)
            {
                return BuffSkills[Random.Range(0, BuffSkills.Count)].Name;
            }
            // 回復スキルがありHPが相手の攻撃力以下の場合は回復スキルを返す
            else if (HealSkills.Count > 0 && hasChanceOfDeath)
            {
                return HealSkills[Random.Range(0, HealSkills.Count)].Name;
            }
            // ダメージスキルがあればダメージスキルを返す
            else if (DamageSkills.Count > 0)
            {
                return DamageSkills[Random.Range(0, DamageSkills.Count)].Name;
            }
            else
            {
                return "None";
            }
        }

        public static bool HasEnoughManaPoint(Entity acter, string skillName)
        {
            return acter.Mp >= acter.SyncedSkills.FirstOrDefault(s => s.Name == skillName).ManaCost;
        }

        public static void StopRolling(DiceBoxComponent diceBoxComponent, int result)
        {
            diceBoxComponent.StopRolling(result);
        }

        /// <summary>
        /// レイキャストで当たった敵の中から最も近い敵を見つける
        /// つまりは障害物を挟む敵は対象外
        /// </summary>
        public static void Move(ControllableEntity acter, bool includePlayersAsTarget)
        {
            acter.MoveNpc(includePlayersAsTarget);
        }

        public static async UniTask SelectReward(BattleController battleController, BattleLogController battleLogController)
        {
            await UniTask.Delay(1000);

            int randomValue = Random.Range(0, battleController.RewardChoices.Count);
            int reward = battleController.RewardChoices[randomValue];

            if (randomValue == 0)
            {
                battleController.OnStatusRewardSelected();
            }
            else
            {
                battleController.OnSkillRewardSelected(reward);
            }
        }
    }
}