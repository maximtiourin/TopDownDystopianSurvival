using UnityEngine;
using System.Collections;

public class DebugLevelOverlay : MonoBehaviour {
    private static readonly int GRID_OVERLAY_RESOLUTION = 4;
    private static readonly int REGION_OVERLAY_RESOLUTION = 4;
    private static readonly int CHUNK_OVERLAY_RESOLUTION = 4;

    public GameObject levelMouseContainer;
    public GameObject levelContainer;
    public Color gridOverlayColor;
    public Color regionOverlayColor;
    public Color chunkOverlayColor;

    private Script_World_Mouse_LevelInteraction levelMouse;
    private Level level;

    private GameObject gridOverlay;
    private GameObject regionOverlay;
    private GameObject chunkOverlay;

    public bool gridOverlayActive = true;
    public bool regionOverlayActive = true;
    public bool chunkOverlayActive = true;

    public Region region;
    public Chunk chunk;
    public Region cacheRegion;
    public Chunk cacheChunk;

    private SpriteRenderer chunkOverlayRenderer;

	// Use this for initialization
	void Start () {
        levelMouse = levelMouseContainer.GetComponent<Script_World_Mouse_LevelInteraction>();
        level = levelContainer.GetComponent<Level>();
	}
	
	// Update is called once per frame
	void Update () {
	    if (level.isLoaded()) {
            int lw = level.getWidth();
            int lh = level.getHeight();
            int tx = levelMouse.getTileX();
            int ty = levelMouse.getTileY();
            int rw = Level.regionWidth;
            int rh = Level.regionHeight;

            region = level.getRegionAtPosition(tx, ty);

            int rx = level.getInnerRegionXAtPositionX(tx);
            int ry = level.getInnerRegionYAtPositionY(ty);

            chunk = region.getChunkAtNodePosition(rx, ry);

            //Grid Overlay
            if (gridOverlay == null) {
                gridOverlay = new GameObject("GridOverlay");
                gridOverlay.transform.parent = transform;

                gridOverlay.transform.position = new Vector3(0f, 0f, 0f);

                SpriteRenderer rend = gridOverlay.AddComponent<SpriteRenderer>();
                rend.sortingLayerName = "Overlay";
                rend.sortingOrder = 0;

                int resolution = GRID_OVERLAY_RESOLUTION;

                int sw = resolution * lw;
                int sh = resolution * lh;

                rend.sprite = Sprite.Create(generateGridOverlayTexture(sw, sh), new Rect(0, 0, sw, sh), new Vector2(0f, 0f));

                float sbx = rend.sprite.bounds.size.x;
                float sby = rend.sprite.bounds.size.y;

                gridOverlay.transform.localScale = new Vector3(lw / sbx, lh / sby, 1f); //Left hand side of division is the desired dimension

                gridOverlay.SetActive(gridOverlayActive);
            }
            else {
                gridOverlay.SetActive(gridOverlayActive);
            }

            //Region Overlay
            if (regionOverlay == null) {
                regionOverlay = new GameObject("RegionOverlay");
                regionOverlay.transform.parent = transform;

                updateRegionOverlayPosition();

                SpriteRenderer rend = regionOverlay.AddComponent<SpriteRenderer>();
                rend.sortingLayerName = "Overlay";
                rend.sortingOrder = 1;

                int resolution = REGION_OVERLAY_RESOLUTION;

                int sw = resolution * rw;
                int sh = resolution * rh;

                rend.sprite = Sprite.Create(generateRegionOverlayTexture(sw, sh), new Rect(0, 0, sw, sh), new Vector2(0, 0));

                float sbx = rend.sprite.bounds.size.x;
                float sby = rend.sprite.bounds.size.y;

                regionOverlay.transform.localScale = new Vector3(rw / sbx, rh / sby, 1f); //Left hand side of division is the desired dimension

                regionOverlay.SetActive(regionOverlayActive);
            }
            else {
                updateRegionOverlayPosition();

                regionOverlay.SetActive(regionOverlayActive);
            }

            //Chunk Overlay
            if (chunkOverlay == null) {
                chunkOverlay = new GameObject("ChunkOverlay");
                chunkOverlay.transform.parent = transform;

                updateChunkOverlayPosition();

                SpriteRenderer rend = chunkOverlay.AddComponent<SpriteRenderer>();
                chunkOverlayRenderer = rend;
                rend.sortingLayerName = "Overlay";
                rend.sortingOrder = 2;

                int resolution = CHUNK_OVERLAY_RESOLUTION;

                int sw = resolution * rw;
                int sh = resolution * rh;

                rend.sprite = Sprite.Create(generateChunkOverlayTexture(sw, sh), new Rect(0, 0, sw, sh), new Vector2(0, 0));

                float sbx = rend.sprite.bounds.size.x;
                float sby = rend.sprite.bounds.size.y;

                chunkOverlay.transform.localScale = new Vector3(rw / sbx, rh / sby, 1f); //Left hand side of division is the desired dimension

                chunkOverlay.SetActive(chunkOverlayActive);
            }
            else {
                if (chunkOverlayActive) {
                    updateChunkOverlayPosition();

                    //Update chunk overlay texture if needed
                    if (cacheChunk != null && cacheRegion != null) {
                        if (chunk == null || !(region.getColumn() == cacheRegion.getColumn() && region.getRow() == cacheRegion.getRow() && chunk.fillID == cacheChunk.fillID)) {
                            updateChunkOverlayTexture();
                        }
                    }
                    else {
                        updateChunkOverlayTexture();
                    }
                }


                chunkOverlay.SetActive(chunkOverlayActive);
            }
        }
	}

