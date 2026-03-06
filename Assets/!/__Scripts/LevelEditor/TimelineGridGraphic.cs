using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class TimelineGridGraphic : MaskableGraphic
{
    [Header("External Settings")]
    public float pixelsPerSecond;
    public float maxTime;

    public bool showQuarterBeats = true;
    public bool showEighthBeats = false;

    [Header("Visuals")]
    public float wholeTickHeight = 80f;
    public float quarterTickHeight = 40f;
    public float eighthTickHeight = 20f;

    public Color wholeColor = new Color(1,1,1,0.5f);
    public Color quarterColor = new Color(1,1,1,0.2f);
    public Color eighthColor = new Color(1,1,1,0.15f);

    protected override void Awake()
    {
        base.Awake();
        maskable = true;
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (pixelsPerSecond <= 0) return;

        if(LevelEditorData.Instance == null) return;    
        float secondsPerBeat = 60f / LevelEditorData.Instance.BPM;

        int beatCount = Mathf.CeilToInt(maxTime / secondsPerBeat);

        for (int b = 0; b <= beatCount; b++)
        {
            float beatTime = b * secondsPerBeat;

            //-----------------------------
            // WHOLE BEAT line
            //-----------------------------
            float xWhole = beatTime * pixelsPerSecond;
            AddVerticalLine(vh, xWhole, wholeTickHeight, wholeColor);

            //-----------------------------
            // QUARTER BEATS
            //-----------------------------
            if (showQuarterBeats)
            {
                float q1 = beatTime + secondsPerBeat * 0.25f;
                float q2 = beatTime + secondsPerBeat * 0.50f;
                float q3 = beatTime + secondsPerBeat * 0.75f;

                if (q1 <= maxTime)
                    AddVerticalLine(vh, q1 * pixelsPerSecond, quarterTickHeight, quarterColor);

                if (q2 <= maxTime)
                    AddVerticalLine(vh, q2 * pixelsPerSecond, quarterTickHeight, quarterColor);

                if (q3 <= maxTime)
                    AddVerticalLine(vh, q3 * pixelsPerSecond, quarterTickHeight, quarterColor);
            }


            //-----------------------------
            // OPTIONAL EIGHTH BEATS
            //-----------------------------
            if (showEighthBeats)
            {
                float e1 = beatTime + secondsPerBeat * 0.125f;
                float e2 = beatTime + secondsPerBeat * 0.375f;
                float e3 = beatTime + secondsPerBeat * 0.625f;
                float e4 = beatTime + secondsPerBeat * 0.875f;

                if (e1 <= maxTime)
                    AddVerticalLine(vh, e1 * pixelsPerSecond, eighthTickHeight, eighthColor);

                if (e2 <= maxTime)
                    AddVerticalLine(vh, e2 * pixelsPerSecond, eighthTickHeight, eighthColor);

                if (e3 <= maxTime)
                    AddVerticalLine(vh, e3 * pixelsPerSecond, eighthTickHeight, eighthColor);

                if (e4 <= maxTime)
                    AddVerticalLine(vh, e4 * pixelsPerSecond, eighthTickHeight, eighthColor);
            }
        }
    }

    void AddVerticalLine(VertexHelper vh, float x, float height, Color color)
    {
        const float width = 2f;

        UIVertex v0 = UIVertex.simpleVert;
        UIVertex v1 = UIVertex.simpleVert;
        UIVertex v2 = UIVertex.simpleVert;
        UIVertex v3 = UIVertex.simpleVert;

        v0.color = v1.color = v2.color = v3.color = color;

        v0.position = new Vector3(x, 0);
        v1.position = new Vector3(x + width, 0);
        v2.position = new Vector3(x + width, height);
        v3.position = new Vector3(x, height);

        int start = vh.currentVertCount;

        vh.AddVert(v0);
        vh.AddVert(v1);
        vh.AddVert(v2);
        vh.AddVert(v3);

        vh.AddTriangle(start, start + 1, start + 2);
        vh.AddTriangle(start, start + 2, start + 3);
    }

    //----------------------------------------------------
    // Call this from Timeline to update grid live
    //----------------------------------------------------
    public void SetProperties(float pps, float max)
    {
        pixelsPerSecond = pps;
        maxTime = max;
        SetVerticesDirty();
    }
}
