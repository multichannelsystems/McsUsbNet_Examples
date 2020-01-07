function run_mea(device)
% Connects to a MCS MEA device and starts data acquisition.
%
% function run_mea(device)
%
% The device parameter is created when calling the init_mea script

    % List all MCS MEA devices and connect to the first one
    devicelist = Mcs.Usb.CMcsUsbListNet(Mcs.Usb.DeviceEnumNet.MCS_MEA_DEVICE);
    status = device.Connect(devicelist.GetUsbListEntry(0));
    
    if status == 0
        % Register cleanup function. This ensures that the data acquisition
        % is properly stopped and the device disconnects when the run_mea
        % function terminates
        cleanupObj = onCleanup(@()cleanup_mea(device));
        
        % Make sure that the data acquisition is stopped
        device.SendStop();
        
        % Get/set the number of channels for the device
        [status,hwchannels] = device.HWInfo().GetNumberOfHWADCChannels();
        status = device.SetNumberOfChannels(hwchannels);
        
        % Retrieve the channel designations from the device
        [status, analogchannels, digitalchannels, checksumchannels, timestampchannels, channelsinblock] = device.GetChannelLayout(0);

        % Set the sampling rate
        device.SetSamplerate(50000, 1, 0); % 50 kHz

        % Select channels and other parameters for data acquisition
        % Parameters are (in order): 
        %   - number of channels
        %   - maximum number of samples in the data queue. Here: 50000 = 1 s of data
        %   - number of samples per data packet. Here: 5000 = 100 ms of data
        %   - data type. Here: 16-Bit unsigned
        %   - channels in block: Here: All channels arrive in a single block
        device.SetSelectedChannels(channelsinblock, 50000, 5000, Mcs.Usb.SampleSizeNet.SampleSize16Unsigned, channelsinblock);

        % Start data acquisition
        device.StartDacq();

        x = 0;
        while 1
            for i = [0 channelsinblock - 1]
                number = device.ChannelBlock_AvailFrames(i);
                % wait until at least 5000 samples are available
                if number >= 5000
                    % read 5000 samples
                    [data, read] = device.ChannelBlock_ReadFramesUI16(i, 5000);

                    if i == 0 % selected channel
                       % Because the data is acquired in 16 Bit unsigned
                       % format, we need to subtract 2^15 to center it
                       % around Zero.
                       y = single(data) - 32768;
                       plot(y); 
                    end
                end
            end
            pause(0.01);
            x = x + 1;
            if (x == 5000)
                break; % stop after 5000 data packets (~ 500 seconds)
            end
        end

    else
        disp ('connection failed');
        disp (dec2hex(status));
        disp (Mcs.Usb.CMcsUsbNet.GetErrorText(status));
        
    end
end