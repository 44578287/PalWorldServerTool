using LoongEgg.LoongLogger;
using static PalWorldServerTool.Models.HardwareMonitoring;

namespace PalWorldServerTool.Models
{
    public class PalWorldServer
    {
        private 小工具集.Windows.File.ExternalAppController externalAppController = new();
        private Task? serverTaks;

        /// <summary>
        /// 服务器性能监控
        /// </summary>
        private ProcessHarvestingTask? processHarvestingTask;
        private TaskExecutor? taskExecutor;

        private string pathDir;
        private string serverPath;

        private SteamCMD steamCMD;

        public bool ServerState { get; private set; } = false;

        public PalWorldServer(string pathDir, SteamCMD steamCMD)
        {
            this.pathDir = pathDir;
            this.serverPath = Path.Combine(this.pathDir, "Pal", "Binaries", "Win64", "PalServer-Win64-Test-Cmd.exe");
            this.steamCMD = steamCMD;
            taskExecutor = new TaskExecutor("PalWorldServerPerformanceMonitorOutput");
            taskExecutor.AddTask(() =>
            {
                if (processHarvestingTask != null && processHarvestingTask.Status == TaskStatus.Running)
                {
                    Console.WriteLine($"CPU占用: {processHarvestingTask.CPUUsage}% 内存占用: {小工具集.Windows.Text.ConvertByteUnits((double)processHarvestingTask.MemoryUsage!)}");
                }
                    
            });
            taskExecutor.Initialize();
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

                //serverTaks = externalAppController.StartAppAsync(@"C:\Program Files\Tailscale\tailscaled.exe", "-h");
                //externalAppController.Send($"{serverPath} -useperfthreads -NoAsyncLoadingThread -UseMultithreadForDS");
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
            taskExecutor?.Pause();
            processHarvestingTask?.Pasue();
            processHarvestingTask?.Destroy();
            await externalAppController.RequestStopApp();
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
            taskExecutor?.Pause();
            processHarvestingTask?.Pasue();
            processHarvestingTask?.Destroy();
            Logger.WriteWarn("PalWorldServer => 服务器意外关闭");
        }
        private void serverStart(object? sender, EventArgs e)
        {
            ServerState = true;
            Logger.WriteInfor("PalWorldServer => 启动服务器");
        }
    }
}
