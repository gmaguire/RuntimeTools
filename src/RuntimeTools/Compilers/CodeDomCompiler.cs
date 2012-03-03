using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace HardcoreDebugging.Compilers
{
    public class CodeDomCompiler : BaseCompiler, ICompiler
    {
        public Type Compile(string filename, Type type)
        {
            var codeProvider = CodeDomProvider.CreateProvider("CSharp");
            var className = type.Name;

            var code = ReadCodeFile(filename);
            var newClassName = String.Format("{0}_{1}", className, DateTime.Now.Ticks);
            code = RenameClass(code, type.Name, newClassName);

            var parameters = new CompilerParameters(GetReferencedAssemblies(type))
                                 {
                                     GenerateExecutable = false,
                                     GenerateInMemory = true
                                 };

            var result = codeProvider.CompileAssemblyFromSource(parameters, code);

            if (result.Errors.HasErrors)
            {
                TraceErrors(result.Errors);
                return type;
            }

            return result.CompiledAssembly.GetTypes().First(t => t.Name.StartsWith(className));
        }

        private static void TraceErrors(CompilerErrorCollection errors)
        {
            foreach (var error in errors)
            {
                Trace.WriteLine(error);
            }
        }

        private static string RenameClass(string code, string className, string newClassName)
        {
            return code.Replace(string.Format("class {0} :", className),
                                string.Format("class {0} :", newClassName));
        }

        private static string[] GetReferencedAssemblies(Type type)
        {
            var assemblyNames = type.Assembly
                                    .GetReferencedAssemblies()
                                    .Select(Assembly.Load)
                                    .Select(a => Path.GetFileName(a.Location))
                                    .ToList();

            assemblyNames.Add("System.dll");

            assemblyNames.Add(Path.GetFileName(type.Assembly.Location));

            return assemblyNames.ToArray();
        }
    }
}
