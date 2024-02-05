using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using UnityEngine.EventSystems;
using System.Diagnostics;

public class MainScript : MonoBehaviour
{
    public static MainScript Instance { get; private set; }

    public Vector2 WorldMousePos { get; private set; }

    public enum Tools
    {
        Pencil,
        Brush,
        Eraser,
        Bucket,
        Picker
    }

    [SerializeField] Color cameraBackgroundColor;

    private Tools currentTool = Tools.Pencil;
    [SerializeField] Image[] toolBackgrounds;
    [SerializeField] MonoBehaviour[] toolScripts;

    private Camera cam;
    private Transform camTrans;
    [HideInInspector] public Vector2Int imageSize;
    private Vector2 camPos, mousePos;
    [SerializeField] RectTransform canvasRect;

    private float orthoSize, maxOrthoSize, startOrthoSize;
    [SerializeField] float minOrthoSize, zoomSpeed;
    private bool verticalLarger;
    [HideInInspector] public RawImage image;

    [SerializeField] Text fpsText, radiusText;

    [HideInInspector] public bool dragStartedFromUi = false;
    EventSystem currentEvenSystem;

    [HideInInspector] public float radius;
    [HideInInspector] public Color32[] textureArray;
    [HideInInspector] public Texture2D tex;

    /*[SerializeField] */Bucket bucket;
    Picker picker;
    [SerializeField] ColorManager colorManager;

    [SerializeField] RectTransform sizeIndicatorTrans;
    [SerializeField] RenderTexture sizeIndicatorTexture;
    [SerializeField] Material renderTextureMaterial, sizeIndicatorMaterial;
    [SerializeField] RawImage sizeIndicatorImage;
    [SerializeField] GameObject sizeIndicatorObj;
    [SerializeField] Shader indicatorShader, pickerIndicatorShader;
    private int sizeIndicatorSize;

    Vector2Int screenPixelSize;

    [SerializeField] Texture2D gridTexture;
    [SerializeField] Texture2D cursorTexture;
    bool wasOverUI;

    void Awake()
    {
        bucket = (Bucket)toolScripts[3];
        picker = (Picker)toolScripts[4];
        Shader.SetGlobalTexture("_GridTex", gridTexture);

        screenPixelSize = new Vector2Int(Screen.width, Screen.height);
        currentEvenSystem = EventSystem.current;
        if (currentEvenSystem.IsPointerOverGameObject())
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            wasOverUI = true;
        }
        else
        {
            Cursor.SetCursor(cursorTexture, Vector2.one * 9.5f, CursorMode.Auto);
            wasOverUI = false;
        }

        Instance = this;

