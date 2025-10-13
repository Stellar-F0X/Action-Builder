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
            this.hash = Animator.StringToHash(name);
            this.tag = "Default";
            this.description = "";
            this.effects = new List<EffectBase>();
            this.channelIds = new List<UGUID>();
        }
        
        public Sprite icon;
        public int hash;
        public string name;
        public string tag;
        public string description;

        [SerializeReference]
        public List<EffectBase> effects;
        public List<UGUID> channelIds;
    }
}