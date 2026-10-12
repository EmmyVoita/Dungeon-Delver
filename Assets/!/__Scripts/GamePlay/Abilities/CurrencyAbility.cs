using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class CurrencyAbility : AbilityBase
{
    [Header("Ability Settings")]
    [SerializeField] private  float baseModifier = 2.0f;
    [SerializeField] private VisualEffect visualEffect;


    public override void Activate(Quaternion rotation)
    {
        CurrencyManager.Instance.AddTemporaryCurrencyModifier(baseModifier,GetModifiedDuration());
        StartCoroutine(StartStopVFX());
        base.Activate(rotation);
    }

    private IEnumerator StartStopVFX()
    {
        visualEffect.Play();
        yield return new WaitForSeconds(GetModifiedDuration());
        visualEffect.Stop();

        AudioHelpers.PlaySoundEffect(Data.deactivateSound, Player.Instance.transform.position);
        EndAbility();
    }
}
