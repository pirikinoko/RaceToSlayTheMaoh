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
        Parameter = parameter;

        Id = EntityMaster.AssignId();
        gameObject.name = parameter.Name;

        _abnormalCondition = new AbnormalCondition();

        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.sprite = Parameter.IconSprite;
    }

    public int Attack(Entity target)
    {
        // 攻撃力のポテンシャルを計算
        int potential = Parameter.Power + _abnormalCondition.PowerGain;
        // 攻撃力のポテンシャルのオフセット内でランダムな値を返す
        int damage = Constants.GetRandomizedValueWithinOffset(potential, Constants.AttackOffsetPercent);
        target.TakeDamage(damage);
        return damage;
    }

    public string[] UseSkill(string skillName, Entity skillUser, Entity opponent)
    {
        Skill skill = Parameter.Skills.Find(s => s.Name == skillName);
        return skill.Execute(skillUser, opponent);
    }


    public void TakeDamage(int damage)
    {
        Parameter.HitPoint -= damage;
    }
}
