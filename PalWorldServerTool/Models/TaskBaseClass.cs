using LoongEgg.LoongLogger;

namespace PalWorldServerTool.Models
{
    /// <summary>
    /// 任务状态
    /// </summary>
    public enum TaskStatus
    {
        /// <summary>
        /// 已初始化
        /// </summary>
        Initialized,
        /// <summary>
        /// 运行
        /// </summary>
        Running,
        /// <summary>
        /// 暂停
        /// </summary>
        Paused,
        /// <summary>
        /// 正在销毁
        /// </summary>
        Destroying,
        /// <summary>
        /// 已销毁
        /// </summary>
        Destroyed
    }

    /// <summary>
    /// ITask接口定义了一些必要的任务管理方法.
    /// </summary>
    public interface ITask
    {
        /// <summary>
        /// 构造器
        /// </summary>
        void Initialize();
        /// <summary>
        /// 运行任务
        /// </summary>
        void Start();
        /// <summary>
        /// 暂停任务
        /// </summary>
        void Pause();
        /// <summary>
        /// 恢复暂停任务
        /// </summary>
        void Resume();
        /// <summary>
        /// 销毁任务
        /// </summary>
        /// <param name="timeout">超时时间</param>
        void Destroy(int timeout);
        /// <summary>
        /// 获取任务状态
        /// </summary>
        /// <returns>当前任务状态</returns>
        TaskStatus GetStatus();
        /// <summary>
        /// 设置间隔时间
        /// </summary>
        /// <param name="interval">毫秒</param>
        void SetInterval(int interval);
        /// <summary>
        /// 添加循环任务
        /// </summary>
        /// <param name="task">任务</param>
        void AddTask(Action task);
        /// <summary>
        /// 删除循环任务
        /// </summary>
        /// <param name="task">任务</param>
        void RemoveTask(Action task);
        /// <summary>
        /// 添加销毁任务
        /// </summary>
        /// <param name="finalTask">任务</param>
        void AddFinalTask(Action finalTask);
        /// <summary>
        /// 删除销毁任务
        /// </summary>
        /// <param name="finalTask">任务</param>
        void RemoveFinalTask(Action finalTask);
        /// <summary>
        /// 添加初始化任务
        /// </summary>
        /// <param name="initialTask">任务</param>
        void AddInitialTask(Action initialTask);
        /// <summary>
        /// 删除初始化任务
        /// </summary>
        /// <param name="initialTask">任务</param>
        void RemoveInitialTask(Action initialTask);
    }

    /// <summary>
    /// TaskExecutor类实现了ITask接口并管理任务的生命周期.
    /// </summary>
    public class TaskExecutor : ITask
    {
        /// <summary>
        /// 定时器
        /// </summary>
        private Timer? timer;
        /// <summary>
        /// 运行状态
        /// </summary>
        private bool isRunning = false;
        /// <summary>
        /// 间隔时间
        /// </summary>
        private int interval;
        /// <summary>
        /// 初始化任务内容
        /// </summary>
        private Action? initialTask;
        /// <summary>
        /// 循环任务内容
        /// </summary>
        private Action? task;
        /// <summary>
        /// 销毁任务内容
        /// </summary>
        private Action? finalTask;
        /// <summary>
        /// 错误触发任务内容
        /// </summary>
        private Action<object?, Exception>? errorTask;
        /// <summary>
        /// 当前任务状态
        /// </summary>
        private TaskStatus status;
        /// <summary>
        /// 任务名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 任务执行器的构造函数.
        /// </summary>
        public TaskExecutor()
        {
            interval = 1000;
            initialTask = () => { };
            task = () => { };
            finalTask = () => { };
            status = TaskStatus.Initialized;
            Name = "TaskExecutor" + Task.CurrentId;
            Logger.WriteDebug($"{Name}=>已创建");
        }

        /// <summary>
        /// 任务执行器的构造函数.
        /// </summary>
        /// <param name="Name">任务名称</param>
        /// <param name="interval">运行间隔时间</param>
        /// <param name="initialTask">初始化任务</param>
        /// <param name="task">循环任务</param>
        /// <param name="finalTask">销毁前任务</param>
        public TaskExecutor(string Name, int interval, Action? initialTask, Action? task, Action? finalTask)
        {
            this.interval = interval;
            this.initialTask = initialTask;
            this.task = task;
            this.finalTask = finalTask;
            status = TaskStatus.Initialized;
            this.Name = Name;
            Logger.WriteDebug($"{Name}=>已创建");
        }

        /// <summary>
        /// 任务执行器的构造函数.
        /// </summary>
        /// <param name="Name">任务名称</param>
        /// <param name="interval">运行间隔时间</param>
        public TaskExecutor(string Name, int interval = 1000)
        {
            this.interval = interval;
            initialTask = () => { };
            task = () => { };
            finalTask = () => { };
            status = TaskStatus.Initialized;
            this.Name = Name;
            Logger.WriteDebug($"{Name}=>已创建");
        }

