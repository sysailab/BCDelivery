using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.AI;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;
using UnityEngine.UI;

public class BlockChainDefine
{
    public string id;
    public string command;
    public int[] sign;
    public string node_type;
    public string state;
}
public class Player : MonoBehaviour
{
    private NavMeshAgent _agent;
    private Rigidbody _rigidbody;

    //private string pemKeyPath = "Assets/usb_private_key.pem";
    private string pemKeyPath = Path.Combine(Application.streamingAssetsPath, "usb_private_key.pem");
    public  string pemKey;

    //[SerializeField]
    public Transform hometarget;
    public Transform storetarget;

    private Transform secondfloor;

    public float fallSpeed = 3.0f; // 오브젝트의 내려가는 속도
    public float riseSpeed = 3.0f; // 오브젝트의 올라가는 속도

    private Vector3 initialPosition; // 기기가 생성된 자리

    public bool DorC = false;
    public bool ontimeflag = false;
    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _rigidbody = GetComponent<Rigidbody>();
        initialPosition = transform.position;

        StartCoroutine(LoadPemKey("usb_private_key.pem"));
        //pemKey = LoadPemKey(pemKeyPath);
        //if (pemKey == null)
        //{
        //    Debug.LogError("PEM key loading failed.");
        //    return;
        //}
    }

    private void Start()
    {
        _agent.enabled = false;
        if (_agent.name.Contains("Drone"))
        {
            secondfloor = GameObject.Find("DroneTrack/all").transform;
            _rigidbody.isKinematic = true; // Rigidbody를 처음에는 비활성화
        }
    }

    //private string LoadPemKey(string path)
    //{
    //    try
    //    {
    //        string pemKey = File.ReadAllText(path);
    //        if (string.IsNullOrEmpty(pemKey))
    //        {
    //            test_mode.text = $"PEM key file is empty or not found at path: {path}";
    //            Debug.LogError($"PEM key file is empty or not found at path: {path}");
    //            return null;
    //        }
    //        return pemKey;
    //    }
    //    catch (IOException ex)
    //    {
    //        test_mode.text = $"E {ex.Message}";
    //        Debug.LogError($"Error reading PEM key file: {ex.Message}");
    //        return null;
    //    }
    //}

    private IEnumerator LoadPemKey(string path)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, path);

        UnityWebRequest request = UnityWebRequest.Get(filePath);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {

            Debug.LogError($"Error reading PEM key file: {request.error}");
            pemKey = null;
        }
        else
        {
            pemKey = request.downloadHandler.text;
            if (string.IsNullOrEmpty(pemKey))
            {
                Debug.LogError($"PEM key file is empty or not found at path: {filePath}");
                pemKey = null;
            }
        }
    }

    // Simulation Input
    public void PrefabUpdate(string ID, string state)
    {
        Debug.Log($"GameObject Name PrefabUpdate : {gameObject.name}");
        string selectdevice_ip = null;

        if (ID == "Car1")
        {
            selectdevice_ip = "192.168.50.207";
            //selectdevice_ip = "192.168.50.205";
            DorC = false;
            BlockChainFunc(selectdevice_ip, state);
        }
        else if (ID == "Car2")
        {
            //selectdevice_ip = "192.168.50.207";
            selectdevice_ip = "192.168.50.205";
            DorC = false;
            BlockChainFunc(selectdevice_ip, state);
        }
        else if (ID == "Car3")
        {
            selectdevice_ip = "c";
        }
        else if (ID == "Drone1")
        {
            //selectdevice_ip = "192.168.50.208";
            selectdevice_ip = "192.168.50.33"; //test
            DorC = true;
            BlockChainFunc(selectdevice_ip, state);

        }
        else if (ID == "Drone2")
        {
            selectdevice_ip = "192.168.50.203";
            DorC = true;
            BlockChainFunc(selectdevice_ip, state);
        }
        else if (ID == "Drone3")
        {
            selectdevice_ip = "d";
        }

        
    }

    private void BlockChainFunc(string selectdevice_ip, string state)
    {

        string blockdata = CreateData(selectdevice_ip, state);

        string signedData = SignData(blockdata, pemKey);
    
        string jsonData = CreateJson(selectdevice_ip, signedData, state);


        Debug.Log($"selectdevice_ip : {selectdevice_ip} / blockdata {blockdata} / state {state}");

        StartCoroutine(BlockChainPost($"http://{selectdevice_ip}:9999/add-block", jsonData));
    }
    private string CreateData(string id, string state)
    {
        string cmd =  null;
        if (state != null)
        {
            if (DorC)
            {
                if (state == "DELIVERY")
                {
                    cmd = "takeoff";
                }
                else if (state == "STAY")
                {
                    cmd = "land";
                }
            }
            else 
            {
                if (state == "DELIVERY")
                {
                    cmd = "w";
                }
                else if (state == "STAY")
                {
                    cmd = "s";
                }
            }
            
        }
        return $"id: {id}, command: {cmd}, node_type: usb, state: {state}";
    }

    private string SignData(string data, string pemKey)
    {
        var signer = new DataSigner(pemKey);
        return signer.SignData(data);
    }
    private string CreateJson(string id, string signedData, string state)
    {
        string cmd = null;
        // 서명된 데이터를 정수 배열로 변환
        byte[] signBytes = Convert.FromBase64String(signedData);
        int[] signArray = new int[signBytes.Length];
        for (int i = 0; i < signBytes.Length; i++)
        {
            signArray[i] = signBytes[i];
        }

        if (state != null)
        {
            if (DorC)
            {
                if (state == "DELIVERY")
                {
                    cmd = "takeoff";
                }
                else if (state == "STAY")
                {
                    cmd = "land";
                }
            }
            else
            {
                if (state == "DELIVERY")
                {
                    cmd = "w";
                }
                else if (state == "STAY")
                {
                    cmd = "s";
                }
            }
        }
        // JSON 데이터 생성
        return JsonUtility.ToJson(new BlockChainDefine
        {
            id = id,
            command = cmd,
            sign = signArray,
            node_type = "usb",
            state = state
        });
    }
    public IEnumerator BlockChainPost(string url, string jsonData)
    {

        //simulation만 테스트 시 주석 코드
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("request.result: " + request.result);
            Debug.LogError(" request.downloadHandler.text: " + request.downloadHandler.text);
            Debug.LogError("Error: " + request.error);
            
        }
        else
        {

            // JSON 응답을 파싱하여 result 값 확인
            var responseJson = JObject.Parse(request.downloadHandler.text);
            bool result = responseJson["result"].Value<bool>();

            if (result)
            {
                Debug.Log("Response: " + request.downloadHandler.text);
                StartBlockchaing_();
                yield return null;
            }

        }
        ////////////////////

        //밑에꺼만 주석 해제하면 simulation만 테스트 가능
        //StartBlockchaing_();
        yield return new WaitForSeconds(3f);
    }

    public void StartBlockchaing_()
    {
        var objectDefine = GetComponent<ObjectDefine>();
        if (objectDefine.state == "ACCIDENT" || objectDefine.state == "CANCEL")
        {
            _agent.isStopped = true;
            _agent.updatePosition = false;
            _agent.updateRotation = false;
            Debug.Log("Device has encountered an accident or cancellation and has stopped.");
            if (objectDefine.state == "CANCEL")
            {
                // 기기가 생성된 자리로 돌아가도록 처리
                StartCoroutine(ReturnToInitialPosition());
            }
            return;
        }


        GameObject findObject_home = GameObject.Find("Arrive" + objectDefine.home);
        GameObject findObject_store = GameObject.Find("Arrive" + objectDefine.store);

        if (findObject_home != null || findObject_store != null)
        {
            hometarget = findObject_home.transform;
            storetarget = findObject_store.transform;
            if (_agent.name.Contains("Drone"))
            {
                StartRising();
                DroneF(hometarget.position);
            }

            if (_agent.name.Contains("Car"))
            {
                StartCoroutine(CarF());
            }
        }
        //else
        //{
        //    Debug.LogWarning("Target object not found!");
        //}
    }

    // Car Code
    IEnumerator CarF()
    {
        yield return null;
        _agent.enabled = true;
        _agent.isStopped = false;
        _agent.updatePosition = true;
        _agent.updateRotation = true;
        _agent.SetDestination(hometarget.position);

        StartCoroutine(CheckCarArrival());
        
    }

    IEnumerator CheckCarArrival()
    {
        gameObject.GetComponent<BoxCollider>().enabled = false;
        _rigidbody.isKinematic = true;
        while (_agent.enabled && (_agent.pathPending || _agent.remainingDistance > _agent.stoppingDistance))
        {
            yield return null;
        }

        while (_agent.enabled && _agent.velocity.sqrMagnitude > 0f)
        {
            yield return null;
        }

        if (_agent.enabled)
        {
            _agent.isStopped = true;
            _agent.updatePosition = false;
            _agent.updateRotation = false;
        }

        // 10초 대기 후 새로운 목적지로 이동
        yield return new WaitForSeconds(5f);
        MoveToStoreTarget();
    }

    private void MoveToStoreTarget()
    {
        _agent.isStopped = false;
        _agent.updatePosition = true;
        _agent.updateRotation = true;
        _agent.SetDestination(storetarget.position);
        StartCoroutine(CheckStoreArrival());
    }

    IEnumerator CheckStoreArrival()
    {
        while (_agent.enabled && (_agent.pathPending || _agent.remainingDistance > _agent.stoppingDistance))
        {
            yield return null;
        }

        while (_agent.enabled && _agent.velocity.sqrMagnitude > 0f)
        {
            yield return null;
        }

        if (_agent.enabled)
        {
            _agent.isStopped = true;
            _agent.updatePosition = false;
            _agent.updateRotation = false;
        }

        gameObject.GetComponent<BoxCollider>().enabled = true;

        StartCoroutine(HandleDeviceAtStore());
    }

    IEnumerator HandleDeviceAtStore()
    {
        yield return null;
        var objectDefine = gameObject.GetComponent<ObjectDefine>();
        objectDefine.home = 0;
        objectDefine.store = 0;
        objectDefine.state = "STAY";

        // 목표 위치 초기화
        hometarget = null;
        storetarget = null;

        // GameManager의 OnDataUpdated 이벤트 호출
        GameManager.Instance.OnDataUpdated.Invoke();

        Debug.Log("OnDataUpdated event invoked.");
    }

    // Drone Code
    private void DroneF(Vector3 targetPosition)
    {
        GetComponent<DronePropelers>().enabled = true;
        StartCoroutine(VerticalRise(targetPosition));
    }

    void StartRising()
    {

        _rigidbody.isKinematic = true; // Rigidbody 활성화
        _rigidbody.useGravity = false; // 중력 비활성화  
    }

    IEnumerator VerticalRise(Vector3 targetPosition)
    {
        while (transform.position.y < secondfloor.position.y)
        {
            transform.Translate(Vector3.up * riseSpeed * Time.deltaTime, Space.World);
            yield return null; // 한 프레임 대기
        }

        // NavMeshAgent 설정
        _agent.enabled = true; // NavMeshAgent 활성화
        _agent.updatePosition = true; // NavMeshAgent의 위치 업데이트 활성화
        _agent.updateRotation = true; // NavMeshAgent의 회전 업데이트 활성화
        _agent.isStopped = false; // NavMeshAgent 활성화

        _rigidbody.isKinematic = true; // NavMeshAgent와 충돌 방지

        _agent.SetDestination(targetPosition);
        StartCoroutine(CheckArrival(targetPosition));
    }

    IEnumerator CheckArrival(Vector3 targetPosition)
    {
        while (_agent.enabled && (_agent.pathPending || _agent.remainingDistance > _agent.stoppingDistance))
        {
            yield return null;
        }

        while (_agent.enabled && _agent.velocity.sqrMagnitude > 0f)
        {
            yield return null;
        }


        _agent.isStopped = true;

        StartFalling();
        StartCoroutine(VerticalDescent(targetPosition));
    }

    void StartFalling()
    {

        _agent.updatePosition = false; // NavMeshAgent의 위치 업데이트 중지
        _agent.updateRotation = false; // NavMeshAgent의 회전 업데이트 중지
        _rigidbody.isKinematic = false; // Rigidbody 비활성화
        _rigidbody.useGravity = true; // 중력 사용
    }

    IEnumerator VerticalDescent(Vector3 targetPosition)
    {
        while (transform.position.y > targetPosition.y + 3f) // 약간의 여유를 줘서 땅속에 안가라앉도록
        {
            transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);
            yield return null; // 한 프레임 대기
        }

        while (transform.position.y > targetPosition.y)
        {
            yield return null; // 한 프레임 대기
        }

        GetComponent<DronePropelers>().enabled = false;

        _rigidbody.isKinematic = true; // Rigidbody를 다시 활성화
        _rigidbody.useGravity = false; // 중력 비활성화

        
        if (targetPosition == hometarget.position)
        {
            Debug.Log($"드론이 home 착륙했습니다.");
            // 착륙 후 5초 대기
            yield return new WaitForSeconds(5f);
            StartRising();
            DroneF(storetarget.position); // 상점 타겟으로 다시 떠오르기
        }
        else if (targetPosition == storetarget.position)
        {
            Debug.Log("드론이 store 착륙했습니다.");
            StartCoroutine(HandleDeviceAtStore());
        }
    }

    IEnumerator ReturnToInitialPosition()
    {
        // 드론일 경우 상승 후 초기 위치로 이동
        if (_agent.name.Contains("Drone"))
        {
            StartRising();
            StartCoroutine(VerticalRise(initialPosition));
        }
        else if (_agent.name.Contains("Car"))
        {
            // 자동차일 경우 바로 초기 위치로 이동
            _agent.enabled = true;
            _agent.isStopped = false;
            _agent.updatePosition = true;
            _agent.updateRotation = true;
            _agent.SetDestination(initialPosition);

            while (_agent.enabled && (_agent.pathPending || _agent.remainingDistance > _agent.stoppingDistance))
            {
                yield return null;
            }

            while (_agent.enabled && _agent.velocity.sqrMagnitude > 0f)
            {
                yield return null;
            }

            _agent.isStopped = true;
            _agent.updatePosition = false;
            _agent.updateRotation = false;
        }

        // 초기 위치에 도착한 후 상태를 STAY로 변경
        var objectDefine = gameObject.GetComponent<ObjectDefine>();
        objectDefine.state = "STAY";

        // GameManager의 OnDataUpdated 이벤트 호출
        GameManager.Instance.OnDataUpdated.Invoke();

        Debug.Log("기기가 초기 위치로 돌아와 STAY 상태로 변경되었습니다.");
    }
}
