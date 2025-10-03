using StatController.Runtime;
using UnityEngine.UIElements;
using UnityEditor;

namespace StatController.Tool
{
    [CustomEditor(typeof(StatsSet))]
    public class StatDictionaryDrawer : Editor
    {
        public VisualTreeAsset visualTreeAsset;
        
        private SerializedProperty _statKeysProperty;
        private SerializedProperty _statsProperty;


        public override VisualElement CreateInspectorGUI()
        {
            return visualTreeAsset.CloneTree();
        }
    }
}