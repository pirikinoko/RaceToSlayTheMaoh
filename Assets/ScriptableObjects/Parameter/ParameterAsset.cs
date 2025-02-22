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
    public EntityType Id;
    public string Name;
    public int HitPoint;
    public int Power;
    public Texture2D Icon;
}
