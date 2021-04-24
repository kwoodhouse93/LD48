using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
public class TerrainGenerator : MonoBehaviour
{
    [Header("Dimensions")]
    [SerializeField] private int height;
    [SerializeField] private float scale;
    [SerializeField] private float offset;

    [Header("Left Terrain")]
    [SerializeField] private Wave[] leftWaves;

    [Header("Right Terrain")]
    [SerializeField] private Wave[] rightWaves;

    [Header("Debug Render")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private int renderWidth;

    [Header("Ledge Generation")]
    [SerializeField] private GameObject ledge;
    [SerializeField] private float minLedgeDist;
    [SerializeField] private float maxLedgeDist;
    [SerializeField] private float minLedgeX;
    [SerializeField] private float maxLedgeX;
    [SerializeField] private float maxLedgeDepth;

    void Start()
    {
        GenerateLedges();
    }

    public void Generate()
    {
        float[] leftMap = NoiseGenerator.Generate(height, scale, leftWaves, 0);
        float[] rightMap = NoiseGenerator.Generate(height, scale, rightWaves, 0);
        GenerateCollider(leftMap, rightMap);

        if (spriteRenderer != null)
        {
            Texture2D texture = new Texture2D(renderWidth, height);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Point;

            Color[] colors = new Color[renderWidth * height];
            for (int x = 0; x < renderWidth; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Color c = RenderPixel(x, y, leftMap, rightMap);
                    if (c == Color.red)
                        colors[y * renderWidth + x] = c;
                }
            }

            texture.SetPixels(0, 0, renderWidth, height, colors);
            texture.Apply();

            Rect rect = new Rect(0, 0, renderWidth, height);
            spriteRenderer.sharedMaterial.mainTexture = texture;
            spriteRenderer.sprite = Sprite.Create(texture, rect, new Vector2(.5f, 1f), 10);
        }
    }

    private Color RenderPixel(int x, int y, float[] leftMap, float[] rightMap)
    {
        float noise;
        if (x <= renderWidth / 2)
        {
            noise = leftMap[y] * renderWidth / 2;
            if (noise > x) return Color.red;
            return Color.cyan;
        }
        else
        {
            noise = rightMap[y] * renderWidth / 2 + renderWidth / 2;
            if (noise < x) return Color.red;
            return Color.cyan;
        }
    }

    private void GenerateCollider(float[] leftMap, float[] rightMap)
    {
        PolygonCollider2D collider = gameObject.GetComponent<PolygonCollider2D>();
        collider.pathCount = 2;

        // Left half
        Vector2[] points = new Vector2[leftMap.Length + 3];
        for (int i = 0; i < leftMap.Length; i++)
        {
            points[i] = new Vector2(leftMap[i] * renderWidth / 2, i);
        }
        points[leftMap.Length] = new Vector2(points[leftMap.Length - 1].x, leftMap.Length);
        points[leftMap.Length + 1] = new Vector2(0, leftMap.Length);
        points[leftMap.Length + 2] = new Vector2(0, 0);

        for (int i = 0; i < points.Length; i++)
        {
            float ppu = spriteRenderer.sprite.pixelsPerUnit;

            points[i] += new Vector2(-renderWidth / 2, -height);
            points[i] *= 1 / ppu;
        }
        collider.SetPath(0, points);

        // Right half
        points = new Vector2[rightMap.Length + 3];
        for (int i = 0; i < rightMap.Length; i++)
        {
            points[i] = new Vector2(rightMap[i] * renderWidth / 2 + renderWidth / 2, i);
        }
        points[rightMap.Length] = new Vector2(points[rightMap.Length - 1].x, rightMap.Length);
        points[rightMap.Length + 1] = new Vector2(renderWidth, rightMap.Length);
        points[rightMap.Length + 2] = new Vector2(renderWidth, 0);

        for (int i = 0; i < points.Length; i++)
        {
            float ppu = spriteRenderer.sprite.pixelsPerUnit;

            points[i] += new Vector2(-renderWidth / 2, -height);
            points[i] *= 1 / ppu;
        }
        collider.SetPath(1, points);
    }

    private void GenerateLedges()
    {
        float y = -5f;
        do
        {
            Object.Instantiate(
                ledge,
                new Vector3(Random.Range(minLedgeX, maxLedgeX), Random.Range(y, y + (maxLedgeDist - minLedgeDist))),
                Quaternion.AngleAxis(Random.Range(-10f, 10f), Vector3.forward),
                transform
            );
            y -= minLedgeDist;
        } while (y < maxLedgeDepth);
    }
}
