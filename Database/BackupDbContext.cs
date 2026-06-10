using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace BackupCR.Database
{
    public class BackupDbContext : DbContext
    {
        public DbSet<BackupJob> BackupJobs { get; set; } = null!;
        public DbSet<BackupDestination> BackupDestinations { get; set; } = null!;
        public DbSet<BackupLog> BackupLogs { get; set; } = null!;
        public DbSet<AppSettings> AppSettings { get; set; } = null!;

        public string DatabaseType { get; private set; } = "SQLite";
        public string DatabaseConnectionString { get; private set; } = "Data Source=backupcr.db";
        public bool IsProduction { get; private set; } = true;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "database.json");
                string connectionString = "";
                string dbType = "SQLite";
                bool isProduction = true; // Default to production (clean database)

                // Check Environment Variables
                string envVal = Environment.GetEnvironmentVariable("BACKUPCR_ENV") 
                             ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") 
                             ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") 
                             ?? "";
                if (envVal.Equals("Development", StringComparison.OrdinalIgnoreCase))
                {
                    isProduction = false;
                }

                if (File.Exists(configPath))
                {
                    try
                    {
                        var json = File.ReadAllText(configPath);
                        using (var doc = JsonDocument.Parse(json))
                        {
                            var root = doc.RootElement;
                            if (root.TryGetProperty("DatabaseType", out var typeProp))
                                dbType = typeProp.GetString() ?? "SQLite";
                            if (root.TryGetProperty("ConnectionString", out var connProp))
                                connectionString = connProp.GetString() ?? "";
                            if (root.TryGetProperty("Production", out var prodProp))
                                isProduction = prodProp.ValueKind == JsonValueKind.True || (prodProp.ValueKind == JsonValueKind.String && prodProp.GetString()?.Equals("true", StringComparison.OrdinalIgnoreCase) == true);
                            else if (root.TryGetProperty("IsProduction", out var isProdProp))
                                isProduction = isProdProp.ValueKind == JsonValueKind.True || (isProdProp.ValueKind == JsonValueKind.String && isProdProp.GetString()?.Equals("true", StringComparison.OrdinalIgnoreCase) == true);
                            else if (root.TryGetProperty("Environment", out var envProp))
                                isProduction = !envProp.GetString()?.Equals("Development", StringComparison.OrdinalIgnoreCase) == true;
                        }
                    }
                    catch
                    {
                        // Fallback
                    }
                }

                DatabaseType = dbType;
                IsProduction = isProduction;

                if (dbType.Equals("MySQL", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(connectionString))
                {
                    DatabaseConnectionString = connectionString;
                    optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
                }
                else
                {
                    string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    string backupCrDir = Path.Combine(appData, "BackupCR");
                    if (!Directory.Exists(backupCrDir))
                    {
                        Directory.CreateDirectory(backupCrDir);
                    }
                    string dbPath = Path.Combine(backupCrDir, "backupcr.db");
                    DatabaseConnectionString = $"Data Source={dbPath}";
                    optionsBuilder.UseSqlite(DatabaseConnectionString);
                }
            }
        }

        public void SeedInitialData()
        {
            // Seed AppSettings
            if (!AppSettings.Any())
            {
                AppSettings.Add(new AppSettings
                {
                    Theme = "Dark",
                    EmailAlertsEnabled = false,
                    RansomwareProtectionEnabled = true,
                    MfaEnabled = false,
                    ActiveDirectoryEnabled = false,
                    DatabaseType = DatabaseType,
                    DatabaseConnectionString = DatabaseConnectionString
                });
                SaveChanges();
            }

            if (IsProduction)
            {
                // Abort example seeding if in production mode
                return;
            }

            // Seed Destinations
            if (!BackupDestinations.Any())
            {
                string localBackupPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "BackupCR_Backups");
                BackupDestinations.Add(new BackupDestination
                {
                    Name = "Disco Local Padrão",
                    Type = "Local",
                    Path = localBackupPath
                });

                BackupDestinations.Add(new BackupDestination
                {
                    Name = "Amazon S3 Cloud Repo",
                    Type = "AmazonS3",
                    Path = "s3://backupcr-enterprise-vault",
                    BucketName = "backupcr-enterprise-vault",
                    Region = "us-east-1",
                    AccessKey = "AKIAIOSFODNN7EXAMPLE",
                    SecretKey = "wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY"
                });

                BackupDestinations.Add(new BackupDestination
                {
                    Name = "SFTP Backup Server",
                    Type = "SFTP",
                    Path = "/mnt/backups/files",
                    ServerAddress = "sftp.litinfor.local",
                    Port = 22,
                    Username = "backup-user",
                    Password = "sftp-strong-password"
                });
            }

            SaveChanges();

            // Seed Jobs
            if (!BackupJobs.Any())
            {
                var defaultDest = BackupDestinations.FirstOrDefault(d => d.Type == "Local");
                var cloudDest = BackupDestinations.FirstOrDefault(d => d.Type == "AmazonS3");

                if (defaultDest != null)
                {
                    string sampleSource = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "BackupCR_SampleSource");
                    if (!Directory.Exists(sampleSource))
                    {
                        try
                        {
                            Directory.CreateDirectory(sampleSource);
                            File.WriteAllText(Path.Combine(sampleSource, "exemplo.txt"), "Este é um arquivo de exemplo para demonstração de backup do BackupCR.");
                            File.WriteAllText(Path.Combine(sampleSource, "dados.cfg"), "config_key=val\nversion=1.0\ndata=random");
                            
                            // Create a subfolder for incremental tests
                            string subfolder = Path.Combine(sampleSource, "Financeiro");
                            Directory.CreateDirectory(subfolder);
                            File.WriteAllText(Path.Combine(subfolder, "balanço.xlsx"), "dados ficticios de balanço financeiro");
                        }
                        catch { }
                    }

                    BackupJobs.Add(new BackupJob
                    {
                        Name = "Backup Diário de Documentos",
                        SourcePaths = sampleSource,
                        BackupType = "Incremental",
                        DestinationId = defaultDest.Id,
                        ScheduleType = "Daily",
                        ScheduleTime = "02:00",
                        RetentionPolicyDays = 14,
                        IsActive = true,
                        CompressData = true,
                        DeduplicateData = true,
                        EncryptData = true,
                        EncryptionKey = "ChaveSecretaSuperForteAES",
                        LastRunStatus = "Never"
                    });

                    BackupJobs.Add(new BackupJob
                    {
                        Name = "Backup de VM Produção - VMware",
                        SourcePaths = "vm-102 (vCenter-Prod)",
                        BackupType = "Full",
                        DestinationId = cloudDest?.Id ?? defaultDest.Id,
                        ScheduleType = "Weekly",
                        ScheduleTime = "Sábado 23:00",
                        RetentionPolicyDays = 30,
                        IsActive = true,
                        CompressData = true,
                        DeduplicateData = true,
                        EncryptData = false,
                        BackupVm = true,
                        VmName = "SRV-WEB-PROD",
                        VmType = "VMware",
                        LastRunStatus = "Never"
                    });

                    // Seed an Active Job that was completed previously to fill the dashboard
                    var completedJob = new BackupJob
                    {
                        Name = "Backup do Banco SQL Server",
                        SourcePaths = "ConnectionString: Server=sql.local;Database=ProdDB;",
                        BackupType = "Full",
                        DestinationId = defaultDest.Id,
                        ScheduleType = "Daily",
                        ScheduleTime = "01:00",
                        RetentionPolicyDays = 7,
                        IsActive = true,
                        CompressData = true,
                        DeduplicateData = false,
                        EncryptData = true,
                        EncryptionKey = "SqlSafePassword",
                        BackupDatabase = true,
                        DbConnectionString = "Server=sql.local;Database=ProdDB;User Id=sa;Password=secret;",
                        LastRunStatus = "Success",
                        LastRunTime = DateTime.Now.AddHours(-15)
                    };
                    BackupJobs.Add(completedJob);
                    SaveChanges();

                    // Seed logs for this completed job
                    BackupLogs.Add(new BackupLog
                    {
                        JobId = completedJob.Id,
                        JobName = completedJob.Name,
                        StartTime = DateTime.Now.AddHours(-15).AddMinutes(-12),
                        EndTime = DateTime.Now.AddHours(-15),
                        Status = "Success",
                        ProcessedBytes = 5368709120, // 5 GB
                        TransferredBytes = 1610612736, // 1.5 GB after compression
                        SpeedMBps = 223.4,
                        BackupType = "Full",
                        LogDetails = "[16:47:58] Iniciando backup do banco de dados SQL Server\n" +
                                     "[16:48:02] Conectando ao servidor sql.local...\n" +
                                     "[16:48:15] Backup nativo do SQL Server iniciado no servidor\n" +
                                     "[16:49:30] Backup exportado com sucesso (5.00 GB)\n" +
                                     "[16:49:32] Iniciando transferência para o destino local...\n" +
                                     "[16:49:50] Criptografando arquivos com AES-256...\n" +
                                     "[16:49:58] Compactando volume...\n" +
                                     "[16:59:45] Transferência concluída. 1.50 GB transferidos a 223.4 MB/s\n" +
                                     "[17:00:00] Backup concluído com sucesso. Política de retenção aplicada (retidos: 7 dias)."
                    });

                    // Add a warning log from yesterday
                    BackupLogs.Add(new BackupLog
                    {
                        JobId = completedJob.Id,
                        JobName = completedJob.Name,
                        StartTime = DateTime.Now.AddDays(-1).AddHours(-15).AddMinutes(-5),
                        EndTime = DateTime.Now.AddDays(-1).AddHours(-15),
                        Status = "Warning",
                        ProcessedBytes = 5368709120,
                        TransferredBytes = 1610612736,
                        SpeedMBps = 180.2,
                        BackupType = "Full",
                        LogDetails = "[16:54:55] Iniciando backup do banco de dados SQL Server\n" +
                                     "[16:55:00] Conectando ao servidor sql.local...\n" +
                                     "[16:55:12] Backup nativo do SQL Server iniciado no servidor\n" +
                                     "[16:56:30] Backup exportado com sucesso (5.00 GB)\n" +
                                     "[16:56:32] Iniciando transferência para o destino local...\n" +
                                     "[16:57:10] ALERTA: Conexão lenta detectada com o destino local\n" +
                                     "[16:59:59] Transferência concluída com avisos de E/S de disco lentos\n" +
                                     "[17:00:00] Backup concluído com avisos."
                    });

                    SaveChanges();
                }
            }
        }
    }
}
