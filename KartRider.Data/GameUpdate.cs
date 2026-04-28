using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using KartRider.IO.Packet;
using Profile;

namespace KartRider;

public class PatchManager
{
    // 配置
    private const int MaxRetryCount = 3;
    private const int MaxParallelDownloads = 4; // 同时下载文件数

    public async Task StartPatchAsync(string update_prefix, string gamePath)
    {
        try
        {
            string PatchListUrl = update_prefix + "/NT.txf";
            string TempPath = Path.Combine(gamePath, "temp");
            string PatchListPath = Path.Combine(gamePath, "NT.txf");

            ServicePointManager.DefaultConnectionLimit = 100;
            Console.WriteLine("=== 跑跑卡丁車自動更新器 ===");

            if (!Directory.Exists(TempPath))
                Directory.CreateDirectory(TempPath);

            Console.WriteLine("正在下載補丁列表...");
            Console.WriteLine($"URL: {PatchListUrl}");
            Console.WriteLine($"保存路徑: {PatchListPath}");

            // 下載補丁列表到 gamePath 目錄
            await DownloadFileAsync(PatchListUrl, PatchListPath);

            Console.WriteLine("補丁列表下載完成，正在解析...");

            // 從本地文件讀取內容
            var content = await File.ReadAllTextAsync(PatchListPath);
            var lines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            var items = ParsePatchLines(lines);
            Console.WriteLine($"獲取到 {items.Count} 個文件，開始校驗更新...\n");

            // 第一階段：並行校驗所有文件，收集需要下載的文件
            Console.WriteLine("開始並行校驗文件...");
            var filesToDownload = new List<PatchItem>();
            int checkedCount = 0;
            var checkLock = new object();

            await Parallel.ForEachAsync(items, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, async (item, ct) =>
            {
                bool needsDownload = await CheckFileNeedsDownloadAsync(item, gamePath);
                lock (checkLock)
                {
                    checkedCount++;
                    if (needsDownload)
                    {
                        filesToDownload.Add(item);
                        Console.WriteLine($"[{checkedCount}/{items.Count}] {item.LocalPath} 需要下載");
                    }
                    else
                    {
                        Console.WriteLine($"[{checkedCount}/{items.Count}] {item.LocalPath} 已最新");
                    }
                }
            });

            Console.WriteLine($"\n校驗完成：{items.Count - filesToDownload.Count} 個文件已最新，{filesToDownload.Count} 個文件需要下載\n");

            // 第二階段：限制並發數並行下載
            if (filesToDownload.Count > 0)
            {
                Console.WriteLine($"開始並行下載（最多 {MaxParallelDownloads} 個文件同時下載）...\n");
                var semaphore = new SemaphoreSlim(MaxParallelDownloads);
                var downloadTasks = filesToDownload.Select(async item =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        Console.WriteLine($"[下載開始] {item.LocalPath}");
                        await ProcessFileDownloadAsync(item, update_prefix, TempPath, gamePath);
                        Console.WriteLine($"[下載完成] {item.LocalPath}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[下載失敗] {item.LocalPath}: {ex.Message}");
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }).ToArray();

                await Task.WhenAll(downloadTasks);
            }

            Console.WriteLine("\n=== 全部更新完成 ===");

            // 重命名 NT.txf 为 LK.txf
            try
            {
                if (File.Exists(PatchListPath))
                {
                    string lkPath = Path.Combine(gamePath, "LK.txf");
                    if (File.Exists(lkPath))
                    {
                        File.Delete(lkPath);
                    }
                    File.Move(PatchListPath, lkPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"重命名文件失败: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n更新過程發生嚴重錯誤: {ex.Message}");
            Console.WriteLine($"堆栈: {ex.StackTrace}");
            throw;
        }
    }

    private List<PatchItem> ParsePatchLines(string[] lines)
    {
        var list = new List<PatchItem>();
        foreach (var line in lines)
        {
            try
            {
                var parts = line.Split(':');
                // LocalPath: 将反斜杠转换为正斜杠（用于URL），同时保留本地路径格式
                var localPath = parts[1].TrimStart('\\').Replace('\\', '/');
                list.Add(new PatchItem
                {
                    FileName = parts[0],
                    LocalPath = localPath,
                    ServerMd5 = parts[2],
                    TotalSize = long.Parse(parts[3].TrimEnd('l'))
                });
            }
            catch { }
        }
        return list;
    }

    // 仅检查文件是否需要下载（不下载）
    private async Task<bool> CheckFileNeedsDownloadAsync(PatchItem item, string gamePath)
    {
        string localFilePath = item.LocalPath.Replace('/', Path.DirectorySeparatorChar);
        string filePath = Path.Combine(gamePath, localFilePath);

        // 文件不存在，需要下载
        if (!File.Exists(filePath))
        {
            return true;
        }

        // 计算MD5校验
        try
        {
            var localMd5 = await Task.Run(() => GetFileMd5(filePath));
            return !string.Equals(localMd5, item.ServerMd5, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            // MD5计算失败，重新下载
            return true;
        }
    }

    // 仅执行下载和安装（不校验）
    private async Task ProcessFileDownloadAsync(PatchItem item, string update_prefix, string TempPath, string gamePath)
    {
        string localFilePath = item.LocalPath.Replace('/', Path.DirectorySeparatorChar);
        string filePath = Path.Combine(gamePath, localFilePath);
        var tempFile = Path.Combine(TempPath, item.FileName);
        var url = update_prefix + "/" + item.LocalPath;

        try
        {
            // 确保临时目录存在
            var tempDir = Path.GetDirectoryName(tempFile);
            if (!Directory.Exists(tempDir))
                Directory.CreateDirectory(tempDir);

            // 下载文件
            await DownloadWithRetryAsync(url, tempFile, item.TotalSize);

            // 校验下载文件MD5
            var downMd5 = await Task.Run(() => GetFileMd5(tempFile));
            if (!string.Equals(downMd5, item.ServerMd5, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"  {item.LocalPath} 下載MD5校驗失敗！");
                File.Delete(tempFile);
                throw new Exception("MD5校驗失敗");
            }

            // 移动到目标位置
            var dir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            if (File.Exists(filePath))
                File.Delete(filePath);
            File.Move(tempFile, filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  {item.LocalPath} 處理失敗：{ex.Message}");
            throw;
        }
    }

    private async Task ProcessFileAsync(PatchItem item, string update_prefix, string TempPath, string gamePath)
    {
        Console.WriteLine($"檢查：{item.LocalPath}");

        try
        {
            // LocalPath 使用正斜杠，转换为系统路径分隔符
            string localFilePath = item.LocalPath.Replace('/', Path.DirectorySeparatorChar);
            string filePath = Path.Combine(gamePath, localFilePath);

            // 检查文件是否存在且MD5匹配
            if (File.Exists(filePath))
            {
                Console.WriteLine($"→ 計算本地MD5...");
                var localMd5 = GetFileMd5(filePath);
                if (string.Equals(localMd5, item.ServerMd5, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"→ {filePath} 已最新，跳過\n");
                    return;
                }
                Console.WriteLine($"→ {filePath} MD5不匹配，需要更新");
            }
            else
            {
                Console.WriteLine($"→ {filePath} 不存在，開始下載");
            }

            // 准备下载
            var tempFile = Path.Combine(TempPath, item.FileName);
            var url = update_prefix + "/" + item.LocalPath;

            Console.WriteLine($"→ URL: {url}");
            Console.WriteLine($"→ 臨時文件: {tempFile}");

            // 确保临时目录存在
            var tempDir = Path.GetDirectoryName(tempFile);
            if (!Directory.Exists(tempDir))
                Directory.CreateDirectory(tempDir);

            // 下载文件
            await DownloadWithRetryAsync(url, tempFile, item.TotalSize);

            // 校验MD5
            Console.WriteLine($"→ 校驗下載文件MD5...");
            var downMd5 = GetFileMd5(tempFile);
            if (!string.Equals(downMd5, item.ServerMd5, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"→ {filePath} 下載MD5校驗失敗！");
                File.Delete(tempFile);
                return;
            }

            // 移动到目标位置
            var dir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            if (File.Exists(filePath))
                File.Delete(filePath);
            File.Move(tempFile, filePath);

            Console.WriteLine($"→ {filePath} 更新成功！\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"→ {item.LocalPath} 處理失敗：{ex.GetType().Name}: {ex.Message}");
            Console.WriteLine($"→ 詳細錯誤：{ex.StackTrace}\n");
            throw;
        }
    }

    private async Task DownloadWithRetryAsync(string url, string path, long total)
    {
        for (int i = 0; i < MaxRetryCount; i++)
        {
            try
            {
                // 单线程下载（服务器不支持多线程）
                await SingleThreadDownloadAsync(url, path, total);
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n→ 下載失敗 ({i + 1}/{MaxRetryCount}): {ex.Message}");
                if (i < MaxRetryCount - 1)
                    await Task.Delay(1000 * (i + 1));
            }
        }
        throw new Exception("重試失敗");
    }

    // 单线程下载（服务器不支持多线程），带进度显示
    private async Task SingleThreadDownloadAsync(string url, string savePath, long totalSize = 0)
    {
        var fileName = Path.GetFileName(savePath);
        var startTime = DateTime.Now;

        Console.WriteLine($"→ 開始下載: {url}");

        // 使用 WebRequest 替代 HttpClient，避免同步上下文問題
        try
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Timeout = 600000; // 10分鐘
            request.ReadWriteTimeout = 600000;
            request.KeepAlive = true;

            using var response = (HttpWebResponse)await request.GetResponseAsync();

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"HTTP错误: {response.StatusCode}");
            }

            var totalBytes = response.ContentLength > 0 ? response.ContentLength : totalSize;

            using var responseStream = response.GetResponseStream();
            using var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

            var buffer = new byte[8192];
            long downloadedBytes = 0;
            long lastReportedBytes = 0;
            var lastReportTime = DateTime.Now;

            while (true)
            {
                int read = await responseStream.ReadAsync(buffer, 0, buffer.Length);
                if (read == 0) break;

                await fileStream.WriteAsync(buffer, 0, read);
                downloadedBytes += read;

                // 每 500ms 或每下载 1MB 更新一次显示
                var now = DateTime.Now;
                var timeDiff = (now - lastReportTime).TotalMilliseconds;
                var bytesDiff = downloadedBytes - lastReportedBytes;

                if (timeDiff >= 500 || bytesDiff >= 1024 * 1024)
                {
                    var speed = bytesDiff / (timeDiff / 1000.0);
                    var speedText = FormatSpeed(speed);
                    var progressText = totalBytes > 0
                        ? $"{(int)((double)downloadedBytes / totalBytes * 100)}%"
                        : $"{FormatBytes(downloadedBytes)}";
                    var totalText = totalBytes > 0 ? $" / {FormatBytes(totalBytes)}" : "";

                    Console.Write($"\r→ 下载 {fileName}: {progressText}{totalText} [{speedText}]       ");

                    lastReportTime = now;
                    lastReportedBytes = downloadedBytes;
                }
            }

            var elapsed = DateTime.Now - startTime;
            var avgSpeed = downloadedBytes / elapsed.TotalSeconds;
            Console.WriteLine($"\r→ 下载 {fileName}: 完成 [{FormatBytes(downloadedBytes)}] [{FormatSpeed(avgSpeed)}]       ");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n→ 下載異常: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    // 格式化字节大小
    private string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        int order = 0;
        double size = bytes;
        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }
        return $"{size:0.00} {sizes[order]}";
    }

    // 格式化速度
    private string FormatSpeed(double bytesPerSecond)
    {
        return $"{FormatBytes((long)bytesPerSecond)}/s";
    }

    // 高速MD5计算：内存映射文件 + 大缓冲顺序读取
    private string GetFileMd5(string path)
    {
        try
        {
            var fileInfo = new FileInfo(path);
            long fileSize = fileInfo.Length;

            // 小文件用普通流，大文件用内存映射
            const long MemoryMapThreshold = 50 * 1024 * 1024; // 50MB
            if (fileSize < MemoryMapThreshold)
            {
                return ComputeMd5WithBuffer(path);
            }

            return ComputeMd5WithMemoryMap(path, fileSize);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  計算MD5失敗: {ex.Message}");
            throw;
        }
    }

    // 大缓冲顺序读取计算MD5
    private string ComputeMd5WithBuffer(string path)
    {
        using var md5 = MD5.Create();
        // 1MB缓冲，顺序读取最大化磁盘吞吐量
        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1024 * 1024, FileOptions.SequentialScan);

        var buffer = new byte[1024 * 1024]; // 1MB缓冲
        int bytesRead;

        while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
        {
            md5.TransformBlock(buffer, 0, bytesRead, null, 0);
        }
        md5.TransformFinalBlock(buffer, 0, 0);

        return BitConverter.ToString(md5.Hash).Replace("-", "").ToLower();
    }

