using UnityEngine;

public class BackgroundVisualManager : MonoBehaviour
{
    [Header("Textures")]
    public Texture2D defaultBackground;
    public Texture2D worldMapBackground;
    public Texture2D gameOverBackground;

    [Header("Colors")]
    public Color defaultColor;
    public Color worldMapColor;
    public Color gameOverColor;

    public Material mat;
    public string texturePropertyName = "_MainTexture";
    public string colorPropertyName = "_Color";

    private void OnEnable()
    {
        SetTexture(defaultBackground);
        SetColor(defaultColor);
        
        GameStateManager.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(GameState previousState, GameState newState)
    {
        switch(newState)
        {
            case GameState.WorldMapView:
                SetTexture(worldMapBackground);
                SetColor(worldMapColor);
                break;
            case GameState.WorldMapViewEnd:
                SetTexture(worldMapBackground);
                SetColor(worldMapColor);
                break;
            case GameState.GameOverTally:
                SetTexture(gameOverBackground);
                SetColor(gameOverColor);
                break;
            case GameState.GameOverResults:
                SetTexture(gameOverBackground);
                SetColor(gameOverColor);
                break;
            default:
                SetTexture(defaultBackground);
                SetColor(defaultColor);
                break;
        }
    }

    private void SetTexture(Texture2D texture)
    {
        mat.SetTexture(texturePropertyName, texture);
    }

    private void SetColor(Color color)
    {
        mat.SetColor(colorPropertyName, color);
    }

    private void OnDestroy()
    {
        SetTexture(defaultBackground);
        SetColor(defaultColor);
    }
}
