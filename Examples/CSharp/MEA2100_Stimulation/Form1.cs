using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Mcs.Usb;

namespace MEA2100_Stimulation
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void BtnStartClick(object sender, EventArgs e)
        {
            // Get a list of all available MEA USB Devices such as the MEA2100, for example.
            CMcsUsbListNet cDeviceList = new CMcsUsbListNet(DeviceEnumNet.MCS_MEAUSB_DEVICE);

            if (cDeviceList.Count == 0)
            {
                MessageBox.Show("No MEA USB Device connected!", "Error Connecting To Device", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var deviceEntry = cDeviceList.GetUsbListEntry(0);

            CStg200xDownloadNet cStgDevice = new CStg200xDownloadNet();
            
            // Connect to the stimulator of the device. The lock mask allows multiple connections to the same device
            cStgDevice.Connect(deviceEntry, 1);

            uint electrode = 3;

            // ElectrodeMode: emManual: electrode is permanently selected for stimulation
            cStgDevice.SetElectrodeMode(electrode, ElectrodeModeEnumNet.emManual);

            // ElectrodeDacMux: DAC to use for stimulation
            cStgDevice.SetElectrodeDacMux(electrode, 0, ElectrodeDacMuxEnumNet.Stg1);

            // ElectrodeEnable: enable electrode for stimulation
            cStgDevice.SetElectrodeEnable(electrode, 0, true);

            // BlankingEnable: false: do not blank the ADC signal while stimulation is running
            cStgDevice.SetBlankingEnable(electrode, false);

            // AmplifierProtectionSwitch: false: Keep ADC connected to electrode even while stimulation is running
            cStgDevice.SetEnableAmplifierProtectionSwitch(electrode, false);

            // array of amplitudes and duration
            int[] amplitude = new int[2] {10000, -10000}; // µV
            int[] syncout = new int[2] { 0x1000, 0x2000 };
            ulong[] duration = new ulong[2] {100000, 100000}; // µs

            // use voltage stimulation
            cStgDevice.SetVoltageMode();

            // send stimulus data to device
            cStgDevice.PrepareAndSendData(0, amplitude, duration, STG_DestinationEnumNet.channeldata_voltage);

            cStgDevice.PrepareAndSendData(0, syncout, duration, STG_DestinationEnumNet.syncoutdata);

            // connect all stimulation channels to the first trigger
            cStgDevice.SetupTrigger(0, new uint[] { 255 }, new uint[] { 255 }, new uint[] { 1 });

            // start the first trigger
            cStgDevice.SendStart(1);

            cStgDevice.Disconnect();

            MessageBox.Show("Stimulation finished!");
        }
    }
}