    // 内存映射文件计算MD5 - 利用操作系统缓存机制
    private string ComputeMd5WithMemoryMap(string path, long fileSize)
    {
        using var md5 = MD5.Create();

        // 创建内存映射文件
        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var memoryMappedFile = System.IO.MemoryMappedFiles.MemoryMappedFile.CreateFromFile(
            fs, null, 0, System.IO.MemoryMappedFiles.MemoryMappedFileAccess.Read, HandleInheritability.None, true);

        const long ViewSize = 16 * 1024 * 1024; // 16MB视图窗口
        long offset = 0;
        var buffer = new byte[ViewSize];

        while (offset < fileSize)
        {
            long remaining = fileSize - offset;
            long currentViewSize = Math.Min(ViewSize, remaining);

            using var accessor = memoryMappedFile.CreateViewAccessor(offset, currentViewSize, System.IO.MemoryMappedFiles.MemoryMappedFileAccess.Read);

            // 分块读取视图内容
            long bytesRead = 0;
            while (bytesRead < currentViewSize)
            {
                int toRead = (int)Math.Min(buffer.Length, currentViewSize - bytesRead);
                accessor.ReadArray(bytesRead, buffer, 0, toRead);
                md5.TransformBlock(buffer, 0, toRead, null, 0);
                bytesRead += toRead;
            }

            offset += currentViewSize;
        }

        md5.TransformFinalBlock(buffer, 0, 0);
        return BitConverter.ToString(md5.Hash).Replace("-", "").ToLower();
    }

