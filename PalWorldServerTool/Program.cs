using System.Diagnostics;
using System.Text;
using LoongEgg.LoongLogger;
using PalWorldServerTool.Models;
using Spectre.Console;
using Spectre.Console.Cli;
using static PalWorldServerTool.Models.Cli;

Logger.Enable(LoggerType.Console, LoggerLevel.Debug);//注册Log日志函数
//Console.OutputEncoding = Encoding.UTF8;

// 启动器获取当前目录
string thisDirPath = AppDomain.CurrentDomain.BaseDirectory;
string steamCMDDirPath = Path.Combine(thisDirPath, "SteamCMD");
string palWorldServerDirPath = Path.Combine(steamCMDDirPath, "steamapps", "common", "PalServer");
string palWorldServerSavedDirPath = Path.Combine(palWorldServerDirPath, "Saved");
string windowsPalWorldServerSavedConfigDirPath = Path.Combine(palWorldServerSavedDirPath, "Config", "WindowsServer");

string palWorldServerName = "PalServer.exe";
string palWorldWorldConfigName = "PalWorldSettings.ini";

PalWorldServerTool.Models.SteamCMD steamCMD = new();

//await steamCMD.Initialization();
PalWorldServerTool.Models.PalWorldServer palWorldServer = new(palWorldServerDirPath, steamCMD);
await palWorldServer.Initialization();
//while (palWorldServer.ServerState)
//{ 
//    await Task.Delay(1000);
//}
Thread.Sleep(50000);
await palWorldServer.Shutdown();
Thread.Sleep(1000);






