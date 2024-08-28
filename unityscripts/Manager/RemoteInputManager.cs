using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RemoteInputManager 
{
    public Action KeyActionAtive = null;

    public void OnUpdate()
    {
        if (Input.anyKey == false)
        {
            return;
        }

        if (KeyActionAtive != null)
        {
            KeyActionAtive.Invoke();
        }
        
    }
}
