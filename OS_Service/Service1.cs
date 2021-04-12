using System;
using System.IO;
using System.Data.Entity;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Media;
using Newtonsoft.Json;


namespace OS_Service
{
    class Text
    {
        public string ID { get; set; }
        public string Post_Text { get; set; }
    }
    class Image
    {

        public string ID { get; set; }
        public string Image_URL { get; set; }
    }
    class Link
    {
        public string ID { get; set; }
        public string Link_URL { get; set; }
    }
    class TextContext : DbContext
    {
        public TextContext()
            : base("name=DbConnection")
        { }

        public DbSet<Text> Texts { get; set; }
    }
    class ImageContext : DbContext
    {
        public ImageContext()
            : base("name=DbConnection")
        { }

        public DbSet<Image> Images { get; set; }
    }
    class LinkContext : DbContext
    {
        public LinkContext()
            : base("name=DbConnection")
        { }

        public DbSet<Link> Links { get; set; }
    }


    public partial class Service1 : ServiceBase
    {
        static List<Text> Texts;
        static List<Link> Links;
        static List<Image> Images;
        static EventWaitHandle[] Service_Events_Start = new EventWaitHandle[3];
        static EventWaitHandle[] Service_Events_Finish = new EventWaitHandle[3];
        static bool run = true;
        static object locker = new object();

        public Service1()
        {
            InitializeComponent();
            this.CanStop = true;
        }

