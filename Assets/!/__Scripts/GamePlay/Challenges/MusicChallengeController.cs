using UnityEngine;
using DG.Tweening;

public class MusicChallengeController : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] private float fadeOutDuration = 0.25f;
    [SerializeField] private float fadeInDuration = 0.35f;
    [SerializeField] private float fadedVolumeMult = 0.8f;

    private Tween mainTween;

    private void OnEnable()
    {
        ObstacleManager.OnFirstObstacleAppeared += EnterChallenge;
        ObstacleManager.OnAllObstaclesCleared += ExitChallenge;
    }

    private void OnDisable()
    {
        ObstacleManager.OnFirstObstacleAppeared -= EnterChallenge;
        ObstacleManager.OnAllObstaclesCleared -= ExitChallenge;
    }

    private void EnterChallenge()
    {
        var music = MusicManager.Instance;
        if (music == null) return;

        mainTween?.Kill();

        mainTween = DOTween.To(
            () => AudioSettingsManager.Instance.musicVolume,
            v => music.SetMainVolume(v),
            fadedVolumeMult * AudioSettingsManager.Instance.musicVolume,
            fadeOutDuration
        );
    }

    private void ExitChallenge(int damageTaken)
    {
        var music = MusicManager.Instance;
        if (music == null) return;

        mainTween?.Kill();

        mainTween = DOTween.To(
            () => fadedVolumeMult * AudioSettingsManager.Instance.musicVolume,
            v => music.SetMainVolume(v),
            AudioSettingsManager.Instance.musicVolume,
            fadeInDuration
        );
    }
}
