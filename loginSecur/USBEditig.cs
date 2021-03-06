﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Management;
using System.Threading;

namespace loginSecur
{
    public partial class USBEditig : Form
    {
        private const int WM_DEVICE_CHANGE = 0x219;
        private const int DEVICE_INSERT = 0x8000;
        private const int DEVICE_REMOVE = 0x8004;

        USBDrive USBInfo = new USBDrive("", "", "");
        List<USBDrive> USBDrivesList = new List<USBDrive>();
        private Enter EnterForm;
        USBDataBase USBInfoDB = new USBDataBase("", "", "", 0);
        List<USBDataBase> USBDataBaseList = new List<USBDataBase>();
        private string PATH = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Database\\";

        public USBEditig()
        {
            InitializeComponent();
            refreshComboBox1();
            refreshComboBox2();
            EnterForm = new Enter();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F1)
            {
                Help.ShowHelp(this, "help.chm");
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        /// Add new USB Drive in DataBase
        /// </summary>
        public void GetDataBaseInList()
        {
            USBDataBaseList.Clear();
            string bufVolumeLabel = "";
            string bufSize = "";
            var dataBase = File.ReadAllLines(PATH + "DataBase.txt");
            foreach (var i in dataBase)
            {
                string strBuf = i;
                int j;
                for (j = 0; j < i.Length; j++)
                {
                    if (strBuf[j] != ',')
                    {
                        bufVolumeLabel += strBuf[j];
                    }
                    else break;
                }
                for (j = bufVolumeLabel.Length + 1; j < i.Length; j++)
                {
                    if (strBuf[j] != '|')
                    {
                        bufSize += strBuf[j];
                    }
                    else break;
                }
                USBDataBaseList.Add(new USBDataBase("", bufVolumeLabel, bufSize, 0));
                bufVolumeLabel = "";
                bufSize = "";
            }
        }

        /// <summary>
        /// looking for moment of inserting the USB Drive
        /// </summary>        
        protected override void WndProc(ref Message m)
        {
            Thread DeviceQuery = new Thread(getUSBListComboBox);
            base.WndProc(ref m);
            switch (m.Msg)
            {
                case WM_DEVICE_CHANGE:
                    {
                        switch ((int)m.WParam)
                        {
                            case DEVICE_INSERT:
                                { 
                                    comboBox1.Items.Clear();
                                    DeviceQuery.Start();
                                    Thread.Sleep(500);
                                    foreach (var i in USBDrivesList)
                                    {
                                        comboBox1.Items.Add(i.volumeLabel + " " + i.totalSize);
                                    }
                                    DeviceQuery.Abort();
                                    break;
                                }
                            case DEVICE_REMOVE:
                                {                                   
                                    comboBox1.Items.Clear();
                                    DeviceQuery.Start();
                                    Thread.Sleep(500);
                                    foreach (var i in USBDrivesList)
                                    {
                                        comboBox1.Items.Add(i.volumeLabel + " " + i.totalSize);
                                    }
                                    DeviceQuery.Abort();
                                    break;
                                }
                        }
                        break;

                    }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// get the list of installed USB Drives with help of DriveInfo class GetDrives() and checked in "Removable" status
        /// </summary>
        public void getUSBListComboBox()
        {
            USBDrivesList.Clear();
            var getUSBListComboBox = new ManagementObjectSearcher("select * from Win32_DiskDrive where InterfaceType='USB'").Get();
            foreach (var i in DriveInfo.GetDrives())
            {
                if (i.DriveType.ToString() == "Removable")
                {
                    USBDrivesList.Add(new USBDrive(i.Name, i.VolumeLabel, i.TotalSize.ToString()));
                }
            }
        }

        /// <summary>
        /// This method is just refresh combobox
        /// </summary>
        void refreshComboBox1()
        {
            comboBox1.Items.Clear();
            getUSBListComboBox();
            foreach (var i in USBDrivesList)
            {
                comboBox1.Items.Add(i.volumeLabel + " " + i.totalSize);                
            }
        }

        /// <summary>
        /// This method is just refresh combobox
        /// </summary>
        void refreshComboBox2()
        {
            comboBox2.Items.Clear();
            GetDataBaseInList();
            foreach (var i in USBDataBaseList)
            {               
                comboBox2.Items.Add(i.volumeLabel);                
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (comboBox2.SelectedItem == null)
                {
                    MessageBox.Show("Select the USB Drive!");
                    return;
                }
                else
                {
                    var dataBase = File.ReadAllLines(EnterForm.PATH + "DataBase.txt");
                    File.WriteAllText(EnterForm.PATH + "DataBase.txt", string.Empty);
                    foreach (var i in dataBase)
                    {
                        string ID = "";
                        for (int j = 0; j < i.Length; j++)
                        {
                            if (i[j] != ',')
                            {
                                ID += i[j];
                            }
                            else break;
                        }
                        if (comboBox2.SelectedItem.ToString() != ID)
                        {
                            using (StreamWriter reWriteDB = File.AppendText(EnterForm.PATH + "DataBase.txt"))
                            {
                                reWriteDB.WriteLine(i);
                            }
                        }
                        else ID = "";
                    }
                    comboBox2.SelectedIndex = -1;
                    refreshComboBox2();
                    MessageBox.Show("Successfully deleted!");
                    return;
                }
            }
            catch (Exception i){ MessageBox.Show(i.ToString()); }
        }

        private void authorizeButton_Click(object sender, EventArgs e)
        {
            try
            {
                getUSBListComboBox();
                string buf = "";
                if (comboBox1.SelectedItem == null)
                {
                    MessageBox.Show("Select the USB Drive!");
                    return;
                }
                else
                {                    
                    using (StreamWriter newUSBDrive = File.AppendText(EnterForm.PATH + "DataBase.txt"))
                    {
                        int i;
                        for (i = 0; i < comboBox1.SelectedItem.ToString().Length; i++)
                        {
                            if (comboBox1.SelectedItem.ToString()[i] != ' ')
                            {
                                buf += comboBox1.SelectedItem.ToString()[i];
                            }
                            else break;
                                foreach (var x in USBDataBaseList)
                                {
                                    if (x.volumeLabel == buf)
                                    {
                                    comboBox1.SelectedIndex = -1;
                                    refreshComboBox2();
                                    refreshComboBox1();
                                    MessageBox.Show("Already exists!");
                                    return;
                                    }
                                }
                        }
                        buf += ',';
                        for (int j = i + 1; j < comboBox1.SelectedItem.ToString().Length; j++)
                        {
                            buf += comboBox1.SelectedItem.ToString()[j];
                        }
                        buf += ("|" + buf.GetHashCode());
                        newUSBDrive.WriteLine(buf);
                        newUSBDrive.Close();                        
                    }
                    comboBox1.SelectedIndex = -1;
                    refreshComboBox2();
                    refreshComboBox1();
                    MessageBox.Show("Successfully added!");
                    return;
                }
            } catch (Exception i) { MessageBox.Show(i.ToString()); }            
        }

        private void help_button_Click(object sender, EventArgs e)
        {
            Help.ShowHelp(this, "help.chm");
        }
    }
}
