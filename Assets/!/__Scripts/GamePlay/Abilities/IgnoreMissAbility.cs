using System.Collections;
using UnityEngine;
public class IgnoreMissAbility : AbilityBase
{
    private Coroutine _durationRoutine;
    private ShieldDamageSave _currentSave;

    private void OnEnable()
    {
        ShieldDamageSave.OnDamagePrevented += HandleDamagePrevented;
    }

    private void OnDisable()
    {
        ShieldDamageSave.OnDamagePrevented -= HandleDamagePrevented;
    }

    private void HandleDamagePrevented(ShieldDamageSave save)
    {
        if(save == _currentSave)
        {
            Debug.LogError("DAMAGE PREVENTED BY SHIELD");
            EndIgnoreMissAbility();
        }
    }

    public override void Activate(Quaternion rotation)
    {
        _currentSave = new ShieldDamageSave(true, true, GetModifiedDuration(), stopTimeDuration: 1.5f);
        DamageSaveManager.Instance.Register(_currentSave);
        ComboManager.Instance.PreventNextComboBreak(GetModifiedDuration());
        
        if(_durationRoutine != null)
        {
            StopCoroutine(_durationRoutine);
            _durationRoutine = null;
        }

        _durationRoutine = StartCoroutine(DurationRoutine());

        base.Activate(rotation);
    }

    private IEnumerator DurationRoutine()
    {
        yield return new WaitForSeconds(GetModifiedDuration());

        _durationRoutine = null;
        EndIgnoreMissAbility();
    }

    private void EndIgnoreMissAbility()
    {
        if(_durationRoutine != null)
        {
            StopCoroutine(_durationRoutine);
            _durationRoutine = null;
        }

        _currentSave = null;

        Debug.LogError("DMAGE PREVENTED BY ABILITY, ENDING ABILITY");


        EndAbility();
    }
}