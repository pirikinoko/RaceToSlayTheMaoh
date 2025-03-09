using System;  public static class SkillList {     public enum SkillType
    {         None,         Heal,         Bite     }      public static Skill GetSkill(SkillType skillType)
    {         return skillType switch
        {
            SkillType.None => throw new InvalidOperationException(),
            SkillType.Heal => Heal(),
            SkillType.Bite => Bite(),
            _ => throw new InvalidOperationException()         };     }      public static Skill Heal()     {         return new Skill("ヒール", (skillUser, target) =>         {
            target.TakeDamage(-10);             return new string[] { $"{skillUser.name}は{target.name}のHPを回復した" };         });     }      public static Skill Bite()     {         return new Skill("噛みつく", (skillUser, target) =>         {
            target.TakeDamage(10);             return new string[] { $"{skillUser.name}は{target.name}に噛みついた" };         });     } }