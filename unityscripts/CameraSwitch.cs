using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI; // UI ������Ʈ�� ����ϱ� ���� �߰�

public class CameraSwitch : MonoBehaviour
{
    private Camera miniMapCamera;
    private string miniMapCameraname = "MinimapCamera";

    private Camera SelectCamera;
    private Camera RE_Camera;
    private GameObject RE_GameObject;

    private Coroutine updateStateCoroutine;
    private Coroutine updateNodeCoroutine;

    private bool isCameraSwitched = false; // ī�޶� ��ȯ ���¸� ��Ÿ���� �÷���

    void Start()
    {
        // MiniMap Camera ã�� �� Ȱ��ȭ
        miniMapCamera = Camera.main; // MainCamera �±װ� �ִ� ī�޶�
        if (miniMapCamera != null)
        {
            miniMapCameraname = miniMapCamera.name; // MiniMap Camera�� �̸��� ����
        }
        else
        {
            Debug.LogError("MiniMap Camera not found or not tagged as MainCamera");
        }
    }

    void Update()
    {
        // ī�޶� ��ȯ�� ���°� �ƴϸ� Ŭ�� �̺�Ʈ�� ó��
        if (!isCameraSwitched && Input.GetMouseButtonDown(0))
        {
            // ī�޶󿡼� ���콺 ��ġ�� ����ĳ��Ʈ
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // Ŭ���� ������Ʈ�� �̸� ���
                GameObject clickedObject = hit.collider.gameObject;
                Transform parentTransform = clickedObject.transform.parent;

                if (parentTransform != null && parentTransform.gameObject.name != miniMapCameraname)
                {
                    GameObject CameraSelectObject = parentTransform.gameObject;
                    if (CameraSelectObject != null)
                    {
                        Camera newSelectCamera = CameraSelectObject.GetComponentInChildren<Camera>(true);
                        if (newSelectCamera != null && newSelectCamera != SelectCamera)
                        {
                            SelectCamera = newSelectCamera;
                            RE_Camera = SelectCamera;
                            RE_GameObject = CameraSelectObject;
                            Debug.Log($"{RE_GameObject.name} -----------******------------");

                            Transform uiCanvasTransform = CameraSelectObject.transform.Find("UICanvas");
                            if (uiCanvasTransform != null && !uiCanvasTransform.gameObject.activeSelf)
                            {
                                //GameManager.Comm.TextUPdateAction?.Invoke(RE_GameObject.name);
                                GameManager.Instance.ReturnClick = true;
                                uiCanvasTransform.gameObject.SetActive(true);
                                // Panel �Ʒ��� �ִ� ID, Home, Store UI ��Ҹ� ã��
                                
                                Transform panelTransform = uiCanvasTransform.Find("Panel");
                                if (panelTransform != null)
                                {
                                    Connect_Device_Class(parentTransform, panelTransform, RE_GameObject.name);
                                    StartCoroutine(UPdateState(parentTransform, panelTransform));
                                }
                            }

                            SwitchCamera();
                            isCameraSwitched = true; // ī�޶� ��ȯ ���·� ����
                        }
                    }
                }
            }
        }
    }
    public void Connect_Device_Class(Transform parentTransform, Transform panelTransform, string selectdevice)
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
            //updateStateCoroutine = StartCoroutine(UpdateLocation(parentTransform, panelTransform, $"http://{selectdevice_ip}/get-location"));
            //updateNodeCoroutine = StartCoroutine(UpdateNodeList(parentTransform,panelTransform, $"http://{selectdevice_ip}/get-nodes"));
            //StartCoroutine(UpdateText("http://192.168.50.113:9999/get-location"));
            //StartCoroutine(UpdateNodeList("http://192.168.50.113:9999/get-nodes"));
        }
        else
        {
            StopUpdateStateCoroutine();
        }

    }
    public IEnumerator UPdateState(Transform parentTransform, Transform panelTransform)
    {
        while (true)
        {
            Text idText = panelTransform.Find("ID")?.GetComponent<Text>();
            Text homeText = panelTransform.Find("HOME")?.GetComponent<Text>();
            Text storeText = panelTransform.Find("STORE")?.GetComponent<Text>();

            if (idText != null) idText.text = parentTransform.GetComponent<ObjectDefine>().id;
            if (homeText != null) homeText.text = parentTransform.GetComponent<ObjectDefine>().home.ToString();
            if (storeText != null) storeText.text = parentTransform.GetComponent<ObjectDefine>().store.ToString();
            yield return null;
        }

    }

    public IEnumerator UpdateLocation(Transform parentTransform,Transform panelTransform, string Url)
    {
        while (true)
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

                        parentTransform.GetComponent<Location_Data>().x = data.x;
                        parentTransform.GetComponent<Location_Data>().y = data.y;
                        parentTransform.GetComponent<Location_Data>().z = data.z;

                        Text LocationText = panelTransform.Find("Lotation")?.GetComponent<Text>();

                        if (LocationText != null) 
                        {
                            LocationText.text = $"x : ({data.x}) , y : ({data.y}) , z : ({data.z})";
                        }
                    }
                }

            }

            yield return null;
        }

    }

    public IEnumerator UpdateNodeList(Transform parentTransform,Transform panelTransform, string Url)
    {
        while (true)
        {
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
                        parentTransform.GetComponent<NodeType>().address = data.address;
                        parentTransform.GetComponent<NodeType>().node_type = data.node_type;

                        Text NodelistText = panelTransform.Find("NODELIST")?.GetComponent<Text>();

                        if (NodelistText != null)
                        {
                            NodelistText.text = $"{data.node_type} : {data.address}";
                        }
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
    private void SwitchCamera()
    {
        if (SelectCamera != null)
        {
            ActivateCamera(SelectCamera);
        }
        else
        {
            Debug.LogError("Camera not found or not set yet");
        }
    }

    public void ActivateCamera(Camera cameraToActivate)
    {
        // ���� Ȱ��ȭ�� ��� ī�޶� ��Ȱ��ȭ
        Camera[] allCameras = Camera.allCameras;
        foreach (Camera cam in allCameras)
        {
            cam.enabled = false;
            cam.tag = "Untagged";
        }

        // ���ο� ī�޶� Ȱ��ȭ
        cameraToActivate.enabled = true;
        cameraToActivate.tag = "MainCamera"; // ���ο� Ȱ��ȭ�� ī�޶� MainCamera �±� ����
    }

    public void SetCameraSwitched(bool state)
    {
        isCameraSwitched = state;
    }

    public Camera GetMiniMapCamera()
    {
        return miniMapCamera;
    }

    public Camera GetRECamera()
    {
        return RE_Camera;
    }

    public GameObject GetREGameObject()
    {
        return RE_GameObject;
    }
}
