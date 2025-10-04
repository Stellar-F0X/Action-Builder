using UnityEngine;

namespace StatController.Runtime
{
    [DefaultExecutionOrder(-1)]
    public class StatController : MonoBehaviour
    {
        [SerializeField]
        private StatsSet _statsSet;
        protected StatsSetInstance _runtimeStats;

        
        protected void Awake()
        {
            Debug.Assert(_statsSet == null, "Stats set is null");
            
            _runtimeStats = _statsSet.CreateInstance();
        }


        private void Update() 
        {
            
        }


        private void Start()
        {
            
        }
    }
}
