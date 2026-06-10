using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using BackupCR.Services;

namespace BackupCR
{
    public partial class MainWindow : Window
    {
        private System.Windows.Forms.NotifyIcon? _notifyIcon;
        private bool _isExplicitClose = false;

        public MainWindow()
        {
            InitializeComponent();
            blazorWebView.Services = App.ServiceProvider;

            InitializeTrayIcon();
        }

        private void InitializeTrayIcon()
        {
            _notifyIcon = new System.Windows.Forms.NotifyIcon();
            try
            {
                var uri = new Uri("pack://application:,,,/icons/icon.ico");
                var streamResourceInfo = System.Windows.Application.GetResourceStream(uri);
                if (streamResourceInfo != null)
                {
                    _notifyIcon.Icon = new Icon(streamResourceInfo.Stream);
                }
                else
                {
                    _notifyIcon.Icon = SystemIcons.Application;
                }
            }
            catch
            {
                _notifyIcon.Icon = SystemIcons.Application;
            }

            _notifyIcon.Text = "BackupCR - Veeam Inspired Agent";
            _notifyIcon.Visible = true;

            // Wire double click to restore window
            _notifyIcon.DoubleClick += (s, e) => ShowWindow();

            // Create context menu strip
            var contextMenu = new System.Windows.Forms.ContextMenuStrip();
            
            var openItem = new System.Windows.Forms.ToolStripMenuItem("Abrir Dashboard", null, (s, e) => ShowWindow());
            var runItem = new System.Windows.Forms.ToolStripMenuItem("Criar Backup", null, (s, e) => RunBackupQuick());
            var configItem = new System.Windows.Forms.ToolStripMenuItem("Configurar Backup", null, (s, e) => NavigateTo("jobs"));
            var scheduleItem = new System.Windows.Forms.ToolStripMenuItem("Agendamento", null, (s, e) => NavigateTo("schedules"));
            var exitItem = new System.Windows.Forms.ToolStripMenuItem("Sair", null, (s, e) => ExitApplication());

            contextMenu.Items.Add(openItem);
            contextMenu.Items.Add(runItem);
            contextMenu.Items.Add(configItem);
            contextMenu.Items.Add(scheduleItem);
            contextMenu.Items.Add(new System.Windows.Forms.ToolStripSeparator());
            contextMenu.Items.Add(exitItem);

            _notifyIcon.ContextMenuStrip = contextMenu;
        }

        public void ShowWindow()
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
        }

        private void NavigateTo(string page)
        {
            ShowWindow();
            var navService = App.ServiceProvider.GetRequiredService<NavigationService>();
            navService.RequestNavigation(page);
        }

        private void RunBackupQuick()
        {
            var backupEngine = App.ServiceProvider.GetRequiredService<IBackupEngine>();
            System.Threading.Tasks.Task.Run(async () =>
            {
                try
                {
                    await backupEngine.RunAllActiveJobsAsync();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erro ao rodar backup rápido: {ex.Message}");
                }
            });
            
            _notifyIcon?.ShowBalloonTip(3000, "BackupCR", "Iniciando execução de todos os backups ativos em segundo plano...", System.Windows.Forms.ToolTipIcon.Info);
        }

        public void ExitApplication()
        {
            _isExplicitClose = true;
            Close();
            System.Windows.Application.Current.Shutdown();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!_isExplicitClose)
            {
                e.Cancel = true;
                Hide();
                _notifyIcon?.ShowBalloonTip(3000, "BackupCR", "O aplicativo continua a rodar em segundo plano na bandeja do sistema.", System.Windows.Forms.ToolTipIcon.Info);
            }
            else
            {
                if (_notifyIcon != null)
                {
                    _notifyIcon.Visible = false;
                    _notifyIcon.Dispose();
                }
                base.OnClosing(e);
            }
        }
    }
}