        /// <summary>
        /// 初始化任务.
        /// </summary>
        public void Initialize()
        {
            try
            {
                timer = new Timer(ExecuteTask!, null, Timeout.Infinite, interval);
                Logger.WriteDebug($"{Name}=>已初始化");
            }
            catch (Exception ex)
            {
                errorTask?.Invoke(this, ex);
                Logger.WriteError($"{Name}=>执行初始化任务时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 开始执行任务.
        /// </summary>
        public void Start()
        {
            // Execute the initial tasks only when the status is "Initialized".
            if (status == TaskStatus.Initialized)
            {
                try
                {
                    initialTask?.Invoke();
                    Logger.WriteDebug($"{Name}=>已执行初始化任务");
                }
                catch (Exception ex)
                {
                    errorTask?.Invoke(this, ex);
                    Logger.WriteError($"{Name}=>执行初始任务时出错: {ex.Message}");
                }
            }

            timer?.Change(0, interval);
            isRunning = true;
            status = TaskStatus.Running;
            Logger.WriteDebug($"{Name}=>已开始执行任务");
        }
        /// <summary>
        /// 异步开始执行任务.
        /// </summary>
        public async Task StartAsync()
        {
            await Task.Run(() => Start());
        }

        /// <summary>
        /// 暂停任务.
        /// </summary>
        public void Pause()
        {
            timer?.Change(Timeout.Infinite, interval);
            isRunning = false;
            status = TaskStatus.Paused;
            Logger.WriteDebug($"{Name}=>已暂停任务");
        }

        /// <summary>
        /// 恢复任务.
        /// </summary>
        public void Resume()
        {
            timer?.Change(0, interval);
            isRunning = true;
            status = TaskStatus.Running;
            Logger.WriteDebug($"{Name}=>已恢复任务");
        }
        /// <summary>
        /// 异步恢复任务.
        /// </summary>
        /// <returns></returns>
        public async Task ResumeAsync()
        {
            await Task.Run(() => Resume());
        }

        /// <summary>
        /// 销毁任务.
        /// </summary>
        public void Destroy(int timeout = 2000)
        {
            if (!isRunning)
            {
                finalTask?.Invoke();
                timer?.Dispose();
                status = TaskStatus.Destroyed;
                Logger.WriteDebug($"{Name}=>已销毁任务");
            }
            else
            {
                timer?.Change(timeout, interval);
                status = TaskStatus.Destroying;
                Logger.WriteDebug($"{Name}=>正在销毁任务");
            }
        }
        /// <summary>
        /// 异步销毁任务.
        /// </summary>
        /// <param name="timeout">超时时间</param>
        public async Task DestroyAsync(int timeout = 2000)
        {
            await Task.Run(() => Destroy(timeout));
        }

        /// <summary>
        /// 获取任务状态.
        /// </summary>
        public TaskStatus GetStatus()
        {
            return status;
        }

        /// <summary>
        /// 设置任务执行间隔.
        /// </summary>
        public void SetInterval(int interval)
        {
            this.interval = interval;
            Logger.WriteDebug($"{Name}=>已设置任务执行间隔为{interval}毫秒");
        }

        /// <summary>
        /// 添加初始化任务.
        /// </summary>
        public void AddInitialTask(Action initialTask)
        {
            this.initialTask += initialTask;
        }

        /// <summary>
        /// 移除初始化任务.
        /// </summary>
        public void RemoveInitialTask(Action initialTask)
        {
            this.initialTask -= initialTask;
        }

        /// <summary>
        /// 添加任务.
        /// </summary>
        public void AddTask(Action task)
        {
            this.task = this.task + task;
        }

        /// <summary>
        /// 移除任务.
        /// </summary>
        public void RemoveTask(Action task)
        {
            this.task = this.task - task;
        }

        /// <summary>
        /// 添加最后执行的任务.
        /// </summary>
        public void AddFinalTask(Action finalTask)
        {
            this.finalTask = this.finalTask + finalTask;
        }

        /// <summary>
        /// 移除最后执行的任务.
        /// </summary>
        public void RemoveFinalTask(Action finalTask)
        {
            this.finalTask = this.finalTask - finalTask;
        }

        /// <summary>
        /// 执行任务.
        /// </summary>
        private void ExecuteTask(object state)
        {
            if (!isRunning)
            {

                return;
            }

            try
            {
                task?.Invoke();
            }
            catch (Exception ex)
            {
                errorTask?.Invoke(this, ex);
                Logger.WriteError($"{Name}=>执行任务时出错: {ex.Message}");
                return;
            }

            if (status == TaskStatus.Destroying)
            {
                try
                {
                    finalTask?.Invoke();
                    timer?.Dispose();
                    status = TaskStatus.Destroyed;
                    Logger.WriteDebug($"{Name}=>已销毁任务");
                }
                catch (Exception ex)
                {
                    errorTask?.Invoke(this, ex);
                    Logger.WriteError($"{Name}=>执行销毁前任务时出错: {ex.Message}");
                }
            }
        }


    }
}
