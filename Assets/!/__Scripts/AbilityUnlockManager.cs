using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class AbilityUnlockManager : MonoBehaviour
{
    [SerializeField] private AbilityDatabase abilityDatabase;
    [SerializeField] private AbilityUnlockPresentationController presentationController;

    private AbilitySaveData _saveData;
    private string _saveFilePath;
    
    
    private void Start()
    {
        if (abilityDatabase == null)
        {
            Debug.LogError("Ability Database is missing!");
            return;
        }

        _saveFilePath = Path.Combine(
            Application.persistentDataPath,
            "abilitySaveData.json"
        );

        LoadData();
        CheckForUpdates();
        SyncMissingAbilities();

        StartCoroutine(
            presentationController.PlayUnlockSequence(GetNewUnlocks())
        );
    }

    private void LoadData()
    {
        if (File.Exists(_saveFilePath))
        {
            string json = File.ReadAllText(_saveFilePath);

            Debug.Log($"Loaded Ability Unlock JSON:\n{json}");

            _saveData = JsonUtility.FromJson<AbilitySaveData>(json);
        }
        else
        {
            List<AbilityUnlockRecord> records = new List<AbilityUnlockRecord>();

            foreach(AbilityData ability in abilityDatabase.abilities)
            {
                Debug.Log($"Highscore => {ScoreManager.Instance.HighScore}, score required => {ability.scoreRequirement}");
                records.Add(new AbilityUnlockRecord()
                {
                    abilityType = ability.abilityType,
                    unlocked = ScoreManager.Instance.HighScore >= ability.scoreRequirement,
                    presented = ability.scoreRequirement == 0 ? true : false
                });
            }

            _saveData = new AbilitySaveData()
            {
                records = records
            };
        }
    }

    private void CheckForUpdates()
    {
        // We want to check to see if any of the abilites should be unlocked when we load the data
        List<AbilityUnlockRecord> records = new List<AbilityUnlockRecord>();

        for(int i = 0; i < _saveData.records.Count; i++)
        {
            records.Add(new AbilityUnlockRecord()
            {
                abilityType = abilityDatabase.abilities[i].abilityType,
                unlocked = ScoreManager.Instance.HighScore >= abilityDatabase.abilities[i].scoreRequirement,
                presented = _saveData.records[i].presented
            });
        }

        _saveData = new AbilitySaveData()
        {
            records = records
        };
    } 

    private List<AbilityType> GetNewUnlocks()
    {
        List<AbilityUnlockRecord> newUnlocks = _saveData.records
            .Where(a => a.unlocked == true && a.presented == false)
            .ToList();

        List<AbilityType> abilityData = new ();

        foreach(AbilityUnlockRecord unlockRecord in newUnlocks)
        {
            abilityData.Add(unlockRecord.abilityType);
        }

        Debug.Log($"Fetching New Unlocks. Amount => {abilityData.Count}");

        return abilityData;
    }

    public bool IsUnlocked(AbilityType type)
    {
        return GetRecordByType(type).unlocked;
    }

    public bool IsPresented(AbilityType type)
    {
        return GetRecordByType(type).presented;
    }

    private AbilityUnlockRecord GetRecordByType(AbilityType type)
    {
        return _saveData.records
            .FirstOrDefault(a => a.abilityType == type);
    }

    public void MarkPresented(AbilityType type)
    {
        AbilityUnlockRecord record =
            _saveData.records.Find(
                r => r.abilityType == type);

        if (record != null)
        {
            record.presented = true;
            SaveDataToFile();
        }
    }



    public AbilityData GetAbility(AbilityType type)
    {
        return abilityDatabase.abilities.Find(a => a.abilityType == type);
    }

    private void SaveDataToFile()
    {
        string json = JsonUtility.ToJson(_saveData, true);
        File.WriteAllText(_saveFilePath, json);
        Debug.Log($"Saved Ability Unlock Records To {_saveFilePath}");
    }

    private void SyncMissingAbilities()
    {
        foreach (AbilityData ability in abilityDatabase.abilities)
        {
            bool exists = _saveData.records.Any(
                r => r.abilityType == ability.abilityType
            );

            if (!exists)
            {
                Debug.Log($"Adding missing ability record: {ability.name}");

                _saveData.records.Add(new AbilityUnlockRecord()
                {
                    abilityType = ability.abilityType,
                    unlocked =
                        ScoreManager.Instance.HighScore
                        >= ability.scoreRequirement,

                    presented = ability.scoreRequirement == 0
                });
            }
        }

        SaveDataToFile();
    }

    [ContextMenu("Delete Ability Save File")]
    public void DeleteSaveFile()
    {
        if (File.Exists(_saveFilePath))
        {
            File.Delete(_saveFilePath);

            Debug.Log("Deleted ability unlock save file.");
        }
    }

}