using System;

namespace RuntimeTools
{
    public interface ICodeWatcher
    {
        void Start(Type concreteType, Action<string> modificationAction);
    }
}