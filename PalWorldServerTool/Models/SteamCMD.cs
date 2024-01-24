using LoongEgg.LoongLogger;
using SteamCMD.ConPTY;
using SteamCMD.ConPTY.Interop.Definitions;
using static 小工具集.Windows.Network;

namespace PalWorldServerTool.Models
{
    /// <summary>
    /// SteamCMD
    /// </summary>
    public class SteamCMD
    {
        private static string urlSteamCMDWindows = "https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip";

        private SteamData steamData;

        private string pathDir;
        private string cmdPath;

        private 小工具集.Windows.File.ExternalAppController externalAppController = new();
        private ProcessInfo? processInfo;
        private bool steamCmdReady = false;
        private SteamCMDConPTY steamCMDConPTY = new();

        public bool SteamCmdRun { get; private set; } = false;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="steamData">Steam信息</param>
        /// <param name="pathDir">SteamCmd目录</param>
        public SteamCMD(SteamData? steamData = null, string? pathDir = null)
        {
            this.steamData = steamData ?? new();
            this.pathDir = pathDir ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SteamCMD");
            this.cmdPath = Path.Combine(this.pathDir, "steamcmd.exe");
        }
        /// <summary>
        ///  初始化SteamCMD
        /// </summary>
        /// <returns>是否初始化成功</returns>
        public async Task<bool> Initialization()
        {
            try
            {
                if (!Directory.Exists(this.pathDir))
                    Directory.CreateDirectory(this.pathDir);
                if (!File.Exists(this.cmdPath))
                {
                    Logger.WriteInfor($"准备安装SteamCMD 目录:{this.pathDir}");
                    Logger.WriteInfor("开始下载SteamCmd");
                    string zipPath = Path.Combine(this.pathDir, "SteamCmd.zip");
                    Download download = new Download();
                    await download.DownloadFileLine(new()
                    {
                        new()
                        {
                            Url = urlSteamCMDWindows,
                            Name = zipPath
                        }
                    });
                    小工具集.Windows.File.Unzip(zipPath, this.pathDir);
                }

                steamCMDConPTY.OutputDataReceived += cmdOutLogInfo;
                steamCMDConPTY.OutputDataReceived += cmdOutSteamCmdOk;
                steamCMDConPTY.Exited += (e, s) => { Logger.WriteInfor("SteamCmd => 已退出!"); };
                steamCMDConPTY.WorkingDirectory = this.pathDir;
                steamCMDConPTY.FilterControlSequences = true;
                steamCMDConPTY.FileName = "steamcmd.exe";
                processInfo = steamCMDConPTY.Start();
                Logger.WriteInfor("SteamCmd => 等待初始化完成");
                await AwaitRunning();
                Logger.WriteInfor("SteamCmd => 初始化完成!");
                Logger.WriteInfor($"SteamCmd => 登入 {steamData.Account}");
                await WriteLineAsync($"login {steamData.Account}");
                await AwaitRunning();
                SteamCmdRun = true;
            }
            catch (Exception ex)
            {
                Logger.WriteError($"SteamCMD => 初始化失败 因为:{ex.Message}");
                return false;
            }
            return true;
        }
        /// <summary>
        /// 停止SteamCmd
        /// </summary>
        public void Stop()
        {
            SteamCmdRun = false;
            steamCMDConPTY.Dispose();
        }
        /// <summary>
        /// 输入命令
        /// </summary>
        /// <param name="data"></param>
        public async Task WriteLineAsync(string data)
        {
            while (!steamCmdReady)
            {
                await Task.Delay(20);
            }
            steamCmdReady = false;
            await steamCMDConPTY.WriteLineAsync(data);
        }
        /// <summary>
        /// 等待运行
        /// </summary>
        /// <returns></returns>
        public async Task AwaitRunning()
        {
            while (!steamCmdReady)
            {
                await Task.Delay(100);
            }
        }

        private static void cmdOutLogInfo(object? sender, string? data)
        {
            if (!string.IsNullOrWhiteSpace(data))
                Logger.WriteInfor($"SteamCmd => {data}");
        }
        private static void cmdOutLogWarn(object? sender, string? data)
        {
            Logger.WriteWarn($"SteamCmd => {data}");
        }
        private void cmdOutSteamCmdOk(object? sender, string? data)
        {
            if (data != null)
                if (data.Contains("Steam>"))
                {
                    steamCmdReady = true;
                }
            //steamCmdReady = false;
        }
    }
    /// <summary>
    /// Steam数据
    /// </summary>
    public class SteamData
    {
        /// <summary>
        /// 账号
        /// </summary>
        public string Account { get; set; } = "anonymous";
        /// <summary>
        /// 密码
        /// </summary>
        public string? Pwd { get; set; }
    }
}

//Steam>
