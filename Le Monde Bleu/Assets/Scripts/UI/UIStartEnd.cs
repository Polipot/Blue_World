using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIStartEnd : Singleton<UIStartEnd>
{
    Animator myAnimator;
    TextMeshProUGUI myText;

    void Awake()
    {
        if (Instance != this)
            Destroy(this);

        myAnimator = GetComponentInChildren<Animator>();
        myText = myAnimator.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    public void TriggerEnd(bool isVictory)
    {
        if (isVictory)
        {
            myText.color = Color.green;
            myText.text = "V I C T O R Y";
        }
        else
        {
            myText.color = Color.red;
            myText.text = "D E F E A T";
        }

        myAnimator.SetTrigger("End");
    }
}
