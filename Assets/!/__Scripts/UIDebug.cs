using UnityEngine;

public class UIDebug : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;

    private void Update()
    {
        Debug.Log(
            $"Screen: {Screen.width} x {Screen.height}\n" +
            $"Camera Rect: {targetCamera.rect}\n" +
            $"Pixel Rect: {targetCamera.pixelRect}\n" +
            $"Pixel Size: {targetCamera.pixelWidth} x {targetCamera.pixelHeight}\n" +
            $"Aspect: {targetCamera.aspect}"
        );
    }
}