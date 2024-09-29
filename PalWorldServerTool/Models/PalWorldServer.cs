using System.Diagnostics;
using System.IO.Compression;
using System.Text.RegularExpressions;
using LoongEgg.LoongLogger;
using PalWorldServerTool.DataModels;
using RconSharp;
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
        private string palWorldSettingsPath;

        //工具配置
        private SteamCMD steamCMD;
        private ConfigDataModel configDataModel;
        //服务器配置
        private PalWorldSettingsDataModel? palWorldSettings;
        //Rcon
        private RconClient? rconClient;
        private bool rconConnect = false;
        private bool authenticated = false;

        private string? serverVersion;
        private int serverPlayerCount = -1;

        public bool ServerState { get; private set; } = false;

        public PalWorldServer(string pathDir, SteamCMD steamCMD, ConfigDataModel configDataModel)
        {
            this.pathDir = pathDir;
            this.serverPath = Path.Combine(this.pathDir, "Pal", "Binaries", "Win64", "PalServer-Win64-Shipping-Cmd.exe");
            this.palWorldSettingsPath = Path.Combine(this.pathDir, "Pal", "Saved", "Config", "WindowsServer", "PalWorldSettings.ini");
            this.steamCMD = steamCMD;
            this.configDataModel = configDataModel;

            this.palWorldSettings = new PalWorldSettingsDataModel();

            if (!File.Exists(this.palWorldSettingsPath))
            {
                Directory.CreateDirectory(Path.Combine(this.pathDir, "Pal", "Saved", "Config", "WindowsServer"));
                File.Create(this.palWorldSettingsPath).Close();

                palWorldSettings.AdminPassword = configDataModel.AdminPassword ??= RandomString(8).Standardize();
                palWorldSettings.Save(this.palWorldSettingsPath);
                Logger.WriteInfor("PalWorldServer => 服务器配置文件创建成功");
            }
            if (!palWorldSettings.Load(this.palWorldSettingsPath))
            {
                palWorldSettings = new PalWorldSettingsDataModel();
                palWorldSettings.Save(this.palWorldSettingsPath);
                Logger.WriteInfor("PalWorldServer => 服务器配置文件加载失败 重新创建");
            }
            if (!palWorldSettings.RCONEnabled)
            {
                palWorldSettings.RCONEnabled = true;
                palWorldSettings.Save(this.palWorldSettingsPath);
                palWorldSettings.Load(this.palWorldSettingsPath);
                Logger.WriteInfor("PalWorldServer => 服务器未开启RCON 重新创建");
            }
            if (string.IsNullOrEmpty(palWorldSettings.AdminPassword) || palWorldSettings.AdminPassword.All(c => c == '"'))
            {
                palWorldSettings.AdminPassword = configDataModel.AdminPassword ??= RandomString(8).Standardize();
                palWorldSettings.Save(this.palWorldSettingsPath);
                palWorldSettings.Load(this.palWorldSettingsPath);
                Logger.WriteInfor("PalWorldServer => 服务器RCON密码为空 重新创建");
            }
            //palWorldSettings.Load(this.palWorldSettingsPath);
            Logger.WriteInfor("PalWorldServer => 服务器配置文件加载成功");

            taskExecutor = new TaskExecutor("PalWorldServerPerformanceMonitorOutput");
            taskExecutor.AddTask(() =>
            {
                if (processHarvestingTask != null && processHarvestingTask.Status == TaskStatus.Running)
                {
                    ClearCMD();
                    var rule = new Rule("[yellow]PalWorldServerTool[/]");
                    AnsiConsole.Write(rule);

                    rule = new Rule("[lime]服务器信息[/]");
                    rule.Justification = Justify.Left;
                    AnsiConsole.Write(rule);

                    AnsiConsole.MarkupLine($"服务器进程: [green]{processHarvestingTask.Process.ProcessName}[/] PID: [green]{processHarvestingTask.ProcessId}[/]");
                    AnsiConsole.MarkupLine($"CPU占用: {SpectreColorConvert(processHarvestingTask.CPUUsage, 0, 20, 50)} %");
                    AnsiConsole.MarkupLine($"内存占用: {SpectreColorByteUnits((double)processHarvestingTask.MemoryUsage!, 0, 8589934592, 17179869184)}");
                    if (serverVersion != null)
                        AnsiConsole.MarkupLine($"服务器版本: [yellow]{serverVersion}[/]");
                    //if (serverVersion != null)
                    //{
                    //    AnsiConsole.MarkupLine($"服务器在线人数: {SpectreColorConvert(serverPlayerCount, 0, palWorldSettings.ServerPlayerMaxNum / 3, (int)(palWorldSettings.ServerPlayerMaxNum / 1.5))}/{palWorldSettings.ServerPlayerMaxNum}");
                    //}

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
                    if (backupSavedPath != null)
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
                    AnsiConsole.Markup("建议使用 [lime]Ctrl+C[/] 关闭本程序 否则PalWorldServer进程可能会没有办法被正常关闭");
                    if (authenticated)
                    {
                        Console.SetCursorPosition(110, Console.WindowHeight - 3);
                        AnsiConsole.Markup("[lime]RCON已连接[/]");
                    }
                    else
                    {
                        Console.SetCursorPosition(110, Console.WindowHeight - 3);
                        AnsiConsole.Markup("[red]RCON未连接[/]");
                    }
                    Console.SetCursorPosition(0, Console.WindowHeight - 2);
                    rule = new Rule("by [yellow]CK小捷[/] 项目地址 [blue]https://github.com/44578287/PalWorldServerTool[/]");
                    AnsiConsole.Write(rule);

                    //采集RCON数据
                    //if (serverVersion != null)
                    //{
                    //    var players = SendCommand("ShowPlayers").Result;
                    //    serverPlayerCount = (players?.Split('\n').Length - 1) ?? -1;
                    //}
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
                    await State();
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
                externalAppController.ProcessExited += serverNormalexit;
                externalAppController.ProcessExitedUnexpectedly += serverExitedUnexpectedly;
                await State();
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
        public async Task State()
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
                await Task.Delay(1000);//让服务器有时间启动
                rconConnect = await RconInitialization();
                if (rconConnect)
                {
                    string pattern = @"\[(.*?)\]";
                    serverVersion = await SendCommand("Info");
                    serverVersion = Regex.Match(serverVersion!, pattern).Groups[1].Value;
                    Logger.WriteInfor($"PalWorldServer => 服务器版本: {serverVersion}");
                }
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
            if (!authenticated)
                await externalAppController.RequestStopApp(null, 500);
            else
            {
                if (rebootServerTimeisRun)
                {
                    await SendCommand($"Shutdown {configDataModel.CloseWaitingTime} The_server_will_reboot_in_{configDataModel.CloseWaitingTime}_seconds");
                    ClearCMD(2);
                    AnsiConsole.MarkupLine($"[green]服务器将在{configDataModel.CloseWaitingTime}秒后重启[/]");
                }
                else
                {
                    await SendCommand($"Shutdown {configDataModel.CloseWaitingTime} The_server_will_shut_down_in_{configDataModel.CloseWaitingTime}_seconds");
                    ClearCMD(2);
                    AnsiConsole.MarkupLine($"[green]服务器将在{configDataModel.CloseWaitingTime}秒后关闭[/]");
                }
                //await Task.Delay(10000);
                for (int i = configDataModel.CloseWaitingTime; i > 0; i--)
                {
                    if (rebootServerTimeisRun)
                    {
                        await SendCommand($"Broadcast Reboot_after_{i}S");
                        ClearCMD(2);
                        AnsiConsole.MarkupLine($"[green]服务器将在 {i} 秒后重启[/]");
                    }
                    else
                    {
                        await SendCommand($"Broadcast Shut_down_after_{i}S");
                        ClearCMD(2);
                        AnsiConsole.MarkupLine($"[green]服务器将在 {i} 秒后关闭[/]");
                    }
                    await Task.Delay(1000);
                }
            }
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
        private async void serverExitedUnexpectedly(object? sender, EventArgs e)
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
                await Task.Delay(1000);
                await State();
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
                await State();
            }
        }
        private async void serverNormalexit(object? sender, EventArgs e)
        {
            ServerState = false;
            rebootServerTime?.Stop();
            backupSavedTime?.Stop();
            taskExecutor?.Pause();
            processHarvestingTask?.Pasue();
            processHarvestingTask?.Destroy();
            Logger.WriteInfor("PalWorldServer => 服务器已正常关闭");

            ClearCMD(0);
            AnsiConsole.MarkupLine("[green3]服务器正常关闭[/]");
            if (configDataModel.AutoServerLive)
            {
                await Task.Delay(1000);
                await State();
                return;
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
        private string? BackupSaved()
        {
            try
            {
                Logger.WriteInfor("备份存档");
                if (!Path.Exists(configDataModel.BackupSavedPath))
                    Directory.CreateDirectory(configDataModel.BackupSavedPath);
                if (!authenticated)
                {
                    SendCommand("Save").Wait();
                }
                // 获取当前系统时间
                DateTime currentTime = DateTime.Now;
                string path = Path.Combine(configDataModel.BackupSavedPath, currentTime.ToString("yyyy-MM-dd-HH-mm-ss") + ".zip");
                ZipFile.CreateFromDirectory(Path.Combine(this.pathDir, "Pal", "Saved"), path);
                backupSavedPath = path;
                Logger.WriteInfor($"备份存档完成 位置: {path}");
                SendCommand($"Broadcast Backup_archive_complete").Wait();
                return path;
            }
            catch (Exception ex)
            {
                Logger.WriteError($"备份存档失败 因为:{ex.Message}");
                return null;
            }
        }


        private readonly SemaphoreSlim _semaphoreSendCommand = new SemaphoreSlim(1, 1);
        /// <summary>
        /// 发送RCON命令
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public async Task<string?> SendCommand(string command, bool isMultiPacketResponse = false)
        {
            await _semaphoreSendCommand.WaitAsync();
            Logger.WriteDebug($"PalWorldServer => RCON发送命令: {command}");
            if (rconClient == null || !authenticated)
                return null;
            try
            {
                await rconClient.ConnectAsync();
                authenticated = await rconClient.AuthenticateAsync(palWorldSettings!.AdminPassword.Replace("\"", ""));
                var data = await rconClient.ExecuteCommandAsync(command, isMultiPacketResponse);
                rconClient.Disconnect();
                return data;
            }
            catch (Exception ex)
            {
                Logger.WriteError($"PalWorldServer => RCON发送命令失败 因为:{ex.Message}");
                return null;
            }
            finally
            {
                _semaphoreSendCommand.Release();
            }
        }

        /// <summary>
        /// 初始化Rcon连接
        /// </summary>
        /// <returns></returns>
        private async Task<bool> RconInitialization()
        {
            //初始化Rcon连接 且用作于判断服务器成功与否 以及获取服务器版本 连接失败会尝试3次
            Logger.WriteInfor($"PalWorldServer => 等待连接RCON: 127.0.0.1:{palWorldSettings!.RCONPort}");
            for (int i = 0; i <= 5; i++)
            {
                try
                {
                    rconClient?.Disconnect();
                    rconClient = RconClient.Create("127.0.0.1", palWorldSettings.RCONPort);
                    await rconClient.ConnectAsync();
                    authenticated = await rconClient.AuthenticateAsync(palWorldSettings.AdminPassword.Replace("\"", ""));
                    Logger.WriteInfor($"PalWorldServer => RCON连接成功");
                    if (!authenticated)
                    {
                        rconClient.Disconnect();
                        Logger.WriteError($"PalWorldServer => RCON认证失败 使用无RCON模式");
                        return false;
                    }
                    else
                        Logger.WriteInfor($"PalWorldServer => RCON认证成功");

                    break;
                }
                catch (Exception ex)
                {
                    Logger.WriteError($"PalWorldServer => RCON连接失败 因为:{ex.Message}");
                }
                if (i < 5)
                {
                    Logger.WriteInfor($"PalWorldServer => RCON尝试重新连接第{i + 1}次");
                    await Task.Delay(3000);
                }
                else
                {
                    rconClient?.Disconnect();
                    Logger.WriteError($"PalWorldServer => RCON连接失败 次数过多使用无RCON模式");
                    return false;
                }
            }
            return true;
        }
    }
}
