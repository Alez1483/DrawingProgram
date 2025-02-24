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

    Bucket bucket;
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

    [Header("Image initialization")]
    [SerializeField, Min(1)] int width = 256;
    [SerializeField, Min(1)] int height = 256;
    [SerializeField] Color32 backGroundColor = Color.white;

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

        //initialize image
        tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;
        image.texture = tex;
        textureArray = new Color32[width * height];

        for (int i = 0; i < textureArray.Length; i++)
        {
            textureArray[i] = backGroundColor;
        }
        ApplyChanges();

        imageSize = new Vector2Int(tex.width, tex.height);

        //initialize camera
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

        //screen size changed
        if (Screen.width != screenPixelSize.x || Screen.height != screenPixelSize.y)
        {
            screenPixelSize = new Vector2Int(Screen.width, Screen.height);
            UvDdx();
        }

        //handle cursor state regarding UI
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

        //update FPS counter
        fpsText.text = (1 / Time.deltaTime).ToString("0");

        //Camera drag
        if (Input.GetKey(KeyCode.Mouse2))
        {
            Vector2 difference = (mousePosition - mousePos) * ((verticalLarger ? imageSize.y : imageSize.x) * orthoSize / (screenPixelSize.y * startOrthoSize));
            camTrans.position = camPos = LimitImageArea(camPos - difference);
        }

        //Zooming in and outt
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

        WorldMousePos = ScreenToWorldPos(mousePosition);
        sizeIndicatorTrans.position = new Vector2(Mathf.Floor(WorldMousePos.x) + .5f, Mathf.Floor(WorldMousePos.y) + .5f);

        //Shortcuts
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
    }

    //Optimized space conversion
    public Vector2 ScreenToWorldPos(Vector2 screenPos)
    {
        float screenHeight = screenPixelSize.y * 0.5f;
        return new Vector2(camPos.x + ((screenPos.x - screenPixelSize.x * 0.5f) * (orthoSize / screenHeight)), camPos.y + ((screenPos.y - screenHeight) * (orthoSize / screenHeight)));
    }
    private Vector2 LimitImageArea(Vector2 a) => new Vector2(Mathf.Clamp(a.x, 0f, imageSize.x), Mathf.Clamp(a.y, 0f, imageSize.y));
    
    //Change to a different tool
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

    //Changes brush radius, updates the sizeIndictatorTexture accordingly
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
        Graphics.Blit(null, sizeIndicatorTexture, renderTextureMaterial);
        UvDdx();
    }

    //Updates the partial derivative of the image UVs
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
}
