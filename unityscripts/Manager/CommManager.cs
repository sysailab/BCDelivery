using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System;
using System.Collections.Specialized;
using System.Text;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;

public class CommManager
{
    public bool CreateCount = false;
    public bool isConnected = false;

    public Action<string, int, int, string> GetDataAction;

    public Action<string> TextUPdateAction;

    public List<OrderedDictionary> re_clientDataList = new List<OrderedDictionary>();
    public List<OrderedDictionary> pre_clientDataList;

    public bool isBlockchainRemote = false;


    //private bool first_time = false;
    //NetMQ public
    /*
    public PublisherSocket publisher;
    public SubscriberSocket subscriber;
    public Thread _clientThread;
    public readonly Action<string> _messageCallback;
    public bool _clientCancelled;
     */

    public void MethodCommuncation()
    {
        //1.UnityWebRequest Comm
        //2.WebRTC

        // ------ webgl x
        //1.TCP Comm
        //2.NetMQ Comm
        //3.UDP Comm

        isConnected = true;
        Debug.Log($"{isConnected} is Connected");
        if (!CreateCount)
        {
            CreateCount = true;
        }
        
    }


    public IEnumerator PostRequest(string url, string jsonData)
    {
        //test invoke
        //TextUPdateAction?.Invoke();
        UnityWebRequest request = new UnityWebRequest(url, "POST");
            
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();

        //first_time = true;
        yield return request.SendWebRequest();

        yield return new WaitForSeconds(0.04f);

        if (request.result != UnityWebRequest.Result.Success)
        {
            
            Debug.LogError("Error: " + request.error);
        }
        else
        {
            //Debug.Log($"Client Data : {request.downloadHandler.text}");
            if (request.downloadHandler.text.Contains("no_data"))
            {
                Debug.Log("아직 클라이언트 데이터가 수신되지 않았습니다.");
            }
            else
            {
                List<OrderedDictionary> clientDataList = null;
                try
                {
                    clientDataList = JsonConvert.DeserializeObject<List<OrderedDictionary>>(request.downloadHandler.text);
                }
                catch (Exception e)
                {
                    Debug.Log("클라이언트 데이터 파싱 중 오류 발생: " + e.Message);
                }

                if (clientDataList != null)
                {
                    //원래 코드
                    checkdData(clientDataList);

                }
                GameManager.Instance.OnDataUpdated.Invoke();
            }
        }
    }
  
    private void checkdData(List<OrderedDictionary> clientDataListPre)
    {
        foreach (OrderedDictionary data in clientDataListPre)
        {
            string id = data["id"].ToString();
            int home = int.Parse(data["home"].ToString());
            int store = int.Parse(data["store"].ToString());
            string state = data["state"].ToString();

            // 이전 상태를 re_clientDataList에서 찾기
            OrderedDictionary previousData = re_clientDataList.Find(d => d["id"].ToString() == id);
            if (previousData != null)
            {
                string previousState = previousData["state"].ToString();
                int previousHome = int.Parse(previousData["home"].ToString());
                int previousStore = int.Parse(previousData["store"].ToString());

                if (previousState == state && previousHome == home && previousStore == store)
                {
                    continue; // 이전 상태와 현재 상태가 모두 동일한 경우 건너뜀
                }

                // re_clientDataList에서 현재 상태로 업데이트
                previousData["home"] = home;
                previousData["store"] = store;
                previousData["state"] = state;
            }
            else
            {
                re_clientDataList.Add(data);
            }

            GetDataAction?.Invoke(id, home, store, state);
        }
    }

    //private static bool isImageStreamRunning = false;
    public IEnumerator GetImageStream(string imageUrl)
    {
        Texture2D videoTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        //if (isImageStreamRunning) yield break;

        //isImageStreamRunning = true;

        while (true)
        {
            UnityWebRequest request = UnityWebRequest.Get(imageUrl);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Image download error: " + request.error);
            }
            else
            {
                Debug.Log($"re {request.downloadHandler.data}");

                byte[] imageData = request.downloadHandler.data;
                //bool isLoaded = videoTexture.LoadImage(imageData);

                // Base64 디코딩
                //string base64String = request.downloadHandler.text;
                //Debug.Log($"re {base64String}");
                //byte[] imageData = Convert.FromBase64String(base64String);

                bool isLoaded = videoTexture.LoadImage(imageData);

                if (isLoaded)
                {
                    // GameManager에 텍스처 전달
                    GameManager.Instance.SetImageTexture(videoTexture);
                }
                else
                {
                    Debug.Log("Failed to load texture from image data");
                }
            }
            yield return new WaitForSeconds(0.1f);  // 0.1초 간격으로 이미지 요청
        }
    }

    //public static void StopImageStream()
    //{
    //    isImageStreamRunning = false;
    //}

    public IEnumerator DevicePostRequest(string url, string jsonData)
    {
        Debug.Log($"json Device Data {jsonData}");
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();

        yield return new WaitForSeconds(1f);

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("request.result: " + request.result);
            Debug.LogError(" request.downloadHandler.text: " + request.downloadHandler.text);
            Debug.LogError("Error: " + request.error);
        }
        else
        {
            Debug.Log("Response: " + request.downloadHandler.text);
            yield return null;
        }
    }

    public IEnumerator BlockChainRemoteMode(string url, string ModeData)
    {
        Debug.Log($"ModeData : {ModeData}");
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(ModeData);
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
                isBlockchainRemote = true;
                Debug.Log("Response: " + request.downloadHandler.text);
                GameManager.Instance.RemoteVideoUpdated.Invoke();
                GameManager.Instance.RemoteControlUpdated.Invoke();
            }

        }
    }

    public string GetDeviceIP(string ID)
    {
        string selectdevice_ip = null;

        if (ID == "Car1")
        {
            selectdevice_ip = "192.168.50.207";
            //selectdevice_ip = "192.168.50.205"; //4
        }
        else if (ID == "Car2")
        {
            //selectdevice_ip = "192.168.50.207";
            selectdevice_ip = "192.168.50.205";
        }
        else if (ID == "Car3")
        {
            selectdevice_ip = "c";
        }
        else if (ID == "Drone1")
        {
            //selectdevice_ip = "192.168.50.208"; //컴퓨터
            selectdevice_ip = "192.168.50.33"; //test
        }
        else if (ID == "Drone2")
        {
            selectdevice_ip = "192.168.50.203"; 
        }
        else if (ID == "Drone3")
        {
            selectdevice_ip = "d";
        }
        return selectdevice_ip;

    }
    public string CreateData(string id, string state)
    {
 
        return $"id: {id}, command: remote, node_type: usb, state: {state}";
    }

    public string SignData(string data, string pemKey)
    {
        var signer = new DataSigner(pemKey);
        return signer.SignData(data);
    }
    public string CreateJson(string id, string cmd,string signedData, string state)
    {

        // 서명된 데이터를 정수 배열로 변환
        byte[] signBytes = Convert.FromBase64String(signedData);
        int[] signArray = new int[signBytes.Length];
        for (int i = 0; i < signBytes.Length; i++)
        {
            signArray[i] = signBytes[i];
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

    public IEnumerator DeviceInitOn(string Link) // device cmd
    {

        UnityWebRequest request = UnityWebRequest.Get(Link);

        yield return request.SendWebRequest();

        Debug.Log(request.result);
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Image download error: " + request.error);
        }
        else
        {
            Debug.Log($"cmd : {request.downloadHandler.text}");
        }

        yield return null;

    }
}
