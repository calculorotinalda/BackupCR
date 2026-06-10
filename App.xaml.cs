using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using BackupCR.Database;
using BackupCR.Services;

namespace BackupCR
{
    public partial class App : System.Windows.Application
    {
        public static IServiceProvider ServiceProvider { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            System.IO.Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            base.OnStartup(e);

            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            AppDomain.CurrentDomain.UnhandledException += (s, ev) => LogUnhandledException(ev.ExceptionObject as Exception);
            DispatcherUnhandledException += (s, ev) => {
                LogUnhandledException(ev.Exception);
                ev.Handled = true;
            };
            System.Threading.Tasks.TaskScheduler.UnobservedTaskException += (s, ev) => LogUnhandledException(ev.Exception);


            var services = new ServiceCollection();
            ConfigureServices(services);

            ServiceProvider = services.BuildServiceProvider();

            // Initialize database
            try
            {
                using (var scope = ServiceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<BackupDbContext>();
                    dbContext.Database.EnsureCreated();
                    dbContext.SeedInitialData();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erro ao inicializar o banco de dados: {ex.Message}", "Erro de Banco de Dados", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Start Scheduler
            try
            {
                var scheduler = ServiceProvider.GetRequiredService<ISchedulerService>();
                scheduler.Start();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao iniciar o agendador: {ex.Message}");
            }

            // Register for Windows Startup
            RegisterForStartup();

            // Instantiate MainWindow
            var mainWindow = new MainWindow();

            bool startMinimized = false;
            foreach (var arg in e.Args)
            {
                if (arg.Equals("/minimized", StringComparison.OrdinalIgnoreCase) || 
                    arg.Equals("--minimized", StringComparison.OrdinalIgnoreCase))
                {
                    startMinimized = true;
                }
            }

            if (!startMinimized)
            {
                mainWindow.Show();
            }
            else
            {
                mainWindow.Hide();
            }
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddWpfBlazorWebView();
            services.AddDbContext<BackupDbContext>();
            services.AddSingleton<NavigationService>();
            services.AddSingleton<NotificationService>();
            services.AddSingleton<IBackupEngine, BackupEngine>();
            services.AddSingleton<ISchedulerService, SchedulerService>();
        }

        private void LogUnhandledException(Exception? ex)
        {
            if (ex == null) return;
            try
            {
                string logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash_log.txt");
                string details = $"[{DateTime.Now}] CRASH DETECTED:\nException: {ex.Message}\nStackTrace:\n{ex.StackTrace}\n";
                if (ex.InnerException != null)
                {
                    details += $"InnerException: {ex.InnerException.Message}\nStackTrace:\n{ex.InnerException.StackTrace}\n";
                }
                System.IO.File.AppendAllText(logPath, details);
                System.Windows.MessageBox.Show($"Ocorreu um erro crítico na inicialização:\n{ex.Message}\n\nDetalhes salvos em crash_log.txt", "Erro Crítico", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch
            {
                // Suppress
            }
        }


        private void RegisterForStartup()
        {
            try
            {
                string exePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BackupCR.exe");
                using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                if (key != null)
                {
                    key.SetValue("BackupCR", $"\"{exePath}\" /minimized");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao registrar no Windows Run: {ex.Message}");
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                var scheduler = ServiceProvider?.GetService<ISchedulerService>();
                scheduler?.Stop();
            }
            catch
            {
                // Suppress exit errors
            }

            base.OnExit(e);
        }
    }
}
