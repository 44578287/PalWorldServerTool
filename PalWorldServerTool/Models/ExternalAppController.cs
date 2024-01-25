using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PalWorldServerTool.Models
{
    /// <summary>
    /// 外部应用程序控制器，用于启动和控制外部程序。
    /// </summary>
    public class ExternalAppController : IDisposable
    {

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);

        // 委托类型
        private delegate bool HandlerRoutine(CtrlTypes CtrlType);

        /// <summary>
        /// 应用进程
        /// </summary>
        public Process process { get; private set; }

        /// <summary>
        /// 当外部程序输出时触发的事件。
        /// </summary>
        public event EventHandler<string?>? OutputReceived;

        /// <summary>
        /// 当外部程序发生错误输出时触发的事件。
        /// </summary>
        public event EventHandler<string?>? ErrorReceived;

        /// <summary>
        /// 当外部程序启动时触发的事件。
        /// </summary>
        public event EventHandler? ProcessStarted;

        /// <summary>
        /// 当外部程序正常退出时触发的事件。
        /// </summary>
        public event EventHandler? ProcessExited;

        /// <summary>
        /// 当外部程序意外退出时触发的事件。
        /// </summary>
        public event EventHandler? ProcessExitedUnexpectedly;

        /// <summary>
        /// 构造函数，初始化一个新的外部应用程序控制器实例。
        /// </summary>
        public ExternalAppController()
        {
            process = new Process();
            process.EnableRaisingEvents = true;
            // 设置控制台关闭事件处理器
            SetConsoleCtrlHandler(new HandlerRoutine(ConsoleCtrlCheck), true);
        }

        private bool ConsoleCtrlCheck(CtrlTypes ctrlType)
        {
            // 检查是否是关闭事件
            if (ctrlType == CtrlTypes.CTRL_CLOSE_EVENT)
            {
                Dispose(); // 清理资源
            }
            return true;
        }

        /// <summary>
        /// 清理资源，确保外部进程被终止。
        /// </summary>
        public void Dispose()
        {
            EnsureProcessTerminated();
            process.Dispose();
        }

        private void EnsureProcessTerminated()
        {
            if (process != null && !process.HasExited)
            {
                process.Kill();
            }
        }


        /// <summary>
        /// 异步启动外部应用程序。
        /// </summary>
        /// <param name="appPath">应用程序路径。</param>
        /// <param name="arguments">启动应用程序的参数。</param>
        /// <returns>任务。</returns>
        public async Task StartAppAsync(string appPath, string arguments)
        {
            process.StartInfo.FileName = appPath;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardInput = true;
            //process.StartInfo.StandardInputEncoding = Encoding.UTF8;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            process.OutputDataReceived += (sender, args) => OutputReceived?.Invoke(this, args.Data);
            process.ErrorDataReceived += (sender, args) => ErrorReceived?.Invoke(this, args.Data);
            process.Exited += (sender, args) =>
            {
                if (process.HasExited)
                    if (process.ExitCode != 0)
                    {
                        ProcessExitedUnexpectedly?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        ProcessExited?.Invoke(this, EventArgs.Empty);
                    }
            };

            process.Start();
            ProcessStarted?.Invoke(this, EventArgs.Empty);

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();
        }

        /// <summary>
        /// 向外部应用程序发送命令。
        /// </summary>
        /// <param name="command">要发送的命令文本。</param>
        public void Send(string command)
        {
            if (process != null && !process.HasExited)
            {
                process.StandardInput.WriteLine(command);
            }
        }

        /// <summary>
        /// 请求停止外部应用程序。
        /// </summary>
        /// <param name="shutdownCommand">发送给应用程序的优雅关闭命令。</param>
        /// <param name="timeoutMilliseconds">等待优雅关闭的超时时间（毫秒）。</param>
        /// <returns>任务。</returns>
        public async Task RequestStopApp(string? shutdownCommand = null, int timeoutMilliseconds = 5000)
        {
            if (process != null && !process.HasExited)
            {
                if (!string.IsNullOrEmpty(shutdownCommand))
                {
                    Send(shutdownCommand);
                }

                bool exited = await Task.Run(() => process.WaitForExit(timeoutMilliseconds));

                if (!exited)
                {
                    process.Kill();
                }
            }
        }
    }
    // 控制台关闭事件类型
    enum CtrlTypes
    {
        CTRL_C_EVENT = 0,
        CTRL_BREAK_EVENT,
        CTRL_CLOSE_EVENT,
        CTRL_LOGOFF_EVENT = 5,
        CTRL_SHUTDOWN_EVENT
    }
}
