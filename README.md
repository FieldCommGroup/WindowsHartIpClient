# HART-IP Client

HART-IP Client is a C# Windows application that uses UDP/TCP to connect to a HART device using the HART-IP protocol.  Devices include: 
* I/O devices (which may have sub-devices connected)
* Gateways
* Multiplexors
* Native HART-IP field devices

This Client enables the user to send HART commands to the connected HART-IP device and any sub-devices through the network connection.  The Client receives and displays the responses on its user interface.

## Features Included

1. Example of TCP and UDP communication with a HART-IP device.
2. Addressing sub-devices through an I/O device using HART-IP protocol.
3. A simple method of parsing HART binary messages.

## Getting Started

These instructions describe how to build, deploy and use the client.

### Prerequisites

```
1. A HART-IP device or server that supports the HART-IP protocol.
2. Visual Studio 2015 or later.
```

### Installing

Development environment

```
Install Visual Studio 2015
```

Building

```
Visual Studio: HartIpClient.Sln
```


## Usage

Launch the application.

Click the Connect to HART-IP Device button.
```
Enter the IP address of the HART-IP device.
Enter the port on the HART-IP device - 5094 is the default.
Select UDP or TCP operation.
Click Ok
```
Client responds:
```
06/16/2017 11:10:56:049 Connected to 10.6.0.3:5094
```

On launch, the Client populates the Device Tag pull-down menu with the names of the HART device and any sub-devices.  By default, it displays the name of the HART-IP device.  

Click the Get Device List button at any time to refresh the list of devices that are connected to a HART-IP I/O device.

Select one of the device names from the pull-down menu.  To send the "Identity" command 0, enter '0' into the Command field, then click Send button.  Client responds:
```
06/16/2017 11:15:30:485, Tx: Message Header: Ver: 1, MsgType: 0, MsgId: 3, MsgStatus: 0x00, TranId: 13, ByteCount: 9, Data: 82 A6 4E 0B 15 38 00 00 4C 

06/16/2017 11:15:30:485, Rx: Message Header: Ver: 1, MsgType: 1, MsgId: 3, MsgStatus: 0x00, TranId: 13, ByteCount: 33, Data: 86 A6 4E 0B 15 38 00 18 00 D0 FE 26 4E 05 07 04 01 0E 0C 0B 15 38 05 02 00 0F D0 00 26 00 26 84 69 
```

To see messages parsed for easier readability, click the Parse HART Responses button, which toggles the parsing on anf off.  It will show a File/Open dialog.  Select the Sample.hdf file that ships with this Client to use its instructions to disassemble the HART response data.

Now click the Send button again.  The Client responds:
```
06/16/2017 11:21:14:495, Tx: Message Header: Ver: 1, MsgType: 0, MsgId: 3, MsgStatus: 0x00, TranId: 15, ByteCount: 9, Data: 82 A6 4E 0B 15 38 00 00 4C 

06/16/2017 11:21:14:495, Rx: Message Header: Ver: 1, MsgType: 1, MsgId: 3, MsgStatus: 0x00, TranId: 15, ByteCount: 33, Data: 86 A6 4E 0B 15 38 00 18 00 D0 FE 26 4E 05 07 04 01 0E 0C 0B 15 38 05 02 00 0F D0 00 26 00 26 84 69 

Rx Cmd=0
Response code=0
Status Byte=D0
Expansion Code=254
Expanded Device Type=9806
# Request Preambles=5
Universal Comand Revision Level=7
Transmitter HART Revision Level=4
Software Revision=1
Hardware Revision Level / Physical Signaling Code=14
Flags=0C
Device ID=726328
Minimum # Response Preambles=5
Max # of device variables=2
Configuration Change Counter=15
Extended Field Device Status=D0
Manufacturer's ID=38
Private Label Distributor=38
Device Profile=132
```
You may alter or extend the Sample.hdf file to parse messages appropriately for the devices on your network.  See the HART Universal Command Specification and the HART Common Practice Command Specification documents for how to create HART request messages and how to interpret HART response messages.

To see the process values currently held by the device, enter 3 into the Command field and click Send again.  The Client responds:
```
06/16/2017 11:25:40:906, Tx: Message Header: Ver: 1, MsgType: 0, MsgId: 3, MsgStatus: 0x00, TranId: 16, ByteCount: 9, Data: 82 A6 4E 0B 15 38 03 00 4F 

06/16/2017 11:25:40:921, Rx: Message Header: Ver: 1, MsgType: 1, MsgId: 3, MsgStatus: 0x00, TranId: 16, ByteCount: 35, Data: 86 A6 4E 0B 15 38 03 1A 00 D0 7F A0 00 00 FB 41 00 00 00 FB 40 00 00 00 20 42 05 00 00 20 42 02 00 00 58 

Rx Cmd=3
Response code=0
Status Byte=D0
PV Current=NaN
PV Units Code=251
PV=8
SV Units Code=251
SV=2
TV Units Code=32
TV=33.25
QV Units Code=32
QV=32.5
```

All the message traffic to and from the device are automatically logged into the folder “Logs\HartClientLog_xxxx text file where xxxxx is the timestamp when the program started.

You can click the Stop Log Messages button to stop the logging.

To get a list of the expanded device type and device ID's of the devices connect to an I/O device, click the Get Sub-Device IDs button:
```
2671 7A520C
2659 6ADE38
2672 000149
2671 6AD913
2658 3B9AAD
2658 6AE030
265A 000122
266C 0027DA
``` 

## Deployment

Build Installer

```
Visual Studio: Install.vdproj
```

Run the installer
```
Setup.exe
```

We use [SemVer](http://semver.org/) for versioning. For the versions available, see the [tags on this repository](https://github.com/your/project/tags). 



## License

This project is licensed under the Apache 2.0 license - see the [LICENSE.md](LICENSE.md) file for details