    private void updateChunkOverlayPosition() {
        int rx = region.getColumn() * Level.regionWidth;
        int ry = region.getRow() * Level.regionHeight;

        chunkOverlay.transform.position = new Vector3(rx, ry, 0f);
    }

    private void updateRegionOverlayPosition() {
        int rx = region.getColumn() * Level.regionWidth;
        int ry = region.getRow() * Level.regionHeight;

        regionOverlay.transform.position = new Vector3(rx, ry, 0f);
    }

    private void updateChunkOverlayTexture() {
        int resolution = CHUNK_OVERLAY_RESOLUTION;

        int rw = Level.regionWidth;
        int rh = Level.regionHeight;

        int sw = resolution * rw;
        int sh = resolution * rh;

        //Texture2D tex = chunkOverlayRenderer.sprite.texture;
        //drawChunkOverlayTexture(tex, tex.width, tex.height);
        Sprite sprite = chunkOverlayRenderer.sprite;
        chunkOverlayRenderer.sprite = Sprite.Create(generateChunkOverlayTexture(sw, sh), new Rect(0, 0, sw, sh), new Vector2(0, 0));
        Destroy(sprite.texture);
    }

    private void drawChunkOverlayTexture(Texture2D tex, int width, int height) {
        int rw = Level.regionWidth;
        int rh = Level.regionHeight;

        float pixelScaleX = rw / (float) width;
        float pixelScaleY = rh / (float) height;
        float reversePixelScaleX = width / (float) rw;
        float reversePixelScaleY = height / (float) rh;

        Color emptyColor = new Color(chunkOverlayColor.r, chunkOverlayColor.g, chunkOverlayColor.b, 0f);
        Color fillColor = chunkOverlayColor;

        bool didChunkDraw = false;
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                int px = (int) Mathf.Floor(x * pixelScaleX);
                int py = (int) Mathf.Floor(y * pixelScaleY);

                if (chunk != null) {
                    if (region.isChunkAtNodePosition(px, py)) {
                        Chunk ch = region.getChunkAtNodePosition(px, py);

                        if (ch.fillID == chunk.fillID) {
                            didChunkDraw = true;
                            tex.SetPixel(x, y, fillColor);
                        }
                        else {
                            tex.SetPixel(x, y, emptyColor);
                        }
                    }
                    else {
                        tex.SetPixel(x, y, emptyColor);
                    }
                }
                else {
                    tex.SetPixel(x, y, emptyColor);
                }
            }
        }

        tex.Apply();

        if (didChunkDraw) {
            cacheRegion = region;
            cacheChunk = chunk;
        }
        else {
            cacheRegion = null;
            cacheChunk = null;
        }
    }

    private Texture2D generateChunkOverlayTexture(int width, int height) {
        Texture2D tex = new Texture2D(width, height);

        drawChunkOverlayTexture(tex, width, height);

        return tex;
    }

    private Texture2D generateRegionOverlayTexture(int width, int height) {
        Color fillColor = regionOverlayColor;

        Texture2D tex = new Texture2D(width, height);

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                tex.SetPixel(x, y, fillColor);
            }
        }

        tex.Apply();

        return tex;
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

    public void setGridOverlayIsActive(bool b) {
        gridOverlayActive = b;
    }

    public void setRegionOverlayIsActive(bool b) {
        regionOverlayActive = b;
    }

    public void setChunkOverlayIsActive(bool b) {
        chunkOverlayActive = b;
    }
}
