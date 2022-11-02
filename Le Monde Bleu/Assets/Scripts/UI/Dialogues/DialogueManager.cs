using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum DialogueType { Start, Event, End }
public enum PortraitSituation { Hidden, First, Second };
public enum PortraitHightlight { Hightlighted, NoHightlighted };

public class DialogueManager : Singleton<DialogueManager>
{
    TurnManager TM;
    InitiativeDisplayer ID;

    GameObject Accroche;
    GameObject AccrocheText;
    Animator Bars;

    [HideInInspector] public aDialogue myDialogue;
    [HideInInspector] public DialogueType myDialogueType;

    aDialoguePortrait P1, P2, P3;
    aDialoguePortrait First;
    aDialoguePortrait Second;

    TextMeshProUGUI myText;

    [Header("Global")]
    int DialogueIndex = -1;
    [HideInInspector] public bool DialogueActive;

    [Header("Identities")]
    [HideInInspector] public List<string> IdentitiesName;
    [HideInInspector] public List<Alignement> IdentitiesAlignement;
    [HideInInspector] public List<Sprite> IdentitiesPortrait;

    [Header("Writing")]
    bool Writing;
    string ColorOfName;
    int IndexWriting;
    public float WritingLatency;
    float WritingTime;
    string Writed;

    void Awake()
    {
        if (Instance != this)
            Destroy(this);

        TM = TurnManager.Instance;
        ID = InitiativeDisplayer.Instance;

        Accroche = transform.GetChild(0).gameObject;
        Bars = Accroche.GetComponent<Animator>();
        
        AccrocheText = Accroche.transform.GetChild(5).gameObject;
        myText = AccrocheText.GetComponentInChildren<TextMeshProUGUI>();

        P1 = Accroche.transform.GetChild(2).GetComponent<aDialoguePortrait>();
        P2 = Accroche.transform.GetChild(3).GetComponent<aDialoguePortrait>();
        P3 = Accroche.transform.GetChild(4).GetComponent<aDialoguePortrait>();

        Accroche.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && DialogueActive)
        {
            if(!Writing)
                UpdateDialogue();
            else
            {
                myText.text = "<color=" + ColorOfName + ">" + myDialogue.DialogueNames[DialogueIndex] + "</color>" + " : " +myDialogue.DialogueLines[DialogueIndex];
                WritingTime = 0;
                IndexWriting = 0;
                Writing = false;
            }
        }

        if (Writing)
        {
            WritingTime += Time.deltaTime;
            if(WritingTime >= WritingLatency)
            {
                WritingTime = 0;
                IndexWriting += 1;
                if(IndexWriting <= myDialogue.DialogueLines[DialogueIndex].Length)
                {
                    Writed = myDialogue.DialogueLines[DialogueIndex].Substring(0, IndexWriting);
                    myText.text = "<color=" + ColorOfName + ">" + myDialogue.DialogueNames[DialogueIndex] + "</color>" + " : " + Writed;
                }
                else
                {
                    IndexWriting = 0;
                    Writing = false;
                }
            }
        }
    }

    public void StartDialogue(aDialogue myNewDialogue, DialogueType newDialogueType = DialogueType.Event)
    {
        myDialogue = myNewDialogue;
        myDialogueType = newDialogueType;

        Accroche.SetActive(true);
        P1.gameObject.SetActive(true);
        P1.Nom = "";
        P2.gameObject.SetActive(true);
        P2.Nom = "";
        P3.gameObject.SetActive(true);
        P3.Nom = "";
        AccrocheText.SetActive(true);
        DialogueActive = true;
        UpdateDialogue();
    }

    void UpdateDialogue()
    {
        DialogueIndex += 1;

        if(DialogueIndex < myDialogue.DialogueNames.Count)
        {
            string newName = myDialogue.DialogueNames[DialogueIndex];

            for (int i = 0; i < IdentitiesName.Count; i++)
            {
                if (IdentitiesName[i] == newName)
                {
                    ColorOfName = DetectedColor(IdentitiesAlignement[i]);
                    break;
                }
            }

            Writing = true;
            Writed = "";
            IndexWriting = 0;
            myText.text = "<color=" + ColorOfName + ">" + myDialogue.DialogueNames[DialogueIndex] + "</color>" + ">" + " : " + Writed;

            bool SecondChanged = false;

            if (!First)
            {
                First = NearestPortrait();
                First.myOrder("Appear", newName);
            }
            else
            {
                if (First.Nom == newName)
                {
                    if (First.myPortraitHighlight == PortraitHightlight.NoHightlighted)
                        First.myOrder("Highlight");
                }
                else
                {
                    if (Second)
                    {
                        if (Second.Nom == newName && First.myPortraitHighlight == PortraitHightlight.Hightlighted)
                            First.myOrder("NoHighlight");

                        else if (Second.Nom != newName)
                        {
                            SecondChanged = true;
                            First.myOrder("ToSecond");
                            Second.myOrder("Disappear");

                            aDialoguePortrait toExcept = Second;
                            Second = First;
                            First = NearestPortrait(toExcept);
                            First.myOrder("Appear", newName);
                        }
                    }
                    else
                    {
                        SecondChanged = true;
                        First.myOrder("ToSecond");
                        Second = First;
                        First = NearestPortrait();
                        First.myOrder("Appear", newName);
                    }
                }
            }

            if (Second && !SecondChanged)
            {
                if (Second.Nom == newName && Second.myPortraitHighlight == PortraitHightlight.NoHightlighted)
                    Second.myOrder("Highlight");
                else if (Second.Nom != newName && Second.myPortraitHighlight == PortraitHightlight.Hightlighted)
                    Second.myOrder("NoHighlight");
            }
        }

        else
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        DialogueActive = false;
        DialogueIndex = -1;
        First = null;
        Second = null;

        P1.gameObject.SetActive(false);
        P2.gameObject.SetActive(false);
        P3.gameObject.SetActive(false);
        AccrocheText.SetActive(false);
        Bars.SetTrigger("Disappear");
        Invoke("Close", 0.8f);
    }

    void Close()
    {
        if (myDialogueType == DialogueType.Start)
        {
            TM.myFS = FightSituation.Fight;
            ID.Appear();
        }
        else if (myDialogueType == DialogueType.Event)
        {
            TM.myFS = FightSituation.Fight;
            if (!TM.NeedsNewTurn)
            {
                FightEntity active = TM.activeFighters[TM.TurnIndex];
                if (active.myAI && active.myAI.NeedsToBeCooled && active.myAI.Cooled)
                    active.myAI.Cooled = false;
                else
                    TM.activeFighters[TM.TurnIndex].ActualizeMovement();
            }
        }
            
        else if (myDialogueType == DialogueType.End)
            TM.EndCombat();
        Accroche.SetActive(false);
    }

    aDialoguePortrait NearestPortrait(aDialoguePortrait Except = null)
    {
        if (P1 != Second && P1 != Except)
            return P1;
        else if (P2 != Second && P2 != Except)
            return P2;
        else if (P3 != Second && P3 != Except)
            return P3;
        else
            return P1;
    }

    string DetectedColor(Alignement myAlignement)
    {
        if (myAlignement == Alignement.Membre)
            return "#0094FF";
        else if (myAlignement == Alignement.Allié)
            return "#007F0E";
        else
            return "#FF0000";
    }
}
