using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OPOS;

namespace SkedulerExibition
{
    class Exibition
    {
        static void Main(string[] args)
        {
            // Uncomment bellow for Non-Preemptive xzbition.

            //Skeduler mySched = Skeduler.GetSkeduler(SchedulingType.NonPreemptive, 5, 60);

            //TaskMetadata t1 = new TaskMetadata(() => { Thread.Sleep(15000); System.Console.WriteLine("Task: 1 \t Sleep(15000) \t Thread: {0}.", Thread.CurrentThread.ManagedThreadId); }, Priority.Critical, 30, new CooperativeToken(), "Task 1");
            //TaskMetadata t2 = new TaskMetadata(() => { Thread.Sleep(10000); System.Console.WriteLine("Task: 2 \t Sleep(10000) \t Thread: {0}.", Thread.CurrentThread.ManagedThreadId); }, Priority.Critical, 30, new CooperativeToken(), "Task 2");
            //TaskMetadata t3 = new TaskMetadata(() => { Thread.Sleep(5000); System.Console.WriteLine("Task: 3 \t Sleep(5000) \t Thread: {0}.", Thread.CurrentThread.ManagedThreadId); }, Priority.Critical, 30, new CooperativeToken(), "Task 3");
            //TaskMetadata t4 = new TaskMetadata(() => { Thread.Sleep(3000); System.Console.WriteLine("Task: 4 \t Sleep(3000) \t Thread: {0}.", Thread.CurrentThread.ManagedThreadId); }, Priority.Critical, 30, new CooperativeToken(), "Task 4");
            //TaskMetadata t5 = new TaskMetadata(() => { Thread.Sleep(1000); System.Console.WriteLine("Task: 5 \t Sleep(1000) \t Thread: {0}.", Thread.CurrentThread.ManagedThreadId); }, Priority.Critical, 30, new CooperativeToken(), "Task 5");
            //TaskMetadata t6 = new TaskMetadata(() => { Thread.Sleep(9500); System.Console.WriteLine("Task: 6 \t Sleep(10000) \t Thread: {0}.", Thread.CurrentThread.ManagedThreadId); }, Priority.Critical, 30, new CooperativeToken(), "Task 6");
            //TaskMetadata t7 = new TaskMetadata(() => { Thread.Sleep(5000); System.Console.WriteLine("Task: 7 \t Sleep(5000) \t Thread: {0}.", Thread.CurrentThread.ManagedThreadId); }, Priority.Critical, 30, new CooperativeToken(), "Task 7");
            //TaskMetadata t8 = new TaskMetadata(() => { Thread.Sleep(2000); System.Console.WriteLine("Task: 8 \t Sleep(2000) \t Thread: {0}.", Thread.CurrentThread.ManagedThreadId); }, Priority.Critical, 30, new CooperativeToken(), "Task 8");
            //TaskMetadata t9 = new TaskMetadata(() => { Thread.Sleep(5000); System.Console.WriteLine("Task: 9 \t Sleep(5000) \t Thread: {0}.", Thread.CurrentThread.ManagedThreadId); }, Priority.Critical, 30, new CooperativeToken(), "Task 9");
            //TaskMetadata t10 = new TaskMetadata(() => { Thread.Sleep(4000); System.Console.WriteLine("Task: 10 \t Sleep(4000) \t Thread: {0}.", Thread.CurrentThread.ManagedThreadId); }, Priority.Critical, 30, new CooperativeToken(), "Task 10");
            //TaskMetadata t11 = new TaskMetadata(() => { Thread.Sleep(7000); System.Console.WriteLine("Task: 11 \t Sleep(7000) \t Thread: {0}.", Thread.CurrentThread.ManagedThreadId); }, Priority.Critical, 30, new CooperativeToken(), "Task 11");
            //TaskMetadata t12 = new TaskMetadata(() => { Thread.Sleep(2000); System.Console.WriteLine("Task: 12 \t Sleep(2000) \t Thread: {0}.", Thread.CurrentThread.ManagedThreadId); }, Priority.Critical, 30, new CooperativeToken(), "Task 12");
            //TaskMetadata t13 = new TaskMetadata(() => { Thread.Sleep(4000); System.Console.WriteLine("Task: 13 \t Sleep(4000) \t Thread: {0}.", Thread.CurrentThread.ManagedThreadId); }, Priority.Critical, 30, new CooperativeToken(), "Task 13");

            //mySched.QueueTask(t1);
            //mySched.QueueTask(t2);
            //mySched.QueueTask(t3);
            //mySched.QueueTask(t4);
            //mySched.QueueTask(t5);
            //mySched.QueueTask(t6);
            //mySched.QueueTask(t7);
            //mySched.QueueTask(t8);
            //mySched.QueueTask(t9);
            //mySched.QueueTask(t10);
            //mySched.QueueTask(t11);
            //mySched.QueueTask(t12);
            //mySched.QueueTask(t13);

            //mySched.Schedule();

            //Thread.Sleep(5000);
            //TaskMetadata t14 = new TaskMetadata(() => { Thread.Sleep(2000); System.Console.WriteLine("Task: 14 \t Sleep(2000) \t Thread: {0}.", Thread.CurrentThread.ManagedThreadId); }, Priority.Critical, 30, new CooperativeToken(), "Task 14");
            //mySched.QueueTask(t14);

            //Thread.Sleep(5000);
            //TaskMetadata t15 = new TaskMetadata(() => { Thread.Sleep(7000); System.Console.WriteLine("Task: 15 \t Sleep(7000) \t Thread: {0}.", Thread.CurrentThread.ManagedThreadId); }, Priority.Critical, 30, new CooperativeToken(), "Task 15");
            //mySched.QueueTask(t15);

            // --------------------------------------
            // --------------------------------------

            // Uncomment bellow for Preemptive xzbition.

            // Skeduler mySched = Skeduler.GetSkeduler(SchedulingType.Preemptive, 2, 60);

            // CooperativeToken x1 = new CooperativeToken();
            // TaskMetadata xa = new TaskMetadata(() =>
            // {
            //     while (!x1.IsPausedOrCanceled())
            //     {
            //         System.Console.Write("a");
            //         Thread.Sleep(300);
            //         while (x1.IsPausedOrCanceled()) ;
            //     }
            // }, Priority.Low, 30, x1, "Task a");

            // CooperativeToken x2 = new CooperativeToken();
            // TaskMetadata xb = new TaskMetadata(() =>
            // {
            //     while (!x2.IsPausedOrCanceled())
            //     {
            //         System.Console.Write("b");
            //         Thread.Sleep(300);
            //         while (x2.IsPausedOrCanceled()) ;
            //     }
            // }, Priority.Critical, 15, x2, "Task b");

            // CooperativeToken x3 = new CooperativeToken();
            // TaskMetadata xc = new TaskMetadata(() =>
            // {
            //     while (!x3.IsPausedOrCanceled())
            //     {
            //         System.Console.Write("c");
            //         Thread.Sleep(300);
            //         while (x3.IsPausedOrCanceled()) ;
            //     }
            // }, Priority.Moderate, 30, x3, "Task c");

            // CooperativeToken x4 = new CooperativeToken();
            // TaskMetadata xd = new TaskMetadata(() =>
            // {
            //     while (!x4.IsPausedOrCanceled())
            //     {
            //         System.Console.Write("d");
            //         Thread.Sleep(300);
            //         while (x4.IsPausedOrCanceled()) ;
            //     }
            // }, Priority.High, 30, x4, "Task d");

            // mySched.QueueTask(xa);
            // mySched.QueueTask(xb);

            // mySched.Schedule();

            // Thread.Sleep(5000);
            // mySched.QueueTask(xc);

            // Thread.Sleep(5000);
            // mySched.QueueTask(xd);

            // R3 na X1 - Low
            // R1 na X2 - Moderate
            // R2 na X3 - High
            // R1 na X4 - Critical

            Skeduler mySched = Skeduler.GetSkeduler(SchedulingType.Preemptive, 1, 60);

            CooperativeToken x1 = new CooperativeToken();
            TaskMetadata xa = new TaskMetadata(() =>
            {
                while (!x1.IsPausedOrCanceled())
                {
                    System.Console.Write("1");
                    Thread.Sleep(750);
                    while (x1.IsPausedOrCanceled()) ;
                }
            }, Priority.Low, 30, x1, "Task 1");//, new List<object> { "R3" });

            CooperativeToken x2 = new CooperativeToken();
            TaskMetadata xb = new TaskMetadata(() =>
            {
                while (!x2.IsPausedOrCanceled())
                {
                    System.Console.Write("2");
                    Thread.Sleep(750);
                    while (x2.IsPausedOrCanceled()) ;
                }
            }, Priority.Moderate, 15, x2, "Task 2");//, new List<object> { 1 });

            CooperativeToken x3 = new CooperativeToken();
            TaskMetadata xc = new TaskMetadata(() =>
            {
                while (!x3.IsPausedOrCanceled())
                {
                    System.Console.Write("3");
                    Thread.Sleep(750);
                    while (x3.IsPausedOrCanceled()) ;
                }
            }, Priority.High, 30, x3, "Task 3");//, new List<object> { "R2" });

            CooperativeToken x4 = new CooperativeToken();
            TaskMetadata xd = new TaskMetadata(() =>
            {
                while (!x4.IsPausedOrCanceled())
                {
                    System.Console.Write("4");
                    Thread.Sleep(750);
                    while (x4.IsPausedOrCanceled()) ;
                }
            }, Priority.Critical, 30, x4, "Task 4");//, new List<object> { "R1" });

            mySched.QueueTask(xa);
            mySched.Schedule();

            Thread.Sleep(2000);
            mySched.QueueTask(xb);

            Thread.Sleep(2000);
            mySched.QueueTask(xc);

            Thread.Sleep(2000);
            mySched.QueueTask(xd);
        }
    }
}