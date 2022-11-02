using System;
using System.Collections.Generic;
using UnityEngine;

[Flags]public enum CaseTypes
{
    None = 0,
    Flamable = 1 << 1,
    Explosive = 2 << 2,
    Everything = ~0,
}

[CreateAssetMenu]
public class CaseProperties : ScriptableObject
{
    public string CaseName;
    [Space]
    public bool isWalkable;
    public bool isBlockingVision;
    [Header("Textures & Material")]
    public List<Sprite> CaseTexture;
    public Material SpecialMaterial;
    [Space]
    public CaseTypes myCaseType;
    [Space]
    public List<string> ConstantEffect;
    [Space, Header("Transformations")]
    public CaseProperties Burned;
}
