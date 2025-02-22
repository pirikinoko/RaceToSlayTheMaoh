using UnityEngine;

public class Entity : MonoBehaviour
{
    public EntityType EntityType;

    private int _id;
    private Parameter _parameter;
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _id = EntityMaster.AssignId();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(Parameter parameter)
    {
        _parameter = parameter;
        _spriteRenderer.sprite = Sprite.Create(parameter.Icon, new Rect(0, 0, parameter.Icon.width, parameter.Icon.height), Vector2.zero);
    }

    private void ReceiveDamage(int damage)
    {
        _parameter.Hp -= damage;
    }
}
