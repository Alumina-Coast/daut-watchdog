using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Watchdog
{
    public class DirectoryGuard
    {
        public required string Directorio { get; init; }
        public List<string> Archivos { get; init; } = [];
        public List<ICondition> Condiciones { get; set; } = [];

        private FileSystemWatcher? watcher;
        private static readonly List<WatcherChangeTypes> _acceptedTypes = [WatcherChangeTypes.Deleted, WatcherChangeTypes.Created, WatcherChangeTypes.Changed];

        public void Guard() 
        {
            watcher = new()
            {
                Path = Directorio,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName
            };

            watcher.Changed += OnChanged;
            watcher.Created += OnChanged;
            watcher.Deleted += OnChanged;
            watcher.Error += OnError;

            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;
        }

        public List<string> GetReport()
        {
            var report = new List<string>();
            var files = System.IO.Directory.GetFiles(Directorio);
            foreach (var file in files)
            {
                if (Archivos.Count !=0 && !Archivos.Contains(Path.GetFileName(file)))
                {
                    continue;
                }
                foreach(var condition in Condiciones.Where(c => c.Event is null || !_acceptedTypes.Contains(c.Event.Value)))
                {
                    if (condition.IsConditionMet(file))
                    {
                        report.Add(condition.PrintAsMessage(file));
                    }
                }
            }
            return report;
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (Archivos.Count != 0 && !Archivos.Contains(Path.GetFileName(e.FullPath)))
            {
                return;
            }
            foreach (var condition in Condiciones.Where(c => c.Event == e.ChangeType))
            {
                if (condition.IsConditionMet(e.FullPath))
                {
                    Debug.WriteLine(condition.PrintAsMessage(e.FullPath));
                }
            }
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            throw e.GetException();
        }
    }
}
