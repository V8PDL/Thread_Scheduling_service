using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading;

namespace Service_For_OS
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
    public partial class Service1 : ServiceBase
    {
        static List<Text> Texts;
        static List<Image> Images;
        static List<Link> Links;
        static Thread[] Threads;
        static EventWaitHandle[] Wait_Service = new EventWaitHandle[3];
        static EventWaitHandle[] Wait_Service_Finish = new EventWaitHandle[3];
        static bool run;
        static object locker = new object();
        static string[] paths = new string[]
        {
            @"C:\Users\Дмитрий\source\repos\OS\OS\bin\Debug\Texts.json",
            @"C:\Users\Дмитрий\source\repos\OS\OS\bin\Debug\Images.json",
            @"C:\Users\Дмитрий\source\repos\OS\OS\bin\Debug\Links.json",
            @"C:\Users\Дмитрий\source\repos\Service_For_OS\Service_For_OS\bin\Debug\halp.txt"        
        };
        public Service1()
        {
            InitializeComponent();
        }
        protected override void OnStart(string[] args)
        {
            Thread thread = new Thread(Start) { Name = "MainThread" };
            thread.Start();
        }
        protected override void OnStop()
        {
            run = false;
            for (int i = 0; i < Wait_Service.Length; i++)
            {
                if (Threads[i] != null && Threads[i].IsAlive)
                {
                    Wait_Service[i].Set();
                    Threads[i].Join();
                    Wait_Service_Finish[i].Set();
                }
            }
            lock (locker)
            {
                using (StreamWriter sr = new StreamWriter(paths[3], true))
                {
                    sr.WriteLine("Ended at all (OnStop()) " + DateTime.Now.ToString("mm:ss:fff"));
                }
            }
        }
        static void Start()
        {
            run = true;
            using (StreamWriter sr = new StreamWriter(paths[3]))
            {
                sr.WriteLine("Started at " + DateTime.Now.ToString("mm:ss.fff"));
            }

            for (int i = 0; i < Wait_Service.Length; i++)
            {
                if (!EventWaitHandle.TryOpenExisting("Global\\Wait_Service" + i.ToString(), out Wait_Service[i]))
                    Wait_Service[i] = new EventWaitHandle(true, EventResetMode.ManualReset, "Global\\Wait_Service" + i.ToString());

                if (!EventWaitHandle.TryOpenExisting("Global\\Wait_Service_Finish" + i.ToString(), out Wait_Service_Finish[i]))
                    Wait_Service_Finish[i] = new EventWaitHandle(false, EventResetMode.ManualReset, "Global\\Wait_Service_Finish" + i.ToString());
            }
            Threads = new Thread[3];

            Threads[0] = new Thread(() => Deserialize(0, paths[0], ref Texts)) { Name = "Thread0" };
            Threads[1] = new Thread(() => Deserialize(1, paths[1], ref Images)) { Name = "Thread1" };
            Threads[2] = new Thread(() => Deserialize(2, paths[2], ref Links)) { Name = "Thread2" };

            while (!(File.Exists(paths[0]) && File.Exists(paths[1]) && File.Exists(paths[2])))
                Thread.Sleep(10000);

            for (int i = 0; i < Threads.Length; i++)
                Threads[i].Start();
        }


        static void Deserialize<T>(int N, string path, ref List<T> list)
        {
            while (run)
            {
                Wait_Service_Finish[N].Reset();
                Wait_Service[N].WaitOne();
                lock (locker)
                {
                    using (StreamWriter sr = new StreamWriter(paths[3], true))
                    {
                        sr.WriteLine(Thread.CurrentThread.Name + " after waiting! " + DateTime.Now.ToString("mm:ss.fff"));
                    }
                }
                using (StreamReader sr = new StreamReader(path))
                    list = JsonConvert.DeserializeObject<List<T>>(sr.ReadToEnd() + "]");
                if (run)
                    AddToDB(N);
                lock (locker)
                {
                    using (StreamWriter sr = new StreamWriter(paths[3], true))
                    {
                        sr.WriteLine(Thread.CurrentThread.Name + " before setting! " + DateTime.Now.ToString("mm:ss.fff"));
                    }
                }
                Wait_Service_Finish[N].Set();
            }
            lock (locker)
            {
                using (StreamWriter sr = new StreamWriter(paths[3], true))
                {
                    sr.WriteLine("Thread " + Thread.CurrentThread.Name + " is terminated! " + DateTime.Now.ToString("mm:ss.fff"));
                }
            }
        }
        static void AddToDB(int N)
        {
                Models.MyDataBaseEntities1000 myDB = new Models.MyDataBaseEntities1000();
                switch (N)
                {
                    case 0:
                        {
                            List<Models.Text> list = (from t in myDB.Texts select t).ToList();
                            //List<Models.Text> list = myDB.Texts.ToList();
                            int id;
                            if (list.Any())
                                id = list.Count;
                            else
                                id = 0;
                            foreach (Text t in Texts)
                            {
                                Models.Text t1 = new Models.Text()
                                {
                                    Id = id,
                                    Post_ID = t.ID,
                                    Post_Text = t.Post_Text.Length <= 8000 ? t.Post_Text : t.Post_Text.Substring(0, 7990) + "\\too long\\"
                                };
                                if (!myDB.Texts.Any(w => w.Post_ID.Equals(t1.Post_ID)))
                                {
                                    myDB.Texts.Add(t1);
                                    id++;
                                }
                            }
                            break;
                        }
                    case 1:
                        {
                            List<Models.Image> list = (from t in myDB.Images select t).ToList();
                            int id;
                            if (list.Any())
                                id = list.Count;
                            else
                                id = 0;
                            foreach (Image i in Images)
                            {
                                Models.Image t1 = new Models.Image() { Id = id, Post_ID = i.ID, Post_Image_URL = i.Image_URL };
                                if (!myDB.Images.Any(w => w.Post_Image_URL.Equals(t1.Post_Image_URL)))
                                {
                                    myDB.Images.Add(t1);
                                    id++;
                                }
                            }
                            break;
                        }
                    case 2:
                    {
                        List<Models.Link> list = (from t in myDB.Links select t).ToList();
                        int id;
                        if (list.Any())
                            id = list.Count;
                        else
                            id = 0;
                        foreach (Link l in Links)
                        {
                            Models.Link t1 = new Models.Link()
                            {
                                Id = id,
                                Post_ID = l.ID,
                                Link_URL = l.Link_URL.Length <= 3000 ? l.Link_URL : l.Link_URL.Substring(0, 2980) + " \\too long\\"
                            };
                            if (!myDB.Links.Any(w => w.Post_ID.Equals(t1.Post_ID)))
                            {
                                myDB.Links.Add(t1);
                                id++;
                            }
                        }
                        break;
                    }
                    default: return;
                }
                myDB.SaveChanges();

        }
    }
}