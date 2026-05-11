using System.Runtime.CompilerServices;
using UnityEngine;

public class DisguiseAxeProjectile : MonoBehaviour
{
    [Header("Rotation Settings")]
    public Vector3 rotationSpeed = new Vector3(0f, 0f, 90f); // degrees per second
    public Space rotationSpace = Space.Self; // or Space.World

    [Header("VFX")]
    [SerializeField] private GameObject poofEffect;

    [Header("Audio")]
    [SerializeField] private SoundEffect poofSound;

    private bool _isDisguise = false;
    private bool _rotate = false;
    private bool hasBeenHit = false;

    void Update()
    {
        if(_rotate)
            transform.Rotate(rotationSpeed * Time.deltaTime, rotationSpace);
    }

    public void Initialize(bool isDisguise)
    {
        _isDisguise = isDisguise;
        _rotate = true;
    }

    public void Reveal()
    {
        if(poofEffect)
            Instantiate(poofEffect, transform.position, Quaternion.identity);

        AudioHelpers.PlaySoundEffect(poofSound,transform.position);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasBeenHit) return; // Prevent multiple triggers
        if (!other.CompareTag("Player")) return;

        hasBeenHit = true;

        if(_isDisguise)
            Reveal();
    }
}