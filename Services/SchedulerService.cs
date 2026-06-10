using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using BackupCR.Database;

namespace BackupCR.Services
{
    public class SchedulerService : ISchedulerService
    {
        private readonly IServiceProvider _serviceProvider;
        private System.Threading.Timer? _timer;
        private bool _isProcessing = false;

        public SchedulerService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Start()
        {
            // Check schedules every 30 seconds
            _timer = new System.Threading.Timer(async _ => await CheckSchedulesAsync(), null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
        }

        public void Stop()
        {
            _timer?.Dispose();
        }

        private async Task CheckSchedulesAsync()
        {
            if (_isProcessing) return;
            _isProcessing = true;

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<BackupDbContext>();
                var backupEngine = scope.ServiceProvider.GetRequiredService<IBackupEngine>();

                var now = DateTime.Now;
                var currentTimeStr = now.ToString("HH:mm");

                var jobs = dbContext.BackupJobs
                    .Where(j => j.IsActive && j.ScheduleType != "Manual")
                    .ToList();

                foreach (var job in jobs)
                {
                    bool shouldRun = false;

                    if (job.ScheduleType == "Daily" && job.ScheduleTime == currentTimeStr)
                    {
                        shouldRun = true;
                    }
                    else if (job.ScheduleType == "Weekly")
                    {
                        var parts = job.ScheduleTime.Split(' ');
                        if (parts.Length == 2)
                        {
                            var dayOfWeekStr = parts[0];
                            var timeStr = parts[1];

                            var currentDayOfWeekStr = now.ToString("dddd", new System.Globalization.CultureInfo("pt-BR"));
                            if (currentDayOfWeekStr.Equals(dayOfWeekStr, StringComparison.OrdinalIgnoreCase) && timeStr == currentTimeStr)
                            {
                                shouldRun = true;
                            }
                        }
                    }
                    else if (job.ScheduleType == "Continuous")
                    {
                        if (job.LastRunTime == null || (now - job.LastRunTime.Value).TotalMinutes >= 15)
                        {
                            shouldRun = true;
                        }
                    }

                    if (shouldRun && !backupEngine.IsRunning(job.Id))
                    {
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                await backupEngine.RunJobAsync(job.Id);
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Erro no agendador ao rodar job {job.Id}: {ex.Message}");
                            }
                        });
                    }
                }
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro na execução do loop do agendador: {ex.Message}");
            }
            finally
            {
                _isProcessing = false;
            }
        }
    }
}
