using UnityEditor.Experimental.GraphView;

namespace ActionBuilder.Tool
{
    internal interface ICategoryTreeProvider
    {
        public SearchTreeEntry[] ProvideCategories(FactoryModule module);
    }
}