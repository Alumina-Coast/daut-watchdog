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
        public required string Directory { get; init; }
        public List<string> Files { get; init; } = [];
        public bool AllConditions { get; set; } = false;
        public List<ICondition> Conditions { get; set; } = [];
        private FileSystemWatcher? watcher;

        private static readonly List<WatcherChangeTypes> _acceptedTypes = [WatcherChangeTypes.Deleted, WatcherChangeTypes.Created, WatcherChangeTypes.Changed];

        public void Guard() 
        {
            watcher = new()
            {
                Path = Directory,
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
            var files = System.IO.Directory.GetFiles(Directory);
            foreach (var file in files)
            {
                if (Files.Count !=0 && !Files.Contains(Path.GetFileName(file)))
                {
                    continue;
                }
                foreach(var condition in Conditions.Where(c => c.Event is null || !_acceptedTypes.Contains(c.Event.Value)))
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
            if (Files.Count != 0 && !Files.Contains(Path.GetFileName(e.FullPath)))
            {
                return;
            }
            foreach (var condition in Conditions.Where(c => c.Event == e.ChangeType))
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

    public interface ICondition
    {
        public WatcherChangeTypes? Event { get; }
        public bool IsConditionMet(string filePath);
        public string PrintAsMessage(string filePath);
    }

    public class NewLineContains : ICondition
    {
        public WatcherChangeTypes? Event { get; init; } = WatcherChangeTypes.Changed;
        public string CompareString { get; init; } = string.Empty;

        public string PrintAsMessage(string filePath)
        {
            return $"La última línea del archivo '{filePath}' contiene el string: '{CompareString}'.";
        }

        public bool IsConditionMet(string filePath)
        {
            if (File.Exists(filePath))
            {
                var lastLine = File.ReadAllLines(filePath).LastOrDefault();
                if (lastLine is not null)
                {
                    return lastLine.Contains(CompareString, StringComparison.InvariantCultureIgnoreCase);
                }
            }
            return false;
        }
    }

    public class InactiveFor : ICondition
    {
        public WatcherChangeTypes? Event { get; } = null;
        public TimeSpan TimeSpan { get; init; } = TimeSpan.FromDays(1);

        public string PrintAsMessage(string filePath)
        {
            return $"Inactividad superior a {TimeSpan} detectada en '{filePath}'.";
        }

        public bool IsConditionMet(string filePath)
        {
            if (File.Exists(filePath))
            {
                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Exists)
                {
                    return DateTime.UtcNow - fileInfo.LastAccessTimeUtc >= TimeSpan;
                }
            }
            return false;
        }
    }
}
