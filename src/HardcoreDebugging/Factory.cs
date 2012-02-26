using System;
using System.Diagnostics;
using System.IO;
using HardcoreDebugging.Compilers;

namespace HardcoreDebugging
{
    public static class Factory<TInterface, TConcrete>
        where TConcrete : TInterface
    {
        private static Type _recompiledType;
        private static readonly Type _concreteType;

        public static ICompiler Compiler = new CodeDomCompiler();

        static Factory()
        {
            _concreteType = typeof (TConcrete);
            WatchCodeFile();
        }

        public static TInterface Create(params object[] arguments)
        {
            return (TInterface)Activator.CreateInstance(_recompiledType ?? _concreteType, arguments);
        }

        private static void WatchCodeFile()
        {
            var pathToWatch = Path.GetFullPath(Path.Combine(_concreteType.Assembly.Location, @"..\..\.."));
            var className = _concreteType.Name;

            var watcher = new FileSystemWatcher
            {
                Path = pathToWatch,
                NotifyFilter = NotifyFilters.LastWrite,
                Filter = String.Format("{0}.cs", className),
                EnableRaisingEvents = true
            };

            watcher.Changed += (o, e) =>
            {
                try
                {
                    watcher.EnableRaisingEvents = false;
                    Trace.WriteLine(string.Format("Modification detected in file '{0}', starting recompilation...", e.Name));
                    _recompiledType = Compiler.Compile(e.FullPath, _concreteType);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                }
                finally
                {
                    Trace.WriteLine("Compilation finished.");
                    watcher.EnableRaisingEvents = true;
                }
            };
        }
    }
}
