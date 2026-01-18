using UnityEngine;
using UnityEngine.Audio;

public static class AudioHelpers
{
    /// <summary>
    /// Play an audio clip at a given position with random pitch variation.
    /// </summary>
    /// <param name="clip">The AudioClip to play.</param>
    /// <param name="position">World position to play sound at.</param>
    /// <param name="basePitch">The center pitch (1 = normal).</param>
    /// <param name="pitchRange">The random offset from base pitch (e.g., 0.1 = ±10%).</param>
    /// <param name="volume">Volume level (default = 1).</param>
    public static void PlayClipWithVariation(AudioClip clip, AudioChannel audioChannel, Vector3 position, float basePitch = 1f, float pitchRange = 0.1f, float volume = 1f)
    {
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

    public static void PlaySoundEffect(SoundEffect sound, Vector3 position, float pitchScalar = 1f)
    {
        if (!sound.IsValid) return;

        GameObject obj = new GameObject($"TempAudio_{sound.clip.name}");
        obj.transform.position = position;

        AudioSource source = obj.AddComponent<AudioSource>();
        source.clip = sound.clip;

        float volume = sound.volume * AudioSettingsManager.GetVolume(sound.channel);
        source.volume = volume;

        float pitch = sound.pitch * pitchScalar;

        if (sound.affectedByTimeScale)
            pitch *= GetPitchOffsetForTimeScale();

        if (sound.usePitchVariation)
            pitch += Random.Range(-sound.pitchVariation, sound.pitchVariation);

        source.pitch = Mathf.Max(pitch, 0.01f);
        source.Play();

        Object.Destroy(obj, sound.clip.length / source.pitch);
    }


    public static float GetPitchOffsetForTimeScale()
    {
        float t = Mathf.Clamp01(Time.timeScale);
        return Mathf.Lerp(0.8f, 1.0f, t); // pitch from 0.5 (slow) to 1.0 (normal)
    }

}
