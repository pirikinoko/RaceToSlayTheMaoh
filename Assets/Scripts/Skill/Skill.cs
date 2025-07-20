
using BossSlayingTourney.Core;

namespace BossSlayingTourney.Skills
{
    public class Skill
    {
        public string Name { get; private set; }
        public string Description { get; private set; } = string.Empty;
        public int ManaCost { get; private set; }
        public string EffectKey { get; private set; } = string.Empty;

        // スキルの結果のログは複数仕込むことができる(string[])
        // タプルの2つ目は、エフェクトのキー(string)
        public delegate SkillResult SkillAction(Entity skillUser, Entity target);

        public SkillAction Action { get; private set; }

        public Skill(string name, string description, int manaCost, string effectKey, SkillAction action)
        {
            Name = name;
            Description = description;
            ManaCost = manaCost;
            Action = action;
        }

        public SkillResult Execute(Entity skillUser, Entity target)
        {
            return Action?.Invoke(skillUser, target);
        }

        public class SkillResult
        {
            public string[] Logs { get; set; }
            public string EffectKey { get; set; }

            public SkillResult(string[] logs, string effectKey)
            {
                Logs = logs;
                EffectKey = effectKey;
            }
        }
    }
}