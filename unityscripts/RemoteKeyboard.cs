using Newtonsoft.Json;
using System.Collections.Specialized;
using UnityEngine;

public class RemoteKeyboard : MonoBehaviour
{
    string get_ip = null;

    bool type_c_d = false;
    public void Start()
    {
        
        GameManager.remoteInput.KeyActionAtive -= OnKeyboard;
        GameManager.remoteInput.KeyActionAtive += OnKeyboard;
    }

    void OnKeyboard()
    {

        string cmd_data = null;
        string jsonDeviceData = null;
        string Select_Device = GameManager.Instance.Select_Device;

        get_ip = GetRealIP(Select_Device);

        if (Select_Device.Contains("Drone"))
        {
            
            type_c_d = false;
        }

        if (Select_Device.Contains("Car"))
        {
            type_c_d = true;
        }

        if (Select_Device != null)
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                Debug.Log("W");
                cmd_data = "forward 20";
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                Debug.Log("S");
                cmd_data = "back 20";
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                Debug.Log("A");
                cmd_data = "left 20";
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                Debug.Log("D");
                cmd_data = "right 20";
            }
            //////////////// Z, X, C only drone key//////////////////
            if (Input.GetKeyDown(KeyCode.Z))
            {
                //landoff
                Debug.Log("Z");
                cmd_data = "command";
            }

            if (Input.GetKeyDown(KeyCode.X))
            {
                Debug.Log("X");
                cmd_data = "takeoff";
            }
            if (Input.GetKeyDown(KeyCode.C))
            {
                Debug.Log("C");
                cmd_data = "land";
            }
            ////////// right key /////////////////////////////////// drone use i,k // robot i,k,j,l
            if (Input.GetKeyDown(KeyCode.I))
            {
                Debug.Log("I");
                cmd_data = "up 20";
            }

            if (Input.GetKeyDown(KeyCode.K))
            {
                Debug.Log("K");
                cmd_data = "down 20";
            }

            if (Input.GetKeyDown(KeyCode.J))
            {
                Debug.Log("J");
                cmd_data = "ccw 20";
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                Debug.Log("L");
                cmd_data = "cw 20";
            }

            if (cmd_data != null)
            {
                if (type_c_d)
                {
                    string cmd = null;
                    if (cmd_data.Contains("forward")) { cmd = "w"; }
                    else if (cmd_data.Contains("back")) { cmd = "s"; }
                    else if (cmd_data.Contains("left")) { cmd = "a"; }
                    else if (cmd_data.Contains("right")) { cmd = "d"; }
                    string link = $"http://192.168.50.254:17148/robot/control?robot_ip={get_ip}&cmd={cmd}";
                    
                    Debug.Log(link);
                    
                    StartCoroutine(GameManager.Comm.DeviceInitOn(link));
                }
                
                else if(!type_c_d)
                {
                    jsonDeviceData = CMDDebviceDefine(get_ip, cmd_data, "string");
                    StartCoroutine(GameManager.Comm.DevicePostRequest("http://192.168.50.254:17148/drone/control/", jsonDeviceData));
                }
                
            }

        }

    }

    public string GetRealIP(string ID)
    {
        string selectdevice_ip = null;

        if (ID == "Car1")
        {
            selectdevice_ip = "192.168.50.39"; //4
            //selectdevice_ip = "192.168.50.31"; //4
        }
        else if (ID == "Car2")
        {
            selectdevice_ip = "192.168.50.39"; //4
        }
        else if (ID == "Car3")
        {
            selectdevice_ip = "c";
        }
        else if (ID == "Drone1")
        {
            //selectdevice_ip = "192.168.50.202";
            //selectdevice_ip = "192.168.50.11"; //ÄÄÇ»ÅÍ
            selectdevice_ip = "192.168.50.11"; //test
        }
        else if (ID == "Drone2")
        {
            //selectdevice_ip = "192.168.50.13";
            selectdevice_ip = "192.168.50.13"; //test
        }
        else if (ID == "Drone3")
        {
            selectdevice_ip = "d";
        }
        return selectdevice_ip;

    }

    [System.Serializable]
    public class DroneData
    {
        public string id;
        public string cmd;
        public string description;
    }

    public string CMDDebviceDefine(string id, string cmd, string description)
    {

        OrderedDictionary updatedData = new OrderedDictionary
        {
            { "ip", id },
            { "cmd", cmd },
            { "description", description }
        };

        //var updatedData = new
        //{
        //    id = id,
        //    cmd = cmd,
        //    description = description
        //};

        //DroneData updatedData = new DroneData
        //{
        //    id = id,
        //    cmd = cmd,
        //    description = description
        //};


        return JsonConvert.SerializeObject(updatedData);
    }

}
