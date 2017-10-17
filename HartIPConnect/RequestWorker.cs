using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Timers;

namespace FieldCommGroup.HartIPConnect
{
    /// <summary>
    /// RequestTask class store the HartIPRequest, HartIPResponse,
    /// number of retries, and the retry delay values.  
    /// </summary>
    internal class RequestTask
    {
        public HartIPRequest HartIPReq;
        public HartIPResponse HartIPRsp;
        public uint nNumberOfRetries;       
        public uint nDrRetries;
        public uint nRetryDelay;
        public bool bIsCompleted;

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="Req">HartIPRequest</param>
        /// <param name="nNumOfRetries">uint</param>
        /// <param name="nRetryDelay">uint</param>
        public RequestTask(HartIPRequest Req, uint nNumOfRetries,
            uint nRetryDelay)
        {
            this.HartIPReq = Req;
            this.HartIPRsp = null;
            this.nNumberOfRetries = nNumOfRetries;
            this.nDrRetries = 0;
            this.nRetryDelay = nRetryDelay;
            this.bIsCompleted = false;
        }
    }        

    /// <summary>
    /// Use a BackgroundWorker to perform the request and a Timer to perform retry request
    /// so that user able to cancel the task.
    /// </summary>
    internal class RequestWorker
    {
        /// <summary>
        /// HandleSendRequestDelegate Callback that handle send HART-IP request.
        /// </summary>
        /// <param name="HandleReqTask"><see cref="RequestTask"/></param>
        public delegate void HandleSendRequestDelegate(RequestTask HandleReqTask);

        private RequestTask m_RequestTask;
        private ProgressForm m_Progress;
        private BackgroundWorker m_worker;
        private Exception m_Ex = null;

        private HandleSendRequestDelegate m_HandleSendReq;
        private System.Timers.Timer m_ExecuteTimer;
        private static Object SyncObject = new Object();        

        /// <summary>
        /// Conturctor
        /// </summary>
        /// <param name="Request"><see cref="RequestTask"/></param>
        /// <param name="HandleSendReq"><see cref="HandleSendRequestDelegate"/> callback</param>
        public RequestWorker(RequestTask ReqTask, HandleSendRequestDelegate HandleSendReq)
        {
            m_RequestTask = ReqTask;
            m_HandleSendReq = HandleSendReq;

            // create the background worker and progress bar form
            m_worker = new BackgroundWorker();
            m_worker.WorkerSupportsCancellation = true;
            m_worker.DoWork += new DoWorkEventHandler(this.Work);
            m_worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.WorkCompleted);
            m_Progress = new ProgressForm(m_worker);                                 
        }
      
        /// <summary>
        /// Display the progress bar form and wait until the task is finished
        /// or user press the stop button in the progress bar form to
        /// cancel the task.
        /// </summary>
        /// <remarks>If user canceled the action, it will send the
        /// Flush Dr HART Command to the HIPDevice.</remarks>
        public void Perform()
        {
            if (m_ExecuteTimer != null)
                throw new Exception("Request worker already started.");

            // start the timer first
            StartExcuteTimer();                                     

            // show the progress bar form
            m_Progress.ShowDialog();
            if (m_Progress.UserWantStop)
                m_Ex = new UserCancelException();

            // task finished or user canceled. Clear up the resource
            m_ExecuteTimer.Enabled = false;
            m_ExecuteTimer.Dispose();
            m_ExecuteTimer = null;

            // send flush dr command to gateway to clear the DR
            if (!m_RequestTask.bIsCompleted)
            {
                HartClient.Instance.SendFlushDrCmd();
            }

            // clear up the resource
            m_Progress.Dispose();
            m_Progress = null;
            m_worker.Dispose();
            m_worker = null;

           // throw if it has exception
           if (m_Ex != null)
               throw m_Ex;                           
        }

        /// <summary>
        /// Create the timer to execute the task
        /// </summary>
        private void StartExcuteTimer()
        {
            m_ExecuteTimer = new System.Timers.Timer();
            m_ExecuteTimer.Interval = m_RequestTask.nRetryDelay;
            m_ExecuteTimer.Elapsed += new ElapsedEventHandler(ExecuteRequestTime);
            m_ExecuteTimer.Start();
        }

        /// <summary>
        /// Called when the background worker RunWorkerAsync is called.        
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ev"></param>
        private void Work(object sender, DoWorkEventArgs ev)
        {
            if ((m_worker != null) && !m_worker.CancellationPending)
            {
                // call the caller's callback function
                m_HandleSendReq(m_RequestTask);               
            }
        }

        /// <summary>
        /// Called when the DoWork event is returned or CancelAsync() called 
        /// user cancelled from the progress bar form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {                     
            // Check if user cancelled the action
            if (e.Error != null)
            {
                if (m_Progress != null)
                    m_Progress.CloseSafely();       

                m_Ex = e.Error;
                return;
            }

            // Check if the request task is not completed
            if ((m_worker != null) && !m_worker.CancellationPending &&
                !m_RequestTask.bIsCompleted)
            {                                
                // change the timer interval and enable it again for retries
                if (m_ExecuteTimer != null)
                {
                    m_ExecuteTimer.Interval = m_RequestTask.nRetryDelay;
                    m_ExecuteTimer.Enabled = true;
                }
                if (m_Progress != null)
                    m_Progress.IncrementStep();
            }
            else
            {
                // close the progress bar form
                if (m_Progress != null)
                    m_Progress.CloseSafely();                
            }            
        }

         /// <summary>
        /// Timer callback function. It starts the background worker's
        /// DoWork event if the task is not completed.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void ExecuteRequestTime(Object source, ElapsedEventArgs e)
        {
            if ((m_worker == null) || m_worker.CancellationPending ||
                (m_ExecuteTimer == null) || !m_ExecuteTimer.Enabled)
                return;

            lock (SyncObject)
            {
                // disable the timer
                m_ExecuteTimer.Enabled = false;

                if (!m_RequestTask.bIsCompleted)
                {
                    // Trigger the background worker's DoWork event callback
                    m_worker.RunWorkerAsync();
                }
            }                          
        }      
    }
}
