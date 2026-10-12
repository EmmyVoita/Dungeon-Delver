
using System;
using UnityEngine;

[Serializable]
public class ChallengeEventData : LevelEventData
{
    public string obstacleType;

    public ChallengeEventData(float beatTime,string obstacleType) : base(beatTime)
    {
        this.obstacleType = obstacleType;
    }

    public override string Serialize()
    {
        return $"{beatTime},obstacle,0,0,{obstacleType}";
    }

    public override string Describe()
    {
        return
            $"Obstacle @ {beatTime:0.##} beats" +
            $"\nType: {obstacleType}";
    }
}

/*
[Serializable]
public class ArrowEventData : LevelEventData
{
    public float beatTime;        
    public string objectType;       // "arrow" or "obstacle"
    public Vector2 direction; 
    public float speed;       
    public string nameOfGameObjectToSpawn;

    public ArrowEventData(float time, string objectType, Vector2 direction, float speed, string nameOfGameObjectToSpawn)
    {
        this.beatTime = time;
        this.objectType = objectType;
        this.direction = direction;
        this.speed = speed;
        this.nameOfGameObjectToSpawn = nameOfGameObjectToSpawn;
    }

    public override string Describe()
    {
        if (objectType == "arrow")
        {
            return
                $"Arrow @ {beatTime:0.##} beats | " +
                $"Dir: {DirectionToString(direction)} | " +
                $"Speed: {speed} | " +
                $"Type: {nameOfGameObjectToSpawn}";
        }

        if (objectType == "obstacle")
        {
            return
                $"Obstacle @ {beatTime:0.##} beats | " +
                $"Type: {nameOfGameObjectToSpawn}";
        }

        return $"Event @ {beatTime}";
    }

    private string DirectionToString(Vector2 dir)
    {
        if (dir == Vector2.up) return "Up";
        if (dir == Vector2.down) return "Down";
        if (dir == Vector2.left) return "Left";
        if (dir == Vector2.right) return "Right";
        if (dir == new Vector2(1, 1).normalized) return "Up-Right";
        if (dir == new Vector2(-1, 1).normalized) return "Up-Left";
        if (dir == new Vector2(1, -1).normalized) return "Down-Right";
        if (dir == new Vector2(-1, -1).normalized) return "Down-Left";
        return "Unknown";
    }
}*/