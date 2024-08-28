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
        // ī�޶��� ��ġ�� Ÿ�� ��ġ + ���������� ����
        transform.position = _drone.transform.position + new Vector3(0,1,-3);

        // Ÿ���� �ٶ󺸵��� ȸ��
        transform.LookAt(_drone.transform);
    }
}
