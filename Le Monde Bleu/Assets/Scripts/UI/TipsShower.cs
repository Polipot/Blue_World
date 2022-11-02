using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class TipsShower : Singleton<TipsShower>
{
    FightCamera FC;

    float NotInstant;
    bool isNotInstant = false;
    aTip Actual;
    TextMeshProUGUI myText;
    GameObject Accroche;
    RectTransform myRectTransform;
    RectTransform TipRectTransform;
    RectTransform CanvasRectTransform;

    private void Awake()
    {
        if (Instance != this)
            Destroy(this);

        FC = FightCamera.Instance;

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

        RaycastHit2D my2DHit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

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

                    }
                    else if (go.gameObject.GetComponent<aTip>() != null && go.gameObject.GetComponent<aTip>() != Actual)
                    {
                        Actual = go.gameObject.GetComponent<aTip>();
                        Changed = true;

                        InitiativeCadre Cadre = Actual.gameObject.GetComponentInParent<InitiativeCadre>();
                        if (Cadre && Cadre.myEntity)
                            FC.shordLivedTarget = Cadre.myEntity.transform;

                        isNotInstant = false;
                    }
                    else
                    {
                        Actual = null;
                        Changed = true;

                        if (FC.shordLivedTarget)
                            FC.shordLivedTarget = null;

                        isNotInstant = false;
                    }
                }
            }
            else
            {
                Actual = null;
                Changed = true;

                if (FC.shordLivedTarget)
                    FC.shordLivedTarget = null;

                isNotInstant = false;
            }
        }

        else if ( my2DHit && my2DHit.collider.GetComponent<aTip>())
        {
            aTip NewTip = my2DHit.collider.GetComponent<aTip>();
            if (NewTip != Actual)
            {
                Actual = NewTip;
                Changed = true;

                if (FC.shordLivedTarget)
                    FC.shordLivedTarget = null;

                NotInstant = 0;
                isNotInstant = true;
            }
            else if(NotInstant < 1.5f)
            {
                NotInstant += Time.deltaTime;
                if(NotInstant  >= 1.5f)
                    Changed = true;
            }                
        }

        else if (Actual != null)
        {
            Actual = null;
            Changed = true;

            if (FC.shordLivedTarget)
                FC.shordLivedTarget = null;

            isNotInstant = false;
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

    void ActualizeShower(bool ForceNotShown = false)
    {
        if (Actual && (!isNotInstant || NotInstant > 1.5f))
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

    public string NameFromWeaponType(WeaponType myWeaponType)
    {
        switch (myWeaponType)
        {
            case WeaponType.None:
                return "";
            case WeaponType.Everything:
                return "";
            case WeaponType.AttackMeleeWeapon:
                return "melee weapon";
            case WeaponType.AttackRangedWeapon:
                return "ranged weapon";
            case WeaponType.HandCombat:
                return "handed combat style";
            case WeaponType.DefenseWeapon:
                return "defense equipement";
            case WeaponType.SpellCaster:
                return "magic object";
            default:
                return "";
        }
    }

    public string NameFromWeaponType(int myWeaponType)
    {
        switch (myWeaponType)
        {
            case (int)WeaponType.None:
                return "";
            case (int)WeaponType.Everything:
                return "";
            case (int)WeaponType.AttackMeleeWeapon:
                return "melee weapon";
            case (int)WeaponType.AttackRangedWeapon:
                return "ranged weapon";
            case (int)WeaponType.HandCombat:
                return "handed combat style";
            case (int)WeaponType.DefenseWeapon:
                return "defense equipement";
            case (int)WeaponType.SpellCaster:
                return "magic object";
            default:
                return "";
        }
    }
}
