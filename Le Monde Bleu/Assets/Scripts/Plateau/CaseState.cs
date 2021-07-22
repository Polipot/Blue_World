using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaseState : ScriptableObject
{
    [Header("Position")]
    public Case myCase;
    [Header("UI")]
    public Sprite Logo;
    public string GlyphPath;
    [Space, TextArea(10, 30)]
    public string TipToShowCrypted;
    [Space]
    [Header("Conditions of spawn")]
    public CaseTypes CaseTypesAllowed;
    [Header("Global")]
    public int BaseTime;
    public int RemainingTime;
    public NatureBehavior myBehavior;
    [Header("character state gived on occupy")]
    public string GivedState;
    [Space]
    [Header("Graphics")]
    public GameObject Effect;
    [Header("Propagation Effect")]
    public int BasePPGTChance;
    public int MyPPGTChance;
    public int LessPPGT;
    public int Heritage;
}
