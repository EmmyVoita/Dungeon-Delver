using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeletingBoss : MonoBehaviour
{
    public int frameInterval = 60; // every 60 frames
    public ParticleSystem deleteEffect;

    private int frameCounter;

    void Update()
    {
        frameCounter++;
        if (frameCounter >= frameInterval)
        {
            frameCounter = 0;
            DeleteRandomArrow();
        }
    }

    void DeleteRandomArrow()
    {
        ArrowBase arrow = ArrowManager.Instance?.GetRandomArrow();
        if (arrow == null) return;

        if (deleteEffect != null)
            Instantiate(deleteEffect, arrow.transform.position, Quaternion.identity);

        Destroy(arrow.gameObject);
        Debug.Log($"🌀 Boss deleted {arrow.name}");
    }
}
