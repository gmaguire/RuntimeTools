using System;

namespace RuntimeTools.Compilers
{
    public interface ICompiler
    {
        Type Compile(string filename, Type type);
    }
}