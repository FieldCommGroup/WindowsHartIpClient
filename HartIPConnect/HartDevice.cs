using System;
using System.Collections.Generic;
using System.Text;

namespace FieldCommGroup.HartIPConnect
{
  /// <summary>
  /// Device profile enum
  /// </summary>
  public enum DeviceProfile
  {
     /// <summary>
     /// Unknown profile
     /// </summary>
     UNKNOWN_PROFILE               = 0x00,
     /// <summary>
     /// Process
     /// </summary>
     PROCESS                       = 0x01,
     /// <summary>
     /// Discrete
     /// </summary>
     DISCRETE                      = 0x02,
     /// <summary>
     /// Hybrid
     /// </summary>
     HYBRID                        = 0x03,
     /// <summary>
     /// IO System
     /// </summary>
     IOSYSTEM                      = 0x04,
     /// <summary>
     /// Wireless Process
     /// </summary>
     WIRELESS_PROCESS              = 0x81,
     /// <summary>
     /// Wireless Discrete
     /// </summary>
     WIRELESS_DISCRETE             = 0x82,
     /// <summary>
     /// Wireless Hybrid
     /// </summary>
     WIRELESS_HYBRID               = 0x83,
     /// <summary>
     /// Wireless Gateway
     /// </summary>
     WIRELESS_GATEWAY              = 0x84,
     /// <summary>
     /// Wireless Process Adapter
     /// </summary>
     WIRELESS_PROCESS_ADAPTER     = 0x8D,
     /// <summary>
     /// Wireless Discrete Adapter
     /// </summary>
     WIRELESS_DISCRETE_ADAPTER    = 0x8E,
     /// <summary>
     /// Wireless Handheld
     /// </summary>
     WIRELESS_HANDHELD             = 0x90,
  };

  /// <summary>
  /// Device Flag Assignment
  /// </summary>
  public enum DeviceFlagAssignment
  {
      /// <summary>
      /// Undefine
      /// </summary>
      UNDEFINED = 0x00,
      /// <summary>
      /// Multi sensor field device
      /// </summary>
      MULTI_SENSOR_FIELD_DEVICE = 0x01,
      /// <summary>
      /// EEPROM control
      /// </summary>
      EEPROM_CONTROL = 0x02,
      /// <summary>
      /// Protocol bridge device
      /// </summary>
      PROTOCOL_BRIDGE_DEVICE = 0x04,
      /// <summary>
      /// IEEE 802.15.4 2.4GHz DSSS with O-QPSK Modulation
      /// </summary>
      DSSS_WITH_O_QPSK_MODULATION = 0x08,
      /// <summary>
      /// C8PSK Capable Field Device
      /// </summary>
      C8PSK_CAPABLE_FIELD_DEVICE = 0x40,
      /// <summary>
      /// C8PSK In Multi-Drop Only
      /// </summary>
      C8PSK_IN_MULIT_DROP_ONLY = 0x80
  };

  /// <summary>
  /// HartDevice contains the HART device command 0 and
  /// command 84 information
  /// </summary>
  public class HartDevice
  {   
    DeviceData m_Device = null;
    byte   m_IOCard;
    byte   m_Channel;
    DeviceProfile  m_Profile;
    DeviceFlagAssignment m_FlagAssignment;
    ushort m_usNumOfSubDevices;
    HartDevice m_Parent;

    /// <summary>
    /// HartDevice Contructor
    /// </summary>
    /// <param name="usDeviceType">ushort device type</param>
    /// <param name="uiDeviceId">uint device id</param>
    /// <param name="cUniversalRev">byte universal revision</param>
    /// <param name="Parent">HartDevice</param>
    public HartDevice(ushort usDeviceType, uint uiDeviceId, byte cUniversalRev, HartDevice Parent)
    {    
      m_Device = new DeviceData(String.Empty, usDeviceType, uiDeviceId, cUniversalRev);
      m_Parent = Parent;
      m_usNumOfSubDevices = 0;
      m_IOCard = 0;
      m_Channel = 0;
      m_Profile = DeviceProfile.UNKNOWN_PROFILE;
      m_FlagAssignment = DeviceFlagAssignment.UNDEFINED;
    }

    /// <summary>
    /// Device name
    /// </summary>
    public String Name
    {
      get { return m_Device.Name; }
      set { m_Device.Name = value; }
    }
    /// <summary>
    /// Device parent
    /// </summary>
    public HartDevice Parent
    {
      get { return m_Parent; }
    }
    /// <summary>
    /// Device type
    /// </summary>
    public ushort DeviceType
    {
      get { return m_Device.DeviceType; }
    }