        //bigface, material dark
        tex = new Texture2D(4096, 4096, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;
        #region bytes
        byte[] pngBytes = new byte[] {
            0x89,0x50,0x4E,0x47,0x0D,0x0A,0x1A,0x0A,0x00,0x00,0x00,0x0D,0x49,0x48,0x44,0x52,
            0x00,0x00,0x00,0x40,0x00,0x00,0x00,0x40,0x08,0x00,0x00,0x00,0x00,0x8F,0x02,0x2E,
            0x02,0x00,0x00,0x01,0x57,0x49,0x44,0x41,0x54,0x78,0x01,0xA5,0x57,0xD1,0xAD,0xC4,
            0x30,0x08,0x83,0x81,0x32,0x4A,0x66,0xC9,0x36,0x99,0x85,0x45,0xBC,0x4E,0x74,0xBD,
            0x8F,0x9E,0x5B,0xD4,0xE8,0xF1,0x6A,0x7F,0xDD,0x29,0xB2,0x55,0x0C,0x24,0x60,0xEB,
            0x0D,0x30,0xE7,0xF9,0xF3,0x85,0x40,0x74,0x3F,0xF0,0x52,0x00,0xC3,0x0F,0xBC,0x14,
            0xC0,0xF4,0x0B,0xF0,0x3F,0x01,0x44,0xF3,0x3B,0x3A,0x05,0x8A,0x41,0x67,0x14,0x05,
            0x18,0x74,0x06,0x4A,0x02,0xBE,0x47,0x54,0x04,0x86,0xEF,0xD1,0x0A,0x02,0xF0,0x84,
            0xD9,0x9D,0x28,0x08,0xDC,0x9C,0x1F,0x48,0x21,0xE1,0x4F,0x01,0xDC,0xC9,0x07,0xC2,
            0x2F,0x98,0x49,0x60,0xE7,0x60,0xC7,0xCE,0xD3,0x9D,0x00,0x22,0x02,0x07,0xFA,0x41,
            0x8E,0x27,0x4F,0x31,0x37,0x02,0xF9,0xC3,0xF1,0x7C,0xD2,0x16,0x2E,0xE7,0xB6,0xE5,
            0xB7,0x9D,0xA7,0xBF,0x50,0x06,0x05,0x4A,0x7C,0xD0,0x3B,0x4A,0x2D,0x2B,0xF3,0x97,
            0x93,0x35,0x77,0x02,0xB8,0x3A,0x9C,0x30,0x2F,0x81,0x83,0xD5,0x6C,0x55,0xFE,0xBA,
            0x7D,0x19,0x5B,0xDA,0xAA,0xFC,0xCE,0x0F,0xE0,0xBF,0x53,0xA0,0xC0,0x07,0x8D,0xFF,
            0x82,0x89,0xB4,0x1A,0x7F,0xE5,0xA3,0x5F,0x46,0xAC,0xC6,0x0F,0xBA,0x96,0x1C,0xB1,
            0x12,0x7F,0xE5,0x33,0x26,0xD2,0x4A,0xFC,0x41,0x07,0xB3,0x09,0x56,0xE1,0xE3,0xA1,
            0xB8,0xCE,0x3C,0x5A,0x81,0xBF,0xDA,0x43,0x73,0x75,0xA6,0x71,0xDB,0x7F,0x0F,0x29,
            0x24,0x82,0x95,0x08,0xAF,0x21,0xC9,0x9E,0xBD,0x50,0xE6,0x47,0x12,0x38,0xEF,0x03,
            0x78,0x11,0x2B,0x61,0xB4,0xA5,0x0B,0xE8,0x21,0xE8,0x26,0xEA,0x69,0xAC,0x17,0x12,
            0x0F,0x73,0x21,0x29,0xA5,0x2C,0x37,0x93,0xDE,0xCE,0xFA,0x85,0xA2,0x5F,0x69,0xFA,
            0xA5,0xAA,0x5F,0xEB,0xFA,0xC3,0xA2,0x3F,0x6D,0xFA,0xE3,0xAA,0x3F,0xEF,0xFA,0x80,
            0xA1,0x8F,0x38,0x04,0xE2,0x8B,0xD7,0x43,0x96,0x3E,0xE6,0xE9,0x83,0x26,0xE1,0xC2,
            0xA8,0x2B,0x0C,0xDB,0xC2,0xB8,0x2F,0x2C,0x1C,0xC2,0xCA,0x23,0x2D,0x5D,0xFA,0xDA,
            0xA7,0x2F,0x9E,0xFA,0xEA,0xAB,0x2F,0xDF,0xF2,0xFA,0xFF,0x01,0x1A,0x18,0x53,0x83,
            0xC1,0x4E,0x14,0x1B,0x00,0x00,0x00,0x00,0x49,0x45,0x4E,0x44,0xAE,0x42,0x60,0x82,
        };
        #endregion
        //tex.LoadImage(File.ReadAllBytes(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "RandomShitiä", "PieniA.png")));
        tex.Apply(false, false);
        image.texture = tex;
        textureArray = tex.GetPixels32();
        imageSize = new Vector2Int(tex.width, tex.height);

        cam = Camera.main;
        cam.orthographicSize = orthoSize = startOrthoSize = ((verticalLarger = imageSize.x < imageSize.y) ? imageSize.y : imageSize.x) * 0.5f;
        maxOrthoSize = orthoSize * 1.5f;

        camTrans = cam.transform;
        camTrans.position = camPos = (Vector2)imageSize * 0.5f;

        canvasRect.sizeDelta = imageSize;

        mousePos = Input.mousePosition;
        sizeIndicatorMaterial.shader = indicatorShader;
        ChangeRadius(0f);

        cam.backgroundColor = cameraBackgroundColor;
        Shader.SetGlobalColor("_CamBckgnd", cameraBackgroundColor);
        Shader.SetGlobalTexture("_Main", tex);
        Shader.SetGlobalVector("_ImgSz", new Vector4(imageSize.x, imageSize.y));
    }

