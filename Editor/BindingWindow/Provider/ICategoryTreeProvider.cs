using UnityEditor.Experimental.GraphView;

namespace StatController.Tool
{
    internal interface ICategoryTreeProvider
    {
        public SearchTreeEntry[] ProvideCategories(FactoryModule module);
    }
}