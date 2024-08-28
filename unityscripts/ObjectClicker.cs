using System;
using System.Collections;
using UnityEngine;


public class ObjectClicker : MonoBehaviour
{
    private CameraSwitch cameraSwitch;
    
    void Start()
    {
        cameraSwitch = FindObjectOfType<CameraSwitch>();

    }

    public void clickreturn()
    {

        Camera miniMapCamera = cameraSwitch.GetMiniMapCamera();
        Camera RE_Camera = cameraSwitch.GetRECamera();
        GameObject RE_GameObject = cameraSwitch.GetREGameObject();

        if (miniMapCamera != null)
        {
            miniMapCamera.enabled = true;
            miniMapCamera.tag = "MainCamera";
        }

        if (RE_GameObject != null)
        {
            Debug.Log($"Deactivating UI for: {RE_GameObject.name}");
            Transform uiCanvasTransform = RE_GameObject.transform.Find("UICanvas");
            
            if (uiCanvasTransform != null)
            {
                GameManager.Instance.ReturnClick = false;
                uiCanvasTransform.gameObject.SetActive(false); 
            }
            
        }

        if (RE_Camera != null)
        {
            RE_Camera.tag = "Untagged";
            RE_Camera.enabled = false;
        }

        cameraSwitch.SetCameraSwitched(false); // ī�޶� ��ȯ ���¸� ������� ����
    }


    
}
