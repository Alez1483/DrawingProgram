using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseInputs : MonoBehaviour
{
    public static MouseInputs Instance { get; private set; }
    public bool GetMouseDown { get; private set; }
    public bool GetMouseUp { get; private set; }
    public bool GetMouse { get; private set; }

    private bool wasLastFrame;

    void Awake()
    {
        Instance = this;
        wasLastFrame = Input.GetKey(KeyCode.Mouse0);
    }

    void Update()
    {
        GetMouse = Input.GetKey(KeyCode.Mouse0);
        if (wasLastFrame != GetMouse)
        {
            if (wasLastFrame)
            {
                GetMouseUp = true;
            }
            else
            {
                GetMouseDown = true;
            }
            wasLastFrame = GetMouse;
        }
        else
        {
            GetMouseUp = GetMouseDown = false;
        }

        //if (GetMouse)
        //{
        //    if (!wasLastFrame)
        //    {
        //        GetMouseDown = true;
        //    }
        //    else
        //    {
        //        GetMouseDown = false;
        //    }
        //}
        //else
        //{
        //    if (wasLastFrame)
        //    {
        //        GetMouseUp = true;
        //    }
        //    else
        //    {
        //        GetMouseUp = false;
        //    }
        //}

        //wasLastFrame = GetMouse;

        if (GetMouse)
            print("GetMouse");
        if (GetMouseUp)
            print("GetMouseUp");
        if (GetMouseDown)
            print("GetMouseDown");
    }
}
