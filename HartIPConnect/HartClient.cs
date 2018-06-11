using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;
using System.Timers;


namespace FieldCommGroup.HartIPConnect
{
    /// <summary>   
    /// It uses UDP or TCP HART-IP Protocol to connect
    /// and send HART-IP messages to a network HART Device. 
    /// </summary>
    public sealed class HartClient
    {
        private static volatile HartClient m_Instance;
        private static Object SyncRoot = new Object();

        private LogMsg Logger = LogMsg.Instance;
        private HARTIPConnect m_HartIPConn = null;
        private uint m_nDrRetryDelay = HARTIPMessage.DR_DELAYRETRY;
        private uint m_nDrRetries = HARTIPMessage.DR_RETRIES;       
        private byte m_PollingAddr = 0;
        private String m_Error = String.Empty;

        private Collection<HartDevice> m_DeviceList = new Collection<HartDevice>();
        private ReadOnlyCollection<HartDevice> m_Devices;   

        /// <summary>
        /// Contructor
        /// </summary>
        private HartClient()
        {
            m_Devices = new ReadOnlyCollection<HartDevice>(m_DeviceList);
        }    

        /// <summary>
        /// Get the HartClient instance
        /// </summary>
        public static HartClient Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    lock (SyncRoot)
                    {
                        m_Instance = new HartClient();
                    }
                }
                return m_Instance;
            }
        }

        #region Properties
        /// <summary>
        /// HARTDevices property
        /// </summary>
        public ReadOnlyCollection<HartDevice> HARTDevices
        {
            get { return m_Devices; }
        }

        /// <summary>
        /// Number of the HART devices on the TCP/IP connection.
        /// </summary>
        public int DevicesCount
        {
            get { return m_DeviceList.Count; }
        }
 
        /// <summary>
        /// Root Device is the first device discovered, either a Bridge or a Native HIP device.
        /// </summary>
        public HartDevice RootDevice
        {
            get { return HARTDevices[0]; }
        }


        /// <summary>
        /// Is Connected property
        /// </summary>
        public bool IsConnected
        {
            get
            {
                bool bConnected = false;
                lock (SyncRoot)
                {
                    if (m_HartIPConn != null)
                        bConnected = m_HartIPConn.IsConnected;
                }

                return bConnected;
            }
        }
      
        /// <summary>
        /// LastActivityTime property
        /// </summary>
        public long LastActivityTime
        {
            get 
            {
                long LastActTime = 0;
                lock (SyncRoot)
                {
                     if (m_HartIPConn != null)
                         LastActTime = m_HartIPConn.LastActivityTime;
                }
                return LastActTime;
            }
        }            

        /// <summary>
        /// Delay Retry Delay property
        /// </summary>
        public uint DrRetryDelay
        {
            get { return m_nDrRetryDelay; }
            set { m_nDrRetryDelay = value; }
        }

        /// <summary>
        /// Delay Retreis property
        /// </summary>
        public uint DrRetreis
        {
            get { return m_nDrRetries; }
            set { m_nDrRetries = value; }
        }

        /// <summary>
        /// HIPDevice Polling Address property
        /// </summary>
        public byte HIPDevicePollingAddr
        {
            get { return m_PollingAddr; }
            set { m_PollingAddr = value; }
        }

        /// <summary>
        /// Last Error property
        /// </summary>
        public String LastError
        {
            get { return m_Error; }
        }
        #endregion


        /// <summary>
        /// Look up device in list of attached devices.
        /// </summary>
        public HartDevice FindDevice(uint id)
        {
            foreach (HartDevice dev in HARTDevices)
            {
                if (dev.DeviceId == id)
                    return dev;
            }

            return null;
        }

        /// <summary>
        /// Subscribe the published command notify event
        /// </summary>
        /// <param name="HandleEvent">EventHandler <see cref="HartIPResponseArg"/> event callback</param>
        /// <returns>bool true if it is success.</returns>
        /// <remarks> This will not send the wireless HART 533 command to the connected HART-IP device.
        /// Client wants to enable the Burst Through Mode in the HART 219 command needs to send one.
        /// After client sent the HART 219 'Write Burst Through Mode' to enable the Burst Through Mode
        /// command to HIPDevice and the it received a broadcast published command from the network, 
        /// it will send notify event to the subscribed clients.</remarks>
        public bool SubscribePublishedCmdNotify(EventHandler<HartIPResponseArg> HandleEvent)
        {
            bool bSuccess = false;
            lock (SyncRoot)
            {
                if (m_HartIPConn != null)
                {
                    m_HartIPConn.PublishedCmdNotify += HandleEvent;
                    bSuccess = true;
                }
            }
            return bSuccess;
        }

        /// <summary>
        /// Unsubscribe the published command notify event
        /// </summary>
        /// <param name="HandleEvent">EventHandler <see cref="HartIPResponseArg"/> the registered
        /// callback</param>
        /// <remarks>This will not send the wirless HART 219 command to disable the 
        /// 'Burst Through Mode' in the connected HIPDevice. Client wants to disable the 
        /// Burst Through Mode in the HART 219 command needs to send one.</remarks>
        public void UnSubscribePublishedCmdNotify(EventHandler<HartIPResponseArg> HandleEvent)
        {            
            lock (SyncRoot)
            {
                if (m_HartIPConn != null)                
                    m_HartIPConn.PublishedCmdNotify -= HandleEvent;                
            }            
        }

        /// <summary>
        /// Clear the built device list.
        /// </summary>
        public void ClearDeviceList()
        {
            lock (SyncRoot)
            {
                // Clear the device list
                if (m_DeviceList.Count > 0)
                    m_DeviceList.Clear();
            }
        }      
        
        /// <summary>
        ///  Get the network HART device that is
        ///  a wireless Gateway, IO System, or protocol bridge device
        ///  from devices list.
        /// </summary>
        /// <returns><see cref="HartDevice"/></returns>
        public HartDevice GetNetworkHARTIPDevice()
        {
            HartDevice Dev = null;
            HartDevice[] HDevices = null;
            int nCount;

            lock (SyncRoot)
            {
                m_Error = String.Empty;
                nCount = m_DeviceList.Count;
                if (nCount > 0)
                {
                    HDevices = new HartDevice[nCount];
                    m_DeviceList.CopyTo(HDevices, 0);
                }
            }

            if (HDevices != null)
            {
                 for (int i = 0; i < HDevices.Length; i++)
                 {
                     // find the network HART device
                     if (HDevices[i].IsWirelessHIPDevice || (HDevices[i].Profile == DeviceProfile.IOSYSTEM) ||
                         (HDevices[i].FlagAssignment == DeviceFlagAssignment.PROTOCOL_BRIDGE_DEVICE))
                     {
                         Dev = HDevices[i];
                         break;
                     }
                 }
             }

             return Dev;
        }        

        /// <summary>
        /// Close the network connection.     
        /// </summary>
        public void Close()
        {
            lock (SyncRoot)
            {
                // Close the network connection        
                if ((m_HartIPConn != null) && m_HartIPConn.IsConnected)
                {
                    m_HartIPConn.Close();
                    Logger.Log("Disconnected the HART-IP device network connection.", true);
                }

                // Clear the device list
                if (m_DeviceList.Count > 0)
                    m_DeviceList.Clear();
            }
        }

        /// <summary>
        /// If the HART-IP devices is a wireless Gateway, IO System, or Protocol bridge device,
        /// it gets all devices that include Gateway, IO System, or Protocol bridge device
        /// into devices list for the network connection.  Otherwise, just has the network 
        /// HART-IP device in the list.
        /// </summary>
        /// <returns>bool returns true if success. Otherwise, false.</returns>
        public bool GetDeviceList()
        {
            bool bSuccess = false;

            do
            {
                lock (SyncRoot)
                {
                    m_Error = String.Empty;
                    // Check if the device list is filled.
                    if (m_DeviceList.Count > 0)
                    {            
                        break;
                    }

                    // Check if it has a connection
                    if ((m_HartIPConn != null) && m_HartIPConn.IsConnected)
                    {
                        // poll for the HART Device
                        HartDevice Dev = null;
                        for (byte polladdr = 0; polladdr < 64; polladdr++)
                        {
                            Dev = DiscoverHartDevice(polladdr);
                            if (Dev != null)
                            { // presume only one device at this HART-IP address
                                m_PollingAddr = polladdr;
                                break;      
                            }
                        }

                        if (Dev != null)
                        {
                            // this is the IO device
                            
                            m_DeviceList.Add(Dev);

                            // Get the HART device's Tag
                            String Tag = GetDeviceTag(Dev.DeviceType, Dev.DeviceId,
                                         (byte)HARTIPMessage.CMD_READ_LONG_TAG);             
                                                               
                           if (String.IsNullOrEmpty(Tag))              
                           {
                               ushort usDeviceType = Dev.DeviceType;
                               uint nDeviceId = Dev.DeviceId;
                               Tag = String.Format("Unknown (00-1B-1E-{0:X2}-{1:X2}-{2:X2}-{3:X2}-{4:X2})",
                                     new object[] {(usDeviceType >> 8),
                                                   (usDeviceType & 0x0ff),
                                                   (nDeviceId >> 16 & 0x0ff),
                                                   (nDeviceId >> 8 & 0x0ff),
                                                   (nDeviceId & 0x0ff)});
                           }

                           Dev.Name = Tag;
            
                           // check if the device has sub-devices, or Flag assignment is Protocol Bridge Device
                           if (Dev.IsBridgeDevice)
                           {
                               // Get the HART Device's IO info
                               bSuccess = GetHartDeviceIO(Dev);

                               if (bSuccess && (Dev.NumOfSubDevices > 0))
                               {
                                   // get sub-devices
                                   bSuccess = GetConnectedDevices(Dev);
                               }
                           }
                           else
                               bSuccess = true;
                      }
                  }
              }
          } while (false); /* ONCE */

          return bSuccess;
      }

      /// <summary>
      /// Connect to HART Device that is in the network 
      /// </summary>
      /// <param name="Server">String HART Device's hostname or IP Address</param>
      /// <param name="nPort">uint</param>
      /// <param name="nConnection">uint</param>
      /// <param name="nTimeout">int Socket timeout</param>     
      /// <returns>bool true if it is success.</returns>
      /// <remarks>     
      /// <para>Inactivity Close Time 600000 milliseconds will be used for 
      /// connected session inactivity timeout.
      /// </para>
      /// <para>It will initiate session with the network HART device if it is
      /// successful connected to that Device.</para>
      /// </remarks>
      public bool Connect(String Server, uint nPort, uint nConnection,
                          int nTimeout)
      {
          bool bSuccess = false;
          String Conn, Msg;
          if (nConnection == HARTIPConnect.UDP)
              Conn = "Udp";
          else
              Conn = "Tcp";

          do
          {
              lock (SyncRoot)
              {
                  m_Error = String.Empty;
                  if (m_HartIPConn != null)
                  {
                      // close the existing connection
                      Close();
                  }

                  Msg = String.Format("Connecting and initialing Session to {0} Port:{1} {2}",
                        Server, nPort, Conn);
                  Logger.Log(Msg, true);

                  if (nConnection == HARTIPConnect.TCP)
                      m_HartIPConn = new TcpHartIPConnection();
                  else
                      m_HartIPConn = new UdpHartIPConnection();

                  bSuccess = m_HartIPConn.Connect(Server, (int)nPort, nTimeout,
                      HARTIPMessage.INACTIVITY_CLOSE_TIME);

                  if (!bSuccess)
                  {
                        m_Error = m_HartIPConn.LastError;
                  }
              }
          } while (false); /* ONCE */

          return bSuccess;
      }

        /// <summary>
        /// Disconnect - end HART-IP session
        /// </summary>
        public void Disconnect()
        {
            HARTMsgResult Result = new HARTMsgResult();
            m_HartIPConn.CloseSession(Result);
        }

        /// <summary>
        /// Send KeepAlive message
        /// </summary>
        public void KeepAlive()
        {
            lock (SyncRoot)
            {
                if (m_HartIPConn.IsConnected)
                {
                    HARTMsgResult Result = new HARTMsgResult();
                    try
                    {
                        m_HartIPConn.SendKeepAlive(Result);
                    }
                    catch (Exception e)
                    {
                        Result.AddMessage(e.Message, false, true);
                        m_Error = "KeepAlive: " + e.Message;
                        Logger.Log("Error, " + m_Error, true);
                    }
                }
            }
        }

        /// <summary>
        /// Build a HartIP Request using the specified request command, request message,
        /// device type, and device id. It will have frame, device address, command, byte count, 
        /// data, and checksum bytes in the returned HartIPRequest.Command.
        /// </summary>
        /// <param name="usReqCmd">ushort Request command</param>
        /// <param name="ReqMsg">String Request message in Hex string</param>
        /// <param name="usDeviceType">ushort Device Type</param>
        /// <param name="nDeviceId">uint Device ID</param>
        /// <returns><see cref="HartIPRequest"/></returns>
        public HartIPRequest BuildHartIPRequest(ushort usReqCmd, String ReqMsg, ushort usDeviceType,
        uint nDeviceId)
      {
          String Msg;
          HartIPRequest HRequest = null;
          int nDataLen = 0;

          if (m_HartIPConn == null)
              throw new Exception("Call Connect to initialize the network connection first.");

          if ((uint)(usReqCmd) > 65536)
          {
              Msg = "Invalid HARTIP Request Command.";
              throw new ArgumentException(Msg);
          }

          if (!String.IsNullOrEmpty(ReqMsg))
          {
              ReqMsg = ReqMsg.Replace(" ", "");
              nDataLen = ReqMsg.Length;
              if ((nDataLen > 0) && (nDataLen % 2) != 0)
              {
                  Msg = "Multiple contiguous bytes must define an even number of hex digits.";
                  throw new ArgumentException(Msg);
              }
          }

          // Check if it is extended command
          bool bExtendedCmd = (usReqCmd > HARTIPMessage.MAX_SINGLE_BYTE_CMD) ? true : false;
          byte cCmd = (byte)(bExtendedCmd ? HARTIPMessage.CMD_EXTENDED_CMD : usReqCmd);
          byte cbyteCount = 9;
          byte cDataByteCount = 0;
          int nIndex = 0;
          int nSubCmdByteCountIndex;
          // Request bytes
          byte[] Data = new byte[HARTIPMessage.MAX_REQUEST_MSG_LEN];
          Array.Clear(Data, 0, Data.Length);

          if (nDataLen > 0)
          {
              cbyteCount += (byte)(nDataLen / 2);
              cDataByteCount = (byte)(nDataLen / 2);
          }

          // form a request
          Data[nIndex++] = 0x82;  // long frame
          Data[nIndex++] = (byte)(((usDeviceType >> 8) & 0x3F) | 0x80);
          Data[nIndex++] = (byte)(usDeviceType & 0x0ff);
          Data[nIndex++] = (byte)((nDeviceId >> 16) & 0x0ff);
          Data[nIndex++] = (byte)((nDeviceId >> 8) & 0x0ff);
          Data[nIndex++] = (byte)(nDeviceId & 0x0ff);
          Data[nIndex++] = cCmd;  // cmd
          nSubCmdByteCountIndex = nIndex;
          // skip the byte count
          nIndex += 1;

          // if it is extended cmd, put the cmd two bytes in data field first
          if (bExtendedCmd)
          {
              Data[nIndex++] = (byte)((usReqCmd >> 8) & 0x0ff);
              Data[nIndex++] = (byte)(usReqCmd & 0x0ff);
              cbyteCount += 2;
              cDataByteCount += 2;
          }

          // set the data byte count value
          Data[nSubCmdByteCountIndex] = cDataByteCount;

          int n = 0;
          byte cY, cW;
          while (n < nDataLen)
          {
              if (HartUtil.HexCharToNibble(ReqMsg[n], out cY))
              {
                  if (HartUtil.HexCharToNibble(ReqMsg[n + 1], out cW))
                  {
                      Data[nIndex++] = (byte)(cY * 16 + cW);
                      n += 2;
                  }
                  else
                  {
                      Msg = String.Format("The character: '{0}' is not allowed in the request data byte value.",
                            ReqMsg[n + 1]);
                      throw new ArgumentException(Msg);
                  }
              }
              else
              {
                  Msg = String.Format("The character: '{0}' is not allowed in the request data byte value.",
                        ReqMsg[n]);
                  throw new ArgumentException(Msg);
              }
          }

			// tunnel messages to attached devices
            if (RootDevice.DeviceType != usDeviceType && RootDevice.DeviceId != nDeviceId && RootDevice.IsBridgeDevice)
            { // message is routed into an attached device

                // wrap the message with command 77 to tunnel to attached device
                byte[] Data77 = new byte[HARTIPMessage.MAX_REQUEST_MSG_LEN];
                Array.Clear(Data77, 0, Data.Length);

                // command 77 request data
                HartDevice dev = FindDevice(nDeviceId);
                Data77[0] = dev.IOCard;
                Data77[1] = dev.Channel;
                Data77[2] = 5;  // transmit preamble count
                Array.Copy(Data, 0, Data77, 3, nIndex);
                nIndex += 3;

                // send the command 77 to the root device (a gateway or IO)
                string hex = BitConverter.ToString(Data77).Replace("-", string.Empty);
                hex = hex.Substring(0, nIndex*2);
                return BuildHartIPRequest(77, hex, RootDevice.DeviceType, RootDevice.DeviceId);
            }

          // Get check sum byte and send the request
          Data[nIndex] = HartUtil.GetCheckSum(Data, (byte)(cbyteCount - 1));
          HRequest = HartIPRequest.HartCommandRequest(m_HartIPConn.TransactionId,
                     Data, cbyteCount);

          return HRequest;
      }


      /// <summary>
      /// Send a Hart Request and handle Delay Retry.
      /// If no more dr retry, it sends nerwork HART device a flush Dr cmd.
      /// </summary>
      /// <param name="Request">byte[] a request byte array</param>
      /// <param name="ByteCount">ushort request array byte count</param>    
      /// <param name="Timeout">int request timeout in ms</param>        /// <returns><see cref="HartIPResponse"/></returns>
        public HartIPResponse SendHartRequest(byte[] Request, ushort ByteCount, int Timeout = HARTIPConnect.USE_SOCKET_TIMEOUT_DEFAULT)
      {
          HartIPRequest Req = HartIPRequest.HartCommandRequest(m_HartIPConn.TransactionId, Request, ByteCount, Timeout);
          return SendHartRequest(Req);
      }

      /// <summary>
      /// Send a Hart Request and it handles Delay Retry.
      /// If no more dr retry, it sends nerwork HART device a flush Dr cmd.
      /// </summary>
      /// <param name="Request"><see cref="HartIPRequest"/></param>     
      /// <returns><see cref="HartIPResponse"/></returns>
      public HartIPResponse SendHartRequest(HartIPRequest Request)
      {
          HartIPResponse Rsp = null;         

          if (Request == null)
              throw new ArgumentException("HartIPRequest is null");

          if ((m_HartIPConn == null) || !m_HartIPConn.IsConnected)
              throw new Exception("Call Connect to initialize the network connection first.");
          
          lock (SyncRoot)
          {
              m_Error = String.Empty;             

              // send the first request
              Rsp = SendRequest(Request);

              if (Rsp != null)
                Logger.Log(Rsp.ToString());

              if ((Rsp != null) &&
                  ((Rsp.ResponseCode == HARTIPMessage.RSP_DR_INITIATE) ||
                  (Rsp.ResponseCode == HARTIPMessage.RSP_DR_CONFLICT) ||
                  (Rsp.ResponseCode == HARTIPMessage.RSP_DR_RUNNING) ||
                  (Rsp.ResponseCode == HARTIPMessage.RSP_DEVICE_BUSY)))
              {
                  if (m_nDrRetries > 0)
                  {
                      Rsp = null;
                      // Create a request task to store the request and retries values.
                      RequestTask ReqTask = new RequestTask(Request, m_nDrRetries,
                          m_nDrRetryDelay);

                      // Create a request worker to do the task with the callback to handles the retries.
                      RequestWorker ReqWorker = new RequestWorker(ReqTask,
                          new RequestWorker.HandleSendRequestDelegate(HandleSendHARTIPRequest));
                      try
                      {
                          // start the task
                          ReqWorker.Perform();
                          Rsp = ReqTask.HartIPRsp;
                      }
                      catch (UserCancelException)
                      {
                          // user canceled action
                          m_Error = "User canceled sending retries command.";                         
                      }
                  }
                  else
                      SendFlushDrCmd();
              }                  
          }

//           if (Rsp.Command == 77)
//           { // unwrap the HART PDU in the response data
//                 Rsp
//           }

          return Rsp;
      }

      /// <summary>
      /// Send a Hart Request
      /// </summary>
      /// <param name="usDeviveType">ushort</param>
      /// <param name="nDeviceId">uint</param>
      /// <param name="cCmd">byte</param>    
      /// <returns><see cref="HartIPResponse"/></returns>
        /// <remarks>It will create long frame, device type, device id,
        /// cmd and checksum bytes array for the request command.</remarks>
      private HartIPResponse SendHartRequest(ushort usDeviveType, uint nDeviceId,
          byte cCmd)
      {
          byte[] Params = new byte[9];

          // Create request cmd
          Params[0] = 0x82;  // long frame
          Params[1] = (byte)(((usDeviveType >> 8) & 0x3F) | 0x80);
          Params[2] = (byte)(usDeviveType & 0x0ff);
          Params[3] = (byte)((nDeviceId >> 16) & 0x0ff);
          Params[4] = (byte)((nDeviceId >> 8) & 0x0ff);
          Params[5] = (byte)(nDeviceId & 0x0ff);
          Params[6] = cCmd;  // cmd byte
          Params[7] = 0x00; // byte count
          Params[8] = HartUtil.GetCheckSum(Params, 8);  // get check sum byte

          return SendHartRequest(Params, 9);
      }

      /// <summary>
      /// Callback from the SendHartRequest() to send HART-IP request.
      /// </summary>
      /// <param name="ReqTask"><see cref="RequestTask"/></param>     
      private void HandleSendHARTIPRequest(RequestTask ReqTask)
      {
          if (m_HartIPConn == null)
              throw new ArgumentException("No Network connection.");

          do
          {
              // send the request in the RequestTask
              ReqTask.HartIPRsp = SendRequest(ReqTask.HartIPReq);
              Logger.Log(ReqTask.HartIPRsp.ToString());

              // check if it need retries
              if ((ReqTask.HartIPRsp != null) &&
                  ((ReqTask.HartIPRsp.ResponseCode == HARTIPMessage.RSP_DR_INITIATE) ||
                   (ReqTask.HartIPRsp.ResponseCode == HARTIPMessage.RSP_DR_CONFLICT) ||
                   (ReqTask.HartIPRsp.ResponseCode == HARTIPMessage.RSP_DR_RUNNING) ||
                   (ReqTask.HartIPRsp.ResponseCode == HARTIPMessage.RSP_DEVICE_BUSY)))
              {                                 

                  // increment the current retry value
                  ReqTask.nDrRetries += 1;

                  // if done the retries, send the flush dr cmd to gateway
                  if (ReqTask.nDrRetries >= ReqTask.nNumberOfRetries)
                  {
                      ReqTask.bIsCompleted = true;
                      SendFlushDrCmd();
                      break;
                  }

                  // double the retry delay value if the delay interval is not exceed DrDelayBaseMax
                  if ((ReqTask.nDrRetries > 1) &&
                      (ReqTask.nRetryDelay < m_nDrRetryDelay))
                      ReqTask.nRetryDelay *= 2;

                  if (ReqTask.nRetryDelay > m_nDrRetryDelay)
                      ReqTask.nRetryDelay = m_nDrRetryDelay;
              }
              else
                  ReqTask.bIsCompleted = true;

          } while (false);
      }

      /// <summary>
      /// Send HART-IP request
      /// </summary>
      /// <param name="Request"><see cref="HartIPRequest"/></param>
      /// <returns><see cref="HartIPResponse"/></returns>
      private HartIPResponse SendRequest(HartIPRequest Request)
      {
          HartIPResponse Rsp = null;

          do
          {
              try
              {
                  if (m_HartIPConn != null)
                  {
                      MsgResponse MsgRsp = new MsgResponse(Request.m_Timeout);
                      if (!m_HartIPConn.SendHartRequest(Request, MsgRsp))
                      {
                          m_Error = m_HartIPConn.LastError;
                          break;
                      }

                      // wait for the object return or timeout
                      if (MsgRsp.GetResponse())
                          Rsp = MsgRsp.ResponseMsg;
                  }

                  if (Rsp == null)
                  {
                      if ((m_HartIPConn != null) && (m_HartIPConn.LastError.Length > 0))
                          m_Error = m_HartIPConn.LastError;
                      else
                          m_Error = "Waiting Hart-IP Request waiting response timeout.";

                      Logger.Log(m_Error, false);
                      break;
                  }
              }
              catch (ThreadInterruptedException Ex)
              {
                  m_Error = "Send Hart Request Exception: " + Ex.Message;
                  Logger.Log(m_Error, true);
              }
              catch (ThreadStateException TSEx)
              {
                  m_Error = "Send Hart Request Exception: " + TSEx.Message;
                  Logger.Log(m_Error, true);
              }
          } while (false); /* ONCE */

          return Rsp;
      }     
      
      /// <summary>
      /// Send 13 or 20 command to get HART device tag.
      /// </summary>
      /// <param name="usDeviceType">ushort</param>
      /// <param name="nDeviceId">uint</param>
      /// <param name="cCmd">byte</param>
        /// <returns>string device's Tag</returns>
        private string GetDeviceTag(ushort usDeviceType, uint nDeviceId, byte cCmd)
        {
            string Tag = String.Empty;            
          HartIPResponse Rsp = SendHartRequest(usDeviceType, nDeviceId, cCmd);

          if (Rsp != null)
          {
              try
              {     
                  Tag = HartUtil.GetHARTDeviceTag(Rsp);
                  Tag = Tag.TrimEnd();
              }
              catch (Exception e)
              {
                  m_Error = "GetDeviceTag Exception: " + e.Message;
                  Logger.Log(m_Error, true);                 
              }
          }

          return Tag;
      }   
    
      /// <summary>
      /// Get the network HART Device IO info.
      /// </summary>
      /// <param name="Dev"><see cref="HartDevice"/></param>
      /// <returns>bool returns true if success. Otherwise, false.</returns>
      private bool GetHartDeviceIO(HartDevice Dev)
      {
          byte cCmd;
          ushort usNumOfDevices = 0;
          bool bSuccess = false;

          if (Dev != null)
          {
              cCmd = (byte)((Dev.UniversalRev >= 7) ? HARTIPMessage.CMD_READ_IO_SYSTEM_CAPABILITIES : 
                  HARTIPMessage.CMD_READ_HARTPORT_PARAMETERS);
              HartIPResponse Rsp = SendHartRequest(Dev.DeviceType, Dev.DeviceId, cCmd);
              if (Rsp != null)
              {
                  try
                  {
                      usNumOfDevices = HartUtil.GetHIPDeviceNumOfDevices(Rsp);
                      bSuccess = true;
                  }
                  catch (Exception e)
                  {
                      m_Error = "GetHartDeviceIO Exception: " + e.Message;
                      Logger.Log(m_Error, true);                       
                  }

                  if (bSuccess && (usNumOfDevices > 0))
                  {
                        // Hart7 device cmd 74 rsp num of devices include HART-IP device itself.
                      if (Dev.UniversalRev >= 7)
                          Dev.NumOfSubDevices = (ushort)(usNumOfDevices - 1);
                      else
                          Dev.NumOfSubDevices = usNumOfDevices;                     
                  }
              }
         }

         return bSuccess;
      }

      /// <summary>
      /// Get the network HART Device connected devices.
      /// </summary>
      /// <param name="Dev"><see cref="HartDevice"/></param>
      /// <returns>bool returns true if success. Otherwise, false.</returns>
      private bool GetConnectedDevices(HartDevice Dev)
      {
          bool bSuccess = false;

          if (Dev != null)
          {
              if (Dev.UniversalRev >= 7)
              {
                  bSuccess = SendCmd84s(Dev);
              }
          }
          return bSuccess;
      }
        
      /// <summary>
      /// Send short frame command 0 to discover the HART Device that is
      /// connected to the network.     
      /// </summary>      
      /// <returns><see cref="HartDevice"/></returns>
      private HartDevice DiscoverHartDevice(byte polladdr)
      {
          HartIPResponse Rsp = null;
          byte[] Params = new byte[5];
          HartDevice Dev = null;

          // short frame
          Params[0] = 0x02;
          // Device's polling address in short frame
          Params[1] = (byte)((polladdr & 0x3F) | 0x80);
          // cmd 0
          Params[2] = HARTIPMessage.CMD_READ_UID;
          // byte count is zero
          Params[3] = 0x00;
          // get check sum byte
          Params[4] = HartUtil.GetCheckSum(Params, 4);

          Rsp = SendHartRequest(Params, 5, HARTIPConnect.IO_SOCKET_TIMEOUT);
          if (Rsp != null)
          {
              try
              {
                  Dev = HartUtil.CreateHartDevice(Rsp, null);
              }
              catch (Exception e)
              {
                  m_Error = "DiscoverHartDevice Exception: " + e.Message;
                  Logger.Log(m_Error, true);                  
              }
          }

          return Dev;
      }

      /// <summary>
      /// Send HART-IP Device the command 84s.
      /// </summary>
      /// <param name="Dev"><see cref="HartDevice"/></param>
      /// <returns>bool returns true if success. Otherwise, false.</returns>
      private bool SendCmd84s(HartDevice Dev)
      {
          bool bSuccess = false;
          ushort usStartDeviceIndex = 1;
          ushort usNumOfSubDevices = 0;

          if (Dev != null)
          {
              usNumOfSubDevices = Dev.NumOfSubDevices;
              while (usStartDeviceIndex <= usNumOfSubDevices)
              {
                  HartDevice HDevice = SendCmd84(Dev, usStartDeviceIndex);
                  if (HDevice != null)
                  {
                      m_DeviceList.Add(HDevice);
                      usStartDeviceIndex += 1;

                      // found all the HART device's sub-devices
                      if (usStartDeviceIndex > usNumOfSubDevices)
                      {
                          bSuccess = true;
                          break;
                      }
                  }
                  else
                      break;
               }
          }

          return bSuccess;
      }

      /// <summary>
      /// Send HART device a command 84.
      /// </summary>
      /// <param name="Dev"><see cref="HartDevice"/></param>
      /// <param name="usDeviceIndex">ushort</param>
      /// <returns><see cref="HartDevice"/></returns>
      private HartDevice SendCmd84(HartDevice Dev, ushort usDeviceIndex)
      {
          HartDevice HDevice = null;
          ushort usDeviceType;
          uint nDeviceId;          
          byte[] Req = new byte[11];
          HartIPResponse Rsp = null;

          if (Dev != null)
          {
              usDeviceType = Dev.DeviceType;
              nDeviceId = Dev.DeviceId;

              // send cmd to HART Device get the device list
              Req[0] = 0x82;  // long frame
              Req[1] = (byte)(((usDeviceType >> 8) & 0x3F) | 0x80);
              Req[2] = (byte)(usDeviceType & 0x0ff);
              Req[3] = (byte)((nDeviceId >> 16) & 0x0ff);
              Req[4] = (byte)((nDeviceId >> 8) & 0x0ff);
              Req[5] = (byte)(nDeviceId & 0x0ff);
              Req[6] = HARTIPMessage.CMD_READ_SUB_DEVICE_IDENTITY;  // cmd byte
              Req[7] = 2; // byte count
              Req[8] = (byte)((usDeviceIndex >> 8) & 0x0ff);
              Req[9] = (byte)(usDeviceIndex & 0x0ff);
              Req[10] = HartUtil.GetCheckSum(Req, 10);  // get check sum byte

              Rsp = SendHartRequest(Req, 11);
              if (Rsp != null)
              {
                  byte cDataCount = Rsp.DataCount;
                  if ((Rsp.ResponseCode != HARTIPMessage.RSP_SUCCESS) &&
                      (Rsp.Command != HARTIPMessage.CMD_READ_SUB_DEVICE_IDENTITY) ||
                      (cDataCount < 44))
                  {
                      m_Error = String.Format("{0} Received Cmd 84 invalid response from HART device.\r\n{1}",
                          HartUtil.GetTimeStamp(), Rsp.ToString());
                      Logger.Log(m_Error);                     
                  }
                  else
                  {                                      
                      byte cIOCard, cChannel, cUniversalCmdRev;
                      byte[] Data = new byte[cDataCount];
                      Data = Rsp.Data;
                      int nIndex = 2;
                      ushort usSubDeviceType;
                      uint nSubDeviceId;
                      String Tag = String.Empty;

                      cIOCard = Data[nIndex++];
                      cChannel = Data[nIndex++];
                      // skip the MfgId (2 bytes)
                      nIndex += 2;

                      // get the device type
                      usSubDeviceType = (ushort)((Data[nIndex] << 8) + (Data[nIndex + 1] & 0x0ff));
                      nIndex += 2;

                     // get the device id
                     nSubDeviceId = (uint)((Data[nIndex] << 16) + (Data[nIndex + 1] << 8) +
                                    (Data[nIndex + 2] & 0x0ff));
                     nIndex += 3;

                     // get the universal cmd rev
                     cUniversalCmdRev = Data[nIndex++];

                    // get the tag
                    for (int i = nIndex; i < (nIndex + HARTIPMessage.MAX_LONG_TAG_LENGTH); i++)
                    {
                        char ch = (char)(Data[i] & 0x00ff);
                        if (ch == 0)
                            break;

                        Tag += ch;
                    }

                    Tag = Tag.TrimEnd();
                    // if tag is empty, set it to 'Unknown' with its device type and device id similar to MAC address
                    if (Tag.Length == 0)
                    {
                        String strOUI = (cUniversalCmdRev >= 7) ? "00-1B-1E" : "00-00-00";

                        Tag = String.Format("Unknown ({0}-{1:X2}-{2:X2}-{3:X2}-{4:X2}-{5:X2})",
                              new object[] {strOUI,
                                            (usSubDeviceType >> 8),
                                            (usSubDeviceType & 0x0ff),
                                            (nSubDeviceId >> 16 & 0x0ff),
                                            (nSubDeviceId >> 8 & 0x0ff),
                                            (nSubDeviceId & 0x0ff)});
                    }

                    // Create sub-device
                    HDevice = new HartDevice(usSubDeviceType, nSubDeviceId, cUniversalCmdRev, Dev);
                    HDevice.Name = Tag;
                    HDevice.IOCard = cIOCard;
                    HDevice.Channel = cChannel;
                }
            }
        }

        return HDevice;
      }       

      /// <summary>
      /// Send HART Device a flush dr command.
      /// </summary>
      internal void SendFlushDrCmd()
      {
          HartDevice Dev = GetNetworkHARTIPDevice();           
          if (Dev != null)
          {
              SendHartRequest(Dev.DeviceType, Dev.DeviceId,
                  HARTIPMessage.CMD_FLUSH_DELAYED_RESPONSES);
          }
      }

      /// <summary>
      /// Reconnect the network connection.
      /// </summary>
      /// <returns>bool true if it is success.</returns>
      /// <remarks>It will clear the discovered devices in the device list and
      /// close the opened network before reconnect it again.  Call 'GetDeviceList"
      /// method after this to discover devices again.</remarks>
      public bool ReconnectNetwork()
      {
            bool bSuccess = false;

            Disconnect();

            do
            {
              lock (SyncRoot)
              {
                  if (m_HartIPConn == null)
                  {
                      m_Error = "ReconnectNetwork: No network connection initialized.";                      
                      Logger.Log("Error, " + m_Error, true);
                      break;
                  }

                  try
                  {
                      Logger.Log("Info, Reconnecting the network...", true);

                      // Clear the device list
                      if (m_DeviceList.Count > 0)
                          m_DeviceList.Clear();

                      if (m_HartIPConn.IsConnected)
                      {
                          m_HartIPConn.Close();
                      }
                      // close the network connection
                      bSuccess = m_HartIPConn.Reconnect();
                  }
                  catch (Exception ex)
                  {
                      m_Error = "ReconnectNetwork Exception: " + ex.Message;
                      Logger.Log("Error, " + m_Error, true);
                  }
              }
          } while (false); /* ONCE */
          return bSuccess;
      }
     
    }

}
