function cleanup_mea(device)
% Cleanup function that stops the data acquisition and disconnects. This
% function is automatically called when the run_mea function terminates.

    device.StopDacq();

    device.Disconnect();

%    delete(device);
end