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

Filename: NetConnect_Form.cs

 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using FieldCommGroup.HartIPConnect;

namespace FieldCommGroup.HartIPClient
{
  /// <summary>
  /// Network Connection form
  /// </summary>
  public partial class NetConnect_Form : Form
  {
    String m_IPAddress;
    uint   m_nPort;
    uint   m_nConnection;
    uint   m_nDrDelayBase;
    uint   m_nDrRetries;
    uint   m_nTimeout;
    byte   m_PollingAddr = 0;

    public NetConnect_Form(uint nDrDelayBase, uint nDrRetries)
    {
      InitializeComponent();
      m_nDrDelayBase = nDrDelayBase;
      m_nDrRetries = nDrRetries;
      DrDelayBase_tb.Text = m_nDrDelayBase.ToString();
      DrRetries_tb.Text = m_nDrRetries.ToString();
      this.AcceptButton = OK_btn;
    }

    public String IPAddr
    {
      get { return m_IPAddress; }
    }
    public uint Port
    {
      get { return m_nPort; }
    }
    public uint Connection
    {
      get { return m_nConnection; }
    }
    public uint DrRetryDelayBase
    {
      get { return m_nDrDelayBase; }
    }
    public uint DrRetries
    {
      get { return m_nDrRetries; }
    }
    public uint SocketTimeout
    {
        get { return m_nTimeout; }
    }
    /// <summary>
    /// HIPDevice Polling Address
    /// </summary>
    public byte PollingAddr
    {
        get { return m_PollingAddr; }         
    }
   
    /// <summary>
    /// Call when user press OK button.
    /// Connect to HIPDevice with IP Address and Port
    /// values.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OK_btn_Click(object sender, EventArgs e)
    {
      System.Globalization.NumberStyles ns = System.Globalization.NumberStyles.Integer;
      String IPAddress = IPAdd_tb.Text.Trim();
      String Port = Port_tb.Text.Trim();
      String DrBase = DrDelayBase_tb.Text.Trim();
      String DrRetries = DrRetries_tb.Text.Trim();
      String Timeout = SocketTimeout_tb.Text.Trim();

      if (IPAddress.Length == 0)
      {
        MessageBox.Show("IP Address is empty.", "Network Connection",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
        IPAdd_tb.Focus();
        return;
      }
      if (Port.Length == 0)
      {
        MessageBox.Show("Port is empty.", "Network Connection",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
        Port_tb.Focus();
        return;
      }
	  if (Udp_RBtn.Checked)
		m_nConnection = HARTIPConnect.UDP;
      else
        m_nConnection = HARTIPConnect.TCP;     

      m_IPAddress = IPAddress;

      try
      {        
        m_nPort = UInt32.Parse(Port, ns);
        if (m_nPort == 0)
        {
          MessageBox.Show("Port cannot be zero.", "Network Connection",
                       MessageBoxButtons.OK, MessageBoxIcon.Information);
          Port_tb.Focus();
          return;
        }
      }
      catch
      {
        MessageBox.Show("Input a positive numeric value for Port.", "Network Connection",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
        Port_tb.Focus();
        return;
      }

      try
      {
        m_nTimeout = UInt32.Parse(Timeout, ns) * 1000;
        if (m_nTimeout < HARTIPConnect.MIN_SOCKET_TIMEOUT)
        {
          String Msg = String.Format("Timeout cannot less than {0} seconds.",
            HARTIPConnect.MIN_SOCKET_TIMEOUT);
          MessageBox.Show(Msg, "Network Connection",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
          SocketTimeout_tb.Focus();
          return;
        }
      }
      catch
      {
        MessageBox.Show("Input a positive numeric value for Timeout.", "Network Connection",
                         MessageBoxButtons.OK, MessageBoxIcon.Information);
        SocketTimeout_tb.Focus();
        return;
      }

      if (DrBase.Length == 0)
      {
        MessageBox.Show("DR Retry Delay Base is empty.", "Network Connection",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
        DrDelayBase_tb.Focus();
        return;
      }

      try
      {
        m_nDrDelayBase = UInt32.Parse(DrBase, ns);        
      }
      catch
      {
        MessageBox.Show("Input a positive numeric value for DR Retry Delay Base.", "Network Connection",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
        DrDelayBase_tb.Focus();
        return;
      }

      if ((m_nDrDelayBase < HARTIPMessage.DrDelayBaseMin) ||
          (m_nDrDelayBase > HARTIPMessage.DrDelayBaseMax))
      {
        MessageBox.Show("DR Retry Delay Base needs to be in " +  HARTIPMessage.DrDelayBaseMin +
            "to " +  HARTIPMessage.DrDelayBaseMax + "ms range.", "Network Connection",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
        DrDelayBase_tb.Focus();
        return;
      }

      if (DrRetries.Length == 0)
      {
        MessageBox.Show("DR Retries is empty.", "Network Connection",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
        DrRetries_tb.Focus();
        return;
      }
      try
      {
        m_nDrRetries = UInt32.Parse(DrRetries, ns);
      }
      catch
      {
        MessageBox.Show("Input a positive numeric value for DR Retries.", "Network Connection",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
        DrRetries_tb.Focus();
        return;
      }
      
      DialogResult = DialogResult.OK;
      Close();      
    }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Tcp_RBtn_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
