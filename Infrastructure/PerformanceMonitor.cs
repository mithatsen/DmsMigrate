using System.Diagnostics;

namespace DMSMigration.Infrastructure;

public class PerformanceMonitor
{
    private readonly Stopwatch _totalStopwatch;
    private readonly Stopwatch _batchStopwatch;
    private long _processedFiles;
    private long _totalFiles;
    private long _totalBytesProcessed;
    private DateTime _startTime;

    public PerformanceMonitor()
    {
        _totalStopwatch = new Stopwatch();
        _batchStopwatch = new Stopwatch();
    }

    public void Start(long totalFiles)
    {
        _totalFiles = totalFiles;
        _processedFiles = 0;
        _totalBytesProcessed = 0;
        _startTime = DateTime.Now;
        _totalStopwatch.Start();
    }

    public void StartBatch()
    {
        _batchStopwatch.Restart();
    }

    public void RecordFile(long fileSize)
    {
        _processedFiles++;
        _totalBytesProcessed += fileSize;
    }

    public void RecordSkippedFile()
    {
        // Skip edilen dosyalar da "işlendi" sayılır (progress için)
        _processedFiles++;
    }

    public void EndBatch(int batchSize)
    {
        _batchStopwatch.Stop();
        
        var batchTime = _batchStopwatch.Elapsed;
        var filesPerSecond = batchSize / (batchTime.TotalSeconds > 0 ? batchTime.TotalSeconds : 1);
        var avgFileSize = _totalBytesProcessed / (_processedFiles > 0 ? _processedFiles : 1);
        var throughputMBps = (_totalBytesProcessed / 1024.0 / 1024.0) / (_totalStopwatch.Elapsed.TotalSeconds > 0 ? _totalStopwatch.Elapsed.TotalSeconds : 1);
        
        Console.WriteLine($"Batch Performansı: {batchSize} dosya / {batchTime.TotalSeconds:F2}sn = {filesPerSecond:F2} dosya/sn");
        Console.WriteLine($"Ortalama Aktarım Hızı: {throughputMBps:F2} MB/sn");
    }

    public PerformanceStats GetStats()
    {
        var elapsed = _totalStopwatch.Elapsed;
        var remainingFiles = _totalFiles - _processedFiles;
        var avgTimePerFile = _processedFiles > 0 ? elapsed.TotalSeconds / _processedFiles : 0;
        var estimatedTimeRemaining = TimeSpan.FromSeconds(remainingFiles * avgTimePerFile);
        var completionPercentage = _totalFiles > 0 ? (_processedFiles * 100.0 / _totalFiles) : 0;

        return new PerformanceStats
        {
            ProcessedFiles = _processedFiles,
            TotalFiles = _totalFiles,
            RemainingFiles = remainingFiles,
            CompletionPercentage = completionPercentage,
            TotalBytesProcessed = _totalBytesProcessed,
            AverageFileSize = _processedFiles > 0 ? _totalBytesProcessed / _processedFiles : 0,
            ElapsedTime = elapsed,
            EstimatedTimeRemaining = estimatedTimeRemaining,
            FilesPerSecond = _processedFiles / (elapsed.TotalSeconds > 0 ? elapsed.TotalSeconds : 1),
            ThroughputMBps = (_totalBytesProcessed / 1024.0 / 1024.0) / (elapsed.TotalSeconds > 0 ? elapsed.TotalSeconds : 1)
        };
    }

    public void Stop()
    {
        _totalStopwatch.Stop();
    }

    public void PrintProgress()
    {
        var stats = GetStats();
        
        Console.WriteLine();
        Console.WriteLine("=== Migration İlerleme Raporu ===");
        Console.WriteLine($"İşlenen Dosya: {stats.ProcessedFiles:N0} / {stats.TotalFiles:N0} ({stats.CompletionPercentage:F2}%)");
        Console.WriteLine($"Toplam Veri: {FormatBytes(stats.TotalBytesProcessed)}");
        Console.WriteLine($"Ortalama Dosya Boyutu: {FormatBytes(stats.AverageFileSize)}");
        Console.WriteLine($"Geçen Süre: {FormatTimeSpan(stats.ElapsedTime)}");
        Console.WriteLine($"Kalan Tahmini Süre: {FormatTimeSpan(stats.EstimatedTimeRemaining)}");
        Console.WriteLine($"Hız: {stats.FilesPerSecond:F2} dosya/sn, {stats.ThroughputMBps:F2} MB/s");
        
        // Progress bar
        var barLength = 50;
        var filledLength = (int)(stats.CompletionPercentage / 100.0 * barLength);
        var bar = new string('=', filledLength) + new string('-', barLength - filledLength);
        Console.WriteLine($"[{bar}] {stats.CompletionPercentage:F1}%");
        Console.WriteLine();
    }

    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:F2} {sizes[order]}";
    }

    private static string FormatTimeSpan(TimeSpan timeSpan)
    {
        if (timeSpan.TotalHours >= 1)
            return $"{timeSpan.Hours}s {timeSpan.Minutes}d {timeSpan.Seconds}sn";
        else if (timeSpan.TotalMinutes >= 1)
            return $"{timeSpan.Minutes}d {timeSpan.Seconds}sn";
        else
            return $"{timeSpan.Seconds}sn";
    }
}

public class PerformanceStats
{
    public long ProcessedFiles { get; set; }
    public long TotalFiles { get; set; }
    public long RemainingFiles { get; set; }
    public double CompletionPercentage { get; set; }
    public long TotalBytesProcessed { get; set; }
    public long AverageFileSize { get; set; }
    public TimeSpan ElapsedTime { get; set; }
    public TimeSpan EstimatedTimeRemaining { get; set; }
    public double FilesPerSecond { get; set; }
    public double ThroughputMBps { get; set; }
}
