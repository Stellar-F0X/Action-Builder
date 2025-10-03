using UnityEditor.Experimental.GraphView;

namespace StatController.Tool
{
    public interface ICategoryTreeProvider
    {
        public SearchTreeEntry[] ProvideCategories(FactoryModule module);
    }
}