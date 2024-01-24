using System.ComponentModel;
using LoongEgg.LoongLogger;
using Spectre.Console;
using Spectre.Console.Cli;

namespace PalWorldServerTool.Models
{
    /// <summary>
    /// 命令
    /// </summary>
    public class Cli
    {
        /// <summary>
        /// 除错选项
        /// </summary>
        public class DebugSettings : CommandSettings
        {
            [CommandOption("-d|--debug <BOOLEAN>")]
            public bool Debug { get; set; }
        }
        public class DebugCommand : Command<DebugSettings>
        {
            public override int Execute(CommandContext context, DebugSettings settings)
            {
                if (settings.Debug)
                {
                    Console.WriteLine("调试模式已启用。");
                    Logger.Enable(LoggerType.Console, LoggerLevel.Debug);//注册Log日志函数
                    // 在这里添加开启调试模式的代码
                }
                else
                {
                    Console.WriteLine("调试模式已禁用。");
                    // 在这里添加关闭调试模式的代码
                }
                return 0;
            }
        }


    }
}