    /// <summary>
    /// Device Id
    /// </summary>
    public uint DeviceId
    {
      get { return m_Device.DeviceId; }
    }
    /// <summary>
    /// Number of sub-devices
    /// </summary>
    public ushort NumOfSubDevices
    {
      get { return m_usNumOfSubDevices; }
      set { m_usNumOfSubDevices = value; }
    }
    /// <summary>
    /// IO card
    /// </summary>
    public byte IOCard
    {
      get { return m_IOCard; }
      set { m_IOCard = value; }
    }

    /// <summary>
    /// Channel
    /// </summary>
    public byte Channel
    {
      get { return m_Channel; }
      set { m_Channel = value; }
    }
    /// <summary>
    /// Universal revision
    /// </summary>
    public byte UniversalRev
    {
      get { return m_Device.UnivRev; }
    }    
    /// <summary>
    /// Device profile
    /// </summary>
    public DeviceProfile Profile
    {
      get { return m_Profile; }
      set { m_Profile = value; }
    }
    /// <summary>
    /// Flag Assignment in HART Command 0 response
    /// </summary>
    public DeviceFlagAssignment FlagAssignment
    {
        get { return m_FlagAssignment; }
        set { m_FlagAssignment = value; }
    }
    /// <summary>
    /// Check if this is a wireless device.
    /// </summary>   
    public bool IsWirelessDevice
    {
      get
      {
        return ((m_Profile == DeviceProfile.WIRELESS_PROCESS) ||
               (m_Profile == DeviceProfile.WIRELESS_DISCRETE) ||
               (m_Profile == DeviceProfile.WIRELESS_HYBRID) ||
               (m_Profile == DeviceProfile.WIRELESS_GATEWAY) ||
               (m_Profile == DeviceProfile.WIRELESS_PROCESS_ADAPTER) ||
               (m_Profile == DeviceProfile.WIRELESS_DISCRETE_ADAPTER) ||
               (m_Profile == DeviceProfile.WIRELESS_HANDHELD));
      }
    }

    /// <summary>
    /// Check if this is a wireless Gateway
    /// </summary>   
    public bool IsWirelessHIPDevice
    {
      get
      {
        return (m_Profile == DeviceProfile.WIRELESS_GATEWAY);
      }
    }
    /// <summary>
    /// Check if this is a WDI7 702 device
    /// </summary>
    public bool IsWirelessWdi7
    {
      get
      {
        return (DeviceType == 9818);
      }
    }
    /// <summary>
    /// Check if this device can manage sub-devices
    /// </summary>
    public bool IsBridgeDevice
    {
      get
      {
        return (
            IsWirelessHIPDevice || 
            Profile == DeviceProfile.IOSYSTEM ||
            FlagAssignment == DeviceFlagAssignment.PROTOCOL_BRIDGE_DEVICE
            );
      }
    }
    /// <summary>
    /// Get the profile in string format
    /// </summary>
    /// <returns>String</returns>
    public String ProfileString
    {
      get
      {
        String strProfile;
        switch (m_Profile)
        {
          case DeviceProfile.PROCESS:
            strProfile = "Process";
            break;

          case DeviceProfile.DISCRETE:
            strProfile = "Discrete";
            break;

          case DeviceProfile.HYBRID:
            strProfile = "Hybrid";
            break;

          case DeviceProfile.IOSYSTEM:
            strProfile = "IO System";
            break;

          case DeviceProfile.WIRELESS_PROCESS:
            strProfile = "Wireless Process";
            break;

          case DeviceProfile.WIRELESS_DISCRETE:
            strProfile = "Wireless Discrete";
            break;

          case DeviceProfile.WIRELESS_HYBRID:
            strProfile = "Wireless Hybrid";
            break;

          case DeviceProfile.WIRELESS_GATEWAY:
            strProfile = "Wireless Gateway";
            break;

          case DeviceProfile.WIRELESS_PROCESS_ADAPTER:
            strProfile = "Wireless Process Adapter";
            break;

          case DeviceProfile.WIRELESS_DISCRETE_ADAPTER:
            strProfile = "Wireless Discrete Adapter";
            break;

          case DeviceProfile.WIRELESS_HANDHELD:
            strProfile = "Wireless Handheld";
            break;

          case DeviceProfile.UNKNOWN_PROFILE:
          default:
            strProfile = "Unknown";
            break;
        }

        return strProfile;
      }
   }

  }
}
