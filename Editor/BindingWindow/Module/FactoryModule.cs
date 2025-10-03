using System;
using UnityEngine;

namespace StatController.Tool
{
    public abstract class FactoryModule
    {
        protected FactoryModule(Type targetType, string title, int layer = 1)
        {
            this.title = string.IsNullOrEmpty(title) ? targetType.Name : title;
            this.targetType = targetType;
            this.layer = layer;
        }

        public Action<Type, Vector2, string, Delegate> onTryCreate
        {
            get;
            protected set;
        }

        public ICategoryTreeProvider categoryProvider
        {
            get;
            set;
        }

        public Type targetType
        {
            get;
            private set;
        }

        public string title
        {
            get;
            private set;
        }

        public int layer
        {
            get;
            private set;
        }
    }


    public abstract class FactoryModule<T> : FactoryModule
    {
        protected FactoryModule(Type targetType, string title, bool useCallback = false, int layer = 1) : base(targetType, title, layer)
        {
            base.onTryCreate = this.ExecuteCreateActions;
            this._useCallback = useCallback;
        }


        private readonly bool _useCallback;


        private void ExecuteCreateActions(Type childType, Vector2 position, string entryName, Delegate createAction)
        {
            this.BeforeCreate(childType, position);

            T creation = this.Create(childType, position, entryName);

            if (this._useCallback)
            {
                createAction?.DynamicInvoke(creation);
            }

            this.AfterCreate(creation);
        }


        protected virtual void BeforeCreate(Type childType, Vector2 position) { }

        protected virtual void AfterCreate(T creation) { }

        protected abstract T Create(Type type, Vector2 position, string entryName);
    }
}