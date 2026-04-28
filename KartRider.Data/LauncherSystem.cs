using KartLibrary.File;
using KartRider.Common.Data;
using KartRider.IO.Packet;
using Profile;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KartRider
{
	public static class LauncherSystem
	{
		public static void MessageBoxType1()
		{
			MessageBox.Show("跑跑卡丁車已經運行了！ ", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		public static void MessageBoxType2()
		{
			MessageBox.Show("已經有一個啟動器在運行了！\n不可以同時運行多個啟動器！\n點擊確認退出程序", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
			Environment.Exit(1);
		}

		public static void MessageBoxType3(string RootDirectory)
		{
			DialogResult result = MessageBox.Show(
				"找不到遊戲文件！\n點擊確認下載遊戲文件到本程序目錄，取消結束程序",
				"確認操作",
				MessageBoxButtons.OKCancel,
				MessageBoxIcon.Question);
			
			if (result == DialogResult.OK)
			{
				// 使用本程序目录作为游戏目录进行下载
				string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                if (string.IsNullOrEmpty(RootDirectory))
				{
                    CheckGame(currentDirectory);
                }
				else
				{
                    CheckGame(RootDirectory);
                }
            }
			else
			{
				Environment.Exit(1);
			}
		}

		public static async Task CheckGameAsync(string kartRiderDirectory)
		{
			// 强制显示终端窗口
			bool wasVisible = Program.isVisible;
			if (!Program.isVisible)
			{
				Program.isVisible = true;
				Program.ShowWindow(Program.consoleHandle, Program.SW_SHOW);
			}
			
			try
			{
				string filePath = JsonHelper.GetFilePath();
				var data = await Update.GetUpdateAsync().ConfigureAwait(false);
				if (data != null)
				{
					await new PatchManager().StartPatchAsync(data.update_prefix, kartRiderDirectory).ConfigureAwait(false);
					Console.WriteLine("遊戲更新完成！");
				}
				else
				{
					MessageBox.Show("獲取遊戲版本失敗！", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"更新過程發生錯誤：{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				Console.Clear();

                if (ProfileService.SettingConfig.ServerIP != "127.0.0.1")
                {
                    PatchManager.StartUpdateAsync(kartRiderDirectory).Wait();
                }

                var packFolderManager = KartRhoFile.Dump(Path.GetFullPath(Path.Combine(kartRiderDirectory, @"Data\aaa.pk")));
				if (packFolderManager == null)
				{
					MessageBox.Show("遊戲文件校驗失敗，請檢查更新伺服器或手動修復遊戲文件。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
					Environment.Exit(1);
				}
				packFolderManager.Reset();
				PINFile val = new PINFile(Path.GetFullPath(Path.Combine(kartRiderDirectory, @"KartRider.pin")));
				ProfileService.SettingConfig.ClientVersion = val.Header.MinorVersion;
				ProfileService.SettingConfig.LocaleID = val.Header.LocaleID;
				ProfileService.SettingConfig.nClientLoc = val.Header.Unk2;
				ProfileService.SaveSettings();
				// 更新完成后，根据设置恢复终端显示状态
				if (!wasVisible && !ProfileService.SettingConfig.Console)
				{
					Program.isVisible = false;
					Program.ShowWindow(Program.consoleHandle, Program.SW_HIDE);
				}
			}
		}

		public static void CheckGame(string kartRiderDirectory)
		{
			Exception capturedException = null;

			// 在新线程中运行异步操作，避免阻塞 UI 线程
			var thread = new Thread(() =>
			{
				try
				{
					Console.WriteLine("[CheckGame] 啟動更新線程");
					CheckGameAsync(kartRiderDirectory).GetAwaiter().GetResult();
					Console.WriteLine("[CheckGame] 更新線程完成");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"[CheckGame] 更新線程異常: {ex.GetType().Name}: {ex.Message}");
					Console.WriteLine($"[CheckGame] 堆棧: {ex.StackTrace}");
					capturedException = ex;
				}
			})
			{
				IsBackground = false, // 改为前台线程，确保异常能被捕获
				Name = "GameUpdateThread"
			};

			Console.WriteLine("[CheckGame] 啟動線程");
			thread.Start();
			Console.WriteLine("[CheckGame] 等待線程完成...");
			thread.Join(); // 等待線程完成
			Console.WriteLine("[CheckGame] 線程已結束");
			if (capturedException != null)
			{
				MessageBox.Show($"更新過程發生錯誤：{capturedException.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
	}
}