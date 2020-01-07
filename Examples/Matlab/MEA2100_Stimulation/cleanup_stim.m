function cleanup_stim(device)
% Cleanup function that disconnects from the device. This function is
% automatically called when the run_stim function terminates.

    % uncomment this line to stop the stimulation before disconnecting 
    % from the device.
    % device.SendStop();

    device.Disconnect();

end