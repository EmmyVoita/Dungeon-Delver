
using System;
using UnityEngine;

[Serializable]
public class ArrowEventData : LevelEventData
{
    public Vector2 direction;
    public float speed;
    public string arrowType;

    public ArrowEventData(float beatTime,Vector2 direction,float speed,string arrowType): base(beatTime)
    {
        this.direction = direction;
        this.speed = speed;
        this.arrowType = arrowType;
    }

    public override string Serialize()
    {
        return $"{beatTime},arrow,{DirectionToString(direction)},{speed},{arrowType}";
    }

    public override string Describe()
    {
        return
            $"Arrow @ {beatTime:0.##} beats" +
            $"\nDir: {DirectionToString(direction)}" +
            $"\nSpeed: {speed}" +
            $"\nType: {arrowType}";
    }

    private string DirectionToString(Vector2 dir)
    {
        if (dir == Vector2.up) return "up";
        if (dir == Vector2.down) return "down";
        if (dir == Vector2.left) return "left";
        if (dir == Vector2.right) return "right";

        if (dir == new Vector2(1, 1).normalized) return "up-right";
        if (dir == new Vector2(-1, 1).normalized) return "up-left";
        if (dir == new Vector2(1, -1).normalized) return "down-right";
        if (dir == new Vector2(-1, -1).normalized) return "down-left";

        return "unknown";
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