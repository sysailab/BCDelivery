using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabManager 
{
    public Action PrefabCreateAction;
    public Action PrefabAddAction;

    public void PrefabContrl()
    {
        if (GameManager.Instance._Mode == "Simulation")
        {
            PrefabCreateAction?.Invoke();
        }
        
        //GameManager.Comm.isConnected = true;     
        
    }
}
