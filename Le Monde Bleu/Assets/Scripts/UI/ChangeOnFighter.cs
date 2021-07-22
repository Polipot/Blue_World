using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ChangeOnFighter : MonoBehaviour
{
    TextMeshProUGUI myText;
    string ToShow;
    float Temps;
    float Latence;
    int myPositiveness;

    Transform myChild;
    bool AlreadyActive;

    // Start is called before the first frame update
    public void Activation(string theNewText, float theLatence, int Positiveness = 1)
    {
        myChild =  transform.GetChild(0).GetComponent<Transform>();
        myChild.gameObject.SetActive(false);
        Latence = theLatence;
        ToShow = theNewText;
        myPositiveness = Positiveness;

        if (Latence == 0)
        {
            AlreadyActive = true;
            myChild.gameObject.SetActive(true);
            myText = GetComponentInChildren<TextMeshProUGUI>();
            myText.color = GetPosColor(myPositiveness);
            myText.text = ToShow;
        }
    }

    Color GetPosColor(int Positiveness)
    {
        if (Positiveness == 0)
            return Color.red;
        else if (Positiveness == 2)
            return Color.green;
        else
            return Color.yellow;
    }

    private void Update()
    {
        if(!AlreadyActive)
        {
            Temps += Time.deltaTime;
            
            if (Temps >= Latence)
            {
                AlreadyActive = true;
                myChild.gameObject.SetActive(true);
                myText = GetComponentInChildren<TextMeshProUGUI>();
                myText.color = GetPosColor(myPositiveness);
                myText.text = ToShow;
            }
        }
    }
}
