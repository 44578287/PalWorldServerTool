using System.Diagnostics;

namespace PalWorldServerTool.Models
{
    public class HardwareMonitoring
    {
        /// <summary>
        /// 进程 采集任务
        /// </summary>
        public class ProcessHarvestingTask
        {
            /// <summary>
            /// 任务执行器
            /// </summary>
            private TaskExecutor TaskExecutor;
            /// <summary>
            /// 进程ID
            /// </summary>
            public int ProcessId { get; private set; }
            /// <summary>
            /// 进程名称
            /// </summary>
            public string ProcessName { get; private set; }
            /// <summary>
            /// 进程
            /// </summary>
            public Process Process { get; private set; }
            /// <summary>
            /// 获取数据任务
            /// </summary>
            public Action<double, long>? GetDataAction;

            /// <summary>
            /// 采集状态
            /// </summary>
            public TaskStatus Status
            {
                get => TaskExecutor.GetStatus();
            }

            private DateTime _lastCheckTime;
            private TimeSpan _lastTotalProcessorTime;
            private long _lastMemoryUsage;

            /// <summary>
            /// CPU使用率
            /// </summary>
            public double CPUUsage { get; private set; }
            /// <summary>
            /// 内存使用率
            /// </summary>
            public long MemoryUsage { get; private set; }


            /// <summary>
            /// 构造化函数
            /// </summary>
            private void Constructor()
            {
                //初始化任务
                TaskExecutor.AddInitialTask(() =>
                {
                    _lastCheckTime = DateTime.UtcNow;
                    _lastTotalProcessorTime = Process.TotalProcessorTime;
                    _lastMemoryUsage = Process.WorkingSet64;
                });
                //循环任务
                TaskExecutor.AddTask(() =>
                {
                    RefreshProcessInfo(out var currentTotalProcessorTime);
                    var currentTime = DateTime.UtcNow;

                    CPUUsage = (currentTotalProcessorTime - _lastTotalProcessorTime).TotalMilliseconds / (currentTime - _lastCheckTime).TotalMilliseconds * 100;
                    CPUUsage /= System.Environment.ProcessorCount;
                    //CPUUsage = Math.Round(CPUUsage, 1); // 保留一位小数

                    using (var process = Process.GetProcessById(ProcessId))
                    {
                        MemoryUsage = process.WorkingSet64; // 重新获取内存使用信息
                        //Console.WriteLine($"CPU Usage: {cpuUsage}%\tMemory Usage: {memoryUsage} bytes");
                        //填充数据回调
                        GetDataAction?.Invoke(CPUUsage, MemoryUsage);
                    }

                    _lastTotalProcessorTime = currentTotalProcessorTime;
                    _lastCheckTime = currentTime;
                });
                //初始化任务
                TaskExecutor.Initialize();
            }

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="ProcessId">进程ID</param>
            /// <param name="interval">采样间隔</param>
            public ProcessHarvestingTask(int ProcessId, int interval = 1000)
            {
                Process = Process.GetProcessById(ProcessId);
                this.ProcessId = ProcessId;
                this.ProcessName = Process.ProcessName;
                TaskExecutor = new($"进程采集任务 ID:{ProcessId} Name:\"{ProcessName}\" ", interval);
                Constructor();
            }
            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="ProcessName">进程名</param>
            /// <param name="interval">采样间隔</param>
            public ProcessHarvestingTask(string ProcessName, int interval = 1000)
            {
                Process = Process.GetProcessesByName(ProcessName).OrderByDescending(p => p.MainWindowHandle != IntPtr.Zero).First();
                this.ProcessName = ProcessName;
                this.ProcessId = Process.Id;
                TaskExecutor = new($"进程采集任务 ID:{ProcessId} Name:\"{ProcessName}\" ", interval);
                Constructor();
            }

            /// <summary>
            /// 开始采集
            /// </summary>
            public void Run()
            {
                TaskExecutor.Start();
            }
            /// <summary>
            /// 销毁采集任务
            /// </summary>
            public void Destroy()
            {
                TaskExecutor.Destroy();
            }
            /// <summary>
            /// 暂停采集
            /// </summary>
            public void Pasue()
            {
                TaskExecutor.Pause();
            }
            /// <summary>
            /// 恢复采集
            /// </summary>
            public void Resume()
            {
                TaskExecutor.Resume();
            }

            /// <summary>
            /// 刷新进程的CPU使用情况信息。
            /// </summary>
            /// <param name="lastTotalProcessorTime">上一次刷新时的总处理器时间。</param>
            private void RefreshProcessInfo(out TimeSpan lastTotalProcessorTime)
            {
                using (var process = Process.GetProcessById(ProcessId))
                {
                    lastTotalProcessorTime = process.TotalProcessorTime;
                }
            }
        }
    }
}
