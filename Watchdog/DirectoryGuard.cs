using Org.BouncyCastle.Math.EC.Multiplier;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Watchdog
{
    public class WarningRaisedEventArgs : EventArgs
    {
        public required DirectoryGuard Parent { get; init; }
        public string Filename { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public bool SystemError { get; init; } = false;
        public DateTime Timestamp { get; } = DateTime.UtcNow;
    }

    public class DirectoryGuard
    {
        public required string Nombre { get; init; }
        public required string Directorio { get; init; }
        public List<string> Filtros { get; init; } = [];
        public List<ICondition> Condiciones { get; set; } = [];
        public bool IncluirSubdirectorios { get; init; } = false;
        public bool IsGuarding { get => watcher is not null; }

        public delegate void WarningRaisedEventHandler(object sender, WarningRaisedEventArgs e);
        public event WarningRaisedEventHandler? WarningRaised;

        private FileSystemWatcher? watcher;
        private static readonly List<WatcherChangeTypes> _acceptedTypes = [WatcherChangeTypes.Deleted, WatcherChangeTypes.Created, WatcherChangeTypes.Changed];

        public bool Start()
        {
            if (Condiciones.All(c => c.TipoDeEvento is null || !_acceptedTypes.Contains(c.TipoDeEvento.Value))) { return false; }
            watcher = new()
            {
                Path = Directorio,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                IncludeSubdirectories = IncluirSubdirectorios,
            };
            if (Filtros.Count == 0)
            {
                Filtros.Add("*");
            }
            foreach (var file in Filtros)
            {
                watcher.Filters.Add(file);
            }
            watcher.Changed += OnChanged;
            watcher.Created += OnChanged;
            watcher.Deleted += OnChanged;
            watcher.Error += OnError;

            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;
            return true;
        }

        public List<string> GetReport()
        {
            var report = new List<string>();
            var files = new HashSet<string>();
            var manualCheckCondiciones = Condiciones.Where(c => c.TipoDeEvento is null || !_acceptedTypes.Contains(c.TipoDeEvento.Value));

            foreach (var filtro in Filtros)
            {
                foreach (var file in Directory.GetFiles(Directorio, filtro))
                {
                    files.Add(file);
                }
            }

            foreach (var condition in manualCheckCondiciones)
            {
                var preReport = new List<string>();
                foreach (var file in files)
                {
                    if (condition.IsConditionMet(file))
                    {
                        preReport.Add(condition.PrintAsMessage(file));
                    }
                    else if (condition.Unanime)
                    {
                        preReport.Clear();
                        break;
                    }
                }
                if (condition.Unanime && preReport.Count != 0)
                {
                    report.Add(condition.PrintAsMessage(Directorio));
                }
                else
                {
                    report.AddRange(preReport);
                }
            }
            return report;
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            foreach (var condition in Condiciones.Where(c => c.TipoDeEvento == e.ChangeType))
            {
                if (condition.IsConditionMet(e.FullPath))
                {
                    WarningRaised?.Invoke(this, new WarningRaisedEventArgs() 
                    { 
                        Parent = this, 
                        Filename = e.FullPath, 
                        Message = condition.PrintAsMessage(e.FullPath) 
                    });
                }
            }
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            WarningRaised?.Invoke(this, new WarningRaisedEventArgs()
            {
                Parent = this,
                Filename = Directorio,
                Message = $"Error en el guardia de '{Directorio}'. Razón: '{e.GetException().Message}'.",
                SystemError = true,
            });
            if (watcher is not null)
            {
                watcher.Dispose();
                watcher = null;
            }
        }
    }
}
