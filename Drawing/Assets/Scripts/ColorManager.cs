using UnityEngine;
using UnityEngine.UI;
using System;
using Unity.IO;

public class ColorManager : MonoBehaviour
{
    public static ColorManager Instance { get; private set; }

    [SerializeField] Color32[] colors = new Color32[10];
    [SerializeField] RawImage[] images;
    private Color32 _current, _background;

    public Color32 Current {
        get { return _current; }
        set 
        { 
            _current = value;
            colors[colorIndex] = images[colorIndex].color = value;
        }
    }
    public Color32 Background
    {
        get { return _background; }
        set
        {
            _background = value;
            backgroundButtonMat.color = value;
        }
    }

    Color32 old, pickerColor;
    int colorIndex;
    bool pickerActive, editingMainColor;
    [SerializeField] RectTransform highLight;
    [SerializeField] GameObject colorPicker;
    [SerializeField] Slider redSlider, greenSlider, blueSlider, alphaSlider;

    [SerializeField] RectTransform wheelRect, quadRect, handleOrigin, quadKnob;
    [SerializeField] Material alphaSliderMat, backgroundButtonMat;

    float h, s, v;

    [SerializeField] Brush brush;

    enum DragMode : byte
    {
        None,
        Wheel,
        Square
    };

    DragMode currentDragMode = DragMode.None;

    const double onePerTau = 1.0 / (Math.PI * 2.0);

    void Awake()
    {
        Instance = this;

        s = v = 1f;
        #region Texture Creator
        //Texture2D tex = new Texture2D(1024, 1024, TextureFormat.RGBAFloat, false);
        //Color[] array = new Color[1024 * 1024];
        //int a = 0;
        //for (float y = -511.5f; y <= 511.5f; y++)
        //{
        //    for (float x = -511.5f; x <= 511.5f; x++)
        //    {
        //        float hue;
        //        if (x > 0)
        //        {
        //            if (y < 0)
        //            {
        //                hue = (float)(Math.Atan(y / (double)x) / (-2.0 * Math.PI));
        //            }
        //            else
        //            {
        //                hue = (float)(1.0 - (Math.Atan(y / (double)x) / (2.0 * Math.PI)));
        //            }
        //        }
        //        else
        //        {
        //            hue = (float)(0.5 - (Math.Atan(y / (double)x) / (2.0 * Math.PI)));
        //        }
        //        array[a] = Color.HSVToRGB(hue, 1f, 1f);
        //        a++;
        //    }
        //}
        //tex.SetPixels(array);
        ////tex.Apply();
        //File.WriteAllBytes(Path.Combine(Application.dataPath, "Images", "ColorWheel"), tex.EncodeToPNG());
        #endregion

        images[0].color = _current = pickerColor = colors[0];
        
        for (int i = 1; i < 10; i++)
        {
            images[i].color = colors[i];
        }
        _background = backgroundButtonMat.color = colors[10];

        brush.CustomAwake();
    }

