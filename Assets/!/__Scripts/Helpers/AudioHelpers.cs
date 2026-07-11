using UnityEngine;
using UnityEngine.Audio;

public static class AudioHelpers
{

    public const float hpfNeutral = 300;
    public const float lpfNeutral = 20000f;
    public const float midNeutral = 0.5f;
    public const float spatialBlend = 0.3f;

    public static void PlayClipWithVariation(AudioClip clip, AudioChannel audioChannel, Vector3 position, float basePitch = 1f, float pitchRange = 0.1f, float volume = 1f)
    {
        if (!Application.isPlaying) return;
        if (clip == null) return;

        // Create a temporary GameObject with an AudioSource
        GameObject obj = new GameObject("TempAudio");
        AudioSource source = obj.AddComponent<AudioSource>();

        source.clip = clip;
        source.volume = volume * AudioSettingsManager.GetVolume(audioChannel);

        // Randomize pitch around the base pitch
        source.pitch = basePitch * GetPitchOffsetForTimeScale() + Random.Range(-pitchRange, pitchRange);


        source.Play();

        // Destroy after clip finishes
        Object.Destroy(obj, clip.length / source.pitch);
    }


    // Audio Mixer Override
    public static void PlayClipWithVariation(
    AudioClip clip,
    AudioMixerGroup mixerGroup,
    Vector3 position,
    float basePitch = 1f,
    float pitchRange = 0.1f,
    float volume = 1f)
    {
        if (!Application.isPlaying) return;
        if (clip == null) return;

        GameObject obj = new GameObject("TempAudio");
        obj.transform.position = position;

        AudioSource source = obj.AddComponent<AudioSource>();
        source.clip = clip;
        source.outputAudioMixerGroup = mixerGroup; 
        source.volume = volume;
        source.pitch = basePitch + Random.Range(-pitchRange, pitchRange);

        source.Play();
        Object.Destroy(obj, clip.length / Mathf.Max(source.pitch, 0.01f));
    }


    public static void PlayMyClipAtPoint(AudioClip clip, AudioChannel audioChannel, Vector3 position, float volume = 1f, float pitch = 1f)
    {
        if (!Application.isPlaying) return;
        if (clip == null) return;

        float adjustedVolume = volume * AudioSettingsManager.GetVolume(audioChannel);

        GameObject temp = new GameObject("TempAudio_" + clip.name);
        temp.transform.position = position;

        AudioSource source = temp.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = adjustedVolume;
        source.pitch = pitch * GetPitchOffsetForTimeScale();
        source.Play();

        Object.Destroy(temp, clip.length / Mathf.Max(source.pitch, 0.01f)); // destroy after playback
    }

    public static void PlaySoundEffect(SoundEffect sound, Vector3 position, float pitchScalar = 1f, float volumeScalar = 1f)
    {
        if (!Application.isPlaying) return;
        if (!sound.IsValid) return;

        GameObject obj = new GameObject($"TempAudio_{sound.clip.name}");
        obj.transform.position = position;

        AudioSource source = obj.AddComponent<AudioSource>();
        source.clip = sound.clip;

        float volume = sound.volume * AudioSettingsManager.GetVolume(sound.channel);
        source.volume = volume * volumeScalar;

        float pitch = sound.pitch * pitchScalar;

        //if (sound.affectedByTimeScale)
        //    pitch *= GetPitchOffsetForTimeScale();

        pitch += Random.Range(-sound.pitchVariation, sound.pitchVariation);

        source.pitch = Mathf.Max(pitch, 0.01f);

        source.spatialBlend = spatialBlend;

        source.Play();

        Object.Destroy(obj, (sound.clip.length / source.pitch) + 0.1f);
    }


    public static float GetPitchOffsetForTimeScale()
    {
        float t = Mathf.Clamp01(Time.timeScale);
        return Mathf.Lerp(0.8f, 1.0f, t); // pitch from 0.5 (slow) to 1.0 (normal)
    }



    public static void PlayDirectionalArrowHit(
        AudioClip clip,
        Vector3 position,
        Vector2 direction,
        float pitch = 1f,
        float volume = 1f,
        float directionalStrength = .5f)
    {
        float hpfTarget = hpfNeutral;
        float lpfTarget = lpfNeutral;
        float midTarget = midNeutral;

        float angle =
            Mathf.Atan2(
                direction.y,
                direction.x
            ) * Mathf.Rad2Deg;

        if (angle > 45f && angle < 135f)
        {
            hpfTarget = 400f;
            midTarget = 1.5f;
        }
        else if (angle < -45f && angle > -135f)
        {
            lpfTarget = 3000f;
        }
        else if (Mathf.Abs(angle) <= 45f)
        {
            midTarget = 2f;
        }
        else
        {
            midTarget = -3f;
        }

        float hpf = Mathf.Lerp(
            hpfNeutral,
            hpfTarget,
            directionalStrength
        );

        float lpf = Mathf.Lerp(
            lpfNeutral,
            lpfTarget,
            directionalStrength
        );

        float midVolume =
            Mathf.Pow(
                10f,
                Mathf.Lerp(
                    midNeutral,
                    midTarget,
                    directionalStrength
                ) / 20f
            );

        PlayArrowAudio(
            clip,
            position,
            pitch,
            volume * midVolume,
            hpf,
            lpf
        );
    }

    public static void PlayArrowAudio(
        AudioClip clip,
        Vector3 position,
        float pitch,
        float volume,
        float hpf,
        float lpf)
    {
        if (!Application.isPlaying) return;
        if (clip == null)
            return;

        GameObject go = new GameObject("ArrowAudio");

        go.transform.position = position;

        AudioSource source = go.AddComponent<AudioSource>();
        AudioLowPassFilter lpfFilter = go.AddComponent<AudioLowPassFilter>();
        AudioHighPassFilter hpfFilter = go.AddComponent<AudioHighPassFilter>();

        source.clip = clip;
        source.pitch = pitch;
        source.volume = volume;

        source.outputAudioMixerGroup = AudioSettingsManager.Instance.arrowHitsGroup;

        source.spatialBlend = 0f; // 2D sound (important for rhythm games)

        lpfFilter.cutoffFrequency = lpf;
        hpfFilter.cutoffFrequency = hpf;
        //lpfFilter.enabled = false;
        //hpfFilter.enabled = false;

        source.Play();

        Object.Destroy(go, clip.length + 0.1f);
    }

}
