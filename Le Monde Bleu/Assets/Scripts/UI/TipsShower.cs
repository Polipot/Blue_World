using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class TipsShower : MonoBehaviour
{
    aTip Actual;
    TextMeshProUGUI myText;
    GameObject Accroche;
    RectTransform myRectTransform;
    RectTransform TipRectTransform;
    RectTransform CanvasRectTransform;

    private void Awake()
    {
        Accroche = transform.GetChild(0).gameObject;
        TipRectTransform = Accroche.transform.GetChild(0).GetComponent<RectTransform>();
        myText = TipRectTransform.GetChild(0).GetComponent<TextMeshProUGUI>();
        myRectTransform = GetComponent<RectTransform>();
        CanvasRectTransform = transform.parent.GetComponent<RectTransform>();
        Accroche.SetActive(false);
    }

    void Update()
    {
        bool Changed = false;

        if (EventSystem.current.IsPointerOverGameObject())
        {
            PointerEventData pointer = new PointerEventData(EventSystem.current);
            pointer.position = Input.mousePosition;

            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, raycastResults);

            if (raycastResults.Count > 0)
            {
                foreach (var go in raycastResults)
                {
                    if (go.gameObject.GetComponent<aTip>() != null && go.gameObject.GetComponent<aTip>() == Actual)
                    {
                        // Oui
                    }
                    else if (go.gameObject.GetComponent<aTip>() != null && go.gameObject.GetComponent<aTip>() != Actual)
                    {
                        Actual = go.gameObject.GetComponent<aTip>();
                        Changed = true;
                    }
                    else
                    {
                        Actual = null;
                        Changed = true;
                    }
                }
            }
            else
            {
                Actual = null;
                Changed = true;
            }
        }

        else if(Actual != null)
        {
            Actual = null;
            Changed = true;
        }

        if(Changed)
            ActualizeShower();

        Move();
    }

    void Move()
    {
        Vector2 NewPosition = Input.mousePosition;

        Vector2 Correction = new Vector2(0, 0);

        if (NewPosition.y >= CanvasRectTransform.rect.height)
            Correction += new Vector2(0, TipRectTransform.rect.yMax);
        if (NewPosition.x >= CanvasRectTransform.rect.width)
            Correction += new Vector2(TipRectTransform.rect.xMax, 0);

        myRectTransform.anchoredPosition = (NewPosition / CanvasRectTransform.localScale.x) - Correction;
    }

    void ActualizeShower()
    {
        if (Actual)
        {
            if (!Accroche.activeSelf)
                Accroche.SetActive(true);
            myText.text = Actual.ToShow;
        }
        else
        {
            if (Accroche.activeSelf)
                Accroche.SetActive(false);
        }
    }
}
