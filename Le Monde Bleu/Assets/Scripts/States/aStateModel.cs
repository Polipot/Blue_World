using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


[CreateAssetMenu]
public class aStateModel : ScriptableObject
{
    [Header("Conditions")]
    [HideInInspector]public bool ShowConditions;
    public WeaponType WeaponFilter;
    public StateClass myClass;
    public string Dependance;

    [Header("Stat change")]
    public StateType myStateType;
    [Space]
    [HideInInspector] public bool ShowStatModifier;
    public string StatModified;
    public float ModifiedOf;
    [Space]
    public int InflictedDamages;
    [Space]
    public int ActiveTurns;
    [Space]
    public StateClass myAnti;

    [Header("Effect giver")]
    public string EffectGivedOnAttackPath;

    [Space]

    [Header("UI")]
    [HideInInspector] public bool ShowUISettings;
    public Sprite Logo;
    public string GlyphPath;
    [Space, TextArea(10, 30)]
    public string TipToShowCrypted;
    public string Positiveness;

    [Space, Header("Graphics")]
    [HideInInspector] public bool ShowGraphics;
    public GameObject Effect;
    public GraphicEffectOn GraphicEffectAppliedOn;
}