    void Update()
    {
        if (pickerActive)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Vector2 mouse = Input.mousePosition;
                if (RectTransformUtility.RectangleContainsScreenPoint(wheelRect, mouse))
                {
                    if (RectTransformUtility.RectangleContainsScreenPoint(quadRect, mouse))
                    {
                        //inside Renctangle
                        currentDragMode = DragMode.Square;
                        SetQuad(mouse);
                    }
                    else
                    {
                        //Inside Wheel
                        currentDragMode = DragMode.Wheel;
                        SetWheel(mouse);
                    }
                }
            }
            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                currentDragMode = DragMode.None;
            }

            if (currentDragMode == DragMode.Wheel)
            {
                SetWheel(Input.mousePosition);
            }
            else if (currentDragMode == DragMode.Square)
            {
                SetQuad(Input.mousePosition);
            }
        }
    }

    public void ScrollColors(float scroll)
    {
        if (pickerActive) return;
        colorIndex = scroll > 0 ? (colorIndex != 9 ? colorIndex + 1 : 0) : (colorIndex != 0 ? colorIndex - 1 : 9);
        _current = colors[colorIndex];
        highLight.anchoredPosition = new Vector2(colorIndex * 35 + 2.5f, 0f);
    }

    public void ChangeColor(int newColor)
    {
        if (colorIndex != newColor)
        {
            if (pickerActive) return;
            _current = colors[colorIndex = newColor];
            highLight.anchoredPosition = new Vector2(colorIndex * 35 + 2.5f, 0f);
        }
        else if (!pickerActive)
        {
            alphaSliderMat.SetFloat("_F", 0.34117647058f);
            alphaSlider.SetValueWithoutNotify(255);
            alphaSlider.interactable = false;
            old = colors[colorIndex];
            editingMainColor = true;

            ActivatePicker();
        }
    }

    public void OnBackgroundColorClick()
    {
        if (!pickerActive)
        {
            alphaSliderMat.SetFloat("_F", 1f);
            alphaSlider.interactable = true;
            old = _background;
            alphaSlider.SetValueWithoutNotify(old.a);
            editingMainColor = false;

            ActivatePicker();
        }
    }

    public void Ok()
    {
        colorPicker.SetActive(false);
        pickerActive = false;

        if (editingMainColor)
        {
            colors[colorIndex] = _current = pickerColor;
        }
        else
        {
            _background = pickerColor;
        }
    }
    public void Cancel()
    {
        colorPicker.SetActive(false);
        pickerActive = false;

        if (editingMainColor)
            images[colorIndex].color = old;
        else
            backgroundButtonMat.color = old;
    }

    public void RedSlider(float newValue)
    {
        pickerColor.r = (byte)newValue;
        SliderValueChanged();
    }
    public void GreenSlider(float newValue)
    {
        pickerColor.g = (byte)newValue;
        SliderValueChanged();
    }
    public void BlueSlider(float newValue)
    {
        pickerColor.b = (byte)newValue;
        SliderValueChanged();
    }

    public void AlphaSlider(float newValue)
    {
        pickerColor.a = (byte)newValue;

        if (editingMainColor)
            images[colorIndex].color = pickerColor;
        else
            backgroundButtonMat.color = pickerColor;
    }

    private void SliderValueChanged()
    {
        Color.RGBToHSV(pickerColor, out h, out s, out v);
        //Color saturated = Color.HSVToRGB(h, 1f, 1f);

        Shader.SetGlobalColor("_Rgb", pickerColor);
        Shader.SetGlobalVector("_Hsv", new Vector4(h, s, v));
        //Shader.SetGlobalColor("_Full", saturated);

        handleOrigin.rotation = Quaternion.Euler(0f, 0f, h * -360f);
        if (editingMainColor)
            images[colorIndex].color = pickerColor;
        else
            backgroundButtonMat.color = new Color32(pickerColor.r, pickerColor.g, pickerColor.b, 255);

        quadKnob.anchoredPosition = new Vector2(s * 100f, v * 100f);
    }

    private void SetWheel(Vector2 mouse)
    {
        Vector2 dir = mouse - (Vector2)handleOrigin.position;
        double angle = Math.Atan2(-dir.x, -dir.y);
        angle = (angle + Math.PI) * onePerTau;
        //angle = angle < 0 ? angle * -0.159154943091895 : angle * -0.159154943091895 + 1.0;
        float anglef = h = (float)angle;
        handleOrigin.rotation = Quaternion.Euler(0f, 0f, anglef * -360f);

        byte alpha = pickerColor.a;
        pickerColor = Color.HSVToRGB(h, s, v);
        pickerColor.a = alpha;

        //Color saturated = Color.HSVToRGB(h , 1f, 1f);

        if (editingMainColor)
            images[colorIndex].color = pickerColor;
        else
            backgroundButtonMat.color = pickerColor;

        redSlider.SetValueWithoutNotify(pickerColor.r);
        greenSlider.SetValueWithoutNotify(pickerColor.g);
        blueSlider.SetValueWithoutNotify(pickerColor.b);

        Shader.SetGlobalVector("_Hsv", new Vector4(h, s, v));
        //Shader.SetGlobalColor("_Full", saturated);
        Shader.SetGlobalColor("_Rgb", pickerColor);
    }

    private void SetQuad(Vector2 mouse)
    {
        Vector2 clamped = quadRect.InverseTransformPoint(mouse);
        clamped.x = Mathf.Clamp(clamped.x + 50, 0, 100);
        clamped.y = Mathf.Clamp(clamped.y + 50, 0, 100);

        s = clamped.x * 0.01f;
        v = clamped.y * 0.01f;

        quadKnob.anchoredPosition = clamped;

        byte alpha = pickerColor.a;
        pickerColor = Color.HSVToRGB(h, s, v);
        pickerColor.a = alpha;
        //Color saturated = Color.HSVToRGB(h, 1f, 1f);

        if (editingMainColor)
            images[colorIndex].color = pickerColor;
        else
            backgroundButtonMat.color = pickerColor;

        redSlider.SetValueWithoutNotify(pickerColor.r);
        greenSlider.SetValueWithoutNotify(pickerColor.g);
        blueSlider.SetValueWithoutNotify(pickerColor.b);

        Shader.SetGlobalVector("_Hsv", new Vector4(h, s, v));
        //Shader.SetGlobalColor("_Full", saturated);
        Shader.SetGlobalColor("_Rgb", pickerColor);
    }

    void ActivatePicker()
    {
        pickerActive = true;
        colorPicker.SetActive(true);
        redSlider.SetValueWithoutNotify(old.r);
        greenSlider.SetValueWithoutNotify(old.g);
        blueSlider.SetValueWithoutNotify(old.b);

        pickerColor = old;
        Color.RGBToHSV(old, out h, out s, out v);
        //Color saturated = Color.HSVToRGB(h, 1f, 1f);

        Shader.SetGlobalColor("_Rgb", pickerColor);
        Shader.SetGlobalVector("_Hsv", new Vector4(h, s, v));
        //Shader.SetGlobalColor("_Full", saturated);

        handleOrigin.rotation = Quaternion.Euler(0f, 0f, h * -360f);

        quadKnob.anchoredPosition = new Vector2(s * 100f, v * 100f);
    }
}
