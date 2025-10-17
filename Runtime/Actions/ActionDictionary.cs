using System.Collections;
using System.Collections.Generic;

namespace ActionBuilder.Runtime
{
    public struct DoubleKeyValuePair<TKey1, TKey2, TValue>
    {
        public DoubleKeyValuePair(TKey1 key1, TKey2 key2, TValue value)
        {
            this.key1 = key1;
            this.key2 = key2;
            this.value = value;
        }
        
        public TKey1 key1;
        public TKey2 key2;
        public TValue value;
    }
    
    
    public class ActionDictionary : IEnumerable<DoubleKeyValuePair<int, string, ActionBase>>
    {
        private readonly Dictionary<int, ActionBase> _actions = new Dictionary<int, ActionBase>();

        private readonly Dictionary<string, ActionBase> _actionsByName = new Dictionary<string, ActionBase>();

        

        public ActionBase this[int index]
        {
            get { return _actions[index]; }
            
            set { _actions[index] = value; }
        }

        public ActionBase this[string name]
        {
            get { return _actionsByName[name]; }

            set { _actionsByName[name] = value; }
        }
        
        public int Count
        {
            get { return _actions.Count; }
        }



        public void Add(ActionBase action)
        {
            _actionsByName[action.name] = action;
            _actions[action.hash] = action;
        }

        
        public void Add(string name, ActionBase action)
        {
            if (this.ContainsKey(name))
            {
                return;
            }
            
            _actionsByName.Add(name, action);
        }


        public void Add(int key, ActionBase action)
        {
            if (this.ContainsKey(key))
            {
                return;
            }
            
            _actions.Add(key, action);
        }
        

        public bool Remove(ActionBase action)
        {
            return _actionsByName.Remove(action.name) && _actions.Remove(action.hash);
        }


        public bool Remove(int actionHash)
        {
            if (_actions.TryGetValue(actionHash, out ActionBase action))
            {
                _actionsByName.Remove(action.name);
                _actions.Remove(actionHash);
                return true;
            }

            return false;
        }


        public bool Remove(string name)
        {
            if (_actionsByName.TryGetValue(name, out ActionBase action))
            {
                _actionsByName.Remove(name);
                _actions.Remove(action.hash);
                return true;
            }

            return false;
        }


        public bool TryGetValue(int key, out ActionBase action)
        {
            return _actions.TryGetValue(key, out action);
        }


        public bool TryGetValue(string name, out ActionBase action)
        {
            return _actionsByName.TryGetValue(name, out action);
        }


        public bool ContainsKey(ActionBase action)
        {
            return _actions.ContainsKey(action.hash) || _actionsByName.ContainsKey(action.actionName);
        }


        public bool ContainsKey(int key)
        {
            return _actions.ContainsKey(key);
        }


        public bool ContainsKey(string name)
        {
            return _actionsByName.ContainsKey(name);
        }


        public void Clear()
        {
            _actions.Clear();
            _actionsByName.Clear();
        }


        public IEnumerator<DoubleKeyValuePair<int, string, ActionBase>> GetEnumerator()
        {
            foreach (KeyValuePair<int, ActionBase> pair in _actions)
            {
                yield return new DoubleKeyValuePair<int, string, ActionBase>(pair.Key, pair.Value.name, pair.Value); 
            }
        }

        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}