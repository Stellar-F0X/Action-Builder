using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace ActionBuilder.Runtime
{
    public class PoolController
    {
        private readonly List<IPoolable> _objectList = new List<IPoolable>();

        private readonly Queue<ExecutableBase> _disableQueue = new Queue<ExecutableBase>();
        

        public IReadOnlyList<IPoolable> list
        {
            get { return _objectList; }
        }



        public T GetPooledObject<T>(int hash) where T : ExecutableBase, IPoolable
        {
            T result = _objectList.Where(obj => obj.isReadyInPool)
                       .OfType<T>()
                       .FirstOrDefault(obj => obj.hash == hash);

            _objectList.Remove(result);
            return result;

        }


        public void PushObjectToPool(ExecutableBase target)
        {
            Assert.IsNotNull(target, "target is null");
            _disableQueue.Enqueue(target);
        }
        
        
        public void ClearPool()
        {
            _objectList.Clear();
            _disableQueue.Clear();
        }


        public void ProcessDestroyQueue()
        {
            while (_disableQueue.TryDequeue(out ExecutableBase destroyTarget))
            {
                Assert.IsNotNull(destroyTarget, "이미 파괴된 오브젝트");

                if (destroyTarget is not IPoolable poolable)
                {
                    Object.Destroy(destroyTarget);
                    continue;
                }

                poolable.OnBackToPool();
                _objectList.Add(poolable);
            }
        }
    }
}