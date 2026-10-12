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

    private bool Unlocked(AbilityData ability)
    {
        return RunHistoryManager.Instance.GetHighScore() >= ability.scoreRequirement || 
               RunHistoryManager.Instance.GetUnlockAll();
    }

    private void OnEnable()
    {
        RunHistoryManager.OnSetUnlockAll += HandleSetUnlockAll;
    }

    private void OnDisable()
    {
        RunHistoryManager.OnSetUnlockAll -= HandleSetUnlockAll;
    }

    private void HandleSetUnlockAll(bool unlocked)
    {
        Debug.Log($"AbilityUnlockManager received UnlockAll: {unlocked}");
        Debug.Log($"GetUnlockAll() = {RunHistoryManager.Instance.GetUnlockAll()}");

        LoadData();
        CheckForUpdates();

        foreach (var record in _saveData.records)
        {
            Debug.Log($"{record.abilityType}: unlocked = {record.unlocked}");
        }

        StartCoroutine(presentationController.PlayUnlockSequence(GetNewUnlocks()));
    }
    
    
    private void Start()
    {
        if (abilityDatabase == null)
        {
            Debug.LogError("Ability Database is missing!");
            return;
        }

        _saveFilePath = Path.Combine(Application.persistentDataPath,"abilitySaveData.json");

        LoadData();
        CheckForUpdates();
        //SyncMissingAbilities();

        StartCoroutine(presentationController.PlayUnlockSequence(GetNewUnlocks()));
    }

    private void LoadData()
    {
        if (File.Exists(_saveFilePath))
        {
            string json = File.ReadAllText(_saveFilePath);
            _saveData = JsonUtility.FromJson<AbilitySaveData>(json);
        }
        else
        {   
            // So the goal here is that if we dont have an existing file we read from out database 
            // of abilities and create a new unlock record for each ability.
            // The unlock record maintins the ability type, which is an enum. This is how we identify
            // which record is assigned to what ability. We will check for whether an ability is unlocked
            // whenever there is a chance for that to be updated. This will be when we load the menu 
            // (from getting back from playing the game which might update the players high score)
            // Then, I also have a setting to unlock everything, so we also need to check it then.
            // The run history manager maintains a veriable for whether everything should be unlocked,
            // so we can check that. What I will do here is subscribe to an event that fires whenever that
            // run history variable changes.

            List<AbilityUnlockRecord> records = new List<AbilityUnlockRecord>();

            foreach(AbilityData ability in abilityDatabase.abilities)
            {

                // When we first unlock an ability, we have an animation for presenting the ability.
                // The idea is that we store a variable that says whether its already been presented. 
                // Its set to false by default, and then when it gets presented, it gets set to true.

                // I want to handle the situation for when we have abilities that are unlocked by default.
                // For those I dont want to play the unlock animation so i just set presented to true when
                // we create the record.
                records.Add(new AbilityUnlockRecord()
                {
                    abilityType = ability.abilityType,
                    unlocked = Unlocked(ability),
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
        List<AbilityUnlockRecord> updatedRecords = new();

        foreach (AbilityData ability in abilityDatabase.abilities)
        {
            AbilityUnlockRecord existingRecord =
                _saveData.records.FirstOrDefault(
                    r => r.abilityType == ability.abilityType
                );

            if (existingRecord != null)
            {
                // Existing ability
                updatedRecords.Add(new AbilityUnlockRecord()
                {
                    abilityType = ability.abilityType,
                    unlocked = Unlocked(ability),
                    presented = existingRecord.presented
                });
            }
            else
            {
                // Newly added ability
                updatedRecords.Add(new AbilityUnlockRecord()
                {
                    abilityType = ability.abilityType,
                    unlocked = Unlocked(ability),
                    presented = ability.scoreRequirement == 0
                });
            }
        }

        _saveData.records = updatedRecords;

        SaveDataToFile();
    }

    /*
    private void UpdateRecordCountChanged()
    {
        // We also need to handle the situation in which we've added new abilities to the
        // ability database.
        if(abilityDatabase.abilities.Count > _saveData.records.Count)
        {
            // loop over the ability data base and check if the record contains that ability type.
            // If the record doesn't have that ability type then add it. 
            // For the other recods we just check whether "unlocked" has changed.
            foreach (AbilityData ability in abilityDatabase.abilities)
            {
                bool exists = _saveData.records.Any(r => r.abilityType == ability.abilityType);

                if (!exists)
                {
                    _saveData.records.Add(new AbilityUnlockRecord()
                    {
                        abilityType = ability.abilityType,
                        unlocked = Unlocked(ability),
                        presented = ability.scoreRequirement == 0
                    });
                }
            }

            SaveDataToFile();
            return;
        }


        // Its possible that the number of records doesnt match the number of abilities in our file
        // This happens if i remove an ability from the ability database. In that case we just copy 
        // the data for the missing abilities.

        if(abilityDatabase.abilities.Count < _saveData.records.Count)
        {
            List<AbilityUnlockRecord> records = new List<AbilityUnlockRecord>();

            foreach (AbilityUnlockRecord record in _saveData.records)
            {
                bool existsInDatabase = abilityDatabase.abilities.Any(r => r.abilityType == record.abilityType);

                if(existsInDatabase)
                {
                    AbilityData abilityData = abilityDatabase.abilities.First(r => r.abilityType == record.abilityType);

                    records.Add(new AbilityUnlockRecord()
                    {
                        abilityType = record.abilityType,
                        unlocked = Unlocked(abilityData),
                        presented = record.presented
                    });
                }
                else
                {
                    records.Add(record);
                }
            }

            _saveData = new AbilitySaveData()
            {
                records = records
            };

            SaveDataToFile();
            return;
        }
    }
    */

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
        return _saveData.records.FirstOrDefault(a => a.abilityType == type);
    }

    public void MarkPresented(AbilityType type)
    {
        AbilityUnlockRecord record =_saveData.records.Find(r => r.abilityType == type);

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