        protected override void OnStart(string[] args)
        {
            Thread thread = new Thread(Start) {Name = "MainThread" };
            thread.Start();
            //lock (locker)
            //{
            //    using (sr)
            //    {
            //        sr.WriteLine("Ended with OnStart() " + DateTime.Now.ToString("mm:ss.fff"));
            //    }
            //}
        }
        protected override void OnStop()
        {
            lock (locker)
            {
                using (StreamWriter sr = new StreamWriter(@"C:\Users\Дмитрий\source\repos\OS_Service\OS_Service\bin\Debug\halp.txt", true))
                {
                    sr.WriteLine("onStop() called! " + DateTime.Now.ToString("mm:ss.fff"));
                }
            }
            base.OnStop();
        }
        static void Start()
        {
            lock (locker)
            {
                using (StreamWriter sr = new StreamWriter(@"C:\Users\Дмитрий\source\repos\OS_Service\OS_Service\bin\Debug\halp.txt"))
                {
                    sr.WriteLine("Start() started " + DateTime.Now.ToString("mm:ss.fff"));
                }
            }
            for (int i = 0; i < Service_Events_Start.Length; i++)
            {
                lock (locker)
                {
                    using (StreamWriter sr = new StreamWriter(@"C:\Users\Дмитрий\source\repos\OS_Service\OS_Service\bin\Debug\halp.txt", true))
                    {
                        sr.WriteLine("Handling Events #" + i.ToString() + " " + DateTime.Now.ToString("mm:ss.fff"));
                    }
                }
                if (!EventWaitHandle.TryOpenExisting("Global\\Service_Thread_Finish" + i.ToString(), out Service_Events_Finish[i]))
                    Service_Events_Finish[i] = new EventWaitHandle(false, EventResetMode.AutoReset, "Global\\Service_Thread_Finish" + i.ToString());

                if (!EventWaitHandle.TryOpenExisting("Global\\Service_Thread_Start" + i.ToString(), out Service_Events_Start[i]))
                    Service_Events_Start[i] = new EventWaitHandle(false, EventResetMode.AutoReset, "Global\\Service_Thread_Start" + i.ToString());
            }
            lock (locker)
            {
                using (StreamWriter sr = new StreamWriter(@"C:\Users\Дмитрий\source\repos\OS_Service\OS_Service\bin\Debug\halp.txt", true))
                {
                    sr.WriteLine("Handled Events " + DateTime.Now.ToString("mm:ss.fff"));
                }
            }
            Thread[] Threads = new Thread[3];
            for (int i = 0; i < Threads.Length; i++)
            {
                Threads[i] = new Thread(new ParameterizedThreadStart(Deserialize)) { Name = "Thread" + i.ToString() };
                using (StreamWriter sr = new StreamWriter(@"C:\Users\Дмитрий\source\repos\OS_Service\OS_Service\bin\Debug\halp.txt", true))
                {
                    sr.WriteLine("Thread[" + i.ToString() + "] created " + DateTime.Now.ToString("mm:ss.fff"));
                }
            }
            while (!IsAlive())
            { Thread.Sleep(3000); }
            for (int i = 0; i < Threads.Length; i++)
            { 
                Threads[i].Start(i);
                lock (locker)
                {
                    using (StreamWriter sr = new StreamWriter(@"C:\Users\Дмитрий\source\repos\OS_Service\OS_Service\bin\Debug\halp.txt", true))
                    {
                        sr.WriteLine("Thread[" + i.ToString() + "].Start() " + DateTime.Now.ToString("mm:ss.fff"));
                    }
                }
            }
            while (IsAlive())
                Thread.Sleep(10000);
            //Texts = new List<Text>();
            //Texts.Add(new Text() { ID = "0123", Post_Text = "hehehe" });
            //Deserialize(0);
            run = false;
            for (int i = 0; i < Service_Events_Start.Length; i++)
            {
                if (Threads[i].IsAlive)
                {
                    Service_Events_Start[i].Set();
                    Threads[i].Join();
                }
            }
            ServiceController sc = new ServiceController("Homework_OS");
            sc.Stop();
        }
        static void Deserialize(object Number)
        {
            lock (locker)
            {
                using (StreamWriter sr = new StreamWriter(@"C:\Users\Дмитрий\source\repos\OS_Service\OS_Service\bin\Debug\halp.txt", true))
                {
                    sr.WriteLine("Started (not in 'while' yet) " + Thread.CurrentThread.Name + " " + DateTime.Now.ToString("mm:ss.fff"));
                }
            }
            int N = (int)Number;
            string path, value;
            while (run)
            {
                lock (locker)
                {
                    using (StreamWriter sr = new StreamWriter(@"C:\Users\Дмитрий\source\repos\OS_Service\OS_Service\bin\Debug\halp.txt", true))
                    {
                        sr.WriteLine(Thread.CurrentThread.Name + " is in the loop! " + DateTime.Now.ToString("mm:ss.fff"));
                    }
                }
                Service_Events_Start[N].WaitOne();
                lock (locker)
                {
                    using (StreamWriter sr = new StreamWriter(@"C:\Users\Дмитрий\source\repos\OS_Service\OS_Service\bin\Debug\halp.txt", true))
                    {
                        sr.WriteLine("WaitOne() is passed by " + Thread.CurrentThread.Name + " " + DateTime.Now.ToString("mm:ss.fff"));
                    }
                }
                switch (N)
                {
                    case 0:
                        {
                            path = @"C:\Users\Дмитрий\source\repos\OS\OS\bin\Debug\Texts.json";
                            using (StreamReader streamReader = new StreamReader(path))
                                value = streamReader.ReadToEnd() + ']';
                            Texts = JsonConvert.DeserializeObject<List<Text>>(value);
                            break;
                        }
                    case 1:
                        {
                            path = @"C:\Users\Дмитрий\source\repos\OS\OS\bin\Debug\Images.json";
                            using (StreamReader streamReader = new StreamReader(path))
                                value = streamReader.ReadToEnd() + ']';
                            Images = JsonConvert.DeserializeObject<List<Image>>(value);
                            break;
                        }
                    case 2:
                        {
                            path = @"C:\Users\Дмитрий\source\repos\OS\OS\bin\Debug\Links.json";
                            using (StreamReader streamReader = new StreamReader(path))
                                value = streamReader.ReadToEnd() + ']';
                            Links = JsonConvert.DeserializeObject<List<Link>>(value);
                            break;
                        }
                    default: return;
                }
                AddToDB(N);
                Service_Events_Finish[N].Set();
                lock (locker)
                {
                    using (StreamWriter sr = new StreamWriter(@"C:\Users\Дмитрий\source\repos\OS_Service\OS_Service\bin\Debug\halp.txt", true))
                    {
                        sr.WriteLine("Thread " + Thread.CurrentThread.Name + " is finished! " + DateTime.Now.ToString("mm:ss.fff"));
                    }
                }
            }
            lock (locker)
            {
                using (StreamWriter sr = new StreamWriter(@"C:\Users\Дмитрий\source\repos\OS_Service\OS_Service\bin\Debug\halp.txt", true))
                {
                    sr.WriteLine("Thread " + Thread.CurrentThread.Name + " is terminated! " + DateTime.Now.ToString("mm:ss.fff"));
                }
            }
        }
        static void AddToDB(int N)
        {
            lock (locker)
            {
                using (StreamWriter sr = new StreamWriter(@"C:\Users\Дмитрий\source\repos\OS_Service\OS_Service\bin\Debug\halp.txt", true))
                {
                    sr.WriteLine("Thread " + Thread.CurrentThread.Name + " is working with db! " + DateTime.Now.ToString("mm:ss.fff"));
                }
            }
            switch (N)
            {
                case 0:
                    {
                        using (TextContext DB = new TextContext())
                        {
                            DB.Texts.AddRange(from t in Texts where !DB.Texts.Any(d => t.ID == d.ID) select t);
                            DB.SaveChanges();
                        }
                        break;
                    }
                case 1:
                    {
                        using (ImageContext DB = new ImageContext())
                        {
                            DB.Images.AddRange(from i in Images where !DB.Images.Any(d => i.ID == d.ID) select i);
                            DB.SaveChanges();
                        }
                        break;
                    }
                case 2:
                    {
                        using (LinkContext DB = new LinkContext())
                        {
                            DB.Links.AddRange(from l in Links where !DB.Links.Any(d => l.ID == d.ID) select l);
                            DB.SaveChanges();
                        }
                        break;
                    }
            }
        }
        static bool IsAlive()
        {
            if (Process.GetProcessesByName("OS").Length == 0)

            {
                lock (locker)
                {
                    using (StreamWriter sr = new StreamWriter(@"C:\Users\Дмитрий\source\repos\OS_Service\OS_Service\bin\Debug\halp.txt", true))
                    {
                        sr.WriteLine("process not started! " + Thread.CurrentThread.Name + " " + DateTime.Now.ToString("mm:ss.fff"));
                    }
                }
                return false;
            }
            return true;
        }
    }
}
