using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;

namespace RuntimeFactory
{
    public static class Factory<TInterface, TConcrete>
        where TConcrete : TInterface, new()
    {
        private static Type _recompiledType;
        private static readonly Type _concreteType;
        static Factory()
        {
            _concreteType = typeof (TConcrete);
            WatchCodeFile();
        }

        public static TInterface Create()
        {
            return (TInterface)Activator.CreateInstance(_recompiledType ?? _concreteType);
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
                    _recompiledType = CompileCode(e.FullPath, className);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                }
            };

        }

        private static Type CompileCode(string filename, string className)
        {
            var newClassName = String.Format("{0}_{1}", className, DateTime.Now.Ticks);
            var code = ReadCodeFile(filename);

            code = ChangeConcreteClassName(code, className, newClassName);

            var syntaxTree = SyntaxTree.ParseCompilationUnit(code);

            var references = new List<AssemblyFileReference>
                                 {
                                     new AssemblyFileReference(_concreteType.Assembly.Location),
                                     new AssemblyFileReference(typeof (object).Assembly.Location)
                                 };

            var compilationOptions = new CompilationOptions(assemblyKind: AssemblyKind.DynamicallyLinkedLibrary);

            // Note: using a fixed assembly name, which doesn't matter as long as we don't expect cross references of generated assemblies
            var compilation = Compilation.Create("AssemblyForRecompiledType", compilationOptions, new[] { syntaxTree }, references);

            var stream = new MemoryStream();
            var emitResult = compilation.Emit(stream);

            if (!emitResult.Success)
            {
                Trace.WriteLine("ERROR: Compilation Failed!");
                foreach (var diag in emitResult.Diagnostics)
                {
                    var message = string.Format("[{0}] {1}", diag.Location.GetLineSpan(false),
                                                diag.Info.GetMessage());
                    Trace.WriteLine(message);
                }
                return _concreteType;
            }

            return Assembly.Load(stream.GetBuffer()).GetTypes().First(t => t.Name.Equals(newClassName));
        }

        private static string ChangeConcreteClassName(string code, string className, string newClassName)
        {
            // Duuuurtty
            return code.Replace(String.Format("class {0} :", className),
                                string.Format("class {0} :", newClassName));
        }

        private static string ReadCodeFile(string filename)
        {
            using (var filestream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var reader = new StreamReader(filestream))
                {
                    return reader.ReadToEnd();
                }
            }

        }
    }
}
