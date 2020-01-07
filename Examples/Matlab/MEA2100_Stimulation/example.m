%dll = NET.addAssembly([pwd '\McsUsbNet.dll']);
import Mcs.Usb.*

deviceList = CMcsUsbListNet();
% change!
deviceList.Initialize(DeviceEnumNet.MCS_STG_DEVICE);

fprintf('Found %d STGs\n', deviceList.GetNumberOfDevices());


for i=1:deviceList.GetNumberOfDevices()
   SerialNumber = char(deviceList.GetUsbListEntry(i-1).SerialNumber);
   fprintf('Serial Number: %s\n', SerialNumber);
end

if exist('device','var')
    % 
end
stgdevice = CStg200xDownloadNet();
stgdevice.Connect(deviceList.GetUsbListEntry(0));

electrode = uint32(3);

% ElectrodeMode: emManual: electrode is permanently selected for stimulation
stgdevice.SetElectrodeMode(electrode, ElectrodeModeEnumNet.emManual);

% ElectrodeDacMux: DAC to use for Stimulation
stgdevice.SetElectrodeDacMux(electrode, 0, ElectrodeDacMuxEnumNet.Stg1);

% ElectrodeEnable: enable electrode for stimulation
stgdevice.SetElectrodeEnable(electrode, 0, true);

% BlankingEnable: false: do not blank the ADC signal while stimulation is running
stgdevice.SetBlankingEnable(electrode, false);

% AmplifierProtectionSwitch: false: Keep ADC connected to electrode even while stimulation is running
stgdevice.SetEnableAmplifierProtectionSwitch(electrode, false);

% array of amplitudes and duration
amplitude_array = int32([10000, -10000]);
duration_array = uint64([100000, 100000]);

% use voltage stimulation
stgdevice.SetVoltageMode();

% send stimlus data to device
stgdevice.PrepareAndSendData(0, NET.convertArray(amplitude_array, 'System.Int32'), NET.convertArray(duration_array, 'System.UInt64'), STG_DestinationEnumNet.channeldata_voltage);

% connect all stimulation channels to the first trigger and repeat the
% pulse 3 times
stgdevice.SetupTrigger(0, NET.convertArray(255, 'System.UInt32'), NET.convertArray(255, 'System.UInt32'), NET.convertArray(3, 'System.UInt32'));

% start the first trigger
stgdevice.SendStart(1);

stgdevice.Disconnect();
