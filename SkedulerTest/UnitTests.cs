using Microsoft.VisualStudio.TestTools.UnitTesting;
using OPOS;
using System.Threading;

namespace SkedulerTest
{
    [TestClass]
    public class UnitTests
    {
        [TestMethod]
        public void PriorityQueuingTest()
        {
            Skeduler mySched = Skeduler.GetSkeduler(SchedulingType.NonPreemptive, 1, 60);

            TaskMetadata t1 = new TaskMetadata(() => { }, Priority.Low, 30, new CooperativeToken(), "Task 1");
            TaskMetadata t2 = new TaskMetadata(() => { }, Priority.Critical, 30, new CooperativeToken(), "Task 2");
            TaskMetadata t3 = new TaskMetadata(() => { }, Priority.Moderate, 30, new CooperativeToken(), "Task 3");
            TaskMetadata t4 = new TaskMetadata(() => { }, Priority.High, 30, new CooperativeToken(), "Task 4");

            mySched.QueueTask(t1);
            mySched.QueueTask(t2);
            mySched.QueueTask(t3);
            mySched.QueueTask(t4);

            ConcurrentSortedList<TaskMetadata> actual = mySched.GetQueuedTasks();

            Assert.AreEqual(t2, actual.GetFirst());
            Assert.AreEqual(t4, actual.GetFirst());
            Assert.AreEqual(t3, actual.GetFirst());
            Assert.AreEqual(t1, actual.GetFirst());
        }

        [TestMethod]
        public void NonPreemptiveTest()
        {
            int[] array = new int[4];
            int i = 0;

            Skeduler nonPreemptive = Skeduler.GetSkeduler(SchedulingType.NonPreemptive, 2, 60);

            TaskMetadata t1 = new TaskMetadata(() => { Thread.Sleep(400); array[i] = 1; i++; }, Priority.Critical, 30, new CooperativeToken(), "Task 1");
            TaskMetadata t2 = new TaskMetadata(() => { Thread.Sleep(200); array[i] = 2; i++; }, Priority.Critical, 30, new CooperativeToken(), "Task 2");
            TaskMetadata t3 = new TaskMetadata(() => { Thread.Sleep(100); array[i] = 3; i++; }, Priority.Critical, 30, new CooperativeToken(), "Task 3");
            TaskMetadata t4 = new TaskMetadata(() => { Thread.Sleep(500); array[i] = 4; i++; }, Priority.Critical, 30, new CooperativeToken(), "Task 4");

            nonPreemptive.QueueTask(t1);
            nonPreemptive.QueueTask(t2);
            nonPreemptive.QueueTask(t3);
            nonPreemptive.QueueTask(t4);

            nonPreemptive.Schedule();

            Thread.Sleep(1000);

            Assert.AreEqual(2, array[0]);
            Assert.AreEqual(3, array[1]);
            Assert.AreEqual(1, array[2]);
            Assert.AreEqual(4, array[3]);
        }

        [TestMethod]
        public void FIFOQueuingTest()
        {
            Skeduler FIFOSched = Skeduler.GetSkeduler(SchedulingType.NonPreemptive, 1, 60);

            TaskMetadata t1 = new TaskMetadata(() => { }, Priority.Critical, 30, new CooperativeToken(), "Task 1");
            TaskMetadata t2 = new TaskMetadata(() => { }, Priority.Critical, 30, new CooperativeToken(), "Task 2");
            TaskMetadata t3 = new TaskMetadata(() => { }, Priority.Critical, 30, new CooperativeToken(), "Task 3");

            FIFOSched.QueueTask(t3);
            FIFOSched.QueueTask(t1);
            FIFOSched.QueueTask(t2);

            ConcurrentSortedList<TaskMetadata> actual = FIFOSched.GetQueuedTasks();

            Assert.AreEqual(t3, actual.GetFirst());
            Assert.AreEqual(t1, actual.GetFirst());
            Assert.AreEqual(t2, actual.GetFirst());
        }
    }
}
