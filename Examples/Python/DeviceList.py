import clr

clr.AddReference('c:\\Programming\\DllNet\\x64\\Release\\McsUsbNet.dll')
from Mcs.Usb import CMcsUsbListNet
from Mcs.Usb import DeviceEnumNet

deviceList = CMcsUsbListNet(DeviceEnumNet.MCS_DEVICE_USB)

print("found %d devices" % (deviceList.Count))

for i in range(deviceList.Count):
    listEntry = deviceList.GetUsbListEntry(i)
    print("Device: %s   Serial: %s" % (listEntry.DeviceName,listEntry.SerialNumber))
