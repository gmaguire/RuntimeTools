using System;

namespace HardcoreDebugging.Compilers
{
    public interface ICompiler
    {
        Type Compile(string filename, Type type);
    }
}