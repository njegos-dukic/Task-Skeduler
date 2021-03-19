# Skeduler - Simple Task Scheduler

<img src="..\assets\Logo.png" alt="drawing" width="230"/>

## ***Description*** 

* **Skeduler** is a simple .NET CLR Task Scheduler that inherits and redefines System.Threading.Tasks.TaskScheduler. 
* Implemented on **.NET Standard 2.0**.
* It provides powerful custom API that enables user to specify the type of scheduling (Either **Preemptive** or **Non-Preemptive**), actions that you want to schedule, their priority, allowed deadline, as well as the deadline for the entire scheduler.
* **Skeduler** is highly reliable and it prevents deadlocks from happening. It uses combination of Banker's Algorithm and PCP (Priority Ceiling Protocol).
* Highly adaptable scheduler that enables scheuling by **priority** wrapped with **FCFS** scheduling as well as **real-time scheduling** and **cooperativeness**.



## ***Installation*** 

- **Skeduler** is delivered as a Dynamically Linked Library (*.dll*) and can be added to your projects in Visual Studio as follows:

  > Dependencies -> Add Project Reference -> Browse -> Find and add external .dll


## ***Usage***

- **Skeduler** follows *Singleton* patter allowing it to only have one instance of it at any given time.
  
1. **Creating Skeduler instance**
    - After declaring Skeduler, use the following method to create and return an instance of it:
  
        ```C#
        Skeduler mySched = Skeduler.GetSkeduler(SchedulingType schedulingType, int threadCount, int schedulingDurationInSeconds);
        ``` 

       - **SchedulingType** is Enum present in the same namespace as Skeduler and specifies either *Preemptive* or *NonPreemptive* scheduling.
       - **threadCount** specifies the maximum number of Tasks that can be active at once.
       - **schedulingDurationInSeconds** specifies a deadline for the scheduler in seconds.

2. **Adding Tasks to Skeduler**
    - After instantination of the scheduler, tasks to be scheduled can be added using the following method: 

        ```C#
        customSch.QueueTask(TaskMetadata task);
        ```
        - TaskMetadata is class from the same namespace as the Skeduler and it can be created by using:
        ```C#
        new TaskMetadata(Action action, Priority priority, int duration, CooperativeToken cooperativeToken, object descriptor, List<object> resources = null);
        ```
      - Where: 
        * **action** is an Action object that will be scheduled.
        * **priority** is Enum that represents the priority of the task. 
        * **duration** specifies the maximum duration in seconds until task will te terminated if it's not finished.
        * **cooperativeToken** represents an object expected to be used by the user in order to achive expected results.
         
            ```C#
            CooperativeToken x1 = new CooperativeToken();
            TaskMetadata xa = new TaskMetadata(() =>
            {
                while (!x1.IsPausedOrCanceled())
                {
                    System.Console.Write("a");
                    Thread.Sleep(300);
                    while (x1.IsPausedOrCanceled()) ;
                }
            }, Priority.Low, 30, x1, "Task a");
            ```

        * **descriptor** object is used for specifiying details of an Action being scheduled.
        * **resources** represents list of resources that task requires to run properly.

3. **Starting the scheduling**
   - Skeduler can be started with the call to the following method:
        ```C#
        customSch.Schedule();
        ```
      - Real-Time scheduling: Tasks can be queued after the ```.Schedule();``` call with the use of ```.QueueAction();```

## ***Technical Disclaimer***

- **Skeduler** will function as expected if it's used as recommended.
- Use of this service is limited by several factors, including but not limiting to:
  - **Cooperative cancelling**: *TaskMetadata* class encapsulates ```CooperationToken``` that user needs to incorporate in his/her methods if they expect proper behaviour of the scheduler. ```CooperativeToken``` is set to *Cancelled* after the duration specified in the constructor regardless of *Preemptive* or *NonPreemptive* scheduling or to *Paused* in case that it is suspended due to the arrival of more prioritized ```Task``` during *Preemptive* scheduling.
  - **Performance and correctnes** of machine it is running on. Minimum system requirements are not precisely specified but **Skeduler** tends to use substential system resources.

## ***Design Decisions***

- **Skeduler** does not alow *Deadlocks* to happen by using the combination of Banker's Algorithm and modified PCP protocol. Deadlocks are prevented by decreasing the priority of the upcoming tasks that could cause them and putting them back to the queue of pending items. Also, in Preemtive scheduling, priority of the tasks that uses shared resources is increased to the maximum thus allowing them to finish regardless of any upcoming tasks.
- Considering that pausing or cancelling of ```Task``` releases all of it's taken resources.
- *Resources* are stored as HashSet of objects because any ```object``` in .NET is derived from ```object``` thus enabling anything to be passed as resource (e.g. Network or DB connection, I/O Device, File on Disk..)
- **Skeduler** will be disposed after the amount of seconds specified at construction.
- Specific logical and implementation decisions taken are documented at their respective places in source code.
