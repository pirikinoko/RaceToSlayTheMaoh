using System;
using System.Collections.Generic;

/// <summary>
/// スキルの定義と管理を行うクラス
/// </summary>
public static class SkillList
{
    #region Enums
    public enum SkillType
    {
        None,
        Heal,
        Bite,
        Ignition,
        Drain,
        Destroy,
        Regen,
        SuperHeal,
        Training,
    }

    public enum SkillEffectType
    {
        Buff,
        Damage
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// スキル名から効果タイプを取得
    /// </summary>
    public static SkillEffectType GetSkillEffectType(string skillName)
    {
        return skillName switch
        {
            var name when name == GetSkillNameByLanguage(SkillType.Heal) => SkillEffectType.Buff,
            var name when name == GetSkillNameByLanguage(SkillType.Bite) => SkillEffectType.Damage,
            var name when name == GetSkillNameByLanguage(SkillType.Ignition) => SkillEffectType.Damage,
            var name when name == GetSkillNameByLanguage(SkillType.Drain) => SkillEffectType.Damage,
            var name when name == GetSkillNameByLanguage(SkillType.Destroy) => SkillEffectType.Damage,
            var name when name == GetSkillNameByLanguage(SkillType.Regen) => SkillEffectType.Buff,
            var name when name == GetSkillNameByLanguage(SkillType.SuperHeal) => SkillEffectType.Buff,
            var name when name == GetSkillNameByLanguage(SkillType.Training) => SkillEffectType.Buff,
            _ => throw new InvalidOperationException("Unknown skill name")
        };
    }

    /// <summary>
    /// スキルタイプからスキルを取得
    /// </summary>
    public static Skill GetSkill(SkillType skillType)
    {
        return skillType switch
        {
            SkillType.None => throw new InvalidOperationException(),
            SkillType.Heal => CreateHealSkill(),
            SkillType.Bite => CreateBiteSkill(),
            SkillType.Ignition => CreateIgnitionSkill(),
            SkillType.Drain => CreateDrainSkill(),
            SkillType.Destroy => CreateDestroySkill(),
            SkillType.Regen => CreateRegenSkill(),
            SkillType.SuperHeal => CreateSuperHealSkill(),
            SkillType.Training => CreateTrainingSkill(),
            _ => throw new InvalidOperationException("Unknown skill type")
        };
    }
    #endregion

    #region Language Methods
    /// <summary>
    /// スキルタイプに応じた名前を言語設定に基づいて取得
    /// </summary>
    public static string GetSkillNameByLanguage(SkillType skillType)
    {
        return skillType switch
        {
            SkillType.Heal => GetSkillNameHealByLanguage(),
            SkillType.Bite => GetSkillNameBiteByLanguage(),
            SkillType.Ignition => GetSkillNameIgnitionByLanguage(),
            SkillType.Drain => GetSkillNameDrainByLanguage(),
            SkillType.Destroy => GetSkillNameDestroyByLanguage(),
            SkillType.Regen => GetSkillNameRegenByLanguage(),
            SkillType.SuperHeal => GetSkillNameSuperHealByLanguage(),
            SkillType.Training => GetSkillNameTrainingByLanguage(),
            _ => throw new InvalidOperationException("Unknown skill type")
        };
    }

    /// <summary>
    /// スキルタイプに応じた説明を言語設定に基づいて取得
    /// </summary>
    public static string GetSkillDescriptionByLanguage(SkillType skillType)
    {
        return skillType switch
        {
            SkillType.Heal => GetSkillDescriptionHealByLanguage(),
            SkillType.Bite => GetSkillDescriptionBiteByLanguage(),
            SkillType.Ignition => GetSkillDescriptionIgnitionByLanguage(),
            SkillType.Drain => GetSkillDescriptionDrainByLanguage(),
            SkillType.Destroy => GetSkillDescriptionDestroyByLanguage(),
            SkillType.Regen => GetSkillDescriptionRegenByLanguage(),
            SkillType.SuperHeal => GetSkillDescriptionSuperHealByLanguage(),
            SkillType.Training => GetSkillDescriptionTrainingByLanguage(),
            _ => throw new InvalidOperationException("Unknown skill type")
        };
    }
    public static string GetSkillNameHealByLanguage()
    {
        return Settings.Language switch
        {
            Language.Japanese => "ヒール",
            Language.English => "Heal",
            _ => "Heal" // デフォルト値
        };
    }

