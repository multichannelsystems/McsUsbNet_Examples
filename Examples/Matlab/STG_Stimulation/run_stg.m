function run_stg(device)
% Connects to a MCS STG device and starts stimulation.
%
% function run_stg(device)
%
% The device parameter is created when calling the init_stg script

    % Enumerate all connected STG devices
    deviceList = Mcs.Usb.CMcsUsbListNet(DeviceEnumNet.MCS_STG_DEVICE);
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
        cleanupObj = onCleanup(@()cleanup_stg(device));
        
        % use voltage stimulation
        device.SetVoltageMode();

        % array of amplitudes and duration
        amplitude = int32([+2000000 -2000000]);  % Amplitude in uV
        duration = uint64([100000 100000]);  % Duration in us

        amplitudeNet = NET.convertArray(amplitude, 'System.Int32');
        durationNet  = NET.convertArray(duration, 'System.UInt64');

        % send stimulus data to device
        device.PrepareAndSendData(0, amplitudeNet, durationNet, Mcs.Usb.STG_DestinationEnumNet.channeldata_voltage);
        
        % start stimulation
        device.SendStart(1);

    else
        disp ('connection failed');
        disp (dec2hex(status));
        disp (Mcs.Usb.CMcsUsbNet.GetErrorText(status));
    end
end