using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace StatController.Tool
{
    public class TypeTreeProvider : ICategoryTreeProvider
    {
        public TypeTreeProvider(bool bindSubClassTypes)
        {
            this._bindSubClassTypes = bindSubClassTypes;
        }

        
        private readonly bool _bindSubClassTypes;


        public SearchTreeEntry[] ProvideCategories(FactoryModule module)
        {
            Type[] typeList = this.GetTypes(module);
            
            SearchTreeEntry[] entries = new SearchTreeEntry[typeList.Length + 1];
            entries[0] = new SearchTreeGroupEntry(new GUIContent(module.title));
            entries[0].level = module.layer;

            for (int i = 1; i < entries.Length; ++i)
            {
                string typeName = ObjectNames.NicifyVariableName(typeList[i - 1].Name);
                entries[i] = new SearchTreeEntry(new GUIContent(typeName));
                entries[i].userData = (typeList[i - 1], module);
                entries[i].level = module.layer + 1;
            }

            return entries;
        }


        private Type[] GetTypes(FactoryModule module)
        {
            if (this._bindSubClassTypes)
            {
                return this.OrderByNameAndFilterAbstracts(TypeCache.GetTypesDerivedFrom(module.targetType));
            }
            else
            {
                return new Type[1] { module.targetType };
            }
        }


        private Type[] OrderByNameAndFilterAbstracts(TypeCache.TypeCollection collection)
        {
            Type[] array = collection.Where(t => t.IsAbstract || t.IsGenericType).ToArray();

            if (array.Length <= 1)
            {
                return array;
            }
            else
            {
                Array.Sort(array, (a, b) => a.Name[0].CompareTo(b.Name[0]));
                return array;
            }
        }
    }
}