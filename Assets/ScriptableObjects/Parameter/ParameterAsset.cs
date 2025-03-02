using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ParameterAsset", menuName = "ScriptableObjects/ParameterAsset")]
public class ParameterAsset : ScriptableObject
{
    public List<Parameter> ParameterList = new();
}

[System.Serializable]
public class Parameter
{
    public EntityType EntityType;
    public string Name;
    public int HitPoint;
    public int ManaPoint;
    public int Power;
    public List<Skill> Skills;
    public Sprite IconSprite;
}
