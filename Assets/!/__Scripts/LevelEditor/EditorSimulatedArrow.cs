using System.Collections;
using UnityEngine;

public class EditorSimulatedArrow : MonoBehaviour
{
    public ArrowEvent data;

    private Vector2 startPos;
    private Vector2 endPos;
    private bool isActive = false;
    private bool arrivalSoundPlayed = false;
    private float lastEditorTime = float.NegativeInfinity;


    public void Init(ArrowEvent evt, float spawnDistance)
    {
        data = evt;

        Vector2 dir = evt.direction;
        if (dir == Vector2.zero)
            dir = Vector2.up;

        dir.Normalize();

        startPos = dir * spawnDistance;
        endPos = Vector2.zero;

        arrivalSoundPlayed = false;
        isActive = true;
    }


    public void Simulate(float editorTime)
    {
        if (!isActive)
            return;

        float prevEditorTime = lastEditorTime;
        lastEditorTime = editorTime;

        bool timeWentBackwards = editorTime < prevEditorTime;

        // ---------------------------
        // Validate timing
        // ---------------------------
        if (float.IsNaN(data.spawnTime) || float.IsNaN(data.arrivalTime) ||
            data.arrivalTime <= data.spawnTime)
        {
            transform.position = endPos;
            gameObject.SetActive(false);
            arrivalSoundPlayed = false;
            return;
        }

        // ---------------------------
        // Before spawn
        // ---------------------------
        if (editorTime < data.spawnTime)
        {
            gameObject.SetActive(false);
            arrivalSoundPlayed = false;
            return;
        }

        // ---------------------------
        // Arrival crossing detection
        // ---------------------------
        bool crossedArrivalThisFrame =
            !arrivalSoundPlayed &&
            !timeWentBackwards &&
            prevEditorTime < data.arrivalTime &&
            editorTime >= data.arrivalTime;

        // ---------------------------
        // After arrival
        // ---------------------------
        if (editorTime >= data.arrivalTime)
        {
            transform.position = endPos;
            gameObject.SetActive(false);

            if (crossedArrivalThisFrame && !EditorPlaybackController.Instance.SuppressSimulationAudio)
            {
                arrivalSoundPlayed = true;
                AudioSettingsManager.Instance.PlayArrowHitSound();
            }

            return;
        }

        // ---------------------------
        // In flight
        // ---------------------------
        arrivalSoundPlayed = false;
        gameObject.SetActive(true);

        float t = Mathf.InverseLerp(data.spawnTime, data.arrivalTime, editorTime);
        t = Mathf.Clamp01(t);

        transform.position = Vector2.Lerp(startPos, endPos, t);
    }






    
    void OnGUI()
    {
        if (data == null || Camera.main == null) return;

        Vector3 screen = Camera.main.WorldToScreenPoint(transform.position);
        if (screen.z < 0) return;

        const float width = 220f;
        const float height = 70f;

        // Convert to GUI space (top-left origin)
        float x = screen.x - width * 0.5f;
        float y = Screen.height - screen.y - height * 0.5f;

        GUI.Label(
            new Rect(x, y, width, height),
            $"t: {EditorPlaybackController.Instance.CurrentTime:F3}\n" +
            $"spawn: {data.spawnTime:F3}\n" +
            $"hit: {data.arrivalTime:F3}\n" +
            $"Δ: {(EditorPlaybackController.Instance.CurrentTime - data.arrivalTime):F3}"
        );
    }
 


    void OnDrawGizmos()
    {
        if (data == null) return;

        Gizmos.color = Color.green;   // hit point
        Gizmos.DrawSphere(endPos, 0.1f);

        Gizmos.color = Color.red;     // spawn point
        Gizmos.DrawSphere(startPos, 0.1f);

        Gizmos.color = Color.yellow;  // current arrow pos
        Gizmos.DrawSphere(transform.position, 0.08f);
    }

}
