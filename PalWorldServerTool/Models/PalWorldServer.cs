using System.Diagnostics;
using System.IO.Compression;
using LoongEgg.LoongLogger;
using PalWorldServerTool.DataModels;
using Spectre.Console;
using static PalWorldServerTool.Models.Consoles;
using static PalWorldServerTool.Models.HardwareMonitoring;

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
        private int serverCrashesSum = 0;
        private DateTime? collapseTime;
        private string? backupSavedPath;
        /// <summary>
        /// 运行计时
        /// </summary>
        private Stopwatch stopwatch = new Stopwatch();
        /// <summary>
        /// 定时重启
        /// </summary>
        private System.Timers.Timer? rebootServerTime;
        private bool rebootServerTimeisRun = false;
        private Stopwatch rebootServerTimeRemaining = new();
        /// <summary>
        /// 定时备份
        /// </summary>
        private System.Timers.Timer? backupSavedTime;
        private Stopwatch backupSavedTimeRemaining = new();

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
                    var rule = new Rule("[yellow]PalWorldServerTool[/]");
                    AnsiConsole.Write(rule);

                    rule = new Rule("[lime]进程信息[/]");
                    rule.Justification = Justify.Left;
                    AnsiConsole.Write(rule);

                    AnsiConsole.MarkupLine($"服务器进程: [green]{processHarvestingTask.Process.ProcessName}[/] PID: [green]{processHarvestingTask.ProcessId}[/]");
                    AnsiConsole.MarkupLine($"CPU占用: {SpectreColorConvert(processHarvestingTask.CPUUsage, 0, 20, 50)} %");
                    AnsiConsole.MarkupLine($"内存占用: {SpectreColorByteUnits((double)processHarvestingTask.MemoryUsage!, 0, 8589934592, 17179869184)}");

                    if (configDataModel.TimedRebootServer > 0 || configDataModel.TimedBackupSaved > 0)
                    {
                        rule = new Rule("[lime]定时任务[/]");
                        rule.Justification = Justify.Left;
                        AnsiConsole.Write(rule);

                        if (configDataModel.TimedBackupSaved > 0)
                        {
                            AnsiConsole.MarkupLine($"距离下次自动备份还有: [green]{ConvertTimeUnitsStr((configDataModel.TimedBackupSaved * 60 - rebootServerTimeRemaining.Elapsed.Seconds) * 1000)}[/]");
                        }
                        if (configDataModel.TimedRebootServer > 0)
                        {
                            AnsiConsole.MarkupLine($"距离下次自动重启还有: [green]{ConvertTimeUnitsStr((configDataModel.TimedRebootServer * 60 - backupSavedTimeRemaining.Elapsed.Seconds) * 1000)}[/]");
                        }
                    }

                    rule = new Rule("[lime]统计数据[/]");
                    rule.Justification = Justify.Left;
                    AnsiConsole.Write(rule);

                    AnsiConsole.MarkupLine($"服务器运行时长: [green]{ConvertTimeUnitsStr(stopwatch.Elapsed.TotalMilliseconds)}[/]");
                    AnsiConsole.MarkupLine($"服务器崩溃次数: {SpectreColorConvert(serverCrashesSum, 0, 1, 5)} 次");
                    if (collapseTime != null)
                        AnsiConsole.MarkupLine($"服务器上次崩溃时间: {collapseTime}");
                    if(backupSavedPath != null)
                        AnsiConsole.MarkupLine($"上个存档备份位置: {backupSavedPath}");

                    rule = new Rule("[lime]配置[/]");
                    rule.Justification = Justify.Left;
                    AnsiConsole.Write(rule);

                    AnsiConsole.MarkupLine($"服务器保活: {SpectreColorConvert(configDataModel.AutoServerLive)}");
                    AnsiConsole.MarkupLine($"服务器定时重启: {SpectreColorConvert(configDataModel.TimedRebootServer > 0)}");
                    AnsiConsole.MarkupLine($"服务器定时备份: {SpectreColorConvert(configDataModel.TimedBackupSaved > 0)}");
                    AnsiConsole.MarkupLine($"服务器关闭备份: {SpectreColorConvert(configDataModel.ShutdownBackupSaved)}");
                    AnsiConsole.MarkupLine($"服务器备份位置: {configDataModel.BackupSavedPath}");

                    Console.SetCursorPosition(0, Console.WindowHeight - 3);
                    AnsiConsole.MarkupLine("建议使用 [lime]Ctrl+C[/] 关闭本程序 否则PalWorldServer进程可能会没有办法被正常关闭");
                    Console.SetCursorPosition(0, Console.WindowHeight-2);
                    rule = new Rule("by [yellow]CK小捷[/] 项目地址 [blue]https://github.com/44578287/PalWorldServerTool[/]");
                    AnsiConsole.Write(rule);
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
                    rebootServerTimeRemaining.Restart();
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
                    backupSavedTimeRemaining.Restart();
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
                    await AnsiConsole.Status()
                        .StartAsync("安装PalWorldServer", async ctx =>
                        {
                            await steamCMD.WriteLineAsync("app_update 2394010 validate");
                            await steamCMD.AwaitRunning();
                            steamCMD.Stop();
                        });
                    AnsiConsole.MarkupLine("[chartreuse2]服务器安装完成[/]");
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
                rebootServerTimeRemaining.Restart();
                backupSavedTime?.Start();
                backupSavedTimeRemaining.Restart();

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
            rebootServerTimeRemaining?.Reset();
            backupSavedTime?.Stop();
            backupSavedTimeRemaining?.Reset();

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
            serverCrashesSum++;
            collapseTime = DateTime.Now;

            rebootServerTime?.Stop();
            backupSavedTime?.Stop();
            taskExecutor?.Pause();
            processHarvestingTask?.Pasue();
            processHarvestingTask?.Destroy();
            Logger.WriteWarn("PalWorldServer => 服务器意外关闭");

            ClearCMD(0);
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
                System.Environment.Exit(0);
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
        private string BackupSaved()
        {
            Logger.WriteInfor("备份存档");
            if (!Path.Exists(configDataModel.BackupSavedPath))
                Directory.CreateDirectory(configDataModel.BackupSavedPath);
            // 获取当前系统时间
            DateTime currentTime = DateTime.Now;
            string path = Path.Combine(configDataModel.BackupSavedPath, currentTime.ToString("yyyy-MM-dd-HH-mm-ss") + ".zip");
            ZipFile.CreateFromDirectory(Path.Combine(this.pathDir, "Pal", "Saved"), path);
            backupSavedPath = path;
            return path;
        }
    }
}
