using System;  public static class SkillList {     public enum SkillType
    {         None,         Heal,         Bite     }      public enum SkillEffectType
    {
        Heal,
        Damage
    }

    public static SkillEffectType GetSkillEffectType(string skillName)
    {
        switch (skillName)
        {
            case var name when name == GetSkillNameHealByLanguage():
                return SkillEffectType.Heal;
            case var name when name == GetSkillNameBiteByLanguage():
                return SkillEffectType.Damage;
            default:
                throw new InvalidOperationException("Unknown skill name");
        }
    }      public static Skill GetSkill(SkillType skillType)
    {         return skillType switch
        {
            SkillType.None => throw new InvalidOperationException(),
            SkillType.Heal => Heal(),
            SkillType.Bite => Bite(),
            _ => throw new InvalidOperationException()         };     }      public static string GetSkillNameHealByLanguage()
    {
        return Settings.Language switch
        {             Language.Japanese => "ヒール",             Language.English => "Heal"         };     }

    public static string GetSkillDescriptionHealByLanguage()
    {
        return Settings.Language switch
        {             Language.Japanese => "HPを回復する",             Language.English => "Heal your hp"         };     }

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
    }      public static Skill Heal()     {         int manaCost = 5;         int effectAmount = 5;          return new Skill(name: GetSkillNameHealByLanguage(), description: GetSkillDescriptionHealByLanguage(), manaCost: manaCost, action: (skillUser, opponent) =>         {
            skillUser.Parameter.ManaPoint -= manaCost;
            skillUser.TakeDamage(-effectAmount);             return new string[] { $"{skillUser.name}は{skillUser.name}のHPを{effectAmount}回復した" };         });     }      public static Skill Bite()     {
        int manaCost = 5;
        int effectAmount = 5;         return new Skill(name: GetSkillNameBiteByLanguage(), description: GetSkillDescriptionBiteByLanguage(), manaCost: 5, action: (skillUser, target) =>         {
            skillUser.Parameter.ManaPoint -= manaCost;
            target.TakeDamage(effectAmount);             return new string[] { $"{skillUser.name}は{target.name}に噛みついた" };         });     } }