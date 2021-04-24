using UnityEngine;

public class GameRenderer : MonoBehaviour
{
    const int CANVAS_WIDTH = 80;
    const int CANVAS_HEIGHT = 45;
    const int TOTAL_PIXELS = CANVAS_WIDTH * CANVAS_HEIGHT;

    [SerializeField] private Vector2Int skyCenter = new Vector2Int(CANVAS_WIDTH / 2, CANVAS_HEIGHT / 6);
    [SerializeField] private float redrawTime = 0.1f;
    [SerializeField] private float drawJitter;
    [SerializeField] private Gradient skyGradient = new Gradient();
    [SerializeField] private Gradient playerGradient = new Gradient();
    [SerializeField] private Vector2Int playerSize;

    // Private vars used fo rendering.
    private Texture2D texture;
    private SpriteRenderer spriteRenderer;
    private Color[] colors;
    private float nextDraw;

    // Private vars used to maintain game state.
    private Vector2Int playerOrigin; // Bottom left corner of player

    void Start()
    {
        // Init texture
        texture = new Texture2D(CANVAS_WIDTH, CANVAS_HEIGHT);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Point;

        // Init texture colors
        colors = new Color[TOTAL_PIXELS];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Color.magenta;
        }

        // First paint
        texture.SetPixels(0, 0, CANVAS_WIDTH, CANVAS_HEIGHT, colors);
        texture.Apply();

        // Render texture to cover camera (assumes a fixed aspect of 16:9)
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        Rect rect = new Rect(0, 0, CANVAS_WIDTH, CANVAS_HEIGHT);
        spriteRenderer.sprite = Sprite.Create(texture, rect, Vector2.one * .5f, (CANVAS_HEIGHT / 2) / Camera.main.orthographicSize);
        spriteRenderer.sortingLayerID = SortingLayer.NameToID("BG");

        // Set up gradients for sampling
        // skyGradient.SetKeys(skyGradientColorKeys, skyGradientAlphaKeys);
    }

    void Update()
    {
        DrawSky();
        // DrawPlayer();
    }

    void LateUpdate()
    {
        if (Time.time > nextDraw)
        {
            ApplyColors();
            nextDraw = Time.time + redrawTime;
        }
    }

    public void SetPlayerPosition(Vector2Int pos)
    {
        if (pos.x < 0) pos.x = 0;
        if (pos.x > CANVAS_WIDTH - 1) pos.x = CANVAS_WIDTH - 1;
        if (pos.y < 0) pos.y = 0;
        if (pos.y > CANVAS_HEIGHT - 1) pos.y = CANVAS_HEIGHT - 1;
        playerOrigin = pos;
    }

    private void DrawPlayer()
    {
        foreach (int pixel in PixelSquare(playerOrigin.x, playerOrigin.y, playerSize.x, playerSize.y))
        {
            colors[pixel] = RandomFromGradient(playerGradient);
        }
    }

    private void DrawSky()
    {
        for (int i = 0; i < colors.Length; i++)
        {
            int x = i % CANVAS_WIDTH;
            int y = (i / CANVAS_WIDTH);
            float distFromSkyCenter = DistanceFromPoint(new Vector2Int(x, y), skyCenter);
            colors[i] = RandomLerpGradient(skyGradient, distFromSkyCenter);
        }
    }

    private Color RandomFromGradient(Gradient gradient)
    {
        return gradient.Evaluate(Random.Range(0f, 1f));
    }

    private Color RandomLerpGradient(Gradient gradient, float t)
    {
        float randT = Random.Range(t - drawJitter, t + drawJitter);
        return gradient.Evaluate(randT);
    }

    // Distance of sample from centre of texture.
    // sample is the index of the pixel in the texture to calculate distance from center for.
    // Output is scaled from 0 to 1. 0 being dead center, 0.5 being a corner (1 would be the distance from corner to opposite corner)
    private float DistanceFromCenter(Vector2Int sample)
    {
        return DistanceFromPoint(sample, new Vector2Int(CANVAS_WIDTH / 2, CANVAS_HEIGHT / 2));
    }


    // Distance of sample from origin of texture (bottom left).
    // sample is the index of the pixel in the texture to calculate distance from center for.
    // Output is scaled from 0 to 1. 0 being at the origin, 1 being in the opposite corner.
    private float DistanceFromOrigin(Vector2Int sample)
    {
        return DistanceFromPoint(sample, Vector2Int.zero);
    }

    // Distance of sample from point.
    // Output is scaled from 0 to 1. 0 being at the same pixel, 1 being at opposite corners.
    private float DistanceFromPoint(Vector2Int sample, Vector2Int point)
    {
        int xDist = Mathf.Abs(sample.x - point.x);
        int yDist = Mathf.Abs(sample.y - point.y);
        float dist = Mathf.Sqrt((xDist * xDist) + (yDist * yDist));
        float maxDist = Mathf.Sqrt((CANVAS_WIDTH * CANVAS_WIDTH) + (CANVAS_HEIGHT * CANVAS_HEIGHT));
        return (dist / maxDist);
    }

    private int Pixel(int x, int y)
    {
        return (y * CANVAS_WIDTH) + x;
    }

    private int[] PixelSquare(int x, int y, int width, int height)
    {
        int[] pixels = new int[width * height];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                pixels[(j * width) + i] = Pixel(x + i, y + j);
            }
        }
        return pixels;
    }

    private void ApplyColors()
    {
        texture.SetPixels(0, 0, CANVAS_WIDTH, CANVAS_HEIGHT, colors);
        texture.Apply();
    }
}
