using System.Diagnostics;
using System.IO.Compression;
using LoongEgg.LoongLogger;
using PalWorldServerTool.DataModels;
using Spectre.Console;
using static PalWorldServerTool.Models.HardwareMonitoring;
using static PalWorldServerTool.Models.Consoles;

namespace PalWorldServerTool.Models
{
    public class PalWorldServer
    {
        private ExternalAppController externalAppController = new();
        private Task? serverTaks;

        /// <summary>
        /// 服务器性能监控
        /// </summary>
        private ProcessHarvestingTask? processHarvestingTask;
        private TaskExecutor? taskExecutor;
        /// <summary>
        /// 运行计时
        /// </summary>
        private Stopwatch stopwatch = new Stopwatch();
        /// <summary>
        /// 定时重启
        /// </summary>
        private System.Timers.Timer? rebootServerTime;
        private bool rebootServerTimeisRun = false;
        /// <summary>
        /// 定时备份
        /// </summary>
        private System.Timers.Timer? backupSavedTime;

        private string pathDir;
        private string serverPath;

        private SteamCMD steamCMD;
        private ConfigDataModel configDataModel;

        public bool ServerState { get; private set; } = false;

        public PalWorldServer(string pathDir, SteamCMD steamCMD, ConfigDataModel configDataModel)
        {
            this.pathDir = pathDir;
            this.serverPath = Path.Combine(this.pathDir, "Pal", "Binaries", "Win64", "PalServer-Win64-Test-Cmd.exe");
            this.steamCMD = steamCMD;
            this.configDataModel = configDataModel;
            taskExecutor = new TaskExecutor("PalWorldServerPerformanceMonitorOutput");
            taskExecutor.AddTask(() =>
            {
                if (processHarvestingTask != null && processHarvestingTask.Status == TaskStatus.Running)
                {
                    ClearCMD();
                    var rule = new Rule("PalWorldServer");
                    AnsiConsole.Write(rule);
                    AnsiConsole.WriteLine($"CPU占用: {processHarvestingTask.CPUUsage}% 内存占用: {ConvertByteUnits((double)processHarvestingTask.MemoryUsage!)}");
                    AnsiConsole.WriteLine($"服务器运行时长: {stopwatch.Elapsed}");
                }

            });
            taskExecutor.Initialize();

            //定时重启
            if (configDataModel.TimedRebootServer > 0)
            {
                rebootServerTime = new System.Timers.Timer(configDataModel.TimedRebootServer * 1000 * 60);
                rebootServerTime.AutoReset = true;
                rebootServerTime.Elapsed += async (o, e) =>
                {
                    Logger.WriteInfor("触发定时重启服务器");
                    rebootServerTimeisRun = true;
                    await Shutdown();
                    State();
                    rebootServerTimeisRun = false;
                };
            }
            //定时备份
            if (configDataModel.TimedBackupSaved > 0)
            {
                backupSavedTime = new System.Timers.Timer(configDataModel.TimedBackupSaved * 1000 * 60);
                backupSavedTime.AutoReset = true;
                backupSavedTime.Elapsed += (o, e) =>
                {
                    BackupSaved();
                };
            }

        }