    public static string GetSkillDescriptionHealByLanguage()
    {
        return Settings.Language switch
        {
            Language.Japanese => "HPを回復する",
            Language.English => "Heal your hp",
            _ => "Heal your hp" // デフォルト値
        };
    }

    public static string GetSkillNameBiteByLanguage()
    {
        return Settings.Language switch
        {
            Language.Japanese => "噛みつく",
            Language.English => "Bite",
            _ => "Bite" // デフォルト値
        };
    }

    public static string GetSkillDescriptionBiteByLanguage()
    {
        return Settings.Language switch
        {
            Language.Japanese => "相手に噛みつく",
            Language.English => "Bite the opponent",
            _ => "Bite the opponent" // デフォルト値
        };
    }

    public static string GetSkillNameIgnitionByLanguage()
    {
        return Settings.Language switch
        {
            Language.Japanese => "イグニッション",
            Language.English => "Ignition",
            _ => "Ignition" // デフォルト値
        };
    }

    public static string GetSkillDescriptionIgnitionByLanguage()
    {
        return Settings.Language switch
        {
            Language.Japanese => "炎で相手を攻撃する",
            Language.English => "Attack the opponent with fire",
            _ => "Attack the opponent with fire" // デフォルト値
        };
    }

    public static string GetSkillNameDrainByLanguage()
    {
        return Settings.Language switch
        {
            Language.Japanese => "ドレイン",
            Language.English => "Drain",
            _ => "Drain" // デフォルト値
        };
    }

    public static string GetSkillDescriptionDrainByLanguage()
    {
        return Settings.Language switch
        {
            Language.Japanese => "相手のHPを吸収する",
            Language.English => "Absorb opponent's HP",
            _ => "Absorb opponent's HP" // デフォルト値
        };
    }

    public static string GetSkillNameDestroyByLanguage()
    {
        return Settings.Language switch
        {
            Language.Japanese => "デストロイ",
            Language.English => "Destroy",
            _ => "Destroy" // デフォルト値
        };
    }

    public static string GetSkillDescriptionDestroyByLanguage()
    {
        return Settings.Language switch
        {
            Language.Japanese => "強力な一撃で相手を攻撃する",
            Language.English => "Attack with a powerful strike",
            _ => "Attack with a powerful strike" // デフォルト値
        };
    }

    public static string GetSkillNameRegenByLanguage()
    {
        return Settings.Language switch
        {
            Language.Japanese => "リジェネ",
            Language.English => "Regen",
            _ => "Regen" // デフォルト値
        };
    }

    public static string GetSkillDescriptionRegenByLanguage()
    {
        return Settings.Language switch
        {
            Language.Japanese => "ターン毎にHPを回復する",
            Language.English => "Recover HP every turn",
            _ => "Recover HP every turn" // デフォルト値
        };
    }

    public static string GetSkillNameSuperHealByLanguage()
    {
        return Settings.Language switch
        {
            Language.Japanese => "スーパーヒール",
            Language.English => "Super Heal",
            _ => "Super Heal" // デフォルト値
        };
    }

    public static string GetSkillDescriptionSuperHealByLanguage()
    {
        return Settings.Language switch
        {
            Language.Japanese => "HPを大幅に回復する",
            Language.English => "Greatly restore HP",
            _ => "Greatly restore HP" // デフォルト値
        };
    }

    public static string GetSkillNameTrainingByLanguage()
    {
        return Settings.Language switch
        {
            Language.Japanese => "トレーニング",
            Language.English => "Training",
            _ => "Training" // デフォルト値
        };
    }

    public static string GetSkillDescriptionTrainingByLanguage()
    {
        return Settings.Language switch
        {
            Language.Japanese => "攻撃力を上昇させる",
            Language.English => "Increase attack power",
            _ => "Increase attack power" // デフォルト値
        };
    }
    #endregion

    #region Skill Creation Methods
    private static Skill CreateHealSkill()
    {
        return new Skill(
            name: GetSkillNameHealByLanguage(),
            description: GetSkillDescriptionHealByLanguage(),
            manaCost: 1,
            effectKey: Constants.ImageAnimationKeyHeal,
            action: (skillUser, opponent) =>
            {
                skillUser.SetManaPoint(skillUser.Parameter.ManaPoint - 1);
                int healAmount = Constants.GetRandomizedValueWithinOffsetWithMissPotential(
                    baseValue: 5,
                    offsetPercent: 50,
                    missPotential: 0
                );
                skillUser.SetHitPoint(skillUser.Parameter.HitPoint + healAmount);
                return new Skill.SkillResult(
                    logs: new string[]
                    {
                        $"{skillUser.name}はHPを{healAmount}回復した"
                    },
                    effectKey: Constants.ImageAnimationKeyHeal
                );
            }
        );
    }