    private async Task<string> DownloadStringAsync(string url)
    {
        using var wc = new WebClient();
        return await wc.DownloadStringTaskAsync(url);
    }

    private async Task DownloadFileAsync(string url, string path)
    {
        Console.WriteLine($"  [DownloadFile] 開始下載到: {path}");
        try
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Timeout = 60000;
            request.ReadWriteTimeout = 60000;

            using var response = (HttpWebResponse)await request.GetResponseAsync();
            using var responseStream = response.GetResponseStream();
            using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);

            await responseStream.CopyToAsync(fileStream);
            Console.WriteLine($"  [DownloadFile] 下载完成");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  [DownloadFile] 錯誤: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 连接TCP服务并获取返回数据
    /// </summary>
    /// <param name="ip">TCP地址</param>
    /// <param name="port">端口</param>
    /// <param name="sendStr">要发送的内容</param>
    /// <returns>服务端返回字符串</returns>
    public static async Task<byte[]> GetPatchUrl(string ip, int port, int recvBufferSize = 4096, int connectTimeoutMs = 3000, int readTimeoutMs = 3000)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(connectTimeoutMs + readTimeoutMs));

            using TcpClient client = new TcpClient(AddressFamily.InterNetwork)
            {
                NoDelay = true
            };

            // 使用 Task.WhenAny 实现连接超时
            var connectTask = client.ConnectAsync(ip, port);
            var timeoutTask = Task.Delay(connectTimeoutMs, cts.Token);

            var completedTask = await Task.WhenAny(connectTask, timeoutTask);
            if (completedTask == timeoutTask)
            {
                return Array.Empty<byte>();
            }

            // 确保连接完成
            await connectTask;

            using NetworkStream stream = client.GetStream();

            // 读取数据，带超时
            byte[] buffer = new byte[recvBufferSize];
            var readTask = stream.ReadAsync(buffer, 0, buffer.Length, cts.Token);
            var readTimeout = Task.Delay(readTimeoutMs, cts.Token);

            completedTask = await Task.WhenAny(readTask, readTimeout);
            if (completedTask == readTimeout)
            {
                return Array.Empty<byte>();
            }

            int readLen = await readTask;
            if (readLen <= 0)
            {
                return Array.Empty<byte>();
            }

            byte[] res = new byte[readLen];
            Buffer.BlockCopy(buffer, 0, res, 0, readLen);

            // 获取数据后断开连接
            client.Close();
            return res;
        }
        catch
        {
            return Array.Empty<byte>();
        }
    }

    public static async Task StartUpdateAsync(string RootDirectory)
    {
        byte[] recvData = await GetPatchUrl(ProfileService.SettingConfig.ServerIP, ProfileService.SettingConfig.ServerPort);

        if (recvData.Length <= 0)
        {
            return;
        }

        InPacket inPacket = new InPacket(recvData);
        inPacket.ReadUInt();
        inPacket.ReadUInt();
        inPacket.ReadUShort();
        inPacket.ReadUShort();
        ushort ClientVersion = inPacket.ReadUShort();
        string updateUrl = inPacket.ReadString();

        if (ClientVersion != ProfileService.SettingConfig.ClientVersion)
        {
            // 彈出“是否”確認框
            DialogResult result = MessageBox.Show(
                $"與伺服器版本不一致，是否需要更新遊戲？",
                "確認操作",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            // 根據用戶選擇執行對應邏輯
            if (result == DialogResult.Yes)
            {
                LauncherSystem.CheckGame(RootDirectory);
                return;
            }
            else
            {
                return;
            }
        }

        if (string.IsNullOrWhiteSpace(updateUrl))
        {
            return;
        }

        // 拼接補丁程序路徑
        string Patcher = Path.GetFullPath(Path.Combine(RootDirectory, "Patcher.exe"));
        Console.WriteLine("啟動路徑：" + Patcher);

        // 安全判斷：文件必須存在
        if (!File.Exists(Patcher))
        {
            Console.WriteLine($"錯誤：找不到文件 {Patcher}");
            return;
        }

        string arguments = $"'1' '123' '{updateUrl}' '{RootDirectory}' '{RootDirectory}'";

        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = Patcher,
                Arguments = arguments,
                UseShellExecute = true,
            };

            using (Process process = Process.Start(startInfo))
            {
                Console.WriteLine("Patcher 已啟動，等待更新完成...");

                // ✅ 異步等待，不卡死，真正等它結束
                await process.WaitForExitAsync();

                int exitCode = process.ExitCode;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"更新啟動失敗：{ex.Message}");
        }
    }

    private class PatchItem
    {
        public string FileName { get; set; }
        public string LocalPath { get; set; }
        public string ServerMd5 { get; set; }
        public long TotalSize { get; set; }
    }
}