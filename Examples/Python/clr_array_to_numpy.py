import ctypes
import numpy as np

import clr 
import System
from System.Runtime.InteropServices import GCHandle, GCHandleType

def asNumpyArray(netArray: System.Array, arrayType: ctypes):
    hnd = GCHandle.Alloc(netArray, GCHandleType.Pinned)
    try:
        ptr = hnd.AddrOfPinnedObject().ToInt64()
        bufType = arrayType * netArray.Length
        cbuf = bufType.from_address(ptr)
        np_data = np.frombuffer(cbuf, cbuf._type_)
    finally:
        if hnd.IsAllocated:
            hnd.Free()

    return np_data