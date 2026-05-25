using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ObjectToEnable
{
    public GameObject obj;
    public float delay;
}

public class EnableAfterDelay : MonoBehaviour
{
    [SerializeField] private List<ObjectToEnable> objects;

    private void Start()
    {
        foreach(ObjectToEnable obj in objects)
        {
            StartCoroutine(EnableRoutine(obj));
        }
    }

    private IEnumerator EnableRoutine(ObjectToEnable objData)
    {
        objData.obj.SetActive(false);
        yield return new WaitForSeconds(objData.delay);
        objData.obj.SetActive(true);
    }
}