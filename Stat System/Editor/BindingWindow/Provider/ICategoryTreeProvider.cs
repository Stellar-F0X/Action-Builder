using UnityEditor.Experimental.GraphView;

namespace StatSystem.Tool
{
    internal interface ICategoryTreeProvider
    {
        public SearchTreeEntry[] ProvideCategories(FactoryModule module);
    }
}