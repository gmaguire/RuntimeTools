using System;

namespace HardcoreDebugging
{
    public interface ICodeWatcher
    {
        void Start(Type concreteType, Action<string> modificationAction);
    }
}