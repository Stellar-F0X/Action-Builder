using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ActionBuilder.Runtime
{
    public partial class MonoObserver : Singleton<MonoObserver>.Singletonable
    {
        private readonly List<ObserveData> _observeList = new List<ObserveData>();


        private int observeCount
        {
            get { return this._observeList.Count; }
        }



        public void Register(Object subject, Func<bool> condition, Action<Object> onDisabled = null)
        {
            if (condition == null)
            {
                Debug.LogWarning("MonoObserver: 조건이 null입니다.");
            }
            else
            {
                _observeList.Add(new ObserveData(subject, condition, onDisabled));
            }
        }


        /// <summary> 해당 조건의 관찰을 해제한다 </summary>
        /// <param name="condition"> 해제할 관찰의 조건 </param>
        public void Unregister(Func<bool> condition)
        {
            for (int i = _observeList.Count - 1; i >= 0; i--)
            {
                if (_observeList[i].condition != condition)
                {
                    continue;
                }

                this._observeList[i].onDestroy?.Invoke(this._observeList[i].target);
                this._observeList.RemoveAt(i);
            }
        }


        /// <summary> 모든 관찰을 해제하고 콜백을 호출한다 </summary>
        public void ClearAll()
        {
            this._observeList?.Clear();
        }


        private void Update()
        {
            for (int index = this._observeList.Count - 1; index >= 0; index--)
            {
                ObserveData observer = this._observeList[index];

                if (observer.condition.Invoke())
                {
                    observer.onDestroy?.Invoke(this._observeList[index].target);
                    
                    observer.onDestroy = null;
                    observer.condition = null;
                    observer.target = null;
                    
                    this._observeList.RemoveAt(index);
                }
            }
        }


        protected override void OnMonoDestroy()
        {
            this._observeList.ForEach(o => o.onDestroy?.Invoke(o.target));
        }
    }


    public partial class MonoObserver
    {
        private struct ObserveData : IEquatable<ObserveData>
        {
            public ObserveData(Object target, Func<bool> condition, Action<Object> onDestroy)
            {
                this.target = target;
                this.condition = condition;
                this.onDestroy = onDestroy;
            }

            public Func<bool> condition;
            public Action<Object> onDestroy;
            public Object target;


            public bool Equals(ObserveData other)
            {
                return condition == other.condition && onDestroy == other.onDestroy && target == other.target;
            }


            public override bool Equals(object obj)
            {
                return obj is ObserveData other && Equals(other);
            }


            public override int GetHashCode()
            {
                return HashCode.Combine(condition, onDestroy, target);
            }
        }
    }
}