Matlab Examples
===============

All examples consist of a init_* script, a run_* and a cleanup_* function. 

To run the examples, you'll first need to execute the init_* script once, which will make the McsUsbNet.dll visible to Matlab. If this fails, please check the init_* script and make sure that the path to the McsUsbNet.dll is valid.

After that, you may execute the run_* function. The cleanup_* function does not need to be called explicitly, it will be automatically called when the run_* function terminates.