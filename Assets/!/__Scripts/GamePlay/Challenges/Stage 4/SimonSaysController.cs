using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class SimonRound
{
    public int sequenceLength = 3;
    public float timerLength = 5f;
}


public class SimonSaysController : MonoBehaviour
{
    public GameObject instructionCanvasPrefab;

    public List<SimonRound> rounds = new List<SimonRound>();
    public GameObject timerRingPrefab;
    public float timerVerticalOffset = 400f;
    public float timerScale = 0.7f;
    public float fadeInDuration = 0.5f;
    public float afterFadeInDelay = 0.5f;
    public SimonLight lightUp;
    public SimonLight lightDown;
    public SimonLight lightLeft;
    public SimonLight lightRight;

    public AudioClip successInput;
    public AudioClip beepSound;
    public AudioClip successSound;
    public AudioClip failSound;
    public float successVolume = 1.0f;

    [Header("Instruction Message Settings")]
    public float messageDuration = 2.0f;   
    public string displayMessage = "REPEAT"; 

    private Dictionary<Vector2, SimonLight> lightMap;
    private List<Vector2> currentSequence = new();
    private int playerProgress;
    private int currentRound = 0;
    private bool inputEnabled = false;

    private Dictionary<Vector2, float> pitchMap;


    AudioSource audioSource;
    private GameObject timerRingInstance;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        lightMap = new Dictionary<Vector2, SimonLight>
        {
            { Vector2.up, lightUp },
            { Vector2.down, lightDown },
            { Vector2.left, lightLeft },
            { Vector2.right, lightRight }
        };

        pitchMap = new Dictionary<Vector2, float>
        {
            { Vector2.up, 1.0f },
            { Vector2.down, 0.8f },
            { Vector2.left, 0.6f },
            { Vector2.right, 1.2f }
        };

        timerRingInstance = Instantiate(timerRingPrefab, transform.position, Quaternion.identity);
    }

    private void OnEnable()
    {
        Player.OnJumped += HandlePlayerInput;
    }

    private void OnDisable()
    {
        Player.OnJumped -= HandlePlayerInput;
    }


    void Start()
    {
        StartCoroutine(StartSimonGame());
        ObstacleManager.Instance.RegisterObstacle(gameObject);
    }

    IEnumerator StartSimonGame()
    {
        yield return StartCoroutine(FadeInAllLights());

        for (currentRound = 0; currentRound < rounds.Count; currentRound++)
        {
            SimonRound round = rounds[currentRound];
            yield return StartCoroutine(PlayRound(round.sequenceLength, round.timerLength));
        }

        yield return StartCoroutine(DestroySequence());

        ObstacleManager.Instance.UnregisterObstacle(gameObject);

        Debug.Log("Simon Game Complete!");

        Destroy(timerRingInstance);
        Destroy(gameObject,1.0f);
    }

    private IEnumerator ShowInstructionMessage(string message = null)
    {
        bool finished = false;

        var canvas = Instantiate(instructionCanvasPrefab);
        canvas.GetComponent<InstructionCanvas>()
            .ShowMessage(message ?? displayMessage, messageDuration,() => finished = true);

        // Wait here until canvas calls callback
        while (!finished)
            yield return null;
    }


    IEnumerator FadeInAllLights()
    {
        int counter = 0;
        foreach (var light in lightMap.Values)
        {
            StartCoroutine(light.FadeIn(fadeInDuration, counter));
            counter ++;
            yield return new WaitForSeconds(0.3f);
        }
        yield return new WaitForSeconds(afterFadeInDelay);
    }

    IEnumerator PlayRound(int sequenceLength, float roundTimeLimit)
    {
        GenerateSequence(sequenceLength);

        StartCoroutine(ShowInstructionMessage("MEMORIZE"));

        yield return StartCoroutine(PlaySequence());

        timerRingInstance?.GetComponent<BasicFillBar>().Show(roundTimeLimit,() => 
        {
            if (inputEnabled)
            {
                StartCoroutine(HandleFailure());
            }
        }, 
        new Vector2(0, timerVerticalOffset));

        // ✨ NEW — show instruction message before sequences
        StartCoroutine(ShowInstructionMessage("REPEAT"));

        inputEnabled = true;
        playerProgress = 0;

        while (playerProgress < sequenceLength && inputEnabled)
            yield return null;

        yield return new WaitForSeconds(2.0f);
    }

    void GenerateSequence(int length)
    {
        currentSequence.Clear();
        for (int i = 0; i < length; i++)
        {
            Vector2 randomDir = new List<Vector2> { Vector2.up, Vector2.down, Vector2.left, Vector2.right }[Random.Range(0, 4)];
            currentSequence.Add(randomDir);
        }
    }

    IEnumerator DestroySequence()
    {
        foreach (SimonLight light in lightMap.Values)
        {
            light.PlayDestroyEffect();
            yield return new WaitForSeconds(0.2f);
        }
    }
    

    IEnumerator PlaySequence()
    {
        foreach (Vector2 dir in currentSequence)
        {
            yield return lightMap[dir].StartCoroutine(lightMap[dir].Glow());

            float pitch = pitchMap[dir];
             AudioHelpers.PlayMyClipAtPoint(
                beepSound, 
                AudioChannel.SFX, 
                Camera.main.transform.position, 
                pitch: pitch
            );

            yield return new WaitForSeconds(0.4f);
        }
    }

    void HandlePlayerInput(Vector2 inputDir)
    {
        if (!inputEnabled || playerProgress >= currentSequence.Count)
            return;

        if (inputDir == currentSequence[playerProgress])
        {
            float pitch = pitchMap[inputDir];
             AudioHelpers.PlayMyClipAtPoint(
                beepSound, 
                AudioChannel.SFX, 
                Camera.main.transform.position, 
                pitch: pitch
            );
            //audioSource.PlayOneShot(beepSound);
            playerProgress++;
            lightMap[inputDir].PlayGlowRotate();

            if (playerProgress == currentSequence.Count)
                StartCoroutine(HandleSuccess());
        }
        else
        {
            StartCoroutine(HandleFailure());
        }
    }

    IEnumerator HandleSuccess()
    {
        inputEnabled = false;
        AudioHelpers.PlayClipWithVariation(successSound, AudioChannel.SFX, Camera.main.transform.position, volume: successVolume, pitchRange: 0.1f);//

        timerRingInstance?.GetComponent<BasicFillBar>().HideImmediate();

        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator HandleFailure()
    {
        List<Vector2> range = new List<Vector2> { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        foreach (Vector2 dir in range)
        {
            lightMap[dir].StartCoroutine(lightMap[dir].GlowFail());
        }

        timerRingInstance?.GetComponent<BasicFillBar>().HideImmediate();
        
        inputEnabled = false;
        AudioHelpers.PlayClipWithVariation(failSound, AudioChannel.SFX, Camera.main.transform.position, pitchRange: 0.1f);//
        //Player.Instance.DamageSelf(1); // or your damage system
        yield return new WaitForSeconds(0.5f);
    }
}
