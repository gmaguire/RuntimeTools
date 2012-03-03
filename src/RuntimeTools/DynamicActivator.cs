using System;
using RuntimeTools.Compilers;

namespace RuntimeTools
{
    public static class DynamicActivator<TInterface, TConcrete>
        where TConcrete : TInterface
    {
        private static Type _recompiledType;
        private static readonly Type _concreteType;

        public static ICompiler Compiler = new CodeDomCompiler();

        public static ICodeWatcher CodeWatcher = new CodeWatcher();

        static DynamicActivator()
        {
            _concreteType = typeof (TConcrete);

            CodeWatcher.Start(_concreteType, 
                                filename => _recompiledType = Compiler.Compile(filename, _concreteType));
        }

        public static TInterface CreateInstance(params object[] arguments)
        {
            return (TInterface)Activator.CreateInstance(_recompiledType ?? _concreteType, arguments);
        }
    }
}
