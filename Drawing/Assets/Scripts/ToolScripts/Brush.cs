using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Brush : MonoBehaviour
{
    Color32[] secondTextureArray;
    Texture2D secondTexture;
    MainScript mainScript;
    ColorManager colorManager;
    Vector2Int imageSize;
    [SerializeField] RawImage secondImage;

    private Vector2Int oldPos;

    int xMin, xMax, yMin, yMax;

    public void CustomAwake()
    {
        mainScript = MainScript.Instance;
        imageSize = mainScript.imageSize;
        colorManager = ColorManager.Instance;
        secondTexture = new Texture2D(imageSize.x, imageSize.y, TextureFormat.RGBA32, false);
        secondTexture.filterMode = FilterMode.Point;
        secondTexture.wrapMode = TextureWrapMode.Clamp;

        secondTextureArray = new Color32[imageSize.x * imageSize.y];
        secondTexture.SetPixels32(secondTextureArray);
        secondTexture.Apply();
        secondImage.texture = secondTexture;

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
            Color32 col = colorManager.Current;
            Vector2 pos = mainScript.WorldMousePos;
            Vector2Int mousePos = new Vector2Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y));
            bool keyDown = false;
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                keyDown = true;
                oldPos = mousePos;
            }
            imageSize = mainScript.imageSize;
            float radius = mainScript.radius;
            int radiusInt = Mathf.FloorToInt(radius);
            float sqrRadius = radius * radius;
            int up, down, right, left;

            if (!keyDown)
            {
                if (mousePos.x > oldPos.x)
                {
                    //new is greater
                    right = Mathf.Min(mousePos.x + radiusInt, imageSize.x - 1);
                    left = Mathf.Max(oldPos.x - radiusInt, 0);
                }
                else
                {
                    //old is greater
                    right = Mathf.Min(oldPos.x + radiusInt, imageSize.x - 1);
                    left = Mathf.Max(mousePos.x - radiusInt, 0);
                }
                if (mousePos.y > oldPos.y)
                {
                    //new is greater
                    up = Mathf.Min(mousePos.y + radiusInt, imageSize.y - 1);
                    down = Mathf.Max(oldPos.y - radiusInt, 0);
                }
                else
                {
                    //old is greater
                    up = Mathf.Min(oldPos.y + radiusInt, imageSize.y - 1);
                    down = Mathf.Max(mousePos.y - radiusInt, 0);
                }
                if (left < xMin) xMin = left;
                if (right > xMax) xMax = right;
                if (down < yMin) yMin = down;
                if (up > yMax) yMax = up;
            }
            else
            {
                left = xMin = Mathf.Max(mousePos.x - radiusInt, 0);
                right = xMax = Mathf.Min(mousePos.x + radiusInt, imageSize.x - 1);
                down = yMin = Mathf.Max(mousePos.y - radiusInt, 0);
                up = yMax = Mathf.Min(mousePos.y + radiusInt, imageSize.y - 1);
            }

            float onePerRadius = 1 / sqrRadius;

            if (mousePos != oldPos)
            {
                for (int x = left; x <= right; x++)
                {
                    for (int y = down; y <= up; y++)
                    {
                        float sqrDistance = new Vector2Int(x, y).SqrDistanceToLineSegment(mousePos, oldPos);
                        if (sqrDistance < sqrRadius)
                        {
                            col.a = (byte)((1f - sqrDistance * onePerRadius) * 255);

                            int i = x + y * imageSize.x;
                            if (col.a > secondTextureArray[i].a)
                            {
                                secondTextureArray[i] = col;
                            }
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
                        float sqrDistance = new Vector2Int(x, y).SqrDistanceToPoint(oldPos);
                        if (sqrDistance < sqrRadius)
                        {
                            col.a = (byte)((1f - sqrDistance * onePerRadius) * 255);
                            
                            int i = x + y * imageSize.x;
                            if (col.a > secondTextureArray[i].a)
                            {
                                secondTextureArray[i] = col;
                            }
                        }
                    }
                }
            }
            secondTexture.SetPixels32(secondTextureArray);
            secondTexture.Apply();
            secondImage.texture = secondTexture;
            oldPos = mousePos;
        }
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            ApplyBrush(secondTextureArray, xMin, xMax, yMin, yMax);
            for (int x = xMin; x <= xMax; x++)
            {
                for (int y = yMin; y <= yMax; y++)
                {
                    secondTextureArray[x + y * imageSize.x] = new Color32();
                }
            }
            secondTexture.SetPixels32(secondTextureArray);
            secondTexture.Apply();
            secondImage.texture = secondTexture;
        }
    }

    public void ApplyBrush(Color32[] scndTexArray, int xMin, int xMax, int yMin, int yMax)
    {
        Color32[] textureArray = mainScript.textureArray;

        for (int x = xMin; x <= xMax; x++)
        {
            for (int y = yMin; y <= yMax; y++)
            {
                int i = x + y * imageSize.x;
                Color brushCol = scndTexArray[i];
                if (brushCol.a != 0)
                {
                    Color backgroundColor = textureArray[i];
                    float inverseAlpha = 1f - brushCol.a;
                    float outputAlpha = brushCol.a + backgroundColor.a * inverseAlpha;
                    Color outputColor = (brushCol * brushCol.a + backgroundColor * backgroundColor.a * inverseAlpha) / outputAlpha;
                    outputColor.a = outputAlpha;
                    textureArray[i] = outputColor;
                }
            }
        }
        mainScript.ApplyChanges();
    }
}
