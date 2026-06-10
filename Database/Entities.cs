using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackupCR.Database
{
    public class BackupDestination
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = "Local"; // Local, USB, NetworkShare, FTP, SFTP, AmazonS3, AzureBlob, GoogleCloud

        [Required]
        [MaxLength(500)]
        public string Path { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? ServerAddress { get; set; }

        public int? Port { get; set; }

        [MaxLength(150)]
        public string? Username { get; set; }

        [MaxLength(150)]
        public string? Password { get; set; }

        [MaxLength(150)]
        public string? BucketName { get; set; }

        [MaxLength(250)]
        public string? AccessKey { get; set; }

        [MaxLength(250)]
        public string? SecretKey { get; set; }

        [MaxLength(100)]
        public string? Region { get; set; }
    }

    public class BackupJob
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string SourcePaths { get; set; } = string.Empty; // Semi-colon separated file/directory paths

        [Required]
        [MaxLength(50)]
        public string BackupType { get; set; } = "Full"; // Full, Incremental, Differential, Synthetic

        [Required]
        public int DestinationId { get; set; }

        [ForeignKey("DestinationId")]
        public BackupDestination? Destination { get; set; }

        [Required]
        [MaxLength(50)]
        public string ScheduleType { get; set; } = "Manual"; // Manual, Daily, Weekly, Monthly, Continuous

        [MaxLength(50)]
        public string ScheduleTime { get; set; } = "02:00"; // HH:mm or schedule details

        public int RetentionPolicyDays { get; set; } = 30;

        public bool IsActive { get; set; } = true;

        public bool CompressData { get; set; } = true;

        public bool DeduplicateData { get; set; } = true;

        public bool EncryptData { get; set; } = false;

        [MaxLength(250)]
        public string? EncryptionKey { get; set; }

        // Virtual Machines Backups
        public bool BackupVm { get; set; } = false;
        
        [MaxLength(150)]
        public string? VmName { get; set; }

        [MaxLength(50)]
        public string? VmType { get; set; } // VMware, Hyper-V

        // Database backups
        public bool BackupDatabase { get; set; } = false;
        
        [MaxLength(500)]
        public string? DbConnectionString { get; set; }

        [MaxLength(50)]
        public string? LastRunStatus { get; set; } = "Never"; // Success, Warning, Error, Never

        public DateTime? LastRunTime { get; set; }
    }

    public class BackupLog
    {
        [Key]
        public int Id { get; set; }

        public int? JobId { get; set; }

        [Required]
        [MaxLength(100)]
        public string JobName { get; set; } = string.Empty;

        public DateTime StartTime { get; set; } = DateTime.Now;

        public DateTime? EndTime { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Running"; // Running, Success, Warning, Error

        public long ProcessedBytes { get; set; }

        public long TransferredBytes { get; set; }

        public double SpeedMBps { get; set; }

        public string LogDetails { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string BackupType { get; set; } = "Full";
    }

    public class AppSettings
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Theme { get; set; } = "Dark"; // Dark, Light

        public bool EmailAlertsEnabled { get; set; } = false;

        [MaxLength(250)]
        public string? SmtpServer { get; set; }

        public int? SmtpPort { get; set; }

        [MaxLength(150)]
        public string? SmtpUser { get; set; }

        [MaxLength(150)]
        public string? SmtpPassword { get; set; }

        [MaxLength(250)]
        public string? AlertEmailAddress { get; set; }

        [MaxLength(500)]
        public string? WebhookUrl { get; set; }

        public bool RansomwareProtectionEnabled { get; set; } = true;

        public bool MfaEnabled { get; set; } = false;

        public bool ActiveDirectoryEnabled { get; set; } = false;

        [MaxLength(250)]
        public string? AdDomain { get; set; }

        [MaxLength(250)]
        public string? AdGroup { get; set; }

        [Required]
        [MaxLength(50)]
        public string DatabaseType { get; set; } = "SQLite"; // SQLite, MySQL

        [Required]
        public string DatabaseConnectionString { get; set; } = "Data Source=backupcr.db";
    }
}
