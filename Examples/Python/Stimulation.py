import time
import clr

from System import Action
from System import *

clr.AddReference('c:\\Programming\\DllNet\\x64\\Release\\McsUsbNet.dll')
from Mcs.Usb import CMcsUsbListNet
from Mcs.Usb import DeviceEnumNet

from Mcs.Usb import CMeaDeviceNet
from Mcs.Usb import McsBusTypeEnumNet
from Mcs.Usb import DataModeEnumNet
from Mcs.Usb import SampleSizeNet

def PollHandler(status, stgStatusNet, index_list)
    print status, stgStatusNet, index_list

deviceList = CMcsUsbListNet(DeviceEnumNet.MCS_DEVICE_USB)

print("found %d devices" % (deviceList.Count))

for i in range(deviceList.Count):
    listEntry = deviceList.GetUsbListEntry(i)
    print("Device: %s   Serial: %s" % (listEntry.DeviceName,listEntry.SerialNumber))


device = CStg200xDownloadNet();

device.Stg200xPollStatusEvent += PollHandler;

device.Connect(deviceList.GetUsbListEntry(0))

device.SetVoltageMode();
voltageRange = device.GetVoltageRangeInMicroVolt(0);
voltageResulution = device.GetVoltageResolutionInMicroVolt(0);

print(voltageRange, voltageResulution)

channelmap = Array[UInt32]([1,0,0,0])
syncoutmap = Array[UInt32]([1,0,0,0])
repeat = Array[UInt32]([10,0,0,0])

amplitude = Array[Int32](-100,100);
duration = Array[UInt64](100,100);

device.SetupTrigger(0, channelmap, syncoutmap, repeat)
device.ClearChannelData(0)
device.SendChannelData(0, amplitude, duration)
device.SendStart(1) // Trigger 1 
time.sleep(2)

device.Disconnect()