    void Update()
    {
        Vector2 mousePosition = Input.mousePosition;

        if (Screen.width != screenPixelSize.x || Screen.height != screenPixelSize.y)
        {
            screenPixelSize = new Vector2Int(Screen.width, Screen.height);
            UvDdx();
        }

        if (currentEvenSystem.IsPointerOverGameObject())
        {
            if (!wasOverUI)
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }
            wasOverUI = true;
            
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                dragStartedFromUi = true;
            }
        }
        else
        {
            if (wasOverUI)
            {
                Cursor.SetCursor(cursorTexture, Vector2.one * 9.5f, CursorMode.Auto);
            }
            wasOverUI = false;

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                dragStartedFromUi = false;
            }
        }
        fpsText.text = (1 / Time.deltaTime).ToString("0");
        #region CameraMove
        if (Input.GetKey(KeyCode.Mouse2))
        {
            Vector2 difference = (mousePosition - mousePos) * ((verticalLarger ? imageSize.y : imageSize.x) * orthoSize / (screenPixelSize.y * startOrthoSize));
            camTrans.position = camPos = LimitImageArea(camPos - difference);
        }
        #endregion

        #region CameraZoom
        else
        {
            float scroll = Input.mouseScrollDelta.y;
            if (scroll != 0)
            {
                if (!Input.GetKey(KeyCode.LeftControl))
                {
                    float oldOrtho = orthoSize;
                    if (scroll > 0)
                    {
                        cam.orthographicSize = orthoSize = Mathf.Clamp(orthoSize * (1 - scroll * zoomSpeed), minOrthoSize, maxOrthoSize);
                    }
                    else
                    {
                        cam.orthographicSize = orthoSize = Mathf.Clamp(orthoSize / (1f + scroll * zoomSpeed), minOrthoSize, maxOrthoSize);
                    }
                    camTrans.position = camPos = LimitImageArea(camPos + (ScreenToWorldPos(mousePosition) - camPos) * ((oldOrtho / orthoSize) - 1));
                    UvDdx();
                }
                else colorManager.ScrollColors(scroll);
            }
        }
        mousePos = mousePosition;
        #endregion

        WorldMousePos = ScreenToWorldPos(mousePosition);
        sizeIndicatorTrans.position = new Vector2(Mathf.Floor(WorldMousePos.x) + .5f, Mathf.Floor(WorldMousePos.y) + .5f);

        #region ShortCuts
        if (Input.anyKeyDown)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.N))
            {
                ChangeTool(0);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.P))
            {
                ChangeTool(1);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.E))
            {
                ChangeTool(2);
            }
            if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.B))
            {
                ChangeTool(3);
            }
            if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.O))
            {
                ChangeTool(4);
            }
        }
        #endregion
    }

    public Vector2 ScreenToWorldPos(Vector2 screenPos)
    {
        float screenHeight = screenPixelSize.y * 0.5f;
        return new Vector2(camPos.x + ((screenPos.x - screenPixelSize.x * 0.5f) * (orthoSize / screenHeight)), camPos.y + ((screenPos.y - screenHeight) * (orthoSize / screenHeight)));
    }
    private Vector2 LimitImageArea(Vector2 a) => new Vector2(Mathf.Clamp(a.x, 0f, imageSize.x), Mathf.Clamp(a.y, 0f, imageSize.y));
    public void ChangeTool(int newTool)
    {
        int current = (int)currentTool;
        if (current != newTool)
        {
            toolBackgrounds[current].color = Color.black;
            toolBackgrounds[newTool].color = Color.white;
            toolScripts[current].enabled = false;
            if (current == 3)
            {
                sizeIndicatorObj.SetActive(true);
                bucket.CustomOnDisable();
            }
            else if (newTool == 3)
            {
                sizeIndicatorObj.SetActive(false);
            }
            if (current == 4)
            {
                sizeIndicatorMaterial.shader = indicatorShader;
                UvDdx();
            }
            else if (newTool == 4)
            {
                sizeIndicatorMaterial.shader = pickerIndicatorShader;
                picker.Update();
                UvDdx();
            }
            toolScripts[newTool].enabled = true;
            currentTool = (Tools)newTool;
        }
    }
    public void ChangeRadius(float newRadius)
    {
        radius = newRadius > 1 ? newRadius - 0.5f : (newRadius == 1 ? 1.0001f : 0.5f);
        float res = Mathf.Floor(radius) * 2 + 3;
        sizeIndicatorSize = (int)res;
        sizeIndicatorTrans.sizeDelta = Vector2.one * res;
        radiusText.text = newRadius.ToString();

        renderTextureMaterial.SetFloat("_Sqr", radius * radius);
        renderTextureMaterial.SetFloat("_Res", sizeIndicatorSize);
        RenderTexture.active = null;
        sizeIndicatorTexture.Release();
        sizeIndicatorTexture.width = sizeIndicatorSize;
        sizeIndicatorTexture.height = sizeIndicatorSize;
        Graphics.Blit(sizeIndicatorTexture, sizeIndicatorTexture, renderTextureMaterial);
        UvDdx();
    }

    private void UvDdx()
    {
        Shader.SetGlobalFloat("_Ddx", (orthoSize * 2f) / (screenPixelSize.y * sizeIndicatorSize));
    }

    public void ApplyChanges()
    {
        tex.SetPixels32(textureArray);
        tex.Apply();
        image.texture = tex;
    }

    public void ApplyBrush(Color32[] scndTexArray, int xMin, int xMax, int yMin, int yMax)
    {
        for (int x = xMin; x <= xMax; x++)
        {
            for (int y = yMin; y <= yMax; y++)
            {
                int i = x + y * imageSize.x;
                Color32 col = scndTexArray[i];
                if (col.a != 0)
                {
                    textureArray[i] = Color32.LerpUnclamped(col, textureArray[i], 1 - col.a / 255f);
                }
            }
        }
        ApplyChanges();
    }
}
