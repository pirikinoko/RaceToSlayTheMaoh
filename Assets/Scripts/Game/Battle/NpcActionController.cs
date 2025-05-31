using Cysharp.Threading.Tasks;
using System.Linq;
using UIToolkit;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

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
                string skillName = await DecideASkillToUseAsync(acter, target);

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

    public static async UniTask<string> DecideASkillToUseAsync(Entity acter, Entity target)
    {
        var HealSkills = acter.Parameter.Skills.FindAll(s => SkillList.GetSkillEffectType(s.Name) == SkillList.SkillEffectType.Heal);
        var DamageSkills = acter.Parameter.Skills.FindAll(s => SkillList.GetSkillEffectType(s.Name) == SkillList.SkillEffectType.Damage);
        var BuffSkills = acter.Parameter.Skills.FindAll(s => SkillList.GetSkillEffectType(s.Name) == SkillList.SkillEffectType.Buff);

        var parameterAsset = await Addressables.LoadAssetAsync<ParameterAsset>(Constants.AssetReferenceParameter).Task;
        var defaultParameter = parameterAsset.ParameterList.FirstOrDefault(p => p.EntityType == acter.EntityType);

        var hasChanceOfDeath = acter.Parameter.HitPoint < target.Parameter.Power * 1.5f;

        // バフスキルがあれば1/3の確率でバフスキルを返す
        if (BuffSkills.Count > 0 && Random.Range(0, 3) == 0)
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
        return acter.Parameter.ManaPoint >= acter.Parameter.Skills.FirstOrDefault(s => s.Name == skillName).ManaCost;
    }

    public static void StopRolling(DiceBoxComponent diceBoxComponent)
    {
        diceBoxComponent.StopRolling();
    }

    /// <summary>
    /// レイキャストで当たった敵の中から最も近い敵を見つける
    /// つまりは障害物を挟む敵は対象外
    /// </summary>
    /// <param name="acter"></param>
    /// <param name="playerController"></param>
    /// <param name="enemyController"></param>
    /// <returns></returns>
    public static async UniTask MoveAsync(ControllableEntity acter)
    {
        await acter.MoveTowardsNearestEntity();
    }

    public static async UniTask SelectReward(BattleController battleController, BattleLogController battleLogController)
    {
        await UniTask.Delay(1000);

        int randomValue = Random.Range(0, battleController.RewardChoices.Count);
        int reward = battleController.RewardChoices[randomValue];
        Debug.Log($"選択された報酬: {reward}");
        if (randomValue == 0)
        {
            battleController.OnStatusRewardSelected();
        }
        else
        {
            battleController.OnSkillRewardSelected(reward);
        }

        battleLogController.FlipLog();
    }
}
