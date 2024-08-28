using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.Events;


public class GameManager : MonoBehaviour
{
    static GameManager instance;
    public static GameManager Instance { get { Init(); return instance; } }

    //PlayerManager
    //PlayerManager _player_camera = new PlayerManager();
    //public static PlayerManager Input { get { return Instance._player_camera; } }

    RemoteInputManager remoteInputManager = new RemoteInputManager();
    public static RemoteInputManager remoteInput { get { return Instance.remoteInputManager; } }

    // communicationManager
    CommManager _commManager = new CommManager();
    public static CommManager Comm { get { return Instance._commManager; } }

    // prefab create/delete Manager
    PrefabManager _prefabManager = new PrefabManager();
    public static PrefabManager Prefab { get { return Instance._prefabManager; } }

    [SerializeField]
    public List<ObjectDefine> objectDefines = new List<ObjectDefine>();
    //public List<string> ObjectStringDefines = new List<string>();

    [SerializeField]
    public string jsonALLData;

    public UnityEvent OnDataUpdated;
    public UnityEvent RemoteVideoUpdated;
    public UnityEvent RemoteControlUpdated;

    public string _Mode = "Simulation";
    private string currentMode = null;
    private Coroutine modeCoroutine;
    public UnityEvent<string> OnModeChanged;

    public string Select_Device;

    private RawImageTexture rawImageTextureInstance;

    public bool ReturnClick = false;

    public string SelectIP = null;

    //private bool _status_ = false;
    private void Awake()
    {
        Init();
        StartCoroutine(DelayedPrefabControl());
    }

    private void Start()
    {

        // 이벤트 리스너 추가
        if (OnDataUpdated == null)
            OnDataUpdated = new UnityEvent();

        // 이벤트 리스너 추가
        if (RemoteVideoUpdated == null)
            RemoteVideoUpdated = new UnityEvent();

        // 이벤트 리스너 추가
        if (RemoteControlUpdated == null)
            RemoteControlUpdated = new UnityEvent();

        OnDataUpdated.AddListener(UpdateJsonDataAsync);

        RemoteVideoUpdated.AddListener(IsBlockChainRemote);
        RemoteControlUpdated.AddListener(StartRemoteControl);

        if (OnModeChanged == null)
            OnModeChanged = new UnityEvent<string>();

        OnModeChanged.AddListener(HandleModeChange);

        // 초기 모드 설정
        HandleModeChange(_Mode);
    }

    static void Init()
    {
        if (instance == null)
        {
            GameObject go = GameObject.Find("@GameManager");
            if (go == null)
            {
                go = new GameObject { name = "@GameManager" };
                go.AddComponent<GameManager>();

            }
            DontDestroyOnLoad(go);
            instance = go.GetComponent<GameManager>();
        }
    }

    IEnumerator DelayedPrefabControl()
    {
        
        
        yield return null; // 한 프레임 대기
        Prefab.PrefabContrl();
        UpdateJsonDataAsync();
        yield return null;
    }

    public static void AddObjectDefine(ObjectDefine objectDefine)
    {
        Instance.objectDefines.Add(objectDefine);
    }

    private void HandleModeChange(string newMode)
    {
        if (currentMode == newMode)
        {
            return; // 현재 모드와 동일하면 실행하지 않음
        }

        currentMode = newMode; // 현재 모드 업데이트

        // 현재 실행 중인 코루틴을 중지
        if (modeCoroutine != null)
        {
            StopCoroutine(modeCoroutine);
        }

        // 새로운 모드에 따라 코루틴을 시작
        if (newMode == "Simulation")
        {
            modeCoroutine = StartCoroutine(SimulationModeRoutine());
        }
        else if (newMode == "Remote")
        {
            modeCoroutine = StartCoroutine(RemoteModeRoutine());
        }
    }

    private IEnumerator SimulationModeRoutine()
    {
        
        Debug.Log("Simulation Mode");
        while (_Mode == "Simulation")
        {
            if (string.IsNullOrEmpty(jsonALLData))
            {

                UpdateJsonDataAsync();

            }

            //Debug.Log($"jsonAllData {jsonALLData}");
            yield return new WaitForSeconds(5f);
            StartCoroutine(Comm.PostRequest("http://192.168.50.254:17148/sim/state/", jsonALLData));

        }
    }

