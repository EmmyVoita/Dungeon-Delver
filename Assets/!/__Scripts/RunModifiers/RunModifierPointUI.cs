using UnityEngine;
using UnityEngine.UI;

public class RunModifierPointUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image image;

    [Header("Colors")]
    [SerializeField] private Color fillColor;
    [SerializeField] private Color emptyColor;

    private Material _runTimeMaterial;


    private void Awake()
    {
        if (image != null && image.material != null)
        {
            _runTimeMaterial = Instantiate(image.material);
            image.material = _runTimeMaterial;
        }
    }

    public void SetFilled(bool filled)
    {
        image.color = filled ? fillColor : emptyColor;
    }
}
