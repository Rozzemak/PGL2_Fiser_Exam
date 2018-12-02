using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PGL2_Fis_Marek_Slavka.StagHelper.Debug;
using PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.DAO;
using PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.Exceptions;

namespace PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.Services.Base_Service
{
    abstract class BaseService<T>
    {
        protected BaseDebug Debug;
        protected Client Client;
        protected List<T> ServiceCollection = new List<T>();
        private Thread _workerThread;
        protected List<Task<List<T>>> Works = new List<Task<List<T>>>();

        protected BaseService(BaseDebug debug, Client client)
        {
            this.Debug = debug;
            this.Client = client;
            DoWork();
        }

        protected void DoWork()
        {
            _workerThread = new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(33);
                    for (int i = 0; i < Works.Count; i++)
                    {
                        if (Works[i].Status == TaskStatus.Created)
                            Works[i].Start();
                        else if (Works[i].Status == TaskStatus.Faulted)
                        {
                            var ex = Works[i].Exception;
                            var faulted = Works[i].IsFaulted;
                            if (faulted && ex != null)
                            {
                                if (Works[i] as Task<List<T>> != null)
                                {
                                    Debug.AddMessage_Assync<object>(new Message<object>(this.GetType().Name + " request faulted." + " |TaskID[" + Works[i].Id + "]"
                                        + " |TaskResult[" + (Works[i] as Task<List<T>>).Exception.InnerException.Message + "]", MessageTypeEnum.Error)).Wait(-1);
                                }
                                else
                                {
                                    Debug.AddMessage_Assync<object>(new Message<object>(this.GetType().Name + " request faulted." + " |TaskID[" + Works[i].Id + "]"
                                       + " |TaskResult[" + Works[i].Exception.Message + "]", MessageTypeEnum.Error)).Wait(-1);
                                    Debug.AddMessage_Assync<object>(new Message<object>(ex.Data, MessageTypeEnum.Exception)).Wait(-1);
                                }
                                Works.RemoveAt(i);
                            };
                        }
                        if (i < Works.Count && Works[i].Status == TaskStatus.RanToCompletion)
                        {
                            Works.RemoveAt(i);
                        }
                    }
                }
            });
            _workerThread.Start();
        }

        protected void AddWork(Task<List<T>> tasks)
        {
            this.Works.Add(tasks);
        }

        protected T ResultHandler(Task<T> task)
        {
            try
            {
                task.Wait(-1);
                return task.Result;
            }
            catch (AggregateException ae)
            {
                foreach (var e in ae.InnerExceptions)
                {
                    // Handle the custom exception.
                    if (e is ServiceException)
                    {
                        Debug.AddMessage_Assync<object>(new Message<object>((e.Message), MessageTypeEnum.Exception)).Wait(-1);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return default(T);
        }

        /// <summary>
        /// There are many collection types, this is maybe not possible without override.
        /// </summary>
        /// <param name="toBeAdded"></param>
        public abstract void AddIntoCollectionIfNotLoaded(List<T> toBeAdded);

        protected List<T> ResultHandler(Task<List<T>> task)
        {
            try
            {
                task.Wait(-1);
                return task.Result;
            }
            catch (AggregateException ae)
            {
                foreach (var e in ae.InnerExceptions)
                {
                    // Handle the custom exception.
                    if (e is ServiceException)
                    {
                        Debug.AddMessage_Assync<object>(new Message<object>((e.Message), MessageTypeEnum.Exception)).Wait(-1);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return new List<T>(){ default(T) };
        }
    }
}
