﻿using System;
using System.Diagnostics;
using System.IO;

namespace HardcoreDebugging
{
    public class CodeWatcher: ICodeWatcher
    {
        public void Start(Type concreteType, Action<string> modificationAction)
        {
            var pathToWatch = Path.Combine(
                                    Path.GetDirectoryName(concreteType.Assembly.Location) ??  "", @"..\..");

            var className = concreteType.Name;

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
                                           modificationAction(e.FullPath);
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