    private static Skill CreateBiteSkill()
    {
        return new Skill(
            name: GetSkillNameBiteByLanguage(),
            description: GetSkillDescriptionBiteByLanguage(),
            manaCost: 1,
            effectKey: Constants.ImageAnimationKeyBite,
            action: (skillUser, opponent) =>
            {
                skillUser.SetManaPoint(skillUser.Parameter.ManaPoint - 1);
                int damageAmount = Constants.GetRandomizedValueWithinOffsetWithMissPotential(
                    baseValue: skillUser.Parameter.Power,
                    offsetPercent: 50,
                    missPotential: Constants.MissPotentialOnEveryDamageAction
                );
                opponent.SetHitPoint(opponent.Parameter.HitPoint - damageAmount);
                int healAmount = Constants.GetRandomizedValueWithinOffsetWithMissPotential(
                    baseValue: 2,
                    offsetPercent: 50,
                    missPotential: 0
                );
                skillUser.SetHitPoint(skillUser.Parameter.HitPoint + healAmount);

                var logs = new List<string>();

                if (damageAmount > 0)
                {
                    logs.Add($"{damageAmount}のダメージを与え,HPを回復した");
                }
                else
                {
                    logs.Add($"{opponent.name}は攻撃をかわした");
                }

                return new Skill.SkillResult(
                    logs: logs.ToArray(),
                    effectKey: Constants.ImageAnimationKeyBite
                );
            }
        );
    }

    private static Skill CreateIgnitionSkill()
    {
        return new Skill(
            name: GetSkillNameIgnitionByLanguage(),
            description: GetSkillDescriptionIgnitionByLanguage(),
            manaCost: 2,
            effectKey: Constants.ImageAnimationKeyIgnition,
            action: (skillUser, opponent) =>
            {
                skillUser.SetManaPoint(skillUser.Parameter.ManaPoint - 2);
                int damageAmount = Constants.GetRandomizedValueWithinOffsetWithMissPotential(
                    baseValue: skillUser.AttackPower,
                    offsetPercent: 30,
                    missPotential: Constants.MissPotentialOnEveryDamageAction
                );
                opponent.SetHitPoint(opponent.Parameter.HitPoint - damageAmount);

                var logs = new List<string>();

                if (damageAmount > 0)
                {
                    logs.Add($"炎の力で{damageAmount}のダメージを与えた");
                }
                else
                {
                    logs.Add($"{opponent.name}は攻撃をかわした");
                }

                return new Skill.SkillResult(
                    logs: logs.ToArray(),
                    effectKey: Constants.ImageAnimationKeyIgnition
                );
            }
        );
    }

    private static Skill CreateDrainSkill()
    {
        return new Skill(
            name: GetSkillNameDrainByLanguage(),
            description: GetSkillDescriptionDrainByLanguage(),
            manaCost: 3,
            effectKey: Constants.ImageAnimationKeyDrain,
            action: (skillUser, opponent) =>
            {
                skillUser.SetManaPoint(skillUser.Parameter.ManaPoint - 3);
                int damageAmount = Constants.GetRandomizedValueWithinOffsetWithMissPotential(
                    baseValue: 5,
                    offsetPercent: 40,
                    missPotential: Constants.MissPotentialOnEveryDamageAction
                );
                opponent.SetHitPoint(opponent.Parameter.HitPoint - damageAmount);

                int healAmount = 0;
                if (damageAmount > 0)
                {
                    healAmount = Constants.GetRandomizedValueWithinOffsetWithMissPotential(
                        baseValue: 3,
                        offsetPercent: 40,
                        missPotential: 0
                    );
                    skillUser.SetHitPoint(skillUser.Parameter.HitPoint + healAmount);
                }

                var logs = new List<string>();

                if (damageAmount > 0)
                {
                    logs.Add($"{opponent.name}のHPを{damageAmount}奪い、自身のHPを{healAmount}回復した");
                }
                else
                {
                    logs.Add($"{opponent.name}は攻撃をかわした");
                }

                return new Skill.SkillResult(
                    logs: logs.ToArray(),
                    effectKey: Constants.ImageAnimationKeyDrain
                );
            }
        );
    }

