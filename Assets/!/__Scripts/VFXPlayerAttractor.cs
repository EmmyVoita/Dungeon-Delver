using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(VisualEffect))]
public class VFXPlayerAttractor : MonoBehaviour
{
    [SerializeField] private string attractorPropertyName = "AttractorPosition";
    [SerializeField] private string effectPropertyName = "EffectPosition";

    private VisualEffect _vfx;

    private void Awake()
    {
        _vfx = GetComponent<VisualEffect>();
    }

    private void Update()
    {
        if (Player.Instance == null)
            return;

        _vfx.SetVector3(
            attractorPropertyName,
            Player.Instance.transform.position
        );

        _vfx.SetVector3(
            effectPropertyName,
            transform.position
        );
    }
}