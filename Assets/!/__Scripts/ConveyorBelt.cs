using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using DG.Tweening;
using Unity.VisualScripting;

[System.Serializable] public struct BeltItemData 
{ 
    public string id; 
    public Sprite sprite; 
}

public class BeltTileMarker : MonoBehaviour
{
    public Vector3 baseLocalPos;
}


public class ConveyorBelt : MonoBehaviour
{
    [Header("Belt Visuals")]
    public SpriteRenderer[] additionalSprites;
    public Sprite beltTileSprite;       // 16x16 tile
    public SortingLayerPicker converyorSortingLayer;   
    public SortingLayerPicker maskSortingLayer;   
    
    public int tileCount = 12;          // how many tiles to generate
    public float tileSize = 1f;         // world space size of each tile
    public bool vertical = false;       // false = horizontal belt


  
    private ConveyorItem lastItem;


    [Header("Movement / Lifetime")]
    public float destroyDistance = 20f;   // how far items should go before disappearing


    [Header("Belt Settings")]
    public Transform spawnPoint;
    public Vector2 moveDirection;
    public float itemSpeed = 4f;
    public float spacingDistance = 1.5f;  // tweak to taste
    public float spawnInterval = 0.8f;

    [Header("Mask Settings")]
    public bool createMask = true;
    public Sprite maskSprite;     // This should be a simple 1×1 white square sprite
    public float maskThickness = 1.5f; // height for horizontal belts, width for vertical
    private SpriteMask beltMask;



    [Header("Item Data List (unique per belt)")]
    public BeltItemData[] items;
    [HideInInspector] public BeltItemData correctItem;

    [Header("Generic item prefab")]
    public ConveyorItem itemPrefab;

    [Header("Conveyor Animation")]
    public bool flipDirection = true;
    public bool animateBelt = true;
    public int animationFrames = 3;     // number of steps in cycle
    public float frameTime = 0.1f;      // time per step

    [Header("Spawn Fairness")]
    [Tooltip("Every X items, force a correct item (0 = disabled)")]
    [SerializeField] private int forceCorrectEvery = 5;

    private int itemsSpawnedSinceCorrect = 0;



    private bool running = false;

    // 🔹 Track spawned items
    public List<ConveyorItem> activeItems = new List<ConveyorItem>();
    private List<GameObject> beltTiles = new List<GameObject>();


    private float animTimer = 0f;
    private int animFrame = 0;
    private Vector3 beltDir;
    private GameObject tempItemContainer;
    private GameObject tempBeltContainer;


    void Awake()
    {
        correctItem = items[Random.Range(0, items.Length)];
        
    }

    void Start()
    {
        tempItemContainer = new GameObject("TempItemContainer");
        tempItemContainer.transform.SetParent(transform, false);

        tempBeltContainer = new GameObject("TempBeltContainer");
        tempBeltContainer.transform.SetParent(transform, false);

        foreach (SpriteRenderer sprite in additionalSprites)
        {
            if(sprite != null)
            sprite.color = new Color(1f, 1f, 1f, 0f);   
        }
        BuildBeltVisual();
        beltDir = (moveDirection == Vector2.zero ? Vector3.right : (Vector3)moveDirection.normalized);
        BuildMask();
    }

    void Update()
    {
        if (!animateBelt || beltTiles.Count == 0)
            return;

        animTimer += Time.deltaTime;

        if (animTimer >= frameTime)
        {
            animTimer -= frameTime;

            animFrame++;
            if (animFrame >= animationFrames)
                animFrame = 0;

            ApplyAnimationOffset();
        }
    }

    private void ApplyAnimationOffset()
    {
        float normalizedStep = (float)animFrame / animationFrames;
        float shiftAmount = tileSize * normalizedStep; // tileSize comes from your belt builder

        Vector3 offset = flipDirection ? -beltDir * shiftAmount : beltDir * shiftAmount;

        foreach (var tile in beltTiles)
        {
            if (tile != null)
            {
                var marker = tile.GetComponent<BeltTileMarker>();
                tile.transform.localPosition = marker.baseLocalPos + offset;
            }
        }
    }


    public void Begin()
    {
        running = true;
        StartCoroutine(SpawnLoop());
    }

    public void Stop()
    {
        running = false;
        foreach (var item in activeItems)
        {
            item.Disable();
        }
    }

    IEnumerator SpawnLoop()
    {
        while (running)
        {
            if (lastItem == null ||
                Vector3.Distance(lastItem.transform.position, spawnPoint.position) >= spacingDistance)
            {
                SpawnItem();
            }

            yield return null; // check every frame
        }
    }


    void SpawnItem()
    {
        BeltItemData chosen;

        bool shouldForceCorrect =
            forceCorrectEvery > 0 &&
            itemsSpawnedSinceCorrect >= forceCorrectEvery - 1;

        if (shouldForceCorrect)
        {
            chosen = correctItem;
            itemsSpawnedSinceCorrect = 0;
        }
        else
        {
            chosen = items[Random.Range(0, items.Length)];

            // If we randomly got the correct item, reset
            if (chosen.id == correctItem.id)
                itemsSpawnedSinceCorrect = 0;
            else
                itemsSpawnedSinceCorrect++;
        }

        ConveyorItem obj = Instantiate(itemPrefab, spawnPoint.position, Quaternion.identity);
        obj.transform.SetParent(tempItemContainer.transform, true);

        bool isCorrect = (chosen.id == correctItem.id);

        obj.Init(
            chosen.id,
            chosen.sprite,
            isCorrect,
            moveDirection.normalized,
            itemSpeed,
            this,
            destroyDistance
        );

        activeItems.Add(obj);
        lastItem = obj;
    }



