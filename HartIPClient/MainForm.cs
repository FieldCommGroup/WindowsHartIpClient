// Copyright 2017 FieldComm Group
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Threading;
using System.Timers;
using System.IO;

using FieldCommGroup.HartIPConnect;

namespace FieldCommGroup.HartIPClient
{
    public partial class MainForm : Form
    {
        // Asynchronous call for the update timer
        private delegate void UpdateTimerFiredDelegate();    
        private static Object SyncRoot = new Object();

        private HartClient m_HartClient = HartClient.Instance;
        private LogMsg Logger = LogMsg.Instance;
        private ParseResponses m_ParseRsps = null;

        private int m_nCheckInactivityInterval = 1000;  // check inactivity every second
        private bool m_bParsingRsps = false;     

        private System.Timers.Timer m_InactivityCloseTimer;

        // Asynchronous call method and it is running on a thread pool thread
        // it handles published response notify     
        public delegate void HandlerPublishedMsg(HartIPResponse Rsp);
     
        public MainForm()
        {
          InitializeComponent();
          DevUnivRev_lb.Text = String.Empty;
          PublishedMsg_Tb.Text = String.Empty;
          this.AcceptButton = SendCmd_btn;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
          try
          {
            string LogPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\HARTIPLogs";

            if (!Directory.Exists(LogPath))
              Directory.CreateDirectory(LogPath);

            String logFilename = LogPath + @"\HartClientLog.txt";

              LogMgsBtn.Size = new System.Drawing.Size(68, 65);
              if (StartLogMsgs(logFilename))
              LogMgsBtn.Checked = true;
         }
         catch (Exception ex)
         {
           MessageBox.Show("Failed to start logging. Error: " + ex.Message, "Log Message Error",
             MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
          }
        }               

        /// <summary>
        /// Call when inactivity update timer elapsed event is raised.
        /// Call delegate function to update form conttrols.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void CheckInactivityCloseTime(object source, ElapsedEventArgs e)
        {
          //Marshal this call back to the UI thread
          this.BeginInvoke(new UpdateTimerFiredDelegate(UpdateInactivity));
        }

        /// <summary>
        /// Disconnect the connection if it passes 
        /// the inactivity close time without send request.
        /// This method prevents concurrently calls.
        /// </summary>
        private void UpdateInactivity()
        {
             if ((m_InactivityCloseTimer == null) || !m_InactivityCloseTimer.Enabled)
                return;

              do
              {
                lock (SyncRoot)
                {
                  if (m_HartClient.IsConnected)
                  {
                    long lNow = DateTime.Now.Ticks;
                    long lLastActiveTime = m_HartClient.LastActivityTime;
                    long lElapsed = (lNow - lLastActiveTime) / 10000;
                
                    if (lElapsed >= 0.9 * HARTIPMessage.INACTIVITY_CLOSE_TIME)
                    {
                        // Disconnect the connection
                        LogMessage("Elapsed the Session Inactivity Close Time without activity. Closing the connection.",
                          true);
                        Disconnect(); // disables m_InactivityCloseTimer
                    }
                    else if (lElapsed >= 0.8 * HARTIPMessage.INACTIVITY_CLOSE_TIME)
                    {
                        if (checkBoxKeepAlive.Checked)
                        {
                            KeepAlive();
                        }
                    }
                    //else nothing

                   } //if (m_HartClient.IsConnected)
                }
              } while (false); /* ONCE */
        }
      
        /// <summary>
        /// Call when user press the network connect menu button.
        /// Toggle the button's image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NetConnectBtn_Click(object sender, EventArgs e)
        {            
          if (NetConnectBtn.Checked)
          {
            bool bSuccess = false;
            String IpAddr = String.Empty;
            uint nPort = 0;
            uint nConnection = 0;
            uint nDrRetryDelayBase = HARTIPMessage.DR_DELAYRETRY;
            uint nDrRetries = HARTIPMessage.DR_RETRIES;
            int nTimeout = HARTIPConnect.SOCKET_TIMEOUT_DEFAULT;
            byte cPollingAddr = 0;

            OutputMsg_lb.Text = String.Empty;
            PublishedMsg_Tb.Text = String.Empty;

            // Use want to connect to HART-IP device, bring up the connect form.            
            NetConnect_Form connectForm = new NetConnect_Form(HARTIPMessage.DR_DELAYRETRY,
              HARTIPMessage.DR_RETRIES);            
            connectForm.ShowDialog();
            if (connectForm.DialogResult == DialogResult.OK)
            { 
              // get the user values
              nDrRetryDelayBase = connectForm.DrRetryDelayBase;
              nDrRetries = connectForm.DrRetries;
              IpAddr = connectForm.IPAddr;
              nPort = connectForm.Port;
              nConnection = connectForm.Connection;
              nTimeout = (int)connectForm.SocketTimeout;
              cPollingAddr = connectForm.PollingAddr;
              bSuccess = true; 
            }
            connectForm.Dispose();

            if (bSuccess)
            {             
              this.Cursor = Cursors.WaitCursor;
              try
              {
                // create a connection 
                m_HartClient.HIPDevicePollingAddr = cPollingAddr;
                bSuccess = m_HartClient.Connect(IpAddr, nPort, nConnection, nTimeout);                 
              }
              catch (Exception ex)
              {
                MessageBox.Show("Failed to connect HART-IP device. Error: " + ex.Message,
                                "Connect to HART-IP device Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
              }

              this.Cursor = Cursors.Default;            
            }

            if (bSuccess)
            {
              // set the dr values
              m_HartClient.DrRetryDelay = nDrRetryDelayBase;
              m_HartClient.DrRetreis = nDrRetries;
              OutputMsg_lb.Text += String.Format("{0} Connected to {1}:{2}\r\n\r\n",
                   HartUtil.GetTimeStamp(), IpAddr, nPort);

              NetConnectBtn.Image = ToolBarImageList.Images[1];
              NetConnectBtn.ToolTipText = "Disconnect the HART-IP device";
              String Conn;
              if (nConnection == HARTIPConnect.UDP)
                Conn = "Udp";
              else if (nConnection == HARTIPConnect.TCP)
                Conn = "Tcp";
              else
                Conn = "Secured Tcp";

              ConnectStatus.Text = String.Format("{0}:{1}:{2}", Conn, IpAddr, nPort);
              if (GetDevices())
              {
                // Create a timer for checking inactivity
                if (m_InactivityCloseTimer == null)
                {
                  m_InactivityCloseTimer = new System.Timers.Timer();
                  m_InactivityCloseTimer.Interval = m_nCheckInactivityInterval;
                  m_InactivityCloseTimer.Elapsed += new ElapsedEventHandler(CheckInactivityCloseTime);
                  m_InactivityCloseTimer.Start();
                }
                else
                  m_InactivityCloseTimer.Enabled = true;

                m_HartClient.SubscribePublishedCmdNotify(new EventHandler<HartIPResponseArg>(this.HandlePublishedCmd));
              }
            }
            else            
              NetConnectBtn.Checked = false;
          }
          else
          {
            // Disconnect the connection
            Disconnect();                      
          }
        }
       
        /// <summary>
        /// Call when user press the parse responses menu button.
        /// Toggle the button's image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ParseResponsesBtn_Click(object sender, EventArgs e)
        {
          if (ParseResponsesBtn.Checked)
          {
            bool bSuccess = false;
            String Filename = String.Empty;            
           
            // User want to parse responses, bring open file dialog
            Filename = FormUntil.BrowseFile(out bSuccess);            

            if (bSuccess)
            {
              try
              {
                StartParseResponses(Filename);
                m_bParsingRsps = true;
                ParseResponsesBtn.Image = ToolBarImageList.Images[3];
                ParseResponsesBtn.ToolTipText = "Stop Parse HART Responses";
                ParseResponsesStatus.Text = "Parsing Responses";
                bSuccess = true;
              }
              catch (Exception ex)
              {
                MessageBox.Show("Failed to load the file. Error" + ex.Message, "Parse Requests Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                bSuccess = false;
              }
            }
            
            if (!bSuccess)            
              ParseResponsesBtn.Checked = false;
          }
          else
          {
            m_bParsingRsps = false;
            // stop parsing requests
            StopParsingResponses();

            // Enable connect and log menu buttons
            ParseResponsesBtn.Image = ToolBarImageList.Images[2];
            ParseResponsesBtn.ToolTipText = "Parse HART Responses";
            ParseResponsesStatus.Text = String.Empty;
          }
        }       

        /// <summary>
        /// Call when user press the log messages to file menu button.
        /// Toggle the button's image.
        /// Add current timestamp at the end of log filename.
        /// When logfile reaches to 10000 entries a 10M file, it will
        /// create a new logfile to continue to log messages.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LogFileBtn_Click(object sender, EventArgs e)
        {
          if (LogMgsBtn.Checked)
          {
            bool bSuccess;
            // User want to log messages to file, bring a Save file dialog
            String Filename = FormUntil.SaveToFile(out bSuccess);
            if (bSuccess)
            {
              bSuccess = StartLogMsgs(Filename);              
            }

            if (!bSuccess)                               
              LogMgsBtn.Checked = false;
          }
          else
          {
            // stop logging messages
            Logger.StopLogging();         
            LogMgsBtn.Image = ToolBarImageList.Images[4];
            LogMgsBtn.ToolTipText = "Log Messages";
            LogFileStatus.Text = "Stopped log messages";
            LogMgsBtn.Size = new System.Drawing.Size(68, 65);
                LogMgsBtn.ImageScaling = ToolStripItemImageScaling.None;
            }
        }
       
        /// <summary>
        /// Call when user press Get sub-devices ids button.
        /// Display all sub-devices' id in hex format.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GetSubDevicesIds_Click(object sender, EventArgs e)
        {
          int nCount = m_HartClient.DevicesCount;         
          String DiDt;
          
          OutputMsg_lb.Text = String.Empty;

          if (nCount > 0)
          {
            HartDevice[] HDevices = new HartDevice[nCount];
            m_HartClient.HARTDevices.CopyTo(HDevices, 0);
            for (int i = 0; i < HDevices.Length; i++)
            {
              if (HDevices[i].IsWirelessHIPDevice)
                continue;

              DiDt = String.Format("{0:X2}{1:X2} {2:X2}{3:X2}{4:X2}",
                     new object[] {(HDevices[i].DeviceType >> 8),
                                   (HDevices[i].DeviceType & 0x0ff),
                                   (HDevices[i].DeviceId >> 16 & 0x0ff),
                                   (HDevices[i].DeviceId  >> 8 & 0x0ff),
                                   (HDevices[i].DeviceId  & 0x0ff)});
              LogMessage(DiDt);
            }
          }
        }       

        /// <summary>
        /// Call when user press the About menu button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AboutBtn_Click(object sender, EventArgs e)
        {
          // Bring the about box dialog
          AboutBox AboutForm = new AboutBox();
          AboutForm.ShowDialog();
        }

        /// <summary>
        /// Call when user press get device list button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GetDeviceListBtn_Click(object sender, EventArgs e)
        {
          OutputMsg_lb.Text = String.Empty;
          m_HartClient.ClearDeviceList();

          // Clear the devices combo box
          if (DeviceList_cb.Items.Count > 0)
            DeviceList_cb.Items.Clear();

          EnableCtls(false);
          ClearInputFields();
          if (GetDevices())
            OutputMsg_lb.Text = "Get device list finished and refreshed the devices in the device list.\r\n\r\n";
        } 

        /// <summary>
        /// Call when user press the Send command button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendCmdBtn_Click(object sender, EventArgs e)
        {        
          // Get the selected item's device type and device id
          ushort usDeviceType = ((DeviceData)DeviceList_cb.SelectedItem).DeviceType;
          uint nDeviceId = ((DeviceData)DeviceList_cb.SelectedItem).DeviceId;

          // Clear text in output messages control
          OutputMsg_lb.Text = String.Empty;

          EnableAll(false);
          this.Cursor = Cursors.WaitCursor;
          String Msg = SendCmd(usDeviceType, nDeviceId);
          this.Cursor = Cursors.Default;
          EnableAll(true);

          if (Msg.Length > 0)
              OutputMsg_lb.Text = Msg;           
        }
        
        /// <summary>
        /// Call when user press the Send command to all button.
        /// Send request to each device in the device list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendCmdToAllBtn_Click(object sender, EventArgs e)
        {
          int nCount = m_HartClient.DevicesCount;
          String Msg = String.Empty;

          // Clear text in output messages control
          OutputMsg_lb.Text = String.Empty;

          if (nCount > 0)
          {
            HartDevice[] HDevices = new HartDevice[nCount];
            m_HartClient.HARTDevices.CopyTo(HDevices, 0);

            EnableAll(false);
            this.Cursor = Cursors.WaitCursor;
            for (int i = 0; i < HDevices.Length; i++)
            {
              Msg = SendCmd(HDevices[i].DeviceType, HDevices[i].DeviceId);
              if (Msg.Length > 0)                              
                break;              
            }

            this.Cursor = Cursors.Default;
            EnableAll(true);

            if (Msg.Length > 0)
            {
               OutputMsg_lb.Text = Msg;             
            }
            OutputMsg_lb.SelectionStart = OutputMsg_lb.Text.Length;
            OutputMsg_lb.ScrollToCaret();
          }
        }                   

        /// <summary>
        /// Call when user changes the devices list combo box selected item.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeviceList_SelectedIndexChanged(object sender, EventArgs e)
        {
          DevUnivRev_lb.Text = ((DeviceData)DeviceList_cb.SelectedItem).UnivRev.ToString();
        }

        /// <summary>
        /// Fill devices name in the devices combo box.
        /// </summary>
        private bool FillDevicesListCtl()
        {
          bool bSuccess = false;
          int nCount = m_HartClient.DevicesCount;

          if (nCount > 0)
          {
            HartDevice[] HDevices = new HartDevice[nCount];
            m_HartClient.HARTDevices.CopyTo(HDevices, 0);

            for (int i = 0; i < HDevices.Length; i++)
            {
              // Store the device type, device id, uviversal rev values in item
              DeviceList_cb.Items.Add(new DeviceData(HDevices[i].Name, 
                                                     HDevices[i].DeviceType,
                                                     HDevices[i].DeviceId,
                                                     HDevices[i].UniversalRev));
              if (HDevices[i].IsBridgeDevice && DeviceList_cb.SelectedIndex == -1)
                // the first encountered is the root bridge device
                DeviceList_cb.SelectedIndex = i;
            }
            bSuccess = true;
          }
          return bSuccess;
        }

        /// <summary>
        /// Get devices and add them into device list
        /// </summary>
        private bool GetDevices()
        {
          bool bSuccess = false;
          
          this.Cursor = Cursors.WaitCursor;
          if (m_HartClient.GetDeviceList())
          {
            if (FillDevicesListCtl())
            {
              GetSubDeviceIdsBtn.Enabled = true;             
              EnableCtls(true);
              ReqCmd_tb.Text = "0";
              bSuccess = true;
            }
          }
          else
          {
            OutputMsg_lb.Text = m_HartClient.LastError + "\r\n\r\n";
          }
          this.Cursor = Cursors.Default;

          return bSuccess;
        }
              
        /// <summary>
        /// Call when user want to close the application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_OnClosing(object sender, FormClosingEventArgs e)
        {
          do
          {
            try
            {
              Disconnect();

              // stop parsing responses
              StopParsingResponses();              
              if (m_InactivityCloseTimer != null)
              {
                if (m_InactivityCloseTimer.Enabled)
                  m_InactivityCloseTimer.Enabled = false;

                m_InactivityCloseTimer.Dispose();
                m_InactivityCloseTimer = null;
              }

              // Clear the devices combo box
              if (DeviceList_cb.Items.Count > 0)
                DeviceList_cb.Items.Clear();

              // close the connection
              m_HartClient.Close();
            }
            catch (Exception)
            {
              // do nothing
            }
          } while (false); /* ONCE */
        }

        /// <summary>
        /// Disconnect the connection
        /// </summary>
        private void KeepAlive()
        {
            lock (SyncRoot)
            {
                m_HartClient.KeepAlive();
                OutputMsg_lb.Text += String.Format("{0} Keep alive.\r\n\r\n", HartUtil.GetTimeStamp());
                OutputMsg_lb.SelectionStart = OutputMsg_lb.Text.Length;
                OutputMsg_lb.ScrollToCaret();
            }
        }
         
        /// <summary>
        /// Disconnect the connection
        /// </summary>
        private void Disconnect()
        {
          lock (SyncRoot)
          {
           m_HartClient.Disconnect();

           // Disable the inactivity close timer
           if ((m_InactivityCloseTimer != null) && m_InactivityCloseTimer.Enabled)
              m_InactivityCloseTimer.Enabled = false;

            // Close the network connection            
            try
            {
              // unsubscribe published command notify event to clean up resource
              m_HartClient.UnSubscribePublishedCmdNotify(this.HandlePublishedCmd);
              m_HartClient.Close();             
            }
            catch (Exception e)
            {
              LogMessage(e.Message, true);
            }           

            // Clear the devices combo box
            if (DeviceList_cb.Items.Count > 0)
              DeviceList_cb.Items.Clear();           
                       
            GetSubDeviceIdsBtn.Enabled = false;           
            EnableCtls(false);
            ClearInputFields();
            NetConnectBtn.Image = ToolBarImageList.Images[0];
            NetConnectBtn.ToolTipText = "Connect to HART-IP device";
            ConnectStatus.Text = "No Connection";

            if (NetConnectBtn.Checked)
              NetConnectBtn.Checked = false;

            OutputMsg_lb.Text += String.Format("{0} Disconnected the HART-IP device connection.\r\n\r\n",
              HartUtil.GetTimeStamp());                  
            OutputMsg_lb.SelectionStart = OutputMsg_lb.Text.Length;
            OutputMsg_lb.ScrollToCaret();          
          }
        }                  

        /// <summary>
        /// Enable/disable controls
        /// </summary>
        /// <param name="bEnable">bool</param>
        private void EnableCtls(bool bEnable)
        {
          GetDeviceList_btn.Enabled = bEnable;
          DeviceList_cb.Enabled = bEnable;
          ReqCmd_tb.Enabled = bEnable;
          ReqData_tb.Enabled = bEnable;
          SendCmd_btn.Enabled = bEnable;
          SendCmdToAll_btn.Enabled = bEnable;         
        }

        /// <summary>
        /// Enable/disable menu controls
        /// </summary>
        /// <param name="bEnable">bool</param>
        private void EnableMenuCtls(bool bEnable)
        {
          NetConnectBtn.Enabled = bEnable;          
          ParseResponsesBtn.Enabled = bEnable;
          LogMgsBtn.Enabled = bEnable;
          GetSubDeviceIdsBtn.Enabled = bEnable;          
        }

        /// <summary>
        /// Enable/disable all controls
        /// </summary>
        /// <param name="bEnable"></param>
        private void EnableAll(bool bEnable)
        {
          EnableCtls(bEnable);
          EnableMenuCtls(bEnable);
        }
        
        /// <summary>
        /// Clear input fields text
        /// </summary>
        private void ClearInputFields()
        {
          ReqCmd_tb.Text = String.Empty;
          ReqData_tb.Text = String.Empty;
          DevUnivRev_lb.Text = String.Empty;
        }         

        /// <summary>
        /// Log Message
        /// </summary>
        /// <param name="Msg">String</param>
        /// <param name="bRequest">bool</param>
        private void LogMessage(String Msg)
        {
          LogMessage(Msg, false);    
        }

        /// <summary>
        /// Log Message
        /// </summary>
        /// <param name="Msg">String</param>
        /// <param name="bRequest">bool</param>
        /// <param name="bAddTimeStamp">bool</param>
        private void LogMessage(String Msg, bool bAddTimeStamp)
        {
          Logger.Log(Msg, bAddTimeStamp);
          OutputMsg_lb.Text += (Msg + "\r\n\r\n");
        }

        /// <summary>
        /// Read the request command and data and send a request command.
        /// </summary>
        /// <param name="usDeviceType">ushort</param>
        /// <param name="nDeviceId">uint</param>
        private String SendCmd(ushort usDeviceType, uint nDeviceId)
        {
          String Msg = String.Empty;
          HartIPRequest Req = null;

          do
          {
            ushort usReqCmd;
            int nDataLen;                       
           
            String ReqCmd = ReqCmd_tb.Text.Trim();
            if (ReqCmd.Length == 0)
            {
              Msg = "Request Command cannot be empty.";              
              ReqCmd_tb.Focus();
              break;
            }

            try
            {
              System.Globalization.NumberStyles ns = System.Globalization.NumberStyles.Integer;
              usReqCmd = UInt16.Parse(ReqCmd_tb.Text, ns);
            }
            catch
            {
              Msg = "Invalid Request Command.";             
              ReqCmd_tb.Focus();
              break;
            }

            if ((uint)(usReqCmd) > 65536)
            {
              Msg = "Invalid Request Command.";
              ReqCmd_tb.Focus();
              break;
            }

            // Remove all the whitespaces in request data
            String ReqData = ReqData_tb.Text.Replace(" ", "");
            nDataLen = ReqData.Length;

            if ((nDataLen > 0) && (nDataLen % 2) != 0)
            {
              Msg = "Multiple contiguous bytes must define an even number of hex digits.";             
              ReqData_tb.Focus();
              break;
            }

            // build the request
            Req = m_HartClient.BuildHartIPRequest(usReqCmd, ReqData, usDeviceType, nDeviceId);

            if (Req != null)
            {
                HartIPResponse Rsp = m_HartClient.SendHartRequest(Req);

                if (Rsp == null)
                {
                        if (m_HartClient.LastError.Length > 0)
                        {
                            Msg = m_HartClient.LastError;
                            LogMessage(Msg, true);
                            break;
                        }
                    }
                else
                {
                    if ((Rsp.Command == 77))
                    { // Command 77: "Send Command to Sub-Device"
                        Rsp.Unwrap77(); // unwrap any response that was tunneled
                    }

                    OutputMsg_lb.Text += (Req.ToString() + "\r\n\r\n");
                    OutputMsg_lb.Text += (Rsp.ToString() + "\r\n\r\n");

                    // Parse the response to readable strings
                    if (m_bParsingRsps)
                    {
                        try
                        {
                            Msg = m_ParseRsps.ParseResponse(Rsp);
                            LogMessage(Msg);
                            LogMessage("");

                            Msg = String.Empty;
                        }
                        catch (Exception e)
                        {
                            LogMessage(e.Message, true);
                        }
                    }
                }
            } // if (Req != null)

           } while (false); /* ONCE */

          return Msg;
        }

        private bool StartLogMsgs(String Filename)
        {
            bool bSuccess = false;
            try
            {
                lock (SyncRoot)
                {
                    // log messages to file
                    
                    Logger.StartLogging(Filename);
                    LogMgsBtn.Image = ToolBarImageList.Images[5];
                    LogMgsBtn.ToolTipText = "Stop Log Messages";
                    LogFileStatus.Text = "Logging messages to " + Logger.GetCurrentLogFile();
                    String StartMsg = String.Format("{0} version: {1}.", AboutBox.AssemblyProduct,
                        AboutBox.AssemblyVersion);
                    Logger.Log(StartMsg, true);                                   
                    bSuccess = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to start logging. Error: " + ex.Message, "Log Message Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);                
            }
            return bSuccess;
        }

        private void HandlePublishedCmd(object src, HartIPResponseArg RspArg)
        {
            this.BeginInvoke(new HandlerPublishedMsg(DislpayPublishedMsg),
                new Object[] { RspArg.Response });
        }

        private void DislpayPublishedMsg(HartIPResponse Rsp)
        {
            if (Rsp != null)
            {
                if (Rsp.Command == 77)
                { // Command 77: "Send Command to Sub-Device"
                    Rsp.Unwrap77(); // unwrap any response that was tunneled
                }

                lock (SyncRoot)
                {
                    String Msg = Rsp.ToString();
                    Logger.Log(Msg);
                    PublishedMsg_Tb.Text += Msg + "\r\n\r\n";
                    PublishedMsg_Tb.SelectionStart = PublishedMsg_Tb.Text.Length;
                    PublishedMsg_Tb.ScrollToCaret(); 
                    
                    if (m_bParsingRsps)
                    {
                        try
                        {
                            Msg = m_ParseRsps.ParseResponse(Rsp);
                        }
                        catch (Exception e)
                        {
                            Msg = e.Message;
                        }
                        Logger.Log(Msg);
                        PublishedMsg_Tb.Text += Msg + "\r\n\r\n";
                        PublishedMsg_Tb.SelectionStart = PublishedMsg_Tb.Text.Length;
                        PublishedMsg_Tb.ScrollToCaret();
                    }
                }
            }
        }

        /// <summary>
        /// Start parse responses using specified file.
        /// </summary>
        /// <param name="Filename">String</param>
        private void StartParseResponses(String Filename)
        {
            lock (SyncRoot)
            {
                if (m_ParseRsps == null)
                    m_ParseRsps = new ParseResponses();

                m_ParseRsps.StartParsing(Filename);
                m_bParsingRsps = true;
            }
        }

        /// <summary>
        /// Stop parsing responses.
        /// </summary>
        private void StopParsingResponses()
        {
            lock (SyncRoot)
            {
                if (m_bParsingRsps && (m_ParseRsps != null))
                    m_ParseRsps.StopParsing();

                m_bParsingRsps = false;
            }
        }
        
    }

    /// <summary>
    /// FormUntil class provides common functions
    /// </summary>
    public sealed class FormUntil
    {
      /// <summary>
      /// Bring a open file dialog
      /// </summary>
      /// <param name="bSuccess">out bool</param>
      /// <returns>String</returns>
      public static String BrowseFile(out bool bSuccess)
      {
        // Bring a open file dialog
        String location = Assembly.GetExecutingAssembly().Location;
        int index = location.LastIndexOf('\\');
        String Filename = String.Empty;
        bSuccess = false;

        OpenFileDialog BrowseFileDlg = new OpenFileDialog();
        BrowseFileDlg.InitialDirectory = location.Substring(0, index);
        BrowseFileDlg.Filter = "All files (*.*)|*.*";
        BrowseFileDlg.FilterIndex = 1;
        BrowseFileDlg.RestoreDirectory = true;
        BrowseFileDlg.CheckFileExists = true;

        if (BrowseFileDlg.ShowDialog() == DialogResult.OK)
        {
          Filename = BrowseFileDlg.FileName;
          bSuccess = true;
        }
        return Filename;
      }

      /// <summary>
      /// Bring a save file dialog
      /// </summary>
      /// <param name="bSuccess">out bool</param>
      /// <returns>String</returns>
      public static String SaveToFile(out bool bSuccess)
      {
        // Bring a save file dialog
        String location = Assembly.GetExecutingAssembly().Location;
        int index = location.LastIndexOf('\\');                  
        String Filename = String.Empty;
        bSuccess = false;
            
        SaveFileDialog saveFileDlg = new SaveFileDialog();
        saveFileDlg.InitialDirectory = location.Substring(0, index);
        saveFileDlg.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
        saveFileDlg.RestoreDirectory = true;
        saveFileDlg.CheckFileExists = false;
        saveFileDlg.Title = "Save to File";

        if (saveFileDlg.ShowDialog() == DialogResult.OK)
        {
          Filename = saveFileDlg.FileName;
          bSuccess = true;
        }
        return Filename;
      }
    }
}
