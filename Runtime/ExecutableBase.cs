using UnityEngine;

namespace ActionBuilder.Runtime
{
    public class ExecutableBase : ScriptableObject
    {
        [SerializeField, Space(-15)]
        protected IdentifyData _identifyData;

        protected float _elapsedTime;
        
        
        public float elapsedTime
        {
            get { return _elapsedTime; }
        }
        
        
        public Sprite icon
        {
            get { return _identifyData.icon; }
        }

        public new string name
        {
            get { return _identifyData.name; }
            set { hash = Animator.StringToHash((base.name = _identifyData.name = value)); }
        }

        public string description
        {
            get { return _identifyData.description; }
        }

        public int hash
        {
            get { return _identifyData.hash; }
            
            private set { _identifyData.hash = value; }
        }

        public string tag
        {
            get { return _identifyData.tag; }
        }
        
        public Transform transform
        {
            get { return this.controller.transform; }
        }

        public GameObject gameObject
        {
            get { return this.controller.gameObject; }
        }
        
        public ActionController controller
        {
            get;
            set;
        }
    }
}