        /// <summary>
        /// 初始化服务器
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Initialization()
        {
            try
            {
                if (!Directory.Exists(this.pathDir))
                    Directory.CreateDirectory(this.pathDir);
                if (!File.Exists(this.serverPath))
                {
                    Logger.WriteInfor("PalWorldServer => 准备安装服务器");
                    if (!steamCMD.SteamCmdRun)
                    {
                        await steamCMD.Initialization();
                    }
                    await steamCMD.WriteLineAsync("app_update 2394010 validate");
                    await steamCMD.AwaitRunning();
                    steamCMD.Stop();
                    Logger.WriteInfor("PalWorldServer => 服务器安装完成!");
                }
                externalAppController.ProcessStarted += serverStart;
                externalAppController.OutputReceived += cmdOutLogInfo;
                externalAppController.ErrorReceived += cmdOutLogWarn;
                externalAppController.ProcessExited += (e, s) =>
                {
                    Logger.WriteInfor("PalWorldServer => 服务器已正常关闭");
                };
                externalAppController.ProcessExitedUnexpectedly += serverExitedUnexpectedly;
                State();
            }
            catch (Exception ex)
            {
                Logger.WriteError($"PalWorldServer => 初始化服务器失败 因为:{ex.Message}");
                return false;
            }
            return true;
        }
        /// <summary>
        /// 启动服务器
        /// </summary>
        public void State()
        {
            if (!ServerState)
            {
                stopwatch.Restart();

                rebootServerTime?.Start();
                backupSavedTime?.Start();

                serverTaks = externalAppController.StartAppAsync(serverPath, "-useperfthreads -NoAsyncLoadingThread -UseMultithreadForDS");
                processHarvestingTask = new(externalAppController.process.Id);
                processHarvestingTask.Run();
                taskExecutor?.Start();
            }
        }
        /// <summary>
        /// 关闭服务器
        /// </summary>
        public async Task Shutdown()
        {
            ClearCMD(1);
            AnsiConsole.MarkupLine("[chartreuse2]服务器正在关闭[/]");

            stopwatch.Reset();
            rebootServerTime?.Stop();
            backupSavedTime?.Stop();

            taskExecutor?.Pause();
            processHarvestingTask?.Pasue();
            processHarvestingTask?.Destroy();

            //关闭备份
            if (configDataModel.ShutdownBackupSaved)
                BackupSaved();

            await externalAppController.RequestStopApp(null, 500);
            ServerState = false;
        }

        private static void cmdOutLogInfo(object? sender, string? data)
        {
            if (!string.IsNullOrWhiteSpace(data))
                Logger.WriteInfor($"PalWorldServer => {data}");
        }
        private static void cmdOutLogWarn(object? sender, string? data)
        {
            if (!string.IsNullOrWhiteSpace(data))
                Logger.WriteWarn($"PalWorldServer => {data}");
        }
        private void serverExitedUnexpectedly(object? sender, EventArgs e)
        {
            ServerState = false;

            rebootServerTime?.Stop();
            backupSavedTime?.Stop();
            taskExecutor?.Pause();
            processHarvestingTask?.Pasue();
            processHarvestingTask?.Destroy();
            Logger.WriteWarn("PalWorldServer => 服务器意外关闭");

            ClearCMD(1);
            AnsiConsole.MarkupLine("[gold3_1]服务器意外关闭[/]");
            if (configDataModel.AutoServerLive)
            {
                Thread.Sleep(1000);
                State();
                return;
            }
            if (!AnsiConsole.Confirm("是否要重启服务器?") && !rebootServerTimeisRun)
            {
                AnsiConsole.MarkupLine("应用即将关闭");
                Thread.Sleep(2000);
                Environment.Exit(0);
            }
            else
            {
                State();
            }
        }
        private void serverStart(object? sender, EventArgs e)
        {
            ServerState = true;
            Logger.WriteInfor("PalWorldServer => 启动服务器");
            ClearCMD();
            var rule = new Rule("PalWorldServer");
            AnsiConsole.Write(rule);
        }
        private void ClearCMD(int startRow = 0, bool goHome = true)
        {
            Console.SetCursorPosition(0, startRow);
            int consoleWidth = Console.WindowWidth;
            int consoleHeight = Console.WindowHeight;

            // 使用空格填充整个控制台屏幕
            for (int i = startRow; i < consoleHeight - 1; i++)
            {
                string line = new string(' ', consoleWidth);
                Console.WriteLine(line);
            }
            Console.SetCursorPosition(0, startRow);
        }
        private void ClearCMDROW(int row)
        {
            Console.SetCursorPosition(0, row);
            string line = new string(' ', Console.WindowWidth);
            Console.WriteLine(line);
            Console.SetCursorPosition(0, row);
        }

        private void BackupSaved()
        {
            Logger.WriteInfor("备份存档");
            if (!Path.Exists(configDataModel.BackupSavedPath))
                Directory.CreateDirectory(configDataModel.BackupSavedPath);
            // 获取当前系统时间
            DateTime currentTime = DateTime.Now;
            ZipFile.CreateFromDirectory(Path.Combine(this.pathDir, "Pal", "Saved"), Path.Combine(configDataModel.BackupSavedPath, currentTime.ToString("yyyy-MM-dd-HH-mm-ss") + ".zip"));
        }
    }
}
