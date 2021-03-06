﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using Microsoft.Win32;

namespace BlueprintEditor
{
    public partial class Form3 : Form
    {
        Button ReportButton;
        public Form3(Button _ReportButton = null)
        {
            InitializeComponent();
            ReportButton = _ReportButton;
        }

        public void ChangeLang(int Lang, Control Contre = null)
        {
            if (Contre == null)
            {
                Contre = this;
                if (Contre.Tag != null)
                {
                    string tag = Contre.Tag.ToString();
                    if (tag != "")
                    {
                        string[] Tagge = tag.Split('|');
                        if (Tagge[0] == "") Contre.Tag = Contre.Text + tag;
                        Contre.Text = Tagge[Lang];
                    }
                }
            }
            foreach (Control Contr in Contre.Controls)
            {
                ChangeLang(Lang, Contr);
                try
                {
                    if (Contr.Tag is null) continue;
                    string tag = Contr.Tag.ToString();
                    if (tag is "") continue;
                    string[] Tagge = tag.Split('|');
                    if (Tagge[0] == "") Contr.Tag = Contr.Text + tag;
                    Contr.Text = Tagge[Lang];
                }
                catch
                {

                }
            }
        }

        public void SetColor(Color Fore, Color Back)
        {
            BackColor = Back;
            Recolor(Controls, Fore, Back);
        }

        void Recolor(Control.ControlCollection Controlls, Color ForeColor, Color BackColor)
        {
            foreach (Control Contr in Controlls)
            {
                Contr.ForeColor = ForeColor;
                if (Contr.BackColor != Color.Transparent) Contr.BackColor = BackColor;
                Recolor(Contr.Controls, ForeColor, BackColor);
            }
        }

        public string GetPCInfo()
        {
            return "Processor: " + GetProcessorInformation() + "<br>Video: " + GetVideoProcessorInformation() + "<br>Board: " + GetBoardProductId() + "<br>Disc: " + GetDisckModel() + "<br>Mem: " + GetPhysicalMemory() + "<br>OS: " + GetOSInformation();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "")
            {
                label4.Text = Form1.Settings.LangID == 0 ? "Sending...":"Отправка...";
                string Report = "Report by " + (textBox2.Text != "" ? textBox2.Text : "Anonymous") +"<br>App v" + Application.ProductVersion + "<br>Message: " + textBox1.Text.Replace("\n", "<br>").Replace("\r", "") + "<br>";
                string returned = "false";
                switch (comboBox1.SelectedIndex)
                {
                    case 0:
                        returned = ArhApi.Server("Report", ",\"body\":\"Bug"+ Report + "PC components: <br>" + GetPCInfo() + "\"","bool");
                        break;
                    case 1:
                        returned = ArhApi.Server("Report", ",\"body\":\"Suggestions"+ Report + "\"", "bool");
                        break;
                }

                if (returned == "true")
                {
                    MessageBox.Show(Form1.Settings.LangID == 0 ? "Thank you for your report":"Спасибо за ваш отчет");
                    if (ReportButton != null) ReportButton.Enabled = false;
                }
                else MessageBox.Show(Form1.Settings.LangID == 0 ? "Error, report not sended." :"Ошибка при отправке отчета.");
                //ReportButton.Dispose();
                int result;
                this.Hide();
            }
            else
            {
                MessageBox.Show(Form1.Settings.LangID == 0?"Report can't be empty":"Отчет не может быть пустым");
            }
        }
        public void Clear()
        {
            textBox1.Text = "";
            textBox2.Text = "";
            label4.Text = "";
            comboBox1.SelectedIndex = 0;
        }
        public static string GetPhysicalMemory()
        {
            ManagementScope oMs = new ManagementScope();
            ObjectQuery oQuery = new ObjectQuery("SELECT Capacity FROM Win32_PhysicalMemory");
            ManagementObjectSearcher oSearcher = new ManagementObjectSearcher(oMs, oQuery);
            ManagementObjectCollection oCollection = oSearcher.Get();

            long MemSize = 0;
            long mCap = 0;

            // In case more than one Memory sticks are installed
            foreach (ManagementObject obj in oCollection)
            {
                mCap = Convert.ToInt64(obj["Capacity"]);
                MemSize += mCap;
            }
            MemSize = (MemSize / 1024) / 1024;
            return MemSize.ToString() + "MB";
        }
        public static string GetOSInformation()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
            foreach (ManagementObject wmi in searcher.Get())
            {
                try
                {
                    return ((string)wmi["Caption"]).Trim() + ", " + (string)wmi["Version"] + ", " + (string)wmi["OSArchitecture"];
                }
                catch { }
            }
            return "BIOS Maker: Unknown";
        }
        public static string GetProcessorInformation()
        {
            ManagementClass mc = new ManagementClass("win32_processor");
            ManagementObjectCollection moc = mc.GetInstances();
            string info = String.Empty;
            foreach (ManagementObject mo in moc)
            {
                string name = (string)mo["Name"];
                name = name.Replace("(TM)", "™").Replace("(tm)", "™").Replace("(R)", "®").Replace("(r)", "®").Replace("(C)", "©").Replace("(c)", "©").Replace("    ", " ").Replace("  ", " ");

                info = name + ", " + (string)mo["Caption"] + ", " + (string)mo["SocketDesignation"];
                //mo.Properties["Name"].Value.ToString();
                //break;
            }
            return info;
        }
        public static string GetVideoProcessorInformation()
        {
            ManagementClass mc = new ManagementClass("Win32_VideoController");
            ManagementObjectCollection moc = mc.GetInstances();
            string info = String.Empty;
            foreach (ManagementObject mo in moc)
            {
                info = (string)mo["Caption"];
            }
            return info;
        }
        public static string GetBoardProductId()
        {

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_BaseBoard");

            foreach (ManagementObject wmi in searcher.Get())
            {
                try
                {
                    return wmi.GetPropertyValue("Product").ToString();

                }

                catch { }

            }

            return "Product: Unknown";

        }
        public static string GetDisckModel()
        {

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_DiskDrive");

            foreach (ManagementObject wmi in searcher.Get())
            {
                try
                {
                    return wmi.GetPropertyValue("Model").ToString();

                }

                catch { }

            }

            return "Model: Unknown";

        }

        private void Form3_Load(object sender, EventArgs e)
        {

        }
    }
}
