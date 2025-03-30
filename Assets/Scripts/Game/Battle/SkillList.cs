using System;

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
        Bite
    }

    public enum SkillEffectType
    {
        Heal,
        Damage
    }
    #endregion

    #region Skill Parameters
    private static class SkillParameters
    {
        // エンティティのパラメータ依存の値には999を設定
        public static class Heal
        {
            public const int ManaCost = 3;
            public const int EffectAmount = 10;
        }

        public static class Bite
        {
            public const int ManaCost = 2;
            public const int EffectAmount = 999;
        }
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
            GetSkillNameHealByLanguage() => SkillEffectType.Heal,
            GetSkillNameBiteByLanguage() => SkillEffectType.Damage,
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
            _ => throw new InvalidOperationException()
        };
    }
    #endregion

    #region Language Methods
    public static string GetSkillNameHealByLanguage()
    {
        return Settings.Language switch
        {
            Language.Japanese => "ヒール",
            Language.English => "Heal"
        };
    }

    public static string GetSkillDescriptionHealByLanguage()
    {
        return Settings.Language switch
        {
            Language.Japanese => "HPを回復する",
            Language.English => "Heal your hp"
        };
    }

    public static string GetSkillNameBiteByLanguage()
    {
        return Settings.Language switch
        {
            Language.Japanese => "噛みつく",
            Language.English => "Bite"
        };
    }

    public static string GetSkillDescriptionBiteByLanguage()
    {
        return Settings.Language switch
        {
            Language.Japanese => "相手に噛みつく",
            Language.English => "Bite the opponent"
        };
    }
    #endregion

    #region Skill Creation Methods
    private static Skill CreateHealSkill()
    {
        return new Skill(
            name: GetSkillNameHealByLanguage(),
            description: GetSkillDescriptionHealByLanguage(),
            manaCost: SkillParameters.Heal.ManaCost,
            action: (skillUser, opponent) =>
            {
                skillUser.Parameter.ManaPoint -= SkillParameters.Heal.ManaCost;
                skillUser.TakeDamage(-SkillParameters.Heal.EffectAmount);
                return new string[] { $"{skillUser.name}は{skillUser.name}のHPを{SkillParameters.Heal.EffectAmount}回復した" };
            }
        );
    }

    private static Skill CreateBiteSkill()
    {
        return new Skill(
            name: GetSkillNameBiteByLanguage(),
            description: GetSkillDescriptionBiteByLanguage(),
            manaCost: SkillParameters.Bite.ManaCost,
            action: (skillUser, opponent) =>
            {
                skillUser.Parameter.ManaPoint -= SkillParameters.Bite.ManaCost;
                opponent.TakeDamage(SkillParameters.Bite.EffectAmount);
                return new string[] { $"{skillUser.name}は{opponent.name}に噛みついた" };
            }
        );
    }
    #endregion
}