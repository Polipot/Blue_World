using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(aStateModel))]
public class StateEditor : Editor
{
    /*public override void OnInspectorGUI()
    {
        aStateModel myState = (aStateModel)target;

        GUIStyle AreaWrapStyle = new GUIStyle(EditorStyles.textArea);
        AreaWrapStyle.wordWrap = true;

        GUIStyle LabelBold = new GUIStyle(EditorStyles.boldLabel);


        EditorGUILayout.SelectableLabel("Conditions", LabelBold);

        myState.ShowConditions = EditorGUILayout.Toggle("Montrer les conditions", myState.ShowConditions);
        if (myState.ShowConditions)
        {
            EditorGUILayout.Space();
            myState.WeaponFilter = (WeaponType)EditorGUILayout.EnumFlagsField("Filtre d'armes", myState.WeaponFilter);
            EditorGUILayout.Space();
        }

        EditorGUILayout.LabelField("------------------");
        EditorGUILayout.SelectableLabel("Paramètres d'UI", LabelBold);

        myState.ShowUISettings = EditorGUILayout.Toggle("Montrer les paramètres", myState.ShowUISettings);
        if (myState.ShowUISettings)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Logo");
            myState.Logo = (Sprite)EditorGUILayout.ObjectField(myState.Logo, typeof(Sprite), false, GUILayout.Width(64), GUILayout.Height(64));
            myState.GlyphPath = EditorGUILayout.TextField("Code du glyph", myState.GlyphPath);
            
            EditorGUILayout.TextArea(myState.TipToShowCrypted, AreaWrapStyle);
            EditorGUILayout.Space();
        }

        EditorGUILayout.LabelField("------------------");
        EditorGUILayout.SelectableLabel("Effets graphiques", LabelBold);

        myState.ShowGraphics = EditorGUILayout.Toggle("Montrer les graphismes", myState.ShowGraphics);
        if (myState.ShowGraphics)
        {
            EditorGUILayout.Space();
            myState.Effect = (GameObject)EditorGUILayout.ObjectField(myState.Effect, typeof(GameObject), false);
            myState.GraphicEffectAppliedOn = (GraphicEffectOn)EditorGUILayout.EnumFlagsField("Appliquer sur...", myState.GraphicEffectAppliedOn);
            EditorGUILayout.Space();
        }

        EditorGUILayout.LabelField("------------------");
        EditorGUILayout.SelectableLabel("Effets", LabelBold);

        myState.ShowStatModifier = EditorGUILayout.Toggle("Montrer le modificateur de statistique", myState.ShowStatModifier);
        if (myState.ShowStatModifier)
        {
            EditorGUILayout.Space();
            myState.myStateType = (StateType)EditorGUILayout.EnumFlagsField("Type d'Etat", myState.myStateType);
            if(myState.myStateType == StateType.StateGiver)
            {
                myState.EffectGivedOnAttackPath = EditorGUILayout.TextField("Lien de secours pour l'Etat appliqué", myState.EffectGivedOnAttackPath);
                myState.EffectGivedOnAttack = (aState)EditorGUILayout.ObjectField(myState.EffectGivedOnAttack, typeof(aState), false);
            }
            if(myState.myStateType == StateType.StartTurnDamageGiver)
            {
                myState.InflictedDamages = EditorGUILayout.IntField("Dégats au début du tour", myState.InflictedDamages);
            }
            if(myState.myStateType == StateType.StatChanger)
            {
                myState.StatModified = EditorGUILayout.TextField("Nom de la statistique modifiée", myState.StatModified);
                myState.ModifiedOf = EditorGUILayout.FloatField("Valeur de la modification", myState.ModifiedOf);
            }
            
            myState.ActiveTurns = EditorGUILayout.IntField("Nombre de tours actifs", myState.ActiveTurns);
            
            EditorGUILayout.Space();
        }

        if (GUILayout.Button("Set Dirty"))
        {
            
        }
    }*/
}
