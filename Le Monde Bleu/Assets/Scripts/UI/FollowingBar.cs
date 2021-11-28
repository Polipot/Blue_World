using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FollowingBar : MonoBehaviour
{
    FightEntity myFightEntity;

    [Header("Elements")]
    GameObject ElementsAnchor;
    [Space]
    Image ArmorIcon;
    Image ArmorBackground;
    Image ArmorChange;
    Image Armor;
    [Space]
    Image HealthIcon;
    Image HealthBackground;
    Image HealthChange;
    Image Health;

    [Header("Animation de Changement")]
    bool NeedsUpdate;
    float TimeHit;

    [Header("Animation de Disparu")]
    bool IsDisappearing;
    float Albedo;

    public void Activation(FightEntity myNewFightEntity)
    {
        ElementsAnchor = transform.GetChild(0).gameObject;

        ArmorBackground = ElementsAnchor.transform.GetChild(0).GetComponent<Image>();
        ArmorChange = ElementsAnchor.transform.GetChild(1).GetComponent<Image>();
        Armor = ElementsAnchor.transform.GetChild(2).GetComponent<Image>();
        ArmorIcon = ElementsAnchor.transform.GetChild(3).GetComponent<Image>();

        HealthBackground = ElementsAnchor.transform.GetChild(4).GetComponent<Image>();
        HealthChange = ElementsAnchor.transform.GetChild(5).GetComponent<Image>();
        Health = ElementsAnchor.transform.GetChild(6).GetComponent<Image>();
        HealthIcon = ElementsAnchor.transform.GetChild(7).GetComponent<Image>();

        myFightEntity = myNewFightEntity;

        if (myFightEntity.BaseArmor <= 0)
        {
            ArmorBackground.color = new Color(ArmorBackground.color.r, ArmorBackground.color.g, ArmorBackground.color.b, 0);
            ArmorChange.color = new Color(ArmorChange.color.r, ArmorChange.color.g, ArmorChange.color.b, 0);
            Armor.color = new Color(Armor.color.r, Armor.color.g, Armor.color.b, 0);
            ArmorIcon.color = new Color(ArmorIcon.color.r, ArmorIcon.color.g, ArmorIcon.color.b, 0);
        }

        Albedo = 1;
        IsDisappearing = true;
    }

    public void BarChange()
    {
        if(myFightEntity.BaseArmor > 0)
        {
            ArmorBackground.color = new Color(ArmorBackground.color.r, ArmorBackground.color.g, ArmorBackground.color.b, 1);
            ArmorChange.color = new Color(ArmorChange.color.r, ArmorChange.color.g, ArmorChange.color.b, 1);
            Armor.color = new Color(Armor.color.r, Armor.color.g, Armor.color.b, 1);
            ArmorIcon.color = new Color(ArmorIcon.color.r, ArmorIcon.color.g, ArmorIcon.color.b, 1);

            UpdateBar(Armor, myFightEntity.Armor, myFightEntity.BaseArmor);
        }

        HealthBackground.color = new Color(HealthBackground.color.r, HealthBackground.color.g, HealthBackground.color.b, 1);
        HealthChange.color = new Color(HealthChange.color.r, HealthChange.color.g, HealthChange.color.b, 1);
        Health.color = new Color(Health.color.r, Health.color.g, Health.color.b, 1);
        HealthIcon.color = new Color(HealthIcon.color.r, HealthIcon.color.g, HealthIcon.color.b, 1);

        UpdateBar(Health, myFightEntity.Hp, myFightEntity.BaseHp);

        NeedsUpdate = true;

        Albedo = 1;
        IsDisappearing = false;
    }

    public void Appear()
    {
        if (myFightEntity.BaseArmor > 0)
        {
            ArmorBackground.color = new Color(ArmorBackground.color.r, ArmorBackground.color.g, ArmorBackground.color.b, 1);
            ArmorChange.color = new Color(ArmorChange.color.r, ArmorChange.color.g, ArmorChange.color.b, 1);
            Armor.color = new Color(Armor.color.r, Armor.color.g, Armor.color.b, 1);
            ArmorIcon.color = new Color(ArmorIcon.color.r, ArmorIcon.color.g, ArmorIcon.color.b, 1);
        }

        HealthBackground.color = new Color(HealthBackground.color.r, HealthBackground.color.g, HealthBackground.color.b, 1);
        HealthChange.color = new Color(HealthChange.color.r, HealthChange.color.g, HealthChange.color.b, 1);
        Health.color = new Color(Health.color.r, Health.color.g, Health.color.b, 1);
        HealthIcon.color = new Color(HealthIcon.color.r, HealthIcon.color.g, HealthIcon.color.b, 1);

        Albedo = 1;
        IsDisappearing = false;
    }

    public void Disappear() => IsDisappearing = true;

    private void Update()
    {
        if (NeedsUpdate)
        {
            TimeHit += Time.deltaTime;
            if (TimeHit > 1)
            {
                float Distance = Health.fillAmount - HealthChange.fillAmount;
                HealthChange.fillAmount += Distance / 20;

                if(myFightEntity.BaseArmor > 0)
                {
                    float Distance2 = Armor.fillAmount - ArmorChange.fillAmount;
                    ArmorChange.fillAmount += Distance2 / 20;
                }
            }
            if(TimeHit > 2)
            {
                HealthChange.fillAmount = Health.fillAmount;
                ArmorChange.fillAmount = Armor.fillAmount;
                TimeHit = 0;
                NeedsUpdate = false;

                IsDisappearing = true;
            }
        }

        if (IsDisappearing)
        {
            Albedo -= Time.deltaTime;

            if (myFightEntity.BaseArmor > 0)
            {
                ArmorBackground.color = new Color(ArmorBackground.color.r, ArmorBackground.color.g, ArmorBackground.color.b, Albedo);
                ArmorChange.color = new Color(ArmorChange.color.r, ArmorChange.color.g, ArmorChange.color.b, Albedo);
                Armor.color = new Color(Armor.color.r, Armor.color.g, Armor.color.b, Albedo);
                ArmorIcon.color = new Color(ArmorIcon.color.r, ArmorIcon.color.g, ArmorIcon.color.b, Albedo);
            }

            HealthBackground.color = new Color(HealthBackground.color.r, HealthBackground.color.g, HealthBackground.color.b, Albedo);
            HealthChange.color = new Color(HealthChange.color.r, HealthChange.color.g, HealthChange.color.b, Albedo);
            Health.color = new Color(Health.color.r, Health.color.g, Health.color.b, Albedo);
            HealthIcon.color = new Color(HealthIcon.color.r, HealthIcon.color.g, HealthIcon.color.b, Albedo);
        }
    }

    void UpdateBar(Image theBar, float Value, float MaxValue)
    {
        Value = Mathf.Clamp(Value, 0, MaxValue);
        float Amount = Value / MaxValue;
        theBar.fillAmount = Amount;
    }

    public void SimulateChanges(FightEntity Caster, aCompetence anAttack)
    {
        float RemainingArmor = myFightEntity.Armor;
        float RemainingHealth = myFightEntity.Hp;

        float AttackDamages = (anAttack.MinDegats) / Caster.ClasseDiviser;
        AttackDamages += Mathf.RoundToInt((AttackDamages * myFightEntity.HitBonuses(Caster, anAttack)) / 100);
        AttackDamages -= Mathf.RoundToInt(AttackDamages * (myFightEntity.Resistance / 100));

        float ToDistribute = AttackDamages;
        float ArmorHit = 0;

        if (RemainingArmor > 0)
        {
            ArmorHit = Mathf.Clamp(ToDistribute, 0, RemainingArmor);
            RemainingArmor -= ArmorHit;
            ToDistribute -= ArmorHit;
        }

        float HpHit = ToDistribute;
        RemainingHealth -= HpHit;

        Armor.fillAmount = Mathf.Clamp(RemainingArmor, 0, myFightEntity.maxArmor) / myFightEntity.maxArmor;
        Health.fillAmount = Mathf.Clamp(RemainingHealth, 0, myFightEntity.MaxHp) / myFightEntity.MaxHp;

        Appear();

        IsDisappearing = false;
        Albedo = 1;
    }

    public void ResetSimulation()
    {
        Armor.fillAmount = ArmorChange.fillAmount;
        Health.fillAmount = HealthChange.fillAmount;
        IsDisappearing = true;
    }
}
