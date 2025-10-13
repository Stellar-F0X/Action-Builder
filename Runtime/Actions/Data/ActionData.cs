using System;
using System.Collections.Generic;
using UnityEngine;

namespace ActionBuilder.Runtime
{
    [Serializable]
    public struct ActionData
    {
        public ActionData(string name)
        {
            this.icon = null;
            this.name = name;
            this.developName = "";
            this.tag = "Default";
            this.description = "";
            this.effects = new List<EffectBase>();
            this.channelIds = new List<UGUID>();
        }
        
        public Sprite icon;
        public string developName;
        public string name;
        public string tag;
        public string description;

        [SerializeReference]
        public List<EffectBase> effects;
        public List<UGUID> channelIds;
    }
}