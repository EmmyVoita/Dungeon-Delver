using System;

[Serializable]
public abstract class LevelEventData
{
    public float beatTime;

    protected LevelEventData(float beatTime)
    {
        this.beatTime = beatTime;
    }

    public abstract string Serialize();
    public abstract string Describe();
}