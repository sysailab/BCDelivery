using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class PrefabControl_NotSimulationControl : MonoBehaviour
{
    [SerializeField]
    private GameObject prefab;

    int maxamount = 1;
    int index = 1;

    void Start()
    {
        GameManager.Prefab.PrefabCreateAction -= CreatePrefab;
        GameManager.Prefab.PrefabCreateAction += CreatePrefab;

        GameManager.Prefab.PrefabAddAction -= AddPrefab;
        GameManager.Prefab.PrefabAddAction += AddPrefab;
    }

    private void CreatePrefab()
    {
        //Debug.Log("CreatePrefab method called in PrefabControl");
        for (int i = 0; i < maxamount; i++)
        {
            GameObject prefabobject = CreateObject(prefab, gameObject.transform);
        }
    }

    private void AddPrefab()
    {
        GameObject prefabobject = CreateObject(prefab, gameObject.transform);
    }

    GameObject CreateObject(GameObject prefab, Transform PrefabPosition)
    {
        GameObject outlineObj = Instantiate(prefab, PrefabPosition, prefab.transform);
        outlineObj.name = prefab.name + index; // �̸��� ��ĭ���� �ʱ�ȭ

        Location_Data LocationInfo = outlineObj.AddComponent<Location_Data>();
        NodeType NodeTypeInfo = outlineObj.AddComponent<NodeType>();
        // ObjectInfo ������Ʈ�� �߰��ϰ� �ʱ�ȭ
        ObjectDefine objectInfo = outlineObj.AddComponent<ObjectDefine>();
        objectInfo.id = outlineObj.name;

        // Add the object info to the GameManager's list
        GameManager.AddObjectDefine(objectInfo);

        outlineObj.transform.position += new Vector3(0, 1, 0);
        if (index == 1)
        {
            outlineObj.transform.position += new Vector3(-3, 0, 0);
        }
        if (index == 3)
        {
            outlineObj.transform.position += new Vector3(3, 0, 0);
        }


        outlineObj.GetComponent<NavMeshAgent>().enabled = false;
        outlineObj.GetComponent<Player>().enabled = false;

        outlineObj.transform.Find("UICanvas").gameObject.SetActive(false);



        /// �ؽ�Ʈ ������Ʈ�� ���� ã�Ƽ� ���� ������ �� ��Ȱ��ȭ
        Transform canvasMarksPanelTransform = outlineObj.transform.Find("CanvasMarks/Panel");

        Text textComponent = canvasMarksPanelTransform.GetComponentInChildren<Text>();
        textComponent.text = outlineObj.name;
        
        outlineObj.SetActive(true); // ó������ Ȱ��ȭ�� ���·� ����
        index++;
        return outlineObj;
    }

}
