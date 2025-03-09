using UnityEngine;

public class Entity : MonoBehaviour
{
    public int Id { get; private set; }

    public EntityType EntityType;
    public Parameter Parameter { get; private set; }

    private AbnormalCondition _abnormalCondition;
    private SpriteRenderer _spriteRenderer;


    public void Initialize(Parameter parameter)
    {
        Parameter = new Parameter(parameter);

        Id = EntityMaster.AssignId();
        gameObject.name = parameter.Name;

        _abnormalCondition = new AbnormalCondition();

        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.sprite = Parameter.IconSprite;
    }

    // こちらからのアクション
    public void Attack(Entity target)
    {
        target.TakeDamage(Parameter.Power + _abnormalCondition.PowerGain);
    }

    public string[] UseSkill(string skillName, Entity skillUser, Entity target)
    {
        Skill skill = Parameter.Skills.Find(s => s.Name == skillName);
        return skill.Execute(skillUser, target);
    }

    // 相手からのアクションを受け取る
    public void TakeDamage(int damage)
    {
        Parameter.HitPoint -= damage;
    }
}
