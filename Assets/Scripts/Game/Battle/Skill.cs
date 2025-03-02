public class Skill
{
    public string Name { get; private set; }

    public delegate string[] SkillAction(Entity skillUser, Entity target);

    public SkillAction Action { get; private set; }

    public Skill(string name, SkillAction action)
    {
        Name = name;
        Action = action;
    }

    public string[] Execute(Entity skillUser, Entity target)
    {
        return Action?.Invoke(skillUser, target);
    }
}

