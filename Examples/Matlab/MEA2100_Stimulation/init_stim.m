% Makes the McsUsbNet.dll visible to Matlab and initializes a device object
% Needs to be called just once at the start

assembly = NET.addAssembly([pwd '\..\..\..\Dll\x64\McsUsbNet.dll']);
device = Mcs.Usb.CStg200xDownloadNet();

% After this is finished, you may call the run_stim function:
%
%  run_stim(device);