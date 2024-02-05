using UnityEngine;

public class Eraser : MonoBehaviour
{
    Color32[] textureArray;
    MainScript mainScript;
    ColorManager colorManager;
    Vector2Int imageSize;

    private Vector2Int oldPos;

    void Awake()
    {
        mainScript = MainScript.Instance;
        colorManager = ColorManager.Instance;
        textureArray = mainScript.textureArray;
    }

    void OnEnable()
    {
        Vector2 pos = mainScript.WorldMousePos;
        oldPos = new Vector2Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y));
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Mouse0) && !mainScript.dragStartedFromUi)
        {
            Color32 col = colorManager.Background;
            Vector2 pos = mainScript.WorldMousePos;
            Vector2Int mousePos = new Vector2Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y));
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                oldPos = mousePos;
            }
            imageSize = mainScript.imageSize;
            float radius = mainScript.radius;
            int radiusInt = Mathf.FloorToInt(radius);
            float sqrRadius = radius * radius;
            int up, down, right, left;
            if (mousePos.x > oldPos.x)
            {
                //new is greater
                right = Min(mousePos.x + radiusInt, imageSize.x - 1);
                left = Max(oldPos.x - radiusInt, 0);
            }
            else
            {
                //old is greater
                right = Min(oldPos.x + radiusInt, imageSize.x - 1);
                left = Max(mousePos.x - radiusInt, 0);
            }
            if (mousePos.y > oldPos.y)
            {
                //new is greater
                up = Min(mousePos.y + radiusInt, imageSize.y - 1);
                down = Max(oldPos.y - radiusInt, 0);
            }
            else
            {
                //old is greater
                up = Min(oldPos.y + radiusInt, imageSize.y - 1);
                down = Max(mousePos.y - radiusInt, 0);
            }

            if (mousePos != oldPos)
            {
                for (int x = left; x <= right; x++)
                {
                    for (int y = down; y <= up; y++)
                    {
                        if (SqrDistanceToLineSegment(new Vector2Int(x, y), mousePos, oldPos) < sqrRadius)
                        {
                            textureArray[x + y * imageSize.x] = col;
                        }
                    }
                }
            }
            else
            {
                for (int x = left; x <= right; x++)
                {
                    for (int y = down; y <= up; y++)
                    {
                        if (SqrDistanceToPoint(new Vector2Int(x, y), oldPos) < sqrRadius)
                        {
                            textureArray[x + y * imageSize.x] = col;
                        }
                    }
                }
            }
            mainScript.ApplyChanges();
            oldPos = mousePos;
        }
    }
    #region Funktions
    private int Max(int a, int b) => a > b ? a : b;
    private int Min(int a, int b) => a < b ? a : b;
    private int Clamp0Max(int a, int max) => a > 0 ? (a < max ? a : max) : 0;
    private float SqrDistanceToLineSegment(Vector2Int point, Vector2Int x, Vector2Int y)
    {
        int px = y.x - x.x;
        int py = y.y - x.y;
        int norm = px * px + py * py;
        float u = ((point.x - x.x) * px + (point.y - x.y) * py) / (float)norm;
        u = Mathf.Clamp01(u);
        float a = x.x + u * px;
        float b = x.y + u * py;
        float da = a - point.x;
        float db = b - point.y;
        return (da * da + db * db);
    }
    private float SqrDistanceToPoint(Vector2Int point, Vector2Int a)
    {
        float x = point.x - a.x, y = point.y - a.y;
        return x * x + y * y;
    }
    #endregion
}
