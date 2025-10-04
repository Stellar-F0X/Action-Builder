using System;

namespace StatController.Runtime
{
    public interface IStatKey : IEquatable<IStatKey>
    {
        public object boxedKeyObject { get; internal set; }
    }
    

    public interface IStatKey<T> : IStatKey
    {
        public T key { get; internal set; }
    }
}