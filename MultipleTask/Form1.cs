using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MultipleTask
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var firstTask = new Task<int>(() => TaskMethod("First Task", 3));
            var secondTask = new Task<int>(() => TaskMethod("Second Task", 2));
            firstTask.ContinueWith(t => Console.WriteLine("The first answer is {0}.Thread id {1},is thread pool thread:{2}", t.Result,
                Thread.CurrentThread.ManagedThreadId,
                Thread.CurrentThread.IsThreadPoolThread),
                TaskContinuationOptions.OnlyOnRanToCompletion);
            firstTask.Start();
            secondTask.Start();
            Thread.Sleep(TimeSpan.FromSeconds(4));

            Task continuation = secondTask.ContinueWith(
                t => Console.WriteLine("The second answer is {0}.ThreadId {1},is thread pool thread:{2}", t.Result,
                Thread.CurrentThread.ManagedThreadId,
                Thread.CurrentThread.IsThreadPoolThread),
                TaskContinuationOptions.NotOnRanToCompletion |
                TaskContinuationOptions.ExecuteSynchronously);

            continuation.GetAwaiter().OnCompleted(
                () => Console.WriteLine("Continuation Task Completed!ThreadId {0},is thread pool thread:{1}",
                Thread.CurrentThread.ManagedThreadId,
                Thread.CurrentThread.IsThreadPoolThread));

            Thread.Sleep(TimeSpan.FromSeconds(2));
            Console.WriteLine();

            firstTask = new Task<int>(() => {
                var innerTask = Task.Factory.StartNew(() => TaskMethod("Second Task", 5), TaskCreationOptions.AttachedToParent);
                innerTask.ContinueWith(t => TaskMethod("Third Task", 2),
                    TaskContinuationOptions.AttachedToParent);
                return TaskMethod("First Task", 2);
            });

            firstTask.Start();

            while (!firstTask.IsCompleted) {
                Console.WriteLine(firstTask.Status);
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
            }
            Console.WriteLine(firstTask.Status);
            Thread.Sleep(TimeSpan.FromSeconds(10));
        }
        static int TaskMethod(string name, int seconds)
        {
            Console.WriteLine("Task {0} is running on a thread id {1}.Is thread pool thread:{2}", name, Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.IsThreadPoolThread);
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
            return 42 * seconds;
        }
    }
}
