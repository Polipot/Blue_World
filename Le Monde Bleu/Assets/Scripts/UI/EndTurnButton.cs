using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndTurnButton : Singleton<EndTurnButton>
{
    Button myEndTurnButton;
    PlayerManager PM;

    private void Awake()
    {
        if (Instance != this)
            Destroy(gameObject);
        myEndTurnButton = GetComponent<Button>();

        PM = PlayerManager.Instance;
    }

    private void Update()
    {
        if(PM.actualEntity && PM.actualEntity.mySituation == Situation.ChooseMove && !myEndTurnButton.interactable)
            myEndTurnButton.interactable = true;
        else if ((!PM.actualEntity || PM.actualEntity.mySituation != Situation.ChooseMove) && myEndTurnButton.interactable)
            myEndTurnButton.interactable = false;
    }

    public void NextTurn()
    {
        if (PM.actualEntity)
            PM.actualEntity.EndTurn();
    }
}
