using System;
using ActionBuilder.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace ActionBuilder.Tool
{
    internal class TagFactoryModule : FactoryModule<TagBase> 
    {
        public TagFactoryModule(string title, int layer = 1) : base(typeof(TagBase), title, layer) { }
        
        protected override TagBase Create(Type type, Vector2 position, string entryName)
        {
            ScriptableObject createdObject = ScriptableObject.CreateInstance(type);
            Assert.IsNotNull(createdObject);
            
            const string resourcesRoot = "Assets/Resources";

            if (AssetDatabase.IsValidFolder(resourcesRoot) == false)
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            const string effectFolder = resourcesRoot + "/Tag";
            
            if (AssetDatabase.IsValidFolder(effectFolder) == false)
            {
                AssetDatabase.CreateFolder(resourcesRoot, "Tag");
            }
            
            createdObject.name = string.IsNullOrEmpty(entryName) ? type.Name : entryName;
            
            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{effectFolder}/{createdObject.name}.asset");
            
            AssetDatabase.CreateAsset(createdObject, assetPath);
            TagBase tagBase = createdObject as TagBase;
            Assert.IsNotNull(tagBase);
            return tagBase;
        }
    }
}