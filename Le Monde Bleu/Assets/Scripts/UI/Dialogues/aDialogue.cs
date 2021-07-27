using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Condition { None, LowLife }

[CreateAssetMenu]
public class aDialogue : ScriptableObject
{
    [Header("Conditions")]
    public string myConditionCrypted;
    [HideInInspector] public Condition myCondition;
    [Space]
    [HideInInspector] public FightEntity mySubject;
    // My life is less than
    [HideInInspector] public int myMinLife;

    public List<string> DialogueNames;

    [TextArea(10,30)]
    public List<string> DialogueLines;

    public void myConditionTranslated()
    {
        TurnManager TM = TurnManager.Instance;

        if (myConditionCrypted == "")
            myCondition = Condition.None;

        else
        {
            string[] splitArray = myConditionCrypted.Split('|');

            for(int i = 0; i < splitArray.Length; i++)
            {
                string[] LocalSplit = splitArray[i].Split('_');
                string What = LocalSplit[0];
                string Remain = LocalSplit[1];

                if (What == "Subject")
                {
                    string mySubjectName = Remain;
                    for (int a = 0; a < TM.activeFighters.Count; a++)
                    {
                        if (TM.activeFighters[a].Nom == mySubjectName)
                        {
                            mySubject = TM.activeFighters[a];
                            break;
                        }
                    }
                    if (!mySubject)
                    {
                        for (int a = 0; a < TM.Reinforcements.Count; a++)
                        {
                            if (TM.Reinforcements[a].Nom == mySubjectName)
                            {
                                mySubject = TM.Reinforcements[a];
                                break;
                            }
                        }
                    }
                } 
                else if (What == "LowLife")
                {
                    myCondition = Condition.LowLife;
                    myMinLife = int.Parse(Remain);
                }
            }
        }
    }

    public bool isVerified()
    {
        switch (myCondition)
        {
            case Condition.None:
                return false;
            case Condition.LowLife:

                if (mySubject.Hp < myMinLife)
                    return true;
                else
                    return false;
            default:
                return false;
        }
    }
}
