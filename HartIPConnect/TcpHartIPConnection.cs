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

Filename: UdpHartIPConnection.cs

Description: Use TCP HART-IP Protocol to send HART-IP messages
to the Server.

 */

 using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace FieldCommGroup.HartIPConnect
{
    /// <summary>
    /// Network connection using the TCP HART-IP Protocol
    /// </summary>
    public class TcpHartIPConnection : HARTIPConnect
    {            
        private Socket m_socket;
        private BinaryReader m_breader;
        private NetworkStream m_inStream;

        /// <summary>
        /// Contructor
        /// </summary>
        public TcpHartIPConnection()
            : base()
        {
            this.m_nConnection = HARTIPConnect.TCP;
        }

        /// <summary>
        /// Close the network connection
        /// </summary>
        public override void Close()
        {
            lock (SyncRoot)
            {
                m_bConnected = false;
                try
                {
                    StopReading();
                    if (m_socket != null)
                    {
                        LogMsg.Instance.Log("Closing the network connection.", true);
                        m_socket.Shutdown(SocketShutdown.Both);
                    }
                }
                catch (SocketException)
                {
                    ; // do nothing
                }
                catch (ObjectDisposedException)
                {
                    ; // do nothing
                }
                finally
                {
                    if (m_breader != null)
                        m_breader.Close();
                    if (m_inStream != null)
                    {
                        m_inStream.Close();
                        m_inStream.Dispose();
                    }
                    m_socket = null;
                }
            }
        }

        /// <summary>
        /// Create a TCP socket and try to connect to HIPDevice.
        /// </summary>
        /// <returns>bool if it is success</returns>
        protected override bool Connect()
        {
            bool bSuccess = false;

            do
            {
                try
                {
                    m_Error = String.Empty;
                    IPEndPoint endPoint = new IPEndPoint(m_HostIp, m_nPort);
                    m_socket = new Socket(AddressFamily.InterNetwork,
                        SocketType.Stream, ProtocolType.Tcp);

                    // set send and receive timeout
                    m_socket.SendTimeout = m_nSocketTimeout;
                    m_socket.ReceiveTimeout = m_nSocketTimeout;

                    // set the option the reuse the ip address
                    m_socket.SetSocketOption(SocketOptionLevel.Socket,
                        SocketOptionName.ReuseAddress, 1);

                    m_socket.Connect(endPoint);
                    // Set option that allows socket to close gracefully without lingering.
                    m_socket.SetSocketOption(SocketOptionLevel.Socket,
                        SocketOptionName.DontLinger, true);

                    // create a network stream object that own the socket
                    m_inStream = new NetworkStream(m_socket, true);
                    m_breader = new System.IO.BinaryReader(m_inStream);
                }
                catch (ArgumentOutOfRangeException aore)
                {
                    m_Error = "Connect Exception: " + aore.Message;
                    LogMsg.Instance.Log("Error, Connect: " + aore.Message, true);
                    break;
                }
                catch (SocketException se)
                {                    
                    m_Error = String.Format("Connect SocketException: ErrorCode:{0}. {1}",
                        se.ErrorCode, se.Message);                  
                    LogMsg.Instance.Log("Error, " + m_Error, true);
                    break;
                }
                catch (ArgumentException ae)
                {
                    m_Error = "Connect Exception: " + ae.Message;
                    LogMsg.Instance.Log("Error, Connect: " + ae.Message, true);
                    break;
                }

                m_socket.SendBufferSize = HARTIPMessage.MAX_HARTMSG_LENGTH;
                m_socket.ReceiveBufferSize = HARTIPMessage.MAX_HARTMSG_LENGTH; 
                bSuccess = true;
            } while (false); /* ONCE */

            if (!bSuccess)
            {
                m_socket.Close();
                m_socket = null;
            }
            return bSuccess;
        }

        /// <summary>
        /// Send a Hart request to connected network HART-IP device.
        /// </summary>
        /// <param name="Request"><see cref="HartIPRequest"/></param>
        /// <returns>bool True if it is success to send the HART request.</returns>
        protected override bool SendRequest(HartIPRequest Request)
        {
            bool bSuccess = false;
            String TimeStamp = HartUtil.GetTimeStamp();
            if (m_socket != null)
            {               
                m_socket.Send(Request.Message);
                Request.TimeStamp = TimeStamp;
                bSuccess = true;
            }
            else
            {
                m_Error = "SendRequest Error: Socket is already closed.";
                LogMsg.Instance.Log("Error, Socket is already closed in SendRequest.", true);
            }

            return bSuccess;
        }

        /// <summary>
        /// Receive a HART-IP Response from the network connection.
        /// </summary>
        /// <returns><see cref="HartIPResponse"/></returns>
        protected override HartIPResponse GetResponse()
        {
            HartIPResponse Rsp = null;
                           
            if (m_breader != null)
               Rsp = new HartIPResponse(m_breader, null);

            return Rsp;
        }
    }
}
