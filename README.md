# README for McsUsbNet.dll

This repository contains the `McsUsbNet.dll` for accessing MCS devices directly, as well as documentation and examples.

It does not contain any support for reading and writing specific MCS file formats like the `MSRD` file format of [Multi Channel Experimenter](https://www.multichannelsystems.com/software/multi-channel-experimenter), the `MCD` file format of [MC_Rack](https://www.multichannelsystems.com/software/mc-rack) or the `STM` file format of [MC_Stimulus II](https://www.multichannelsystems.com/software/mc-stimulus-ii).

## Repository Structure
- 32-Bit and 64-Bit binaries of the `McsUsbNet.dll` can be found in the `\McsUsbNet\x86` and  `\McsUsbNet\x64` folders, respectively. 
 
> Please note that you need to have **.NET 4.7.2** installed to interact with the dll.

- Documentation for the McsUsbNet API is located in the `\McsUsbNet\docu` folder
- Usage examples for the `McsUsbNet.dll` in C#, Python and Matlab are located in the `\Examples\CSharp`, `\Examples\Python` and `\Examples\Mathlab` folders, respectively.

## Example Code

### C#

> All example projects require **.NET 4.7.2** or later.

| Folder | Purpose |
|--------|---------|
| `\Examples\CSharp\MEA_Recording` | Data acquisition from MEA devices |
| `\Examples\CSharp\MEA2100_Stimulation` | Stimulation for the MEA2100 device platform |
| `\Examples\CSharp\STG_Stimulation` | Stimulation for [STG](https://www.multichannelsystems.com/products/stimulus-generators) devices |

### Python

| File | Purpose |
|--------|---------|
| `\Examples\Python\Recording.p | Data acquisition from MEA devices |
| `\Examples\Python\Stimulation.py | Stimulation for [STG](https://www.multichannelsystems.com/products/stimulus-generators) devices |
| `\Examples\Python\DeviceList.py | Enumerate MCS devices |

### Matlab

> All examples require **.NET 4.7.2** or later. Tested on Matlab 2019b.

| Folder | Purpose |
|--------|---------|
| `\Examples\Matlab\MEA_Recording` | Data acquisition from MEA devices |
| `\Examples\Matlab\MEA2100_Stimulation` | Stimulation for the MEA2100 device platform |
| `\Examples\Matlab\STG_Stimulation` | Stimulation for [STG](https://www.multichannelsystems.com/products/stimulus-generators) devices |

## Open Issues
- `.chm` or `.pdf` Documentation for current McsUsbNet
