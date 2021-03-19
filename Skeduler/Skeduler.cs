using System;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OPOS
{
    /// <summary>
    /// Simple .NET CLR Task Scheduler that inherits and redefines System.Threading.Tasks.TaskScheduler.
    /// Implemented on.NET Standard 2.0.
    /// </summary>
    public class Skeduler : System.Threading.Tasks.TaskScheduler
    {
        // Singleton.
        private static Skeduler _scheduler = null; 

        private SchedulingType _schedulingType;
        private int _totalNumberOfThreads; 
        private int _schedulingDurationInSeconds;
        private readonly CancellationTokenSource _deadlineToken = new CancellationTokenSource();

        private bool _schedulingActive;
        private static int _numberOfCurrentlyActive = 0;

        // SortedList<T> that allows duplicate keys and sorts tasks by priority and by arrival time (FIFO).
        private readonly ConcurrentSortedList<TaskMetadata> _pendingTasks = new ConcurrentSortedList<TaskMetadata>();
        private readonly List<TaskMetadata> _runningTasks = new List<TaskMetadata>();
        // Hashes of resources taken by currently active Tasks.
        private readonly HashSet<object> _takenResources = new HashSet<object>();

        private static readonly object _lock = new object();
        private static readonly object _countingLock = new object();
        private static readonly object _singletonLock = new object();

        // Private constructor: Singleton.
        private Skeduler(SchedulingType schedulingType, int totalNumberOfThreads, int schedulingDurationInSeconds)
        {
            if (totalNumberOfThreads < 1)
                throw new ArgumentOutOfRangeException(nameof(totalNumberOfThreads), "Must be at least 1.");

            if (schedulingDurationInSeconds < 10)
                throw new ArgumentOutOfRangeException(nameof(schedulingDurationInSeconds), "Must be at least 10.");

            _schedulingActive = false;
            _schedulingType = schedulingType;
            _totalNumberOfThreads = totalNumberOfThreads;
            _schedulingDurationInSeconds = schedulingDurationInSeconds;
        }

        /// <summary>
        /// Returns and/or creates Singleton instance of Skeduler.
        /// </summary>
        /// <param name="schedulingType">SchedulingType Enum that represents type of scheduling. Either Preemtpive or NonPreemptive.</param>
        /// <param name="totalNumberOfThreads">Number of threads allocated to the scheduler.</param>
        /// <param name="schedulingDurationInSeconds">Scheduler level deadline for all arrived tasks.</param>
        /// <returns></returns>
        public static Skeduler GetSkeduler(SchedulingType schedulingType, int totalNumberOfThreads, int schedulingDurationInSeconds)
        {
            lock (_singletonLock)
            {
                if (_scheduler == null)
                    _scheduler = new Skeduler(schedulingType, totalNumberOfThreads, schedulingDurationInSeconds);

                return _scheduler;
            }
        }

        /// <summary>
        /// Queues task to the list of pending tasks.
        /// </summary>
        /// <param name="task">Task to be queued.</param>
        public void QueueTask(TaskMetadata task)
        {
            // Casting Enum to int because ConcurrentSortedList<> expects int as key.
            _pendingTasks.Add((int) task.GetPriority(), task);
        }

        /// <summary>
        /// Starts the scheduling of tasks that have been queued.
        /// </summary>
        public void Schedule()
        {
            _schedulingActive = true;
            // Starting scheduler-level deadline.
            _deadlineToken.CancelAfter(_schedulingDurationInSeconds * 1000);
            Task.Factory.StartNew(SchedulerDeadline);
            // Helper daemon Task that updates list of currently running tasks.
            Task.Factory.StartNew(GetRunningTasksCount);

            // Launching scheduling on separate Thread so it doesn't stop when Main program ends.
            if (_schedulingType.Equals(SchedulingType.NonPreemptive))
                new Thread(new ThreadStart(ScheduleTasksNoPreemption)).Start();

            else if (_schedulingType.Equals(SchedulingType.Preemptive))
                new Thread(new ThreadStart(ScheduleTasksPreemption)).Start();
        }

        private void SchedulerDeadline()
        {
            // Runs on ThreadPool thread and analayzes did scheduler met it's deadline.
            while (_schedulingActive)
                // Using CancellationTokenSource's property "IsCancellationRequested".
                if (_deadlineToken.IsCancellationRequested)
                {
                    lock (_lock)
                        _schedulingActive = false;

                    Console.WriteLine("\nINFO: Skeduler deadline met. Scheduling disposed.");
                }
        }

        // Runs on ThreadPool thread and schedules queued Tasks without preemption.
        private void ScheduleTasksNoPreemption() 
        {
            while (_schedulingActive)
                // Number of currently active Tasks is updated on another thread. (Thread-Safe Interlocked implemented)
                if (_numberOfCurrentlyActive < _totalNumberOfThreads)
                {
                    TaskMetadata pendingTask = _pendingTasks.GetFirst();

                    if (pendingTask == null)
                        continue;

                    object[] resources = pendingTask?.GetResources()?.ToArray();
                    // Atomic operations.
                    lock (_lock)
                    {
                        // Check will scheduling the task cause deadlock.
                        if (DetectDeadlock(resources))
                        {
                            // Check will scheduling the task cause deadlock, lightweight variation of Banker's algorithm.
                            pendingTask.AddWithLowerPriority(_pendingTasks);
                            continue;
                        }

                        // Starting and storing task started on `this` scheduler; Using user defined CooperationToken, not CancellationToken.
                        Task startedTask = Task.Factory.StartNew(pendingTask.GetAction(), CancellationToken.None, TaskCreationOptions.None, this);
                        // Mapping started task to the action.
                        pendingTask.SetTask(startedTask);
                        // Adding TaskMetadata object to the list of currently running tasks.
                        _runningTasks.Add(pendingTask);
                        // Maintain resources; Assuming that finished tasks release resources.
                        MaintainResources();
                    }
                    // Atomic increment.
                    Interlocked.Increment(ref _numberOfCurrentlyActive);
                }
        }

        // Runs on ThreadPool thread and schedules queued Tasks preemptively.
        private void ScheduleTasksPreemption() 
        {
            while (_schedulingActive)
            {
                // Checks for more prioritized task in list of pending tasks and pauses least priviledged running task.
                CheckForMorePrioritizedTask();
                // Number of currently active Tasks is updated on another thread. (Thread-Safe Interlocked implemented)
                if (_numberOfCurrentlyActive < _totalNumberOfThreads)
                {
                    TaskMetadata pendingTask = _pendingTasks.GetFirst();

                    if (pendingTask == null)
                        continue;

                    object[] resources = pendingTask?.GetResources()?.ToArray();
                    // Atomic operations.
                    lock (_lock)
                    {
                        // Check will scheduling the task cause deadlock, lightweight variation of Banker's algorithm.
                        if (DetectDeadlock(resources))
                        {
                            // If scheduling of the task will cause deadlock, increase priority and put it back to the queue.
                            pendingTask.AddWithLowerPriority(_pendingTasks);
                            continue;
                        }

                        // If scheduling of the task won't cause the deadlock and task was previously paused, resume task.
                        else if (pendingTask.GetCooperativeToken().IsPauseRequested())
                        {
                            pendingTask.GetCooperativeToken().Resume();
                            _runningTasks.Add(pendingTask);
                            // Console.WriteLine("Resumed {0}.", pendingTask.Descriptor);
                        }

                        // If scheduling of the task won't cause the deadlock and task wasn't previously started, start task.
                        else
                        {
                            Task startedTask = Task.Factory.StartNew(pendingTask.GetAction(), CancellationToken.None, TaskCreationOptions.None, this);
                            pendingTask.SetTask(startedTask);
                            // Console.WriteLine("Starting {0}.", pendingTask.Descriptor);
                        }

                        // Variation on PCP, if task takes resources, increase the priority of it.
                        if (resources != null) 
                            pendingTask.SetPriority(Priority.Critical);

                        _runningTasks.Add(pendingTask);
                        // Maintain resources; Assuming that finished and paused tasks release resources.
                        MaintainResources();
                    }
                    // Atomic increment.
                    Interlocked.Increment(ref _numberOfCurrentlyActive);
                }
            }
        }

        // Runs on ThreadPool thread and keeps list of running tasks up to date.
        private void GetRunningTasksCount() 
        {
            while (_schedulingActive)
            {
                // Casting list of running Taskmetadatas to array to make copy that won't be affected by other threads.
                foreach (TaskMetadata task in _runningTasks.ToArray())
                {
                    // Extract current task if running one is not null.
                    Task currentTask = task?.GetTask();

                    // Checking is task properly instantiated.
                    if (currentTask == null || task.GetCooperativeToken() == null)
                        continue;

                    // If current task is completed, paused or canceled.
                    if (currentTask != null && (currentTask.IsCompleted || task.GetCooperativeToken().IsPausedOrCanceled())) 
                    // Atomic operations.
                    {
                        // Lock the list.
                        lock (_countingLock)
                            _runningTasks.Remove(task);
                        // Console.WriteLine("Removing {0}.", task.Descriptor);
                        // Maintain list of taken resourcse. Assuming resources are relased if task is finished, cancelled or paused.
                        MaintainResources(); 
                        Interlocked.Decrement(ref _numberOfCurrentlyActive); // Decrementing number of running tasks.
                    }
                }
            }
        }

        // Variation on Banker's algorithm; Prevents task from being scheduled if it will cause deadlock.
        private bool DetectDeadlock(object[] requestedResources) 
        {
            if (requestedResources != null)
                foreach (object o in requestedResources)
                {
                    if (_takenResources.Contains(o))
                    {
                        // Thread.Sleep(3000);
                        // if (_takenResources.Contains(o))
                        // return false;

                        Console.WriteLine("Deadlock {0}.", o);
                        return true;
                    }
                }

            return false;
        }

        // Keeping HashSet of taken resources up to date.
        private void MaintainResources() 
        {
            // UnlockResources();
            // Atomic operation.
            lock (_lock)
            {
                // Clear HashSet of currently taken resources.
                _takenResources.Clear();
                // Assume that cancelled or paused resources relased all taken resources.
                foreach (TaskMetadata task in _runningTasks)
                {
                    // Creating local copy to prevent thread interference.
                    object[] resources = task?.GetResources()?.ToArray();
                    
                    if (resources != null)
                        foreach (object o in resources)
                            // Updating HashSet of taken resources.
                            _takenResources?.Add(o);
                }
            }
            // LockResources();
        }

        // Checks for more prioritized task in list of pending tasks and pauses least priviledged running task.
        private void CheckForMorePrioritizedTask()
        {
            TaskMetadata pendingTask = _pendingTasks?.GetFirstWithoutRemoving();
            int indexOfLeastPrioritizedRunningTask = -1;
            // Real priority can't be zero.
            int worstPriority = 0;

            lock (_lock)
            {
                foreach (TaskMetadata t in _runningTasks.ToArray())
                {
                    if (t == null)
                        return;

                    // Priority of the running task from current iteration.
                    int runningPriority = (int) t.GetPriority();

                    // Comparing current priority to previous iterations.
                    if (runningPriority > worstPriority || indexOfLeastPrioritizedRunningTask == -1)
                    {
                        worstPriority = runningPriority;
                        indexOfLeastPrioritizedRunningTask = _runningTasks.IndexOf(t);
                    }
                }

                // Preventing exceptions and control statement.
                if (indexOfLeastPrioritizedRunningTask == -1)
                    return;

               // Extract least priviledged task from the list of running tasks. 
                TaskMetadata leastPriviledgedRunningTask = _runningTasks[indexOfLeastPrioritizedRunningTask];

                // Exceptino prevention.
                if (leastPriviledgedRunningTask == null || pendingTask == null)
                    return;

                int leastPriviledgeRunningTaskPriority = (int) leastPriviledgedRunningTask.GetPriority();
                int pendingTaskPriority = (int) pendingTask.GetPriority();

                // Comparing priority of least priviledged running tasks and pending task.
                if (leastPriviledgeRunningTaskPriority > pendingTaskPriority)
                {
                    // If least priviledged running task has lower priority than pending task, pausing it.
                    leastPriviledgedRunningTask?.GetCooperativeToken()?.Pause();
                    // Removing it from the list of running tasks.
                    _runningTasks?.RemoveAt(indexOfLeastPrioritizedRunningTask);
                    // Adding it back to the list of pending tasks.
                    _pendingTasks.Add((int)leastPriviledgedRunningTask.GetPriority(), leastPriviledgedRunningTask);
                    // Updating resources, assuming that function released all resources after being paused.
                    MaintainResources();
                    // Console.WriteLine("Pausing {0}.", leastPriviledgedRunningTask?.Descriptor);
                }
            }
        }

        /// <summary>
        /// Provides insight into the list of currently queued tasks.
        /// </summary>
        /// <returns>Returns the list of currently queued tasks.</returns>
        public ConcurrentSortedList<TaskMetadata> GetQueuedTasks()
        {
            return _pendingTasks;
        }

        /// <summary>
        /// Provides insight into the list of currently queued tasks.
        /// </summary>
        /// <returns>List of queued tasks as IEnumerable.</returns>
        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return (IEnumerable<Task>)_runningTasks;
        }

        // Tries to execute task on ThreadPool.
        private void Execute(Task task)
        {
            try
            {
                // Using UnsafeQueueWorkItem due to performance reasons and trying to execute task.
                ThreadPool.UnsafeQueueUserWorkItem(_ => { TryExecuteTask(task); }, null);
            }

            catch (Exception) { throw; }
        }
        
        /// <summary>
        /// Internally called after Task.Factory.StartNew() or Task.Start().
        /// </summary>
        /// <param name="task">Task that is being started.</param>
        protected override void QueueTask(Task task)
        {
            // Trying to execute task.
            Execute(task);
        }

        /// <summary>
        /// Allows task to be executed on current thread. (Current Synchronization Context)
        /// </summary>
        /// <param name="task">Task to be inlined.</param>
        /// <param name="taskWasPreviouslyQueued">Control variable showing was task previously queued.</param>
        /// <returns></returns>
        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            // Not implemented because Skeduler don't limit number of ThreadPool threads but rather uses control counting variable.
            return false;
        }

        private void LockResources()
        {
            foreach (TaskMetadata tm in _runningTasks)
                foreach (var o in tm.GetResources())
                    if (!o.Equals(null))
                        Monitor.Enter(o);
        }

        private void UnlockResources()
        {
            foreach (TaskMetadata tm in _runningTasks)
                foreach (var o in tm.GetResources())
                    if (!o.Equals(null))
                        Monitor.Exit(o);
        }
    }

    /// <summary>
    /// Enum representing type of scheduling to be used, either Preemtpive or Non-Preemptive.
    /// </summary>
    public enum SchedulingType
    {
        Preemptive,
        NonPreemptive
    }
}