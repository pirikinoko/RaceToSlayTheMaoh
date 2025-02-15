using UnityEngine;

public class Enemy : MonoBehaviour
{
    public EntityIdentifier Identifier;
    private Parameter _parameter;

    public void SetParameter(Parameter parameter)
    {
        _parameter = parameter;
    }

    private void ReceiveDamage(int damage)
    {
        _parameter.Hp -= damage;
    }
}