    private IEnumerator RemoteModeRoutine()
    {
        Debug.Log($"device {Select_Device}");
        string state = GameObject.Find(Select_Device).GetComponent<ObjectDefine>().state;
        Debug.Log($"state : {state} / device {Select_Device}");
        string pemKey = GameObject.Find(Select_Device).GetComponent<Player>().pemKey;
        string ip = Comm.GetDeviceIP(Select_Device);
        string device_realname = Re_Generate_Device_name(Select_Device);

        string blockdata = Comm.CreateData(ip, state);
        string signedData = Comm.SignData(blockdata, pemKey);
        string jsonData = Comm.CreateJson(ip, "remote", signedData, state);

        SelectIP = ip;

        StartCoroutine(Comm.BlockChainRemoteMode($"http://{ip}:9999/change-remote-mode", jsonData));

        yield return null; // 한 프레임 대기

       

    }

    public void IsBlockChainRemote()
    {
        if (Comm.isBlockchainRemote)
        {
            Debug.Log("Remote Mode");

            if (rawImageTextureInstance == null)
            {
                rawImageTextureInstance = FindObjectOfType<RawImageTexture>();

                if (rawImageTextureInstance == null)
                {
                    Debug.LogError("RawImageTexture instance is not found in the scene.");
                }
            }

            //blockchaing url로 변경예정
            //StartCoroutine(Comm.GetImageStream($"http://{ip}:9999/drone/video/{device_realname}"));
            StartCoroutine(Comm.GetImageStream($"http://{SelectIP}:9999/get-video"));
            //StartCoroutine(Comm.GetImageStream($"http://192.168.50.254:17148/drone/video/192.168.50.13")); //test


        }
    }

    public void StartRemoteControl()
    {
        StartCoroutine(RemoteControlKeyboard());
    }
    public IEnumerator RemoteControlKeyboard()
    {
        while (_Mode == "Remote")
        {
            //block
            remoteInputManager.OnUpdate(); // OnUpdate 메서드를 호출하여 키 입력 처리
            yield return null; // 한 프레임 대기
        }
    }
    public void SetImageTexture(Texture2D texture)
    {
        if (rawImageTextureInstance != null)
        {
            rawImageTextureInstance.GetImageTexture(texture);
        }
        else
        {
            Debug.LogError("RawImageTexture instance is not assigned.");
        }
    }
    public void UpdateJsonDataAsync()
    {
        StartCoroutine(UpdateJsonData());
    }

    public IEnumerator UpdateJsonData()
    {
        // 
        // id 
        // 각 오브젝트의 데이터를 종합하여 새로운 clientDataList 생성
        List<OrderedDictionary> updatedClientDataList = new List<OrderedDictionary>();
        foreach (ObjectDefine obj in objectDefines)
        {
            OrderedDictionary updatedData = new OrderedDictionary
            {
                { "id", obj.id },
                { "home", obj.home },
                { "store", obj.store },
                { "state", obj.state }
            };

            updatedClientDataList.Add(updatedData);
        }

        jsonALLData = JsonConvert.SerializeObject(updatedClientDataList);
        yield return null;
    }

    
    public void ChangeMode(string newMode)
    {
        if (_Mode != newMode || currentMode == null)
        {
            _Mode = newMode;
            OnModeChanged.Invoke(newMode);
        }
    }

    public string Re_Generate_Device_name(string selectdevicename)
    {

        if (selectdevicename == "Car1")
        {
            selectdevicename = "3JKCK2S00305WL";
        }
        else if (selectdevicename == "Car2")
        {
            selectdevicename = "3JKCK980030EKR";
        }
        else if (selectdevicename == "Car3")
        {
            selectdevicename = "b";
        }
        else if (selectdevicename == "Drone1")
        {
            selectdevicename = "tt-15";
        }
        else if (selectdevicename == "Drone2")
        {
            selectdevicename = "tt-17";
        }
        else if (selectdevicename == "Drone3")
        {
            selectdevicename = "tt-25";
        }

        return selectdevicename;
    }
}
