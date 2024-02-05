using UnityEngine;

public class Picker : MonoBehaviour
{
    Color32[] textureArray;
    MainScript mainScript;
    ColorManager colorManager;
    Vector2Int imageSize;

    [SerializeField] Material pickerSizeIndicatorMaterial;

    void Awake()
    {
        mainScript = MainScript.Instance;
        colorManager = ColorManager.Instance;
        textureArray = mainScript.textureArray;
    }

    void OnEnable()
    {

    }

    public void Update()
    {
        Vector2 pos = mainScript.WorldMousePos;
        Vector2Int mousePos = new Vector2Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y));
        imageSize = mainScript.imageSize;
        float radius = mainScript.radius;
        int radiusInt = Mathf.FloorToInt(radius);
        float sqrRadius = radius * radius;

        int right = Min(mousePos.x + radiusInt, imageSize.x - 1);
        int left = Max(mousePos.x - radiusInt, 0);
        int up = Min(mousePos.y + radiusInt, imageSize.y - 1);
        int down = Max(mousePos.y - radiusInt, 0);

        int r = 0;
        int g = 0;
        int b = 0;
        int a = 0;
        int count = 0;
        Color32 col;
        for (int x = left; x <= right; x++)
        {
            for (int y = down; y <= up; y++)
            {
                if (SqrDistanceToPoint(new Vector2Int(x, y), mousePos) < sqrRadius)
                {
                    col = textureArray[x + y * imageSize.x];
                    r += col.r;
                    g += col.g;
                    b += col.b;
                    a += col.a;
                    count++;
                }
            }
        }
        Color32 color;

        if (count > 0)
        {
            color = new Color32(
            (byte)Mathf.RoundToInt(r / (float)count),
            (byte)Mathf.RoundToInt(g / (float)count),
            (byte)Mathf.RoundToInt(b / (float)count),
            (byte)Mathf.RoundToInt(a / (float)count));
        }
        else
        {
            color = new Color32(255, 255, 255, 0);
        }
        pickerSizeIndicatorMaterial.SetColor("_Color", color);

        if (Input.GetKeyDown(KeyCode.Mouse0) && !mainScript.dragStartedFromUi)
        {
            color.a = 255;
            colorManager.Current = color;
        }
        else if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            colorManager.Background = color;
        }
    }
    #region Functions
    private int Max(int a, int b) => a > b ? a : b;
    private int Min(int a, int b) => a < b ? a : b;
    private int Clamp0Max(int a, int max) => a > 0 ? (a < max ? a : max) : 0;
    private int SqrDistanceToPoint(Vector2Int point, Vector2Int a)
    {
        int x = point.x - a.x;
        int y = point.y - a.y;
        return x * x + y * y;
    }
    #endregion
}
