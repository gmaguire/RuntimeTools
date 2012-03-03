using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;

namespace HardcoreDebugging.Compilers
{
    public class RoslynCompiler : BaseCompiler, ICompiler
    {
        public Type Compile(string filename, Type type)
        {
            var className = type.Name;
            var newClassName = String.Format("{0}_{1}", className, DateTime.Now.Ticks);
            var code = ReadCodeFile(filename);

            var syntaxTree = SyntaxTree.ParseCompilationUnit(code);

            RenameClass(syntaxTree, className, newClassName);

            var references = GetAssemblyReferencesForType(type);

            var compilationOptions = new CompilationOptions(assemblyKind: AssemblyKind.DynamicallyLinkedLibrary);

            var compilation = Compilation.Create("AssemblyForRecompiledType", compilationOptions, new[] { syntaxTree }, references);

            using (var stream = new MemoryStream())
            {
                var emitResult = compilation.Emit(stream);

                if (!emitResult.Success)
                {
                    TraceCompilaitionDiagnostics(emitResult);
                    return type;
                }

                return Assembly.Load(stream.GetBuffer()).GetTypes().First(t => t.Name.StartsWith(className));
            }
        }

        private static IEnumerable<AssemblyFileReference> GetAssemblyReferencesForType(Type type)
        {
            var references = type.Assembly
                .GetReferencedAssemblies()
                .Select(Assembly.Load)
                .Select(referencedAssembly => new AssemblyFileReference(referencedAssembly.Location))
                .ToList();

            references.Add(new AssemblyFileReference(type.Assembly.Location));

            return references;
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
    }
}
