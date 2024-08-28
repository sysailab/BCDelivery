using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RemoteButtonClick : MonoBehaviour
{
    GameObject canvas;
    void FindParents()
    {
        Transform selectDevice = transform.parent.parent;

        //Re_Generate_Device_name(selectDevice.name);
        GameManager.Instance.Select_Device = selectDevice.name;
        canvas = selectDevice.Find("RemoteCanvas")?.gameObject;
    }
    public void SwitchSceneToRemote()
    {
        FindParents();
        canvas.SetActive(true);
        GameManager.Instance.ChangeMode("Remote");

        
    }

    public void SwitchSceneToSimulation()
    {
        FindParents();
        canvas.SetActive(false);
        GameManager.Instance.ChangeMode("Simulation");

    }

    public void Re_Generate_Device_name(string selectdevicename)
    {

        if (selectdevicename == "Car1")
        {
            selectdevicename = "a";
        }
        else if(selectdevicename == "Car2")
        {
            selectdevicename = "3JKCK980030EKR";
        }
        else if (selectdevicename == "Car3")
        {
            selectdevicename = "b";
        }
        else if (selectdevicename == "Drone1")
        {
            selectdevicename = "tt-16";
        }
        else if (selectdevicename == "Drone2")
        {
            selectdevicename = "tt-17";
        }
        else if (selectdevicename == "Drone3")
        {
            selectdevicename = "tt-25";
        }


        GameManager.Instance.Select_Device = selectdevicename;
    }
}
