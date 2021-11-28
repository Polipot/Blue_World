using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(FightEntity))]
public class FighterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        FightEntity myFightEntity = (FightEntity)target;

        base.OnInspectorGUI();

        if (GUILayout.Button("Sauvegarder"))
        {
            AFighterSave loadInto = ScriptableObject.CreateInstance<AFighterSave>();

            // Global

            loadInto.Nom = myFightEntity.Nom;
            loadInto.SaveIndex = myFightEntity.SaveIndex;
            loadInto.HeroType = myFightEntity.HeroType;
            loadInto.Description = myFightEntity.Description;
            loadInto.myClasse = myFightEntity.myClasse;
            loadInto.myAlignement = myFightEntity.myAlignement;

            // Current Weapons

            loadInto.FirstWeaponName = myFightEntity.FirstWeaponName;
            loadInto.SideWeaponName = myFightEntity.SideWeaponName;


            // Stats & Comps

            loadInto.Hp = loadInto.MaxHp = myFightEntity.MaxHp;
            loadInto.maxArmor = myFightEntity.maxArmor;

            loadInto.Resistance = myFightEntity.Resistance;
            loadInto.Parade = myFightEntity.Parade;
            loadInto.Esquive = myFightEntity.Esquive;

            loadInto.MaxEnergy = myFightEntity.MaxEnergy;
            loadInto.EnergyGain = myFightEntity.EnergyGain;

            loadInto.Tranchant = myFightEntity.Tranchant;
            loadInto.Perforant = myFightEntity.Perforant;
            loadInto.Magique = myFightEntity.Magique;
            loadInto.Choc = myFightEntity.Choc;
            loadInto.FrappeHeroique = myFightEntity.FrappeHeroique;

            loadInto.InitiativeSpeed = myFightEntity.InitiativeSpeed;
            loadInto.Speed = myFightEntity.Speed;
            loadInto.myNativeCompetences = new List<string>();

            for (int i = 0; i < myFightEntity.myCompetences.Count; i++)
            {
                string Nom = myFightEntity.myCompetences[i].Name;
                loadInto.myNativeCompetences.Add(Nom);
            }

            // Graphics

            string path = "CharactersSprites/";
            string ValidName = "";

            if (loadInto.HeroType == "")
            {
                for (int i = 0; i < loadInto.Nom.Length; i++)
                {
                    char ToVerify = loadInto.Nom[i];
                    if (ToVerify == ' ')
                        ValidName += '_';
                    else if (ToVerify == 'é' || ToVerify == 'è')
                        ValidName += 'e';
                    else
                        ValidName += ToVerify;
                }
            }
            else
                ValidName = loadInto.HeroType;

            path += ValidName;

            loadInto.Path = path;
            myFightEntity.Save(loadInto);
            AssetDatabase.Refresh();
        }

        if (GUILayout.Button("New Index & New Dir. if null") && myFightEntity.HeroType != "")
        {
            if (!Directory.Exists(Application.streamingAssetsPath + "/SavedCharacters/" + myFightEntity.HeroType))
                Directory.CreateDirectory(Application.streamingAssetsPath + "/SavedCharacters/" + myFightEntity.HeroType);
            myFightEntity.SaveIndex = Directory.GetFiles(Application.streamingAssetsPath + "/SavedCharacters/" + myFightEntity.HeroType, "*", SearchOption.TopDirectoryOnly).Length / 2;
        }
    }
}
