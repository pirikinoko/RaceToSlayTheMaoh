using Cysharp.Threading.Tasks;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

public static class EnemyActer
{
    public static async UniTask Act(BattleController battleController, Entity acter)
    {
        // 攻撃かスキルを使用するかをランダムで決定
        int action = Random.Range(0, 2);

        switch (action)
        {
            case 0:
                Attack(battleController);
                break;
            case 1:
                string skillName = await DecideASkillToUseAsync(acter);

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

    public static async UniTask<string> DecideASkillToUseAsync(Entity acter)
    {
        var HealSkills = acter.Parameter.Skills.FindAll(s => SkillList.GetSkillEffectType(s.Name) == SkillList.SkillEffectType.Heal);
        var DamageSkills = acter.Parameter.Skills.FindAll(s => SkillList.GetSkillEffectType(s.Name) == SkillList.SkillEffectType.Damage);
        var parameterAsset = await Addressables.LoadAssetAsync<ParameterAsset>(Constants.AssetReferenceParameter).Task;
        var defaultParameter = parameterAsset.ParameterList.FirstOrDefault(p => p.EntityType == acter.EntityType);

        // 回復スキルがない場合はダメージスキルを返す
        if (HealSkills.Count == 0 && DamageSkills.Count > 0)
        {
            return DamageSkills[Random.Range(0, DamageSkills.Count)].Name;
        }
        // HPが半分以下の場合は回復スキルを返す
        else if (HealSkills.Count > 0 && acter.Parameter.HitPoint < defaultParameter.HitPoint / 2)
        {
            return HealSkills[Random.Range(0, HealSkills.Count)].Name;
        }
        // HPが半分以上の場合はダメージスキルを返す
        else if (DamageSkills.Count > 0 && acter.Parameter.HitPoint >= defaultParameter.HitPoint / 2)
        {
            return DamageSkills[Random.Range(0, DamageSkills.Count)].Name;
        }
        // HPが半分以上でダメージスキルがない場合はスキルを使用しない
        else
        {
            return "None";
        }
    }

    public static bool HasEnoughManaPoint(Entity acter, string skillName)
    {
        return acter.Parameter.ManaPoint >= acter.Parameter.Skills.FirstOrDefault(s => s.Name == skillName).ManaCost;
    }
}
