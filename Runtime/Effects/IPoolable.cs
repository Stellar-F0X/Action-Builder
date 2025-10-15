namespace ActionBuilder.Runtime
{
    public interface IPoolable
    {
        public void ResetOnPoolReturn();
        
        public void OnBackToPool();
        
        public void OnGetFromPool();
    }
}