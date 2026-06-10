using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackupCR.Database;

namespace BackupCR.Services
{
    public interface IBackupEngine
    {
        event Action<int, string, double>? OnJobProgressUpdated; // JobId, Status, ProgressPercent
        event Action? OnAnyJobCompleted;
        bool IsRunning(int jobId);
        double GetProgress(int jobId);
        string GetCurrentOperation(int jobId);
        Task RunJobAsync(int jobId);
        Task RunAllActiveJobsAsync();
        Task RestoreJobAsync(int logId, string targetRestorePath, List<string>? specificFiles = null);
        Task DeleteBackupArchiveAsync(int logId);
    }
}
