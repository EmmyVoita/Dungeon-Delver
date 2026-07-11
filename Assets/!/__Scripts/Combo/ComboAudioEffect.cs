using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(menuName = "ComboEffect/ComboAudioEffect")]
public class ComboAudioEffect : ComboEffect
{

    [SerializeField] private SoundEffect soundEffect;
    


    [Header("Trigger Condition")]
    [SerializeField] private bool triggerOnlyAtIndex = true;
    [SerializeField] private int comboInterval;
    [SerializeField] private int startCombo;
   
    
    [Header("Pitch")]
    [SerializeField] private float pitchStep;
    [SerializeField] private float minPitch = 0.3f;
    [SerializeField] private float maxPitch;


    [Header("Volume")]
    [SerializeField] private float volumeStep;
    [SerializeField] private float maxVolume;

    

    public override void Initialize()
    {
        
    }

    public override bool ShouldTrigger(int comboCount)
    {
        if (comboCount < startCombo)
            return false;

        if(triggerOnlyAtIndex)
            return (comboCount - startCombo) % comboInterval == 0;
        else
            return comboCount >= startCombo;
    }

    public override void Execute(int comboCount)
    {
        int currentStep = (int)(comboCount - startCombo) / comboInterval;
        
        float pitch = Mathf.Clamp(
            1 + currentStep * pitchStep,
            minPitch,
            maxPitch
        );

        float vol = Mathf.Clamp(
            1 + currentStep * volumeStep,
            0f,
            maxVolume
        );

        AudioHelpers.PlaySoundEffect(soundEffect, Camera.main.transform.position, pitch, vol);
    }
}