    // 🔥 Stop belt movement smoothly
    public void SlowStop(float duration = 0.5f)
    {
        running = false;  // no more spawning

        foreach (var item in activeItems)
        {
            if (item != null)
                item.SlowToStop(duration);
        }
    }

    // Clean out destroyed items
    public void DeregisterItem(ConveyorItem item)
    {
        activeItems.Remove(item);
    }

    public void TemporaryBoost(float amount, float duration)
    {
        // Increase item speed & spawn rate
        float originalSpeed = itemSpeed;
        float originalInterval = spawnInterval;

        itemSpeed *= amount;
        spawnInterval /= amount;

        foreach (var item in activeItems)
            if (item != null)
                item.speed *= amount;

        // Reset after duration
        StartCoroutine(ResetBoost(originalSpeed, originalInterval, duration));
    }

    IEnumerator ResetBoost(float originalSpeed, float originalInterval, float duration)
    {
        yield return new WaitForSeconds(duration);

        itemSpeed = originalSpeed;
        spawnInterval = originalInterval;

        foreach (var item in activeItems)
            if (item != null)
                item.speed = originalSpeed;
    }

    public void KillAllItems()
    {
        running = false;

        // Make a copy so removing items doesn't break the loop
        var copy = new List<ConveyorItem>(activeItems);

        foreach (var item in copy)
        {
            if (item != null)
                item.ForceDestroy();
        }

        activeItems.Clear();
    }


    public void FadeOutSprites(float fadeTime = 0.3f)
    {
        SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>();

        foreach (var sr in sprites)
        {
            if (sr != null)
            {
                sr.DOKill(); // stop any belt animation tweens
                sr.DOColor(new Color(sr.color.r, sr.color.g, sr.color.b, 0f), fadeTime)
                .SetEase(Ease.OutSine);
            }
        }

        foreach (SpriteRenderer sr in additionalSprites)
        {
            sr.DOKill(); // stop any belt animation tweens
            sr.DOColor(new Color(sr.color.r, sr.color.g, sr.color.b, 0f), fadeTime)
            .SetEase(Ease.OutSine); 
        }
    }

    public void FadeInSprites(float duration = 0.5f)
    {
        SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>();

        foreach (var sr in sprites)
        {
            Color c = sr.color;
            sr.color = new Color(c.r, c.g, c.b, 0f);
            sr.DOFade(1f, duration);
        }

        foreach (SpriteRenderer sr in additionalSprites)
        {
            Color c = sr.color;
            sr.color = new Color(c.r, c.g, c.b, 0f);
            sr.DOFade(1f, duration);
        }
    }


    public void BuildBeltVisual()
    {
        // Clear old tiles
        foreach (var tile in beltTiles)
            if (tile != null) Destroy(tile);
        beltTiles.Clear();

        if (beltTileSprite == null)
        {
            Debug.LogWarning("No beltTileSprite assigned!");
            return;
        }

        // Normalize moveDirection (avoid zero vector)
        Vector3 dir = (moveDirection == Vector2.zero ? Vector3.right : (Vector3)moveDirection.normalized);

        bool isHorizontal = Mathf.Abs(dir.x) > Mathf.Abs(dir.y);

        for (int i = 0; i < tileCount; i++)
        {
            GameObject tile = new GameObject($"BeltTile_{i}");
            tile.transform.SetParent(tempBeltContainer.transform, false);

            // Position along direction
            tile.transform.localPosition = spawnPoint.localPosition + dir * (i * tileSize);

            // Add sprite renderer
            SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();
            sr.sprite = beltTileSprite;
            sr.sortingLayerID = converyorSortingLayer.layerID;
            sr.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;   
            sr.color = new Color(1f, 1f, 1f, 0f);

            var marker = tile.AddComponent<BeltTileMarker>();
            marker.baseLocalPos = tile.transform.localPosition;


            // ---- Apply rotation ----
            if (isHorizontal)
            {
                // Horizontal belts → rotate 90 degrees
                tile.transform.localRotation = Quaternion.Euler(0, 0, 90f);
            }
            else
            {
                // Vertical belts → no rotation
                tile.transform.localRotation = Quaternion.identity;
            }

            // OPTIONAL: for diagonal movement, rotate to face the direction:
            // tile.transform.right = dir;

            beltTiles.Add(tile);
        }
    }

    private void BuildMask()
    {
        if (!createMask) return;

        if (maskSprite == null)
        {
            Debug.LogWarning("No mask sprite assigned!");
            return;
        }

        float totalLength = (tileCount - 1) * tileSize;

        GameObject maskObj = new GameObject("ConveyorMask");
        maskObj.transform.SetParent(transform, false);

        beltMask = maskObj.AddComponent<SpriteMask>();
        beltMask.sprite = maskSprite;
        beltMask.isCustomRangeActive = true;
        beltMask.backSortingLayerID = maskSortingLayer.layerID;
        beltMask.frontSortingLayerID = maskSortingLayer.layerID;

        // Match sorting ranges so tiles get masked correctly
        beltMask.frontSortingOrder = 200;
        beltMask.backSortingOrder = -200;

        // Decide orientation
        Vector3 dir = moveDirection.normalized;
        bool horizontal = Mathf.Abs(dir.x) > Mathf.Abs(dir.y);

        if (horizontal)
        {
            maskObj.transform.localScale = new Vector3(totalLength, maskThickness,1f);
        }
        else
        {
            maskObj.transform.localScale = new Vector3(totalLength, maskThickness, 1f);
        }

        // Position the mask over the belt
        maskObj.transform.localPosition = spawnPoint.localPosition 
            + (dir * (totalLength * 0.5f - tileSize * 0.5f)); // center along belt

        // Rotate mask to match belt direction
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        maskObj.transform.localEulerAngles = new Vector3(0, 0, angle);
    }


}
