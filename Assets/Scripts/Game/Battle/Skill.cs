public class Skill
{
    public string Name { get; private set; }
    public int ManaCost { get; private set; }
    public string Description { get; private set; } = string.Empty;

    public delegate string[] SkillAction(Entity skillUser, Entity target);

    public SkillAction Action { get; private set; }

    public Skill(string name, string description, int manaCost, SkillAction action)
    {
        Name = name;
        Description = description;
        ManaCost = manaCost;
        Action = action;
    }

    public string[] Execute(Entity skillUser, Entity target)
    {
        return Action?.Invoke(skillUser, target);
    }
}

