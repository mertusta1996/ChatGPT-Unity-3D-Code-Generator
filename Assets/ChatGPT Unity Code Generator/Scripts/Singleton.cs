using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Component
{
    [Header("Singleton Properties")] public bool DontDestroyOnLoading = false;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<T>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject(typeof(T).Name);
                    obj.AddComponent<T>();
                }
            }

            return _instance;
        }
    }

    private static T _instance;

    #region BuiltIn Methods

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            if (DontDestroyOnLoading)
            {
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }
        }
        else
        {
            if (_instance.GetInstanceID() != this.GetInstanceID())
            {
                Destroy(gameObject);
            }
        }
    }
    #endregion
}
