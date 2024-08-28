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
        outlineObj.name = prefab.name + index; // 이름을 빈칸으로 초기화

        Location_Data LocationInfo = outlineObj.AddComponent<Location_Data>();
        NodeType NodeTypeInfo = outlineObj.AddComponent<NodeType>();
        // ObjectInfo 컴포넌트를 추가하고 초기화
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



        /// 텍스트 컴포넌트를 먼저 찾아서 값을 변경한 후 비활성화
        Transform canvasMarksPanelTransform = outlineObj.transform.Find("CanvasMarks/Panel");

        Text textComponent = canvasMarksPanelTransform.GetComponentInChildren<Text>();
        textComponent.text = outlineObj.name;
        
        outlineObj.SetActive(true); // 처음부터 활성화된 상태로 유지
        index++;
        return outlineObj;
    }

}
