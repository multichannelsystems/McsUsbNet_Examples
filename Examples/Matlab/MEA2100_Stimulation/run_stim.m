function run_stim(device)
% Connects to a MCS MEA2100-based device and starts stimulation.
%
% function run_stim(device)
%
% The device parameter is created when calling the init_stim script

    % Enumerate all connected STG devices
    deviceList = Mcs.Usb.CMcsUsbListNet(Mcs.Usb.DeviceEnumNet.MCS_STG_DEVICE);
    fprintf('Found %d STGs\n', deviceList.GetNumberOfDevices());

    for i=1:deviceList.GetNumberOfDevices()
       SerialNumber = char(deviceList.GetUsbListEntry(i-1).SerialNumber);
       fprintf('Serial Number: %s\n', SerialNumber);
    end

    % Connect to the first STG object
    status = device.Connect(deviceList.GetUsbListEntry(0));
    
    if status == 0
        % Register cleanup function. This ensures that the device
        % disconnects when the run_stim function terminates
        cleanupObj = onCleanup(@()cleanup_stim(device));
        
        % Make sure that the stimulation is stopped
        device.SendStop(uint32(1));
        
        % ElectrodeMode: emManual: electrode is permanently selected for stimulation
        device.SetElectrodeMode(electrode, Mcs.Usb.ElectrodeModeEnumNet.emManual);

        % ElectrodeDacMux: DAC to use for Stimulation
        device.SetElectrodeDacMux(electrode, 0, Mcs.Usb.ElectrodeDacMuxEnumNet.Stg1);

        % ElectrodeEnable: enable electrode for stimulation
        device.SetElectrodeEnable(electrode, 0, true);

        % BlankingEnable: false: do not blank the ADC signal while stimulation is running
        device.SetBlankingEnable(electrode, false);

        % AmplifierProtectionSwitch: false: Keep ADC connected to electrode even while stimulation is running
        device.SetEnableAmplifierProtectionSwitch(electrode, false);

        % array of amplitudes and duration
        amplitude_array = int32([10000, -10000]);
        duration_array = uint64([100000, 100000]);

        % use voltage stimulation
        device.SetVoltageMode();

        % send stimulus data to device
        device.PrepareAndSendData(0, NET.convertArray(amplitude_array, 'System.Int32'), NET.convertArray(duration_array, 'System.UInt64'), Mcs.Usb.STG_DestinationEnumNet.channeldata_voltage);

        % connect all stimulation channels to the first trigger and repeat the
        % pulse 3 times
        device.SetupTrigger(0, NET.convertArray(255, 'System.UInt32'), NET.convertArray(255, 'System.UInt32'), NET.convertArray(3, 'System.UInt32'));

        % start the first trigger
        device.SendStart(uint32(1));
    else
        disp ('connection failed');
        disp (dec2hex(status));
        disp (Mcs.Usb.CMcsUsbNet.GetErrorText(status));
    end
end