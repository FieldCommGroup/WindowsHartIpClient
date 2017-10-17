/*********************************************************************
 * Filename: UdpHartIPConnection.cs
 *
 * Description: Use UDP HART-IP Protocol to send HART-IP messages
 * to the Server.
 * 
 * Creation Date: 02/26/2015
 * Copyright (c) by FieldComm Group Inc.
 *********************************************************************/

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
    /// Network connection using the UDP HART-IP Protocol
    /// </summary>
    public class UdpHartIPConnection : HARTIPConnect
    {            
        private Socket m_socket;

        /// <summary>
        /// Contrutor
        /// </summary>
        public UdpHartIPConnection()
            : base()
        {
            this.m_nConnection = HARTIPConnect.UDP;
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
                        m_socket.Close();
                        m_socket = null;
                    }
                }
                catch (Exception e)
                {
                    m_Error = "Close network Exception: " + e.Message;
                    LogMsg.Instance.Log("Error, Close network: " + e.Message, true);
                }                
            }
        }
        
        /// <summary>
        /// Create a UDP socket and try to connect to network HART-IP device.
        /// </summary>
        /// <returns>bool true if it is success.</returns>
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
                     SocketType.Dgram, ProtocolType.Udp);

                    // set send and receive timeout
                    m_socket.SendTimeout = m_nSocketTimeout;
                    m_socket.ReceiveTimeout = m_nSocketTimeout;

                    // set the option the reuse the ip address
                    m_socket.SetSocketOption(SocketOptionLevel.Socket,
                        SocketOptionName.ReuseAddress, 1);
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
        /// Send a Hart-IP request to connected network HART-IP device.     
        /// </summary>
        /// <param name="Request"><see cref="HartIPRequest"/></param>      
        /// <returns>bool True if it is success to send the HART request.</returns>
        protected override bool SendRequest(HartIPRequest Request)
        {
            bool bSuccess = false;
            String TimeStamp = HartUtil.GetTimeStamp();           
            if (m_socket != null)
            {                   
                IPEndPoint endPoint = new IPEndPoint(m_HostIp, m_nSendPort); 
                m_socket.SendTo(Request.Message, endPoint);                                      
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
            BinaryReader breader = null;
            MemoryStream inStream = null;           

            do
            {
                if (m_socket == null)
                    break;

                try
                {
                    byte[] RspMsg = new byte[HARTIPMessage.MAX_HARTMSG_LENGTH];
                    int nRecvLen = 0;
                    IPEndPoint sender = new IPEndPoint(/*m_HostIp*/IPAddress.Any, 0);
                    EndPoint senderRemote = (EndPoint)sender;
                    nRecvLen = m_socket.ReceiveFrom(RspMsg, ref senderRemote);
                    if (nRecvLen == 0)
                        break;

                    String TimeStamp = HartUtil.GetTimeStamp();
                    inStream = new System.IO.MemoryStream(RspMsg);
                    breader = new System.IO.BinaryReader(inStream);
                    // set position to the beginning of the stream
                    breader.BaseStream.Position = 0;

                    int nRecvPort = ((IPEndPoint)senderRemote).Port;
                    if (m_nSendPort != nRecvPort)
                    {
                        m_nSendPort = ((IPEndPoint)senderRemote).Port;
                    }

                    Rsp = new HartIPResponse(breader, TimeStamp);                               
                }                
                finally
                {
                    if (breader != null)
                        breader.Close();
                    if (inStream != null)
                    {
                        inStream.Close();
                        inStream.Dispose();
                    }
                }
            } while (false);           

            return Rsp;
        }
    }
}
