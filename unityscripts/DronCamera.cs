using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class DronCamera : MonoBehaviour
{
    [SerializeField]
    GameObject _drone;

    private void Update()
    {
        // 카메라의 위치를 타겟 위치 + 오프셋으로 설정
        transform.position = _drone.transform.position + new Vector3(0,1,-3);

        // 타겟을 바라보도록 회전
        transform.LookAt(_drone.transform);
    }
}
