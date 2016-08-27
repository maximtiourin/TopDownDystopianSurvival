using UnityEngine;
using System.Collections;

public class DebugLevelOverlay : MonoBehaviour {
    public GameObject levelContainer;
    public Color gridOverlayColor;

    private Level level;

    private GameObject gridOverlay;
    private GameObject regionHover;
    private GameObject chunkHover;

    private static readonly float unit = .194f; //The scale of one single tile when scaling an object

	// Use this for initialization
	void Start () {
        level = levelContainer.GetComponent<Level>();
	}
	
	// Update is called once per frame
	void Update () {
	    if (level.isLoaded()) {
            int lw = level.getWidth();
            int lh = level.getHeight();

            int pixelsPerUnit = Level.pixelsPerUnit;

            if (gridOverlay == null) {
                gridOverlay = new GameObject("GridOverlay");
                gridOverlay.transform.parent = transform;

                gridOverlay.transform.position = new Vector3(0f, 0f, 0f);

                SpriteRenderer rend = gridOverlay.AddComponent<SpriteRenderer>();
                rend.sortingLayerName = "Overlay";

                int resolution = 4;

                int sw = resolution * lw;
                int sh = resolution * lh;

                rend.sprite = Sprite.Create(generateGridOverlayTexture(sw, sh), new Rect(0, 0, sw, sh), new Vector2(0f, 0f));

                float sbx = rend.sprite.bounds.size.x;
                float sby = rend.sprite.bounds.size.y;

                gridOverlay.transform.localScale = new Vector3(lw / sbx, lh / sby, 1f);          
            }
        }
	}

    private Texture2D generateGridOverlayTexture(int width, int height) {
        int lw = level.getWidth();
        int lh = level.getHeight();

        int rw = Level.regionWidth;
        int rh = Level.regionHeight;

        float pixelScaleX = lw / (float) width;
        float pixelScaleY = lh / (float) height;
        float reversePixelScaleX = width / (float) lw;
        float reversePixelScaleY = height / (float) lh;

        Color emptyColor = new Color(gridOverlayColor.r, gridOverlayColor.g, gridOverlayColor.b, 0f);
        Color fillColor = gridOverlayColor;

        Texture2D tex = new Texture2D(width, height);

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                int px = (int) Mathf.Floor(x * pixelScaleX);
                int py = (int) Mathf.Floor(y * pixelScaleY);

                if (px % rw == 0 && x % (int) reversePixelScaleX == 0) {
                    tex.SetPixel(x, y, fillColor);
                }
                else if (py % rh == 0 && y % (int) reversePixelScaleY == 0) {
                    tex.SetPixel(x, y, fillColor);
                }
                else {
                    tex.SetPixel(x, y, emptyColor);
                }
            }
        }

        tex.Apply();

        return tex;
    }
}
