using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }
    private Tween transitionTween;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadScene(string sceneName, ScreenSpaceTransitionData data)
    {
        StartCoroutine(LoadSceneRoutine(sceneName, data));
    }

    private IEnumerator LoadSceneRoutine(string sceneName, ScreenSpaceTransitionData data)
    {
        transitionTween?.Kill();

        data.inMaterial.SetFloat(data.transitionInPropertyName, data.inStartValue);

        transitionTween = data.inMaterial
            .DOFloat(data.inTargetValue, data.transitionInPropertyName, data.transitionInDuration)
            .SetUpdate(true);

        yield return transitionTween.WaitForCompletion();

        SceneManager.LoadScene(sceneName);

        data.inMaterial.SetFloat(data.transitionInPropertyName, data.inStartValue);

        data.outMaterial.SetFloat(data.transitionOutPropertyName, data.outStartValue);

        yield return new WaitForSeconds(data.outStartDelay);

        transitionTween = data.outMaterial
            .DOFloat(data.outTargetValue, data.transitionOutPropertyName, data.transitionOutDuration)
            .SetUpdate(true);
    }
}