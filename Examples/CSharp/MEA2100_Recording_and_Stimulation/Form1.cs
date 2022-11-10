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

namespace MEA2100_Recording_and_Stimulation
{
    public partial class Form1 : Form
    {
        private CMcsUsbListNet list = new CMcsUsbListNet(DeviceEnumNet.MCS_DEVICE_USB);

        private CMeaUSBDeviceNet dacq = new CMeaUSBDeviceNet();
        private CStg200xDownloadNet stg = new CStg200xDownloadNet();

        public Form1()
        {
            InitializeComponent();

            list.DeviceArrival += List_DeviceArrivalRemoval;
            list.DeviceRemoval += List_DeviceArrivalRemoval;

            dacq.ErrorEvent += Dacq_ErrorEvent;
            dacq.ChannelDataEvent += Dacq_ChannelDataEvent;

            RefreshDeviceList();
        }
        private void RefreshDeviceList()
        {
            cbDeviceList.Items.Clear();
            cbDeviceList.Items.AddRange(list.GetUsbListEntries());
            if (cbDeviceList.Items.Count > 0)
            {
                cbDeviceList.SelectedIndex = 0;
            }
        }

        private void List_DeviceArrivalRemoval(CMcsUsbListEntryNet entry)
        {
            RefreshDeviceList();
        }

        private void Dacq_ErrorEvent(string msg, int action)
        {
            if (InvokeRequired)
            {
                Invoke(new OnError(Dacq_ErrorEvent), msg, action);
            }
            else
            {
                listBox1.Items.Add(msg);
            }
        }

        private void Dacq_ChannelDataEvent(CMcsUsbDacqNet dacq, int CbHandle, int numFrames)
        {
            throw new NotImplementedException();
        }

        private void btRecordingStart_Click(object sender, EventArgs e)
        {

        }

        private void btRecordingStop_Click(object sender, EventArgs e)
        {

        }

        private void btStimulationStart_Click(object sender, EventArgs e)
        {

        }

        private void btStimulatinStop_Click(object sender, EventArgs e)
        {

        }
    }
}
