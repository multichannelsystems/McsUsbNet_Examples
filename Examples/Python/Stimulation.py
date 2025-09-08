import time
import os
import clr

from System import Action
from System import *

clr.AddReference(os.getcwd() + r'\..\..\McsUsbNet\x64\McsUsbNet.dll')
from Mcs.Usb import CMcsUsbListNet
from Mcs.Usb import DeviceEnumNet

from Mcs.Usb import CStg200xDownloadNet
from Mcs.Usb import McsBusTypeEnumNet
from Mcs.Usb import STG_DestinationEnumNet

def PollHandler(status, stgStatusNet, index_list):
    print('%x %s' % (status, str(stgStatusNet.TiggerStatus[0])))

deviceList = CMcsUsbListNet(DeviceEnumNet.MCS_DEVICE_USB)

print("found %d devices" % (deviceList.Count))

for i in range(deviceList.Count):
    listEntry = deviceList.GetUsbListEntry(i)
    print("Device: %s   Serial: %s" % (listEntry.DeviceName,listEntry.SerialNumber))


device = CStg200xDownloadNet();

device.Stg200xPollStatusEvent += PollHandler;

device.Connect(deviceList.GetUsbListEntry(0))

voltageRange = device.GetVoltageRangeInMicroVolt(0);
voltageResulution = device.GetVoltageResolutionInMicroVolt(0);
currentRange = device.GetCurrentRangeInNanoAmp(0);
currentResolution = device.GetCurrentResolutionInNanoAmp(0);

print('Voltage Mode:  Range: %d mV  Resolution: %1.2f mV' % (voltageRange/1000, voltageResulution/1000.0))
print('Current Mode:  Range: %d uA  Resolution: %1.2f uA' % (currentRange/1000, currentResolution/1000.0))

channelmap = Array[UInt32]([1,0,0,0])
syncoutmap = Array[UInt32]([1,0,0,0])
repeat = Array[UInt32]([10,0,0,0])

amplitude = Array[Int32]([-100,100]);
duration = Array[UInt64]([10000,10000]);

device.SetupTrigger(0, channelmap, syncoutmap, repeat)
device.SetVoltageMode();
device.PrepareAndSendData(0, amplitude, duration, STG_DestinationEnumNet.channeldata_voltage)
device.SendStart(1) 
time.sleep(10)

device.Disconnect()
