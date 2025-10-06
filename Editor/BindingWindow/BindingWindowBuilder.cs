using System;
using System.Collections.Generic;
using UnityEngine;

namespace StatController.Tool
{
    /// <summary> 클래스는 BindingWindow를 구성하기 위한 빌더 패턴을 제공합니다. </summary>
    internal class BindingWindowBuilder
    {
        /// <summary> BindingWindow 인스턴스를 관리하기 위한 정적 리스트입니다. </summary>
        private readonly static List<BindingWindow> _Windows = new List<BindingWindow>();

        
        /// <summary> FactoryModule 객체를 생성하기 위한 팩토리 객체를 생성하는 콜백이 저장된 컬렉션입니다. </summary>
        private readonly List<Func<FactoryModule>> _modules = new List<Func<FactoryModule>>(1);

        
        /// <summary> ICategoryTreeProvider 타입의 팩토리 객체를 생성하는 콜백을 저장하는 리스트입니다. </summary>
        private readonly List<Func<ICategoryTreeProvider>> _providers = new List<Func<ICategoryTreeProvider>>(1);

        
        /// <summary>현재 작업 중인 BindingWindow 인스턴스를 나타내는 필드입니다.</summary>
        private readonly BindingWindow _bindingWindow;

        
        /// <summary> 창의 제목을 나타내는 문자열입니다. </summary>
        private string _windowTitle;

        
        /// <summary> 이미 만들어진 창을 재사용할지 여부를 나타냅니다. </summary>
        /// <value> False : 창을 매번 다시 생성합니다. </value>
        private bool _reuseWindow;

        
        /// <summary> 헤드 엔트리 사용 여부를 나타냅니다. </summary>
        private bool _useHeadEntry;



        /// <summary>지정된 제목과 옵션에 따라 BindingWindowBuilder 인스턴스를 생성합니다.</summary>
        /// <param name="title">창의 제목을 지정합니다. 비어있거나 null이 될 수 없습니다.</param>
        /// <param name="useHeadEntry">머리글 항목 사용 여부를 지정합니다.</param>
        /// <param name="reuse">창 재사용 여부를 지정합니다.</param>
        /// <returns>BindingWindowBuilder 인스턴스를 반환합니다.</returns>
        public static BindingWindowBuilder GetBuilder(string title, bool useHeadEntry = true, bool reuse = true)
        {
            Debug.Assert(string.IsNullOrEmpty(title) == false, $"{nameof(BindingWindow)} Title must not be null or empty");
            
            BindingWindowBuilder builder = new BindingWindowBuilder();
            builder._useHeadEntry = useHeadEntry;
            builder._reuseWindow = reuse;
            builder._windowTitle = title;
            return builder;
        }
        


        /// <summary>FactoryModule과 ICategoryTreeProvider를 추가합니다.</summary>
        /// <param name="factoryModule">추가할 FactoryModule 생성자 함수.</param>
        /// <param name="treeProvider">추가할 ICategoryTreeProvider 생성자 함수.</param>
        /// <returns>자신의 인스턴스를 반환합니다.</returns>
        public BindingWindowBuilder AddFactoryModule(Func<FactoryModule> factoryModule, Func<ICategoryTreeProvider> treeProvider)
        {
            Debug.Assert(factoryModule is not null, "module is null");
            Debug.Assert(treeProvider is not null, "categoryProvider is null");

            Debug.Assert(_modules.Contains(factoryModule) == false, "module is already added");
            Debug.Assert(_providers.Contains(treeProvider) == false, "categoryProvider is already added");
            
            _modules.Add(factoryModule);
            _providers.Add(treeProvider);
            return this;
        }



        /// <summary>지정된 조건에 따라 창의 모듈을 업데이트하려 시도합니다.</summary>
        /// <param name="updatable">업데이트 가능 여부를 결정하는 함수입니다. null일 경우 조건 확인 없이 업데이트를 시도합니다.</param>
        /// <returns>현재의 BindingWindowBuilder 인스턴스를 반환합니다.</returns>
        public BindingWindowBuilder TryUpdateModules(Func<bool> updatable = null)
        {
            if (updatable is not null && updatable.Invoke() == false)
            {
                return this;
            }
            
            BindingWindow window = _Windows.Find(w => this._windowTitle == w.windowTitle);

            if (window == null)
            {
                return this;
            }

            this.TryUpdateModules(window);
            return this;
        }

        

        /// <summary> 모듈의 상태를 업데이트하려 시도합니다. </summary>
        /// <param name="window"> 업데이트 대상이 되는 BindingWindow 객체입니다. </param>
        private void TryUpdateModules(BindingWindow window)
        {
            //바인딩을 위해 등록되어 있는 모듈이 없다면 Early Return.
            if (_modules.Count == 0 || _providers.Count == 0)
            {
                return;
            }
            
            Debug.Assert(_modules.Count == _providers.Count, "Mismatched module and provider count");
            
            window.modules = new List<FactoryModule>();
            
            for (int i = 0; i < _modules.Count; ++i)
            {
                FactoryModule module = _modules[i].Invoke();
                module.categoryProvider = _providers[i].Invoke();
                window.modules.Add(module);
            }
        }

        

        /// <summary> BindingWindow 객체를 생성 또는 반환한다. </summary>
        /// <returns> 생성 혹은 검색된 BindingWindow 객체를 반환한다. </returns>
        public BindingWindow Build()
        {
            BindingWindow window = null;
            
            if (_reuseWindow) //창을 재사용하기 위해 창을 탐색한다. 
            {
                window = _Windows.Find(w => this._windowTitle == w.windowTitle);
            }

            if (window == null) //창이 없다면 새로 만든다.
            {
                window = ScriptableObject.CreateInstance<BindingWindow>();
                _Windows.Add(window);
            }
            
            //창을 재사용하지 않아도, 모듈이 없다면 초기화하기 위해 모듈을 등록한다.
            if (_reuseWindow == false || window.modules is null || window.modules.Count == 0)
            {
                this.TryUpdateModules(window);
            }
            
            window.useHeadEntry = this._useHeadEntry;
            window.windowTitle = this._windowTitle;
            return window;
        }
    }
}