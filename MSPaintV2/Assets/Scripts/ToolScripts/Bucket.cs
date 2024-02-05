using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class Bucket : MonoBehaviour
{
    MainScript mainScript;
    ColorManager colorManager;
    [SerializeField] Color32 color;
    Vector2Int imageSize;
    Color32[] textureArray;

    int threshold;
    [SerializeField] GameObject toolSettings;
    [SerializeField] Text thresholdText;

    void Start()
    {
        mainScript = MainScript.Instance;
        colorManager = ColorManager.Instance;
        imageSize = mainScript.imageSize;
        textureArray = mainScript.textureArray;
    }

    void OnEnable()
    {
        toolSettings.SetActive(true);
    }
    public void CustomOnDisable()
    {
        toolSettings.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && !mainScript.dragStartedFromUi)
        {
            Color32 col = colorManager.Current;
            Vector2 pos = mainScript.ScreenToWorldPos(Input.mousePosition);
            Vector2Int mousePos = new Vector2Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y));
            if (mousePos.x >= 0 && mousePos.x <= imageSize.x && mousePos.y >= 0 && mousePos.y <= imageSize.y)
            {
                Color32 pixelColor = textureArray[mousePos.x + mousePos.y * imageSize.x];
                int length = textureArray.Length;
                for (int i = 0; i < length; i++)
                {
                    if (textureArray[i].ColorEquals(pixelColor, threshold))
                    {
                        textureArray[i] = col;
                    }
                }
                mainScript.ApplyChanges();
            }
        }
    }

    public void ChangeThreshold(float newThreshold)
    {
        threshold = (int)(newThreshold);
        thresholdText.text = threshold.ToString();
    }
}
