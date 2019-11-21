using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Part_B
{
    // File Model
    public class FileModelClass
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
    }
    // Queue handler
    public class QueueWorker
    {
        private Queue<FileModelClass> _jobs = new Queue<FileModelClass>();
        private bool _delegateQueuedOrRunning = false;

        public void Enqueue(FileModelClass job)
        {
            lock (_jobs)
            {
                _jobs.Enqueue(job);
                if (!_delegateQueuedOrRunning)
                {
                    _delegateQueuedOrRunning = true;
                    ThreadPool.UnsafeQueueUserWorkItem(ProcessQueuedItems, null);
                }
            }
        }

        private void ProcessQueuedItems(object ignored)
        {
            while (true)
            {
                FileModelClass item;
                lock (_jobs)
                {
                    if (_jobs.Count == 0)
                    {
                        _delegateQueuedOrRunning = false;
                        break;
                    }

                    item = _jobs.Dequeue();
                }

                try
                {
                    using (var writeText = new StreamWriter(item.FilePath + item.FileName))
                    {
                        writeText.WriteLine("Text example");
                    }
                    Console.WriteLine(item);
                }
                catch
                {
                    ThreadPool.UnsafeQueueUserWorkItem(ProcessQueuedItems, null);
                    throw;
                }
            }
        }
    }
    class Program
    {
        private const int FilesToWrite = 3;
        private const string FilePath = "\\FilesFolder\\";
        private FileModelClass _fileEntity;
        
        static void Main(string[] args)
        {
            QueueWorker queueWorker = new QueueWorker();
            Console.WriteLine("App started");
            for (var i = 0; i < FilesToWrite; i++)
            {
                Console.WriteLine("Please, enter file name: ");
                var fileName = Console.ReadLine();
                var fileEntityNewElement = new FileModelClass { FilePath = FilePath, FileName = fileName + ".txt" };
                queueWorker.Enqueue(fileEntityNewElement);
            }
        }
    }
}
