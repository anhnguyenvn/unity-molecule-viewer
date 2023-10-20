using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;

    // A hackaway to avoid instancing singleton on disable or destroy method (mostly removing event subscription)
    public static bool IsAlive
    {
        get
        {
            return isAlive;
        }
    }
    static bool isAlive = true;
    protected void OnApplicationQuit()
    {
        Debug.Log("Applciation quittt****");
        isAlive = false;
    }
    //private void OnDestroy()
    //{
    //    Destroy(instance.gameObject);
    //    instance = null;
    //}

    /// <summary>
    ///
    /// </summary>
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.FindObjectOfType<T>();

                if (instance == null)
                {
                    var instanceGO = new GameObject();
                    instance = instanceGO.AddComponent<T>();
                    instanceGO.name = typeof(T).ToString() + " (Singleton)";
                    DontDestroyOnLoad(instanceGO);
                }
            }
            return instance;
        }
    }
}