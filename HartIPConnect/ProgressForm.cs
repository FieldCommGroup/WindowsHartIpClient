/*
Copyright 2019 FieldComm Group, Inc.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

Filename: ProgressForm.cs
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FieldCommGroup.HartIPConnect
{
    public partial class ProgressForm : Form
    {
        private BackgroundWorker m_BgWorker;
        private static Object SyncObject = new Object();       
        private bool m_userWantStop = false;

        public delegate void CSHandler();

        public ProgressForm(BackgroundWorker BgWorker)
        {
            InitializeComponent();
            m_BgWorker = BgWorker;           
        }

        /// <summary>
        /// User want to stop the process
        /// </summary>
        public bool UserWantStop
        {
            get { return m_userWantStop; }
        }    

        private void StopBtn_Click(object sender, EventArgs e)
        {
            lock (SyncObject)
            {
                m_userWantStop = true;
                if ((m_BgWorker != null) && m_BgWorker.WorkerSupportsCancellation)
                    m_BgWorker.CancelAsync();
            }
        }

        /// <summary>
        /// Increment the progress bar by the specified number of steps
        /// </summary>
        /// <param name="steps">number of steps</param>
        public void IncrementStep()
        {            
            if (InvokeRequired)
            {
                try
                {
                    BeginInvoke(new CSHandler(IncrementStepSafelyHandler),
                         new Object[0]);
                }
                catch
                {
                    // do nothing
                }
            }
            else
            {
                IncrementStepSafelyHandler();
            }
        }

        private void IncrementStepSafelyHandler()
        {
            lock (SyncObject)
            {
                if (!IsHandleCreated || IsDisposed)
                    return;              
               
                ProgressBar.PerformStep();                                                  
            }
        }

        /// <summary>
        /// Close the dialog.  This can be called from any thread and safely
        /// close dialog.
        /// </summary>
        public void CloseSafely()
        {
            if (InvokeRequired)
            {
                try
                {
                    BeginInvoke(new CSHandler(CloseSafelyHandler), new Object[0]);
                }
                catch
                {
                    // do nothing
                }
            }
            else
            {
                CloseSafelyHandler();
            }
        }

        private void CloseSafelyHandler()
        {
            lock (SyncObject)
            {
                if (!IsHandleCreated || IsDisposed)
                    return;

                Close();
            }
        }   
    }
}
