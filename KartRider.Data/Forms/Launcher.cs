using ExcData;
using KartRider.Common.Data;
using KartRider.Common.Utilities;
using KartRider.IO.Packet;
using LoggerLibrary;
using Profile;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KartRider
{
    public partial class Launcher : Form
    {
        public string kartRiderDirectory;
        public static string KartRider;
        public static string pinFile;
        public static string pinFileBak;

        public Launcher()
        {
            RestorePinFile();
            this.InitializeComponent();
        }

        /// <summary>
        /// 恢复备份的 PIN 文件
        /// </summary>
        private void RestorePinFile()
        {
            if (string.IsNullOrEmpty(pinFileBak) || string.IsNullOrEmpty(pinFile))
                return;

            if (File.Exists(pinFileBak))
            {
                try
                {
                    if (File.Exists(pinFile))
                        File.Delete(pinFile);
                    File.Move(pinFileBak, pinFile);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"恢復 PIN 檔案失敗: {ex.Message}");
                }
            }
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            RestorePinFile();
        }

        private void OnLoad(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(pinFile) || !File.Exists(pinFile))
            {
                MessageBox.Show("PIN 檔案路徑無效或檔案不存在！ ", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                ClientVersion.Text = ProfileService.SettingConfig.ClientVersion.ToString();
                Console.WriteLine($"客戶端版本: {ClientVersion.Text}");
                Console.WriteLine($"程序編譯時間: {CompileTime.Time}");
                VersionLabel.Text = CompileTime.Time;
                Console.WriteLine("進程: {0}", KartRider);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加載 PIN 檔案失敗: {ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Start_Button_Click(object sender, EventArgs e)
        {
            if (Process.GetProcessesByName("KartRider").Length != 0)
            {
                LauncherSystem.MessageBoxType1();
            }
            else
            {
                var thread = new Thread(() =>
                {
                    try
                    {
                        LaunchGame();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"啟動遊戲失敗: {ex.Message}");
                    }
                })
                {
                    IsBackground = true,
                    Name = "GameLauncherThread"
                };
                thread.Start();
            }
        }

        /// <summary>
        /// 启动游戏的核心逻辑
        /// </summary>
        private void LaunchGame()
        {
            if (string.IsNullOrEmpty(pinFile) || !File.Exists(pinFile))
            {
                Console.WriteLine("PIN 檔案不存在，無法啟動遊戲");
                return;
            }

            RestorePinFile();

            Console.WriteLine("正在備份舊的PinFile檔。.");
            Console.WriteLine(pinFile);

            try
            {
                File.Copy(pinFile, pinFileBak, overwrite: true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"備份 PIN 檔案失敗: {ex.Message}");
                return;
            }

            PINFile val;
            try
            {
                val = new PINFile(pinFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"讀取 PIN 檔案失敗: {ex.Message}");
                return;
            }

            if (val.AuthMethods != null)
            {
                foreach (PINFile.AuthMethod authMethod in val.AuthMethods)
                {
                    Console.WriteLine("將 IP 位址變更為本機... {0}", authMethod.Name);
                    authMethod.LoginServers?.Clear();
                    authMethod.LoginServers?.Add(new PINFile.IPEndPoint
                    {
                        IP = ProfileService.SettingConfig.ServerIP,
                        Port = ProfileService.SettingConfig.ServerPort
                    });
                }
            }

            if (!ProfileService.SettingConfig.NgsOn && val.BmlObjects != null)
            {
                foreach (BmlObject bml in val.BmlObjects)
                {
                    if (bml.Name == "extra" && bml.SubObjects != null)
                    {
                        for (int i = bml.SubObjects.Count - 1; i >= 0; i--)
                        {
                            Console.WriteLine("正在移除 {0}", bml.SubObjects[i].Item1);
                            if (bml.SubObjects[i].Item1 == "NgsOn")
                            {
                                bml.SubObjects.RemoveAt(i);
                                break;
                            }
                        }
                    }
                }
            }

            try
            {
                File.WriteAllBytes(pinFile, val.GetEncryptedData());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"寫入 PIN 檔案失敗: {ex.Message}");
                return;
            }

            var modifier = new MemoryModifier();
            modifier.LaunchAndModifyMemory(kartRiderDirectory);
        }

        private void Setting_Button_Click(object sender, EventArgs e)
        {
            Program.SettingDlg = new Setting();
            Program.SettingDlg.ShowDialog();
        }

        private void GitHub_Click(object sender, EventArgs e)
        {
            string url = "https://yanygm.github.io/Launcher_V2/";
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"錯誤: {ex.Message}");
            }
        }

        private void KartInfo_Click(object sender, EventArgs e)
        {
            string url = "https://kartinfo.me/thread-9369-1-1.html";
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 标签点击事件处理器（同步包装）
        /// </summary>
        private async void label_Client_Click(object sender, EventArgs e)
        {
            try
            {
                await label_Client_ClickAsync(sender, e);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"檢查更新時出錯: {ex.Message}");
                MessageBox.Show($"檢查更新失敗: {ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 异步执行检查更新逻辑
        /// </summary>
        private async Task label_Client_ClickAsync(object sender, EventArgs e)
        {
            var data = await global::KartRider.Update.GetUpdateAsync();
            if (data == null)
            {
                MessageBox.Show("獲取遊戲版本失敗！", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 彈出“是否”確認框
            DialogResult result = MessageBox.Show(
                $"當前版本為：P{ClientVersion.Text}\n最新版本為：{data.version}\n是否需要更新？",
                "確認操作",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            // 根据用户选择执行对应逻辑
            if (result == DialogResult.Yes)
            {
                RestorePinFile();
                LauncherSystem.CheckGame(kartRiderDirectory);
            }
        }

        private void button_ToggleTerminal_Click(object sender, EventArgs e)
        {
            Program.isVisible = !Program.isVisible;
            Program.ShowWindow(Program.consoleHandle, Program.isVisible ? Program.SW_SHOW : Program.SW_HIDE);
            ProfileService.SettingConfig.Console = Program.isVisible;
            ProfileService.SaveSettings();
        }

        private void ConsoleLogger_Click(object sender, EventArgs e)
        {
            CachedConsoleWriter.SaveToFile();
            CachedConsoleWriter.cachedWriter.ClearCache();
        }

        /// <summary>
        /// 检查指定名称的进程是否正在运行
        /// </summary>
        /// <param name="processName">进程名（不含.exe后缀）</param>
        /// <returns>true=运行中，false=未运行</returns>
        static bool IsProcessRunning(string processName)
        {
            try
            {
                // 关键方法：根据进程名获取所有正在运行的进程
                // GetProcessesByName 会忽略大小写，且不需要.exe后缀
                Process[] processes = Process.GetProcessesByName(processName);

                // 如果数组长度大于0，说明进程正在运行
                return processes.Length > 0;
            }
            catch (Exception ex)
            {
                // 捕获可能的异常（比如权限不足）
                Console.WriteLine($"檢查進程時發生錯誤：{ex.Message}");
                return false;
            }
        }
    }
}