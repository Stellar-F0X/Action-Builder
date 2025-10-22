namespace ActionBuilder.Runtime
{
    public interface IPoolable
    {
        public bool isReadyInPool
        {
            get;
        } 
        
        
        public void OnBackToPool();
        
        
        public void OnGetFromPool();
    }
}