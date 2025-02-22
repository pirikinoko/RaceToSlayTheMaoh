using UnityEngine;

public class EntityMaster : MonoBehaviour
{
    public static int _idToAssign;

    private void Awake()
    {
        _idToAssign = 1;
    }

    public static int AssignId()
    {
        return _idToAssign++;
    }
}
