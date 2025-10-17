namespace ActionBuilder.Runtime
{
    public interface IPoolable
    {
        public void ReturnToPool();
        
        
        public void OnBackToPool();
        
        
        public void OnGetFromPool();
    }
}