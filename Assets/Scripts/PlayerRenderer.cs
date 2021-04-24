using UnityEngine;

public class PlayerRenderer : MonoBehaviour
{
    [SerializeField] private float redrawTime = 0.1f;
    [SerializeField] private float drawJitter;
    [SerializeField] private Gradient playerGradient = new Gradient();
    [SerializeField] private Vector2Int playerSize;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameRenderer gameRenderer; // Just to make sure the PPU on the rendered texture matches

    // Private vars used fo rendering.
    private Texture2D texture;
    private Color[] colors;
    private float nextDraw;

    // Private vars used to maintain game state.
    private Vector2Int playerOrigin; // Bottom left corner of player

    private float ppu;
    public float PPU => ppu;

    void Start()
    {
        ppu = gameRenderer.PPU;

        // Init texture
        texture = new Texture2D(playerSize.x, playerSize.y);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Point;

        // Init texture colors
        colors = new Color[playerSize.x * playerSize.y];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Color.magenta;
        }

        // First paint
        texture.SetPixels(0, 0, playerSize.x, playerSize.y, colors);
        texture.Apply();

        // Render texture
        Rect rect = new Rect(0, 0, playerSize.x, playerSize.y);
        spriteRenderer.sprite = Sprite.Create(texture, rect, new Vector2(.5f, 0f), ppu);
    }

    void Update()
    {
        DrawPlayer();
    }

    void LateUpdate()
    {
        if (Time.time > nextDraw)
        {
            ApplyColors();
            nextDraw = Time.time + redrawTime;
        }
    }

    private void DrawPlayer()
    {
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = RandomFromGradient(playerGradient);
        }
    }

    private Color RandomFromGradient(Gradient gradient)
    {
        return gradient.Evaluate(Random.Range(0f, 1f));
    }

    private int Pixel(int x, int y)
    {
        return (y * playerSize.x) + x;
    }

    // private int[] PixelSquare(int x, int y, int width, int height)
    // {
    //     int[] pixels = new int[width * height];
    //     for (int i = 0; i < width; i++)
    //     {
    //         for (int j = 0; j < height; j++)
    //         {
    //             pixels[(j * width) + i] = Pixel(x + i, y + j);
    //         }
    //     }
    //     return pixels;
    // }

    private void ApplyColors()
    {
        texture.SetPixels(0, 0, playerSize.x, playerSize.y, colors);
        texture.Apply();
    }
}
