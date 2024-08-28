using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System;
using UnityEngine.UI;  // UI 컴포넌트를 사용하기 위해 추가

public class httpmodule : MonoBehaviour
{
    [System.Serializable]
    public class DroneData
    {
        public string drone_id;
        public int home;
        public int store;
        public string state;
    }

    public RawImage imageDisplay;  // Unity 인스펙터에서 할당해야 하는 RawImage 컴포넌트

    private void Start()
    {
        //DroneData data = new DroneData
        //{
        //    drone_id = "DR12345",
        //    home = 1,
        //    store = 2,
        //    state = "idle"
        //};

        //string jsonData = JsonConvert.SerializeObject(data);
        //StartCoroutine(PostRequest("http://192.168.50.254:17148/sim/state/", jsonData));
        ////StartCoroutine(GetImageStream("http://192.168.50.254:17148/drone/video"));  // 이미지 스트림 URL을 추가
        //// 서버로부터 클라이언트 데이터를 주기적으로 수신
        //StartCoroutine(GetClientData());
    }

    private IEnumerator PostRequest(string url, string jsonData)
    {
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error: " + request.error);
        }
        else
        {
            Debug.Log("Response: " + request.downloadHandler.text);
            DroneData responseData = JsonConvert.DeserializeObject<DroneData>(request.downloadHandler.text);
            Debug.Log("Received Data: \nDrone ID: " + responseData.drone_id +
                      "\nHome: " + responseData.home +
                      "\nStore: " + responseData.store +
                      "\nState: " + responseData.state);
        }
    }

    private IEnumerator GetImageStream(string imageUrl)
    {
        while (true)
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Image download error: " + request.error);
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                imageDisplay.texture = texture;  // RawImage 컴포넌트에 텍스처를 설정
            }
            yield return new WaitForSeconds(0.1f);  // 0.1초 간격으로 이미지 요청
        }
    }

    private IEnumerator GetClientData()
    {
        while (true)
        {
            UnityWebRequest request = UnityWebRequest.Get("http://192.168.50.75:13158/unity/receive");
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + request.error);
            }
            else
            {
                Debug.Log("Client Data: " + request.downloadHandler.text);
                if (request.downloadHandler.text.Contains("no_data"))
                {
                    Debug.Log("No client data received yet.");
                }
                else
                {
                    try
                    {
                        ObjectDefine clientData = JsonConvert.DeserializeObject<ObjectDefine>(request.downloadHandler.text);
                        Debug.Log("Client Data Received: \nDrone ID: " + clientData.id +
                                  "\nHome: " + clientData.home +
                                  "\nStore: " + clientData.store +
                                  "\nState: " + clientData.state);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Error parsing client data: " + e.Message);
                    }
                }
            }

            // 주기적으로 10초 대기 후 다시 요청
            yield return new WaitForSeconds(10);
        }
    }
}
