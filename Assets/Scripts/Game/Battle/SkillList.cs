public static class SkillList {     public static Skill Heal()     {         return new Skill("ヒール", (skillUser, target) =>         {
            // Healの実装
            target.TakeDamage(-10); // 例: 10ポイント回復             return new string[] { $"{skillUser.name}は{target.name}のHPを回復した" };         });     }      public static Skill Bite()     {         return new Skill("噛みつく", (skillUser, target) =>         {
            // Biteの実装
            target.TakeDamage(10); // 例: 10ポイントのダメージ             return new string[] { $"{skillUser.name}は{target.name}に噛みついた" };         });     }

    // 他のスキルも同様に定義
}