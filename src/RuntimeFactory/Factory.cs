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
        where TConcrete : TInterface
    {
        private static Type _recompiledType;
        private static readonly Type _concreteType;

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

            var syntaxTree = SyntaxTree.ParseCompilationUnit(code);

            RenameClass(syntaxTree, className, newClassName);

            var references = new List<AssemblyFileReference>
                                 {
                                     new AssemblyFileReference(_concreteType.Assembly.Location),
                                     new AssemblyFileReference(typeof (object).Assembly.Location)
                                 };

            var compilationOptions = new CompilationOptions(assemblyKind: AssemblyKind.DynamicallyLinkedLibrary);

            // Note: using a fixed assembly name, which doesn't matter as long as we don't expect cross references of generated assemblies
            var compilation = Compilation.Create("AssemblyForRecompiledType", compilationOptions, new[] { syntaxTree }, references);

            using (var stream = new MemoryStream())
            {
                var emitResult = compilation.Emit(stream);

                if (!emitResult.Success)
                {
                    TraceCompilaitionDiagnostics(emitResult);
                    return _concreteType;
                }

                return Assembly.Load(stream.GetBuffer()).GetTypes().FirstOrDefault();
            }
        }

        private static void TraceCompilaitionDiagnostics(EmitResult emitResult)
        {
            Trace.WriteLine("ERROR: Compilation Failed!");
            foreach (var diag in emitResult.Diagnostics)
            {
                var message = string.Format("[{0}] {1}", diag.Location.GetLineSpan(false),
                                            diag.Info.GetMessage());
                Trace.WriteLine(message);
            }
        }

        private static void RenameClass(SyntaxTree syntaxTree, string className, string newClassName)
        {
            var classNode = syntaxTree.Root
                                        .DescendentNodes()
                                        .OfType<ClassDeclarationSyntax>()
                                        .FirstOrDefault(n => n.Identifier.ValueText == className);

            var idNode = classNode.DescendentNodes()
                                    .OfType<IdentifierNameSyntax>()
                                    .First();

            classNode.ReplaceNode(idNode, Syntax.IdentifierName(newClassName));
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
