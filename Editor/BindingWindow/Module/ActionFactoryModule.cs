using System;
using ActionBuilder.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace ActionBuilder.Tool
{
    internal class ActionFactoryModule : FactoryModule<ActionBase>
    {
        public ActionFactoryModule(string title, int layer = 1) : base(typeof(ActionBase), title, layer) { }

        protected override ActionBase Create(Type type, Vector2 position, string entryName)
        {
            ScriptableObject createdObject = ScriptableObject.CreateInstance(type);
            Assert.IsNotNull(createdObject);
            
            const string resourcesRoot = "Assets/Resources";

            if (AssetDatabase.IsValidFolder(resourcesRoot) == false)
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            const string actionsFolder = resourcesRoot + "/Actions";
            
            if (AssetDatabase.IsValidFolder(actionsFolder) == false)
            {
                AssetDatabase.CreateFolder(resourcesRoot, "Actions");
            }
            
            createdObject.name = string.IsNullOrEmpty(entryName) ? type.Name : entryName;
            
            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{actionsFolder}/{createdObject.name}.asset");
            
            AssetDatabase.CreateAsset(createdObject, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            return createdObject as ActionBase;
        }
    }
}