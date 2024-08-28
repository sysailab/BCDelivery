using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerTextUpdate : MonoBehaviour
{
    [SerializeField]
    Text Nodelist;

    [SerializeField]
    Text Location;

    private Coroutine updateStateCoroutine;
    private Coroutine updateNodeCoroutine;
    // Start is called before the first frame update
    //void Start()
    //{
    //    GameManager.Comm.TextUPdateAction -= StartUpdateTextCoroutine;
    //    GameManager.Comm.TextUPdateAction += StartUpdateTextCoroutine;
    //}
    public void Connect_Device_Class(string selectdevice)
    {
        string selectdevice_ip = null;

        if (selectdevice == "Car1")
        {
            selectdevice_ip = "a";
        }
        else if (selectdevice == "Car2")
        {
            selectdevice_ip = "3JKCK980030EKR";
        }
        else if (selectdevice == "Car3")
        {
            selectdevice_ip = "b";
        }
        else if (selectdevice == "Drone1")
        {
            selectdevice_ip = "tt-16";
        }
        else if (selectdevice == "Drone2")
        {
            selectdevice_ip = "192.168.50.113:9999";
        }
        else if (selectdevice == "Drone3")
        {
            selectdevice_ip = "tt-25";
        }

        if (selectdevice_ip != null && GameManager.Instance.ReturnClick == true)
        {
            //updateStateCoroutine = StartCoroutine(UpdateLocation($"http://{selectdevice_ip}/get-location"));
            //updateNodeCoroutine = StartCoroutine(UpdateNodeList($"http://{selectdevice_ip}/get-nodes"));
            selectdevice_ip = null;
            //StartCoroutine(UpdateText("http://192.168.50.113:9999/get-location"));
            //StartCoroutine(UpdateNodeList("http://192.168.50.113:9999/get-nodes"));
        }
        else
        {
            selectdevice_ip = null;
            StopUpdateStateCoroutine();
        }

    }

    public void StartUpdateTextCoroutine(string device)
    {
        Debug.Log($"device name /// {device}");
        Connect_Device_Class(device);
    }

    public IEnumerator UpdateLocation(string Url)
    {
        while(true)
        {

            UnityWebRequest request = UnityWebRequest.Get(Url);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                StopUpdateStateCoroutine();
                Debug.LogError("Image download error: " + request.error);
            }
            else
            {
                List<Location_Data> DeviceDataList = null;
                try
                {
                    // Try to deserialize as a list first
                    DeviceDataList = JsonConvert.DeserializeObject<List<Location_Data>>(request.downloadHandler.text);
                }
                catch (JsonSerializationException)
                {
                    // If it fails, try to deserialize as a single object and wrap it in a list
                    try
                    {
                        Location_Data singleData = JsonConvert.DeserializeObject<Location_Data>(request.downloadHandler.text);
                        DeviceDataList = new List<Location_Data> { singleData };
                    }
                    catch (JsonSerializationException ex)
                    {
                        //StopUpdateStateCoroutine();
                        Debug.LogError("JSON deserialization error: " + ex.Message);
                    }
                }

                if (DeviceDataList != null)
                {
                    foreach (Location_Data data in DeviceDataList)
                    {
                        
                        string x = data.x;
                        string y = data.y;
                        string z = data.z;

                        Location.text = $"x : ({x}) , y : ({y}) , z : ({z})";
                    }
                }

            }

            yield return null;
        }
        
    }

    public IEnumerator UpdateNodeList(string Url)
    {
        while (true)
        {
            Debug.Log("node: " + "aaaaaaaaaaaaa");
            UnityWebRequest request = UnityWebRequest.Get(Url);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                StopUpdateStateCoroutine();
                //Debug.LogError("node error: " + request.error);
            }
            else
            {

                List<NodeType> nodeList = null;
                try
                {
                    // Try to deserialize as a list first
                    nodeList = JsonConvert.DeserializeObject<List<NodeType>>(request.downloadHandler.text);
                }
                catch (JsonSerializationException)
                {
                    // If it fails, try to deserialize as a single object and wrap it in a list
                    try
                    {
                        NodeType singleData = JsonConvert.DeserializeObject<NodeType>(request.downloadHandler.text);
                        nodeList = new List<NodeType> { singleData };
                    }
                    catch (JsonSerializationException ex)
                    {
                        //StopUpdateStateCoroutine();
                        Debug.LogError("JSON deserialization error: " + ex.Message);
                    }
                }

                if (nodeList != null)
                {
                    foreach (NodeType data in nodeList)
                    {
                        string address = data.address;
                        string node_type = data.node_type;

                        Nodelist.text = $"{node_type} : {address}";
                    }
                }
            }

            yield return null;

        }

    }
    public void StopUpdateStateCoroutine()
    {
        if (updateStateCoroutine != null)
        {
            StopCoroutine(updateStateCoroutine);
            updateStateCoroutine = null;
        }

        if (updateNodeCoroutine != null)
        {
            StopCoroutine(updateNodeCoroutine);
            updateNodeCoroutine = null;
        }
    }

}

//public class Location_Data
//{
//    public string x;
//    public string y;
//    public string z;

//}

//public class NodeType
//{
//    public string address;
//    public string node_type;


//}
