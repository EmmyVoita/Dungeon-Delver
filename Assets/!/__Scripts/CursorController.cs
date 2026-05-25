using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CursorController : MonoBehaviour
{
    public static CursorController Instance { get; private set; }

    [SerializeField] private RectTransform cursorRect;
    [SerializeField] private RectTransform cursorVisibleRect;

    [Header("Settings")]
    [SerializeField] private float followSpeed = 25f;
    [SerializeField] private bool smoothFollow = true;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Camera gameCamera;

    //[SerializeField] private Vector2 offset;

    private Vector2 velocity;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        //Cursor.visible = false;
    }

    /*
    private void Update()
    {
        Vector2 localPoint;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            Input.mousePosition,
            canvas.worldCamera,
            out localPoint
        );

        if (smoothFollow)
        {
            cursorRect.localPosition = Vector2.SmoothDamp(
                cursorRect.localPosition,
                localPoint,
                ref velocity,
                1f / followSpeed
            );
        }
        else
        {
            cursorRect.localPosition = localPoint;
            cursorVisibleRect.localPosition = localPoint;
        }
    }
    */
    
    private void Update()
    {
        
        //Where on the screen is the camera rendered in pixel coordinates.
        //r.xMin => 360
        //r.xMax => 1560
        //r.yMin => 0
        //r.yMax => 1080

        Rect viewport = gameCamera.pixelRect;


        // The current mouse position in pixel coordinates.
        // The bottom-left of the screen or window is at (0, 0). 
        // The top-right of the screen or window is at (Screen.width, Screen.height).
        // Default resolution this will be (0,0) => (1920,1080)

        Vector2 adjustedMouse = Input.mousePosition;

        // We subtract the Input.mousposition is relative to the full screen and the
        // game camera only renders to part of the screen.
        // we subtract to convert it from full screen space to game camera space.

        /*

        0                                               1920
        |-------------------------------------------------|

        |----360px----|------GAME CAMERA------|---360px---|
                        ^ mouse at x = 960

        x = 960 in full screen space
        but in game space it is = 960 - 360 which is 600    

        */

        //adjustedMouse.x -= viewport.x;
        //adjustedMouse.y -= viewport.y;

    
        Vector2 localPoint;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            adjustedMouse,
            canvas.worldCamera,
            out localPoint
        );

        //localPoint += offset;

        if (smoothFollow)
        {
            cursorRect.anchoredPosition = Vector2.SmoothDamp(
                cursorRect.anchoredPosition,
                localPoint,
                ref velocity,
                1f / followSpeed
            );
        }
        else
        {
            cursorRect.localPosition = localPoint;
            cursorVisibleRect.localPosition = localPoint;
        }
    }
    

    public void ShowCursor(bool show)
    {
        cursorRect.gameObject.SetActive(show);
        cursorVisibleRect.gameObject.SetActive(show);
    }

    public void ClickAnimation()
    {
        cursorRect.DOKill();

        cursorRect.localScale = Vector3.one;

        cursorRect
            .DOScale(0.8f, .06f)
            .OnComplete(() =>
            {
                cursorRect.DOScale(1f,.1f);
            });
    }

    public void HoverAnimation()
    {
        cursorRect.DOKill();

        cursorRect.DOScale(1.2f,.15f);
    }

    public void ExitHover()
    {
        cursorRect.DOKill();

        cursorRect.DOScale(1f,.15f);
    }
}