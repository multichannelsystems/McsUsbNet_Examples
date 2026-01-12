# README for McsUsbNet_Examples

This repository contains the `McsUsbNet.dll` for accessing MCS devices directly, as well as documentation and examples.

It does not contain any support for reading and writing specific MCS file formats like the `MSRD` file format of [Multi Channel Experimenter](https://www.multichannelsystems.com/software/multi-channel-experimenter), the `MCD` file format of [MC_Rack](https://www.multichannelsystems.com/software/mc-rack) or the `STM` file format of [MC_Stimulus II](https://www.multichannelsystems.com/software/mc-stimulus-ii).

## Repository Structure
- 32-Bit and 64-Bit binaries of the `McsUsbNet.dll` can be found in the `\McsUsbNet\x86` and  `\McsUsbNet\x64` folders, respectively.
 
> Please note that you need to have **.NET Framework 4.7.2** and **Microsoft Visual C++ Redistributable for Visual Studio 2019** installed to interact with the dll.

> Please note as well: If you download this repository as a .zip archive, GitHub doesn't include the McsUsbNet submodule. Please download the [McsUsbNet](https://github.com/multichannelsystems/McsUsbNet) repository as well and copy its contents into the McsUsbNet_Examples/McsUsbNet folder.

- Documentation for the McsUsbNet API is located in the `\McsUsbNet\docu` folder. 

> Please note : The dll comes with an xml file `McsUsbNet.xml`, that allows, for example in Visual Studio, to have a **code completion**. We regard this a **part of the documentation** and you should use a tool that allows for code completion at least in parallel to you actual target development system.

- Usage examples for the `McsUsbNet.dll` in C# and Python are located in the `\Examples\CSharp` and `\Examples\Python` folders, respectively.

## Example Code

### C#

> All example projects require **.NET 4.7.2** or later.

| Folder | Purpose |
|--------|---------|
| `\Examples\CSharp\MEA_Recording` | Data acquisition from MEA devices |
| `\Examples\CSharp\MEA2100_Stimulation` | Stimulation for the MEA2100 device platform |
| `\Examples\CSharp\STG_Stimulation` | Stimulation for [STG](https://www.multichannelsystems.com/products/stimulus-generators) devices |

### Python

> All examples require **.NET 4.7.2** or later.
> Tested with Python 3, use `pip install pythonnet`

| File | Purpose |
|--------|---------|
| `\Examples\Python\Recording.py` | Data acquisition from MEA devices |
| `\Examples\Python\Stimulation.py` | Stimulation for [STG](https://www.multichannelsystems.com/products/stimulus-generators) devices |
| `\Examples\Python\DeviceList.py` | Enumerate MCS devices |

