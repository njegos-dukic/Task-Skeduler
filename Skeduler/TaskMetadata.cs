using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OPOS
{
    /// <summary>
    /// Class that encapsulates action, it's associated task, priority, duration as well as cooperative token and list of requested resources.
    /// </summary>
    public class TaskMetadata
    {
        private Task _task;
        private readonly Action _action;
        // Priority is not readonly because it can be altered in PCP process.
        private Priority _priority;
        private readonly int _duration;
        private readonly CooperativeToken _cooperativeToken;
        private readonly List<object> _resources = null;

        /// <summary>
        /// Object describing the task.
        /// </summary>
        public object Descriptor { get; private set; }

        /// <summary>
        /// Creates new TaskMetadata object.
        /// </summary>
        /// <param name="action">Action to be executed.</param>
        /// <param name="priority">Priority enum of the task.</param>
        /// <param name="duration">Duration limit in seconds.</param>
        /// <param name="cooperativeToken">Cooperative token used as cooperating mechanism.</param>
        /// <param name="descriptor">Descriptor object.</param>
        /// <param name="resources">List of objects representing resources that task need.</param>
        public TaskMetadata(Action action, Priority priority, int duration, CooperativeToken cooperativeToken, object descriptor, List<object> resources = null)
        {
            _action = action;
            _priority = priority;
            _duration = duration;
            _cooperativeToken = cooperativeToken;
            _resources = resources;
            Descriptor = descriptor;
        }

        /// <summary>
        /// Grabs task from the TaskMetadata object.
        /// </summary>
        /// <returns>Task associated with this object.</returns>
        public Task GetTask()
        {
            return _task;
        }

        /// <summary>
        /// Sets the task of this object.
        /// </summary>
        /// <param name="task">Task to be set.</param>
        public void SetTask(Task task)
        {
            _task = task;
        }

        /// <summary>
        /// Grabs the action from the TaskMetadata object.
        /// </summary>
        /// <returns>Action associated with this object.</returns>
        public Action GetAction()
        {
            // GetAction() is used only when starting the task so we use it to instantinated task deadline through .Cancel() method of it's CooperativeToken.
            Task.Delay(_duration * 1000).ContinueWith(t => { _cooperativeToken.Cancel(); }); 
            return _action;
        }

        /// <summary>
        /// Grabs the priority from the TaskMetadata object.
        /// </summary>
        /// <returns>Priority associated with this object.</returns>
        public Priority GetPriority()
        {
            return _priority;
        }

        /// <summary>
        /// Sets the priority of this object.
        /// </summary>
        /// <param name="priority">Priority to be set.</param>
        public void SetPriority(Priority priority)
        {
            _priority = priority;
        }

        /// <summary>
        /// Grabs the CooperativeToken from the TaskMetadata object.
        /// </summary>
        /// <returns>CooperativeToken associated with this object.</returns>
        public CooperativeToken GetCooperativeToken()
        {
            return _cooperativeToken;
        }

        /// <summary>
        /// Grabs the Resources from the TaskMetadata object.
        /// </summary>
        /// <returns>List of resources associated with this object.</returns>
        public List<object> GetResources()
        {
            return _resources;
        }


        /// <summary>
        /// Adds TaskMetadata object to the ConcurrentSortedList with lower priority or to the end of current priority part if it's already at Low.
        /// </summary>
        /// <param name="list"></param>
        public void AddWithLowerPriority(ConcurrentSortedList<TaskMetadata> list)
        {
            if (_priority.Equals(Priority.Low))
                list.Add((int)Priority.Low, this);
            else
            {
                _priority += 1;
                list.Add((int)_priority, this);
            }
        }
    }

    /// <summary>
    /// Class representing cooperative mechanism and used for cooperative scheduling.
    /// </summary>
    public class CooperativeToken
    {
        private bool isPauseRequested;
        private bool isCancellationRequested;

        /// <summary>
        /// Creating new CooperativeToken that is not paused or cancelled.
        /// </summary>
        public CooperativeToken()
        {
            isPauseRequested = false;
            isCancellationRequested = false;
        }

        /// <summary>
        /// Check is CooperationToken paused.
        /// </summary>
        /// <returns>true if token is paused, otherwise false.</returns>
        public bool IsPauseRequested()
        {
            return isPauseRequested;
        }

        /// <summary>
        /// Check is CooperationToken cancelled.
        /// </summary>
        /// <returns>true if token is cancelled, otherwise false.</returns>
        public bool IsCancellationRequested()
        {
            return isCancellationRequested;
        }

        /// <summary>
        /// Check is CooperationToken paused or cancelled.
        /// </summary>
        /// <returns>true if token is paused or cancelled, otherwise false.</returns>
        public bool IsPausedOrCanceled()
        {
            return isPauseRequested || isCancellationRequested;
        }

        /// <summary>
        /// Mark this cooperation token as cancelled.
        /// </summary>
        public void Cancel()
        {
            isCancellationRequested = true;
        }

        /// <summary>
        /// Mark this CooperationToken as cancelled after duration in seconds.
        /// </summary>
        /// <param name="seconds">Duration in seconds.</param>
        public void CancelAfter(int seconds)
        {
            // Delaying Cancelation with the use of Task.Delay();
            if (Task.Delay(seconds * 1000) != null)
                Cancel();
        }

        /// <summary>
        /// Mark this CooperationToken as paused.
        /// </summary>
        public void Pause()
        {
            isPauseRequested = true;
        }

        /// <summary>
        /// Remove pause flag from this CooperationToken.
        /// </summary>
        public void Resume()
        {
            isCancellationRequested = false;
            isPauseRequested = false;
        }
    }
    
    /// <summary>
    /// Enum representing priority of the task to be scheduled.
    /// </summary>
    public enum Priority
    {
        Low = 4,
        Moderate = 3,
        High = 2,
        Critical = 1
    }
}
