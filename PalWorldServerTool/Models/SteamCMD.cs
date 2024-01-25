using LoongEgg.LoongLogger;
using Spectre.Console;
using SteamCMD.ConPTY;
using SteamCMD.ConPTY.Interop.Definitions;

namespace PalWorldServerTool.Models
{
    /// <summary>
    /// SteamCMD
    /// </summary>
    public class SteamCMD
    {
        //private static string urlSteamCMDWindows = "https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip";

        private SteamData steamData;

        private string pathDir;
        private string cmdPath;

        private ExternalAppController externalAppController = new();
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
                var rule = new Rule("SteamCMD");
                AnsiConsole.Write(rule);

                steamCMDConPTY.OutputDataReceived += cmdOutLogInfo;
                steamCMDConPTY.OutputDataReceived += cmdOutSteamCmdOk;
                steamCMDConPTY.Exited += (e, s) => 
                {
                    Logger.WriteInfor("SteamCmd => 已退出!");
                    AnsiConsole.MarkupLine("[yellow2]SteamCMD 已退出[/]");
                };
                steamCMDConPTY.WorkingDirectory = this.pathDir;
                steamCMDConPTY.FilterControlSequences = true;
                steamCMDConPTY.FileName = "steamcmd.exe";
                processInfo = steamCMDConPTY.Start();
                await AnsiConsole.Status()
                    .StartAsync("等待SteamCmd初始化", async ctx => 
                    {
                        Logger.WriteInfor("SteamCmd => 等待初始化完成");
                        await AwaitRunning();
                        AnsiConsole.MarkupLine("[chartreuse2]初始化完成[/]");
                        Logger.WriteInfor("SteamCmd => 初始化完成!");
                        AnsiConsole.MarkupLine($"[chartreuse2]登入 {steamData.Account}[/]");
                        Logger.WriteInfor($"SteamCmd => 登入 {steamData.Account}");
                        await WriteLineAsync($"login {steamData.Account}");
                        await AwaitRunning();
                        AnsiConsole.MarkupLine("[chartreuse2]SteamCMD就绪[/]");
                    });
               
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

        private void cmdOutLogInfo(object? sender, string? data)
        {
            if (!string.IsNullOrWhiteSpace(data))
            {
                Logger.WriteInfor($"SteamCmd => {data}");
                AnsiConsole.MarkupInterpolated($"[steelblue1]{data}[/]");
            } 
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
