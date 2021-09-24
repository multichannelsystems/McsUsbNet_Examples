import time
import os
import clr
import ctypes

from System import *
clr.AddReference('System.Collections')
from System.Collections.Generic import List

from clr_array_to_numpy import asNumpyArray

clr.AddReference(os.getcwd() + '\\..\\..\\McsUsbNet\\x64\\\McsUsbNet.dll')
from Mcs.Usb import CMcsUsbListNet
from Mcs.Usb import DeviceEnumNet

from Mcs.Usb import CMeaDeviceNet
from Mcs.Usb import McsBusTypeEnumNet
from Mcs.Usb import DataModeEnumNet
from Mcs.Usb import SampleSizeNet

def OnChannelData(x, cbHandle, numSamples):
    if dataMode == DataModeEnumNet.Unsigned_16bit:
        data, frames_ret = device.ChannelBlock_ReadFramesUI16(0, callbackThreshold, Int32(0));
        np_data = asNumpyArray(data, ctypes.c_uint16)
        print(".Net numSamples: %d frames_ret: %d size: %d Data: %04x %04x Checksum: %04x %04x %04x %04x" % (numSamples, frames_ret, len(np_data), np_data[0], np_data[1], np_data[2], np_data[3], np_data[4], np_data[5]))
    else: # dataMode == DataModeEnumNet.Signed_32bit
        data, frames_ret = device.ChannelBlock_ReadFramesI32(0, callbackThreshold, Int32(0));
        np_data = asNumpyArray(data, ctypes.c_int32)
        print(".Net numSamples: %d frames_ret: %d size: %d Data: %08x %08x Checksum: %08x %08x" % (numSamples, frames_ret, len(np_data), np_data[0], np_data[1], np_data[2], np_data[3]))
    
def OnError(msg, info):
    print(msg, info)

deviceList = CMcsUsbListNet(DeviceEnumNet.MCS_DEVICE_USB)
DataModeToSampleSizeDict = {
    DataModeEnumNet.Unsigned_16bit : SampleSizeNet.SampleSize16Unsigned,
    DataModeEnumNet.Signed_32bit :  SampleSizeNet.SampleSize32Signed
}

print("found %d devices" % (deviceList.Count))

for i in range(deviceList.Count):
    listEntry = deviceList.GetUsbListEntry(i)
    print("Device: %s   Serial: %s" % (listEntry.DeviceName,listEntry.SerialNumber))

dataMode = DataModeEnumNet.Unsigned_16bit;
#dataMode = DataModeEnumNet.Signed_32bit;

device = CMeaDeviceNet(McsBusTypeEnumNet.MCS_USB_BUS);
device.ChannelDataEvent += OnChannelData
device.ErrorEvent += OnError

device.Connect(deviceList.GetUsbListEntry(0))

Samplingrate = 20000;
device.SetSamplerate(Samplingrate, 1, 0);

miliGain = device.GetGain();
voltageRanges = device.HWInfo().GetAvailableVoltageRangesInMicroVoltAndStringsInMilliVolt(miliGain);
for i in range(0, len(voltageRanges)):
    print("(" + str(i) + ") " + voltageRanges[i].VoltageRangeDisplayStringMilliVolt);

device.SetVoltageRangeByIndex(0, 0);
            
device.SetDataMode(dataMode, 0)
device.SetNumberOfChannels(2)
device.EnableDigitalIn(Boolean(False), UInt32(0))
device.EnableChecksum(True, 0)
block = device.GetChannelsInBlock(0);
print("Channels in Block: ", block)
callbackThreshold = Samplingrate // 10

if dataMode == DataModeEnumNet.Unsigned_16bit:
    mChannels = device.GetChannelsInBlock(0)
else: # dataMode == DataModeEnumNet.Signed_32bit
    mChannels = device.GetChannelsInBlock(0) // 2;
print("Number of Channels: ", mChannels)
device.SetSelectedData(mChannels, callbackThreshold * 10, callbackThreshold, DataModeToSampleSizeDict[dataMode], block)

device.StartDacq()
time.sleep(2)
device.StopDacq()
device.Disconnect()
