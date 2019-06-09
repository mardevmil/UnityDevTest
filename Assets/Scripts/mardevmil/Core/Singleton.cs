using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;

    public static T Instance { get { return instance; } }
    
    protected virtual void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogError("Created multiple instances of " + typeof(T) + " ! It will be removed!");
            Destroy(this.gameObject);
        }
        else
        {
            instance = FindObjectOfType<T>();
            if(instance != null)
            {
                instance.gameObject.name += " [Singleton]";
                DontDestroyOnLoad(instance);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
