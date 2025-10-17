using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ActionBuilder.Runtime
{
    [DefaultExecutionOrder(-10)]
    public class Singleton<TSingleton> where TSingleton : Singleton<TSingleton>.Singletonable
    {
        public class Singletonable : MonoBehaviour
        {
            protected void Awake()
            {
                if (_instance != null && _instance != this)
                {
                    Object.Destroy(this);
                    return;
                }

                _instance = this as TSingleton;
                IsInitialized = true;
                OnInitialized?.Invoke(_instance);
                Object.DontDestroyOnLoad(this.gameObject);
                this.OnMonoAwake();
            }

            protected void OnDestroy()
            {
                this.OnMonoDestroy();
                OnDestroyed?.Invoke(_instance);
                Object.DestroyImmediate(_instance);
            }
        
            protected void OnApplicationQuit()
            {
                this.OnMonoDestroy();
                OnDestroyed?.Invoke(_instance);
            }
            
            
            protected virtual void OnMonoAwake() { }
            
            
            protected virtual void OnMonoDestroy() { } 
            
            
            protected virtual void OnMonoQuit() { }
        }
        
        
        public static event Action<TSingleton> OnInitialized;

        public static event Action<TSingleton> OnDestroyed;
        
        private static TSingleton _instance;
        
        
        public static TSingleton Instance
        {
            get { return _instance = _instance != null ? _instance : MakeOrFindObject(); }
        }


        public static bool IsInitialized
        {
            get;
            private set;
        }
        


        private static TSingleton MakeOrFindObject()
        {
            _instance = Object.FindAnyObjectByType<TSingleton>();

            if (_instance == null)
            {
                GameObject newGameObject = new GameObject(typeof(TSingleton).Name);
                _instance = newGameObject.AddComponent<TSingleton>();
            }

            return _instance;
        }
    }
}