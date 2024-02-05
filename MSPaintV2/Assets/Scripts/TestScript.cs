using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using UnityEngine.UI;
using UnityEditor;
using System.IO;

public class TestScript : MonoBehaviour
{
    //Sprite sprite;
    [SerializeField] Text fpsText;
    [SerializeField] RawImage image;
    [SerializeField] Color color1, color2;
    Texture2D texture;
    Color32[] textureArray = new Color32[250000]/*, array2 = new Color32[4000000]*/;
    private double time;
    private bool juu;
    int counter;
    [SerializeField] RectTransform rect;
    void Start()
    {
        counter = -1;
        //DriveInfo[] info = DriveInfo.GetDrives();
        //string[] directory = Directory.GetFiles(info[0].Name);
        //foreach (var i in directory)
        //{
        //    print(i);
        //}
        //print(Directory.GetDirectories(info[0].Name));
        //counter = 0;
        texture = new Texture2D(500, 500, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.name = "MyTexture";

        //for (int i = 0; i < 250000; i++)
        //{
        //    //byte x = (byte)((i*255f) / 250000f);
        //    //if (i % 25 == 1) print(x);
        //    textureArray[i] = new Color32((byte)(i%500 * 255/500f), (byte)(i/500 * (255 / 500f)), 0, 255);
        //}
        texture.SetPixels32(textureArray);
        texture.Apply(false, false);
        image.texture = texture;
    }

    public Vector3 MultiplyByVector3Up(Quaternion rotation)
    {
        float x = rotation.x * 2f;
        float y = rotation.y * 2f;
        float z = rotation.z * 2f;
        float xx = rotation.x * x;
        float zz = rotation.z * z;
        float xy = rotation.x * y;
        float yz = rotation.y * z;
        float wx = rotation.w * x;
        float wz = rotation.w * z;

        Vector3 res;
        res.x = xy - wz;
        res.y = 1f - (xx + zz);
        res.z = yz + wx;
        return res;
    }

    public Vector3 MultiplyByVector3Forward(Quaternion rotation)
    {
        float x = rotation.x * 2f;
        float y = rotation.y * 2f;
        float z = rotation.z * 2f;
        float xx = rotation.x * x;
        float yy = rotation.y * y;
        float xz = rotation.x * z;
        float yz = rotation.y * z;
        float wx = rotation.w * x;
        float wy = rotation.w * y;

        Vector3 res;
        res.x = xz + wy;
        res.y = yz - wx;
        res.z = 1f - (xx + yy);
        return res;
    }
    void Update()
    {
        for (int i = 0; i < 100; i++)
        {
            if (++counter < 250000)
            {
                textureArray[counter] = color1;
            }
        }
        texture.SetPixels32(textureArray);
        texture.Apply(false, false);
        image.texture = texture;
        fpsText.text = (1/Time.deltaTime).ToString("0");
    }
}
