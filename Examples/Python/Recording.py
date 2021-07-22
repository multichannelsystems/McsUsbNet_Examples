import time
import os
import clr

from System import Action
from System import *

clr.AddReference(os.getcwd() + '\\..\\..\\McsUsbNet\\x64\\\McsUsbNet.dll')
from Mcs.Usb import CMcsUsbListNet
from Mcs.Usb import DeviceEnumNet

from Mcs.Usb import CMeaDeviceNet
from Mcs.Usb import McsBusTypeEnumNet
from Mcs.Usb import DataModeEnumNet
from Mcs.Usb import SampleSizeNet

def OnChannelData(x, cbHandle, numSamples):
    data, size = device.ChannelBlock_ReadFramesUI16(0, 5000, Int32(0));
    print("size: %d numSamples: %d Data: %04x %04x Checksum: %04x %04x %04x %04x" % (size, numSamples, data[0], data[1], data[2], data[3], data[4], data[5]))
    
def OnError(msg, info):
    print(msg, info)

deviceList = CMcsUsbListNet(DeviceEnumNet.MCS_DEVICE_USB)

print("found %d devices" % (deviceList.Count))

for i in range(deviceList.Count):
    listEntry = deviceList.GetUsbListEntry(i)
    print("Device: %s   Serial: %s" % (listEntry.DeviceName,listEntry.SerialNumber))


device = CMeaDeviceNet(McsBusTypeEnumNet.MCS_USB_BUS);
device.ChannelDataEvent += OnChannelData
device.ErrorEvent += OnError

device.Connect(deviceList.GetUsbListEntry(0))

device.SetDataMode(DataModeEnumNet.Unsigned_16bit, 0)
device.SetNumberOfChannels(2)
device.EnableDigitalIn(Boolean(False), UInt32(0))
device.EnableChecksum(True, 0)
print("Channels in Block: ", device.GetChannelsInBlock(0))
device.SetSelectedData(device.GetChannelsInBlock(0), 50000, 5000, SampleSizeNet.SampleSize16Unsigned, device.GetChannelsInBlock(0))

device.StartDacq()
time.sleep(2)
device.StopDacq()
device.Disconnect()
