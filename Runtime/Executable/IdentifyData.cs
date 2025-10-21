using System;
using UnityEngine;

namespace ActionBuilder.Runtime
{
    [Serializable]
    public struct IdentifyData
    {
        public IdentifyData(string name)
        {
            this.icon = null;
            this.name = name;
            this.tag = "Default";
            this.description = "";
            this.hash = Animator.StringToHash(name);
        }
        
        public Sprite icon;
        public int hash;
        public string name;
        public string tag;
        public string description;
    }
}