    private static Skill CreateDestroySkill()
    {
        return new Skill(
            name: GetSkillNameDestroyByLanguage(),
            description: GetSkillDescriptionDestroyByLanguage(),
            manaCost: 4,
            effectKey: Constants.ImageAnimationKeyDestroy,
            action: (skillUser, opponent) =>
            {
                skillUser.SetManaPoint(skillUser.Parameter.ManaPoint - 4);
                int damageAmount = Constants.GetRandomizedValueWithinOffsetWithMissPotential(
                    baseValue: 15,
                    offsetPercent: 20,
                    missPotential: Constants.MissPotentialOnEveryDamageAction + 15 // より高いミス率
                );
                opponent.SetHitPoint(opponent.Parameter.HitPoint - damageAmount);

                var logs = new List<string>();

                if (damageAmount > 0)
                {
                    logs.Add($"強力な一撃で{damageAmount}のダメージを与えた");
                }
                else
                {
                    logs.Add($"{opponent.name}は攻撃をかわした");
                }

                return new Skill.SkillResult(
                    logs: logs.ToArray(),
                    effectKey: Constants.ImageAnimationKeyDestroy
                );
            }
        );
    }

    private static Skill CreateRegenSkill()
    {
        return new Skill(
            name: GetSkillNameRegenByLanguage(),
            description: GetSkillDescriptionRegenByLanguage(),
            manaCost: 2,
            effectKey: Constants.ImageAnimationKeyRegen,
            action: (skillUser, opponent) =>
            {
                skillUser.SetManaPoint(skillUser.Parameter.ManaPoint - 2);
                int healPerTurn = Constants.GetRandomizedValueWithinOffsetWithMissPotential(
                    baseValue: 3,
                    offsetPercent: 20,
                    missPotential: 0
                );
                skillUser.SetHitPoint(skillUser.Parameter.HitPoint + healPerTurn);
                return new Skill.SkillResult(
                    logs: new string[]
                    {
                        $"{skillUser.name}は回復の力を得た。HPが{healPerTurn}回復した"
                    },
                    effectKey: Constants.ImageAnimationKeyRegen
                );
            }
        );
    }

    private static Skill CreateSuperHealSkill()
    {
        return new Skill(
            name: GetSkillNameSuperHealByLanguage(),
            description: GetSkillDescriptionSuperHealByLanguage(),
            manaCost: 5,
            effectKey: Constants.ImageAnimationKeySuperHeal,
            action: (skillUser, opponent) =>
            {
                skillUser.SetManaPoint(skillUser.Parameter.ManaPoint - 5);
                int healAmount = Constants.GetRandomizedValueWithinOffsetWithMissPotential(
                    baseValue: 15,
                    offsetPercent: 30,
                    missPotential: 0
                );
                skillUser.SetHitPoint(skillUser.Parameter.HitPoint + healAmount);
                return new Skill.SkillResult(
                    logs: new string[]
                    {
                        $"強力な回復魔法で{skillUser.name}のHPが{healAmount}回復した"
                    },
                    effectKey: Constants.ImageAnimationKeySuperHeal
                );
            }
        );
    }

    private static Skill CreateTrainingSkill()
    {
        return new Skill(
            name: GetSkillNameTrainingByLanguage(),
            description: GetSkillDescriptionTrainingByLanguage(),
            manaCost: 3,
            effectKey: Constants.ImageAnimationKeyTraining,
            action: (skillUser, opponent) =>
            {
                skillUser.SetManaPoint(skillUser.Parameter.ManaPoint - 3);
                int finalPowerBoost = Constants.GetRandomizedValueWithinOffsetWithMissPotential(
                    baseValue: 2,
                    offsetPercent: 30,
                    missPotential: 0
                );
                skillUser.SetAbnormalCondition(new AbnormalCondition
                {
                    PowerGain = finalPowerBoost
                });
                return new Skill.SkillResult(
                    logs: new string[]
                    {
                        $"{skillUser.name}のトレーニングにより攻撃力が{finalPowerBoost}上昇した"
                    },
                    effectKey: Constants.ImageAnimationKeyTraining
                );
            }
        );
    }
    #endregion
}