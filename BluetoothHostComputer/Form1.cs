using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BluetoothHostComputer
{
    public partial class Form1 : Form
    {
        SerialPort sp = null;
        bool isOpen = false;
        bool isSetProperty = false;
        bool isHex = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.MaximumSize = this.Size;
            this.MinimumSize = this.Size;
            this.MaximizeBox = false;
            //检查是否含有串口  
            string[] str = SerialPort.GetPortNames();
            if (str.Length!=0)
            {
                //添加串口项目  
                foreach (string s in System.IO.Ports.SerialPort.GetPortNames())
                {//获取有多少个COM口  
                    cbxCOMPort.Items.Add(s);
                }

                //串口设置默认选择项  
                cbxCOMPort.SelectedIndex = 0;         //设置cbSerial的默认选项  
            }
            else
            {
                MessageBox.Show("请检查连接线或者驱动！", "Error");
                for (int i = 0; i < 10; i++)
                {
                    cbxCOMPort.Items.Add("COM" + (i + 1).ToString());
                }
                cbxCOMPort.SelectedIndex = 0;
            }
            cbxBaudRate.Items.Add("921600");
            cbxBaudRate.SelectedIndex = 0;

            cbxStopBits.Items.Add("1");
            cbxStopBits.SelectedIndex = 0;

            cbxDataBits.Items.Add("8");
            cbxDataBits.SelectedIndex = 0;

            cbxParity.Items.Add("NONE");
            cbxParity.Items.Add("ODD");
            cbxParity.Items.Add("EVEN");
            cbxParity.SelectedIndex = 0;

            rbnHex.Checked = true;

        }

        private void btnCheckCOM_Click(object sender, EventArgs e)
        {
            bool comExistence = false;
            cbxCOMPort.Items.Clear();
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    SerialPort sp = new SerialPort("COM" + (i + 1).ToString());
                    sp.Open();
                    sp.Close();
                    cbxCOMPort.Items.Add("COM" + (i + 1).ToString());
                    comExistence = true;
                }
                catch (Exception)
                {
                    continue;
                }
            }
            if (comExistence)
            {
                cbxCOMPort.SelectedIndex = 0;
            }
            else
            {
                MessageBox.Show("无可用串口！", "错误提示");
            }
        }

        private bool CheckPortSetting()
        {
            if (cbxCOMPort.Text.Trim() == "") return false;
            if (cbxBaudRate.Text.Trim() == "") return false;
            if (cbxDataBits.Text.Trim() == "") return false;
            if (cbxParity.Text.Trim() == "") return false;
            if (cbxStopBits.Text.Trim() == "") return false;
            return true;
        }

        private bool CheckSendData()
        {
            if (tbxSendData.Text.Trim() == "") return false;
            return true;
        }

        private void SetPortProperty()//设置串口的属性
        {
            sp = new SerialPort();
            sp.PortName = cbxCOMPort.Text.Trim();//设置串口名
            sp.BaudRate = Convert.ToInt32(cbxBaudRate.Text.Trim());//设置串口的波特率floatf=Convert.ToSingle(cbxStopBits.Text.Trim());//设置停止位if(f==0){
            float f = Convert.ToSingle(cbxStopBits.Text.Trim());//设置停止位
            if (f == 0)
            {
                sp.StopBits = StopBits.None;
            }
            else if (f == 1.5)
            {
                sp.StopBits = StopBits.OnePointFive;
            }
            else if (f == 1) {
                sp.StopBits = StopBits.One;
            }
            else if (f == 2) {
                sp.StopBits = StopBits.Two;
            } else {
                sp.StopBits = StopBits.One;
            }

            sp.DataBits = Convert.ToInt16(cbxDataBits.Text.Trim());//设置数据位
            string s = cbxParity.Text.Trim();//设置奇偶校验位
            if (s.CompareTo("无") == 0)
            {
                sp.Parity = Parity.None;
            }
            else if (s.CompareTo("奇校验") == 0)
            {
                sp.Parity = Parity.Odd;
            }
            else if (s.CompareTo("偶校验") == 0)
            {
                sp.Parity = Parity.Even;
            }
            else
            {
                sp.Parity = Parity.None;
            }
            sp.ReadTimeout = -1;//设置超时读取时间sp.RtsEnable=true;

            sp.RtsEnable = true;

            //定义DataReceived事件，当串口收到数据后触发事件
            sp.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived);
            if (rbnHex.Checked)
            {
                isHex = true;
            }
            else
            {
                isHex = false;
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (isOpen)//写串口数据
            {
                try
                {
                    sp.WriteLine(tbxSendData.Text);
                }
                catch (Exception)
                {
                    MessageBox.Show("发送数据时发生错误！", "错误提示");
                    return;
                }
            }
            else
            {
                MessageBox.Show("串口未打开！", "错误提示");
                return;
            }
            if (!CheckSendData())//检测要发送的数据
            {
                MessageBox.Show("请输入要发送的数据！", "错误提示");
                return;
            }
        }

        private void btnOpenCOM_Click(object sender, EventArgs e)
        {
            if (isOpen == false)
            {
                if (!CheckPortSetting())//检测串口设置
                {
                    MessageBox.Show("串口未设置！", "错误提示");
                    return;
                }
                if (!isSetProperty)//串口未设置则设置串口
                {
                    SetPortProperty(); isSetProperty = true;
                }
                try//打开串口
                {
                    sp.Open();
                    isOpen = true;
                    btnOpenCOM.Text = "关闭串口";
                    //串口打开后则相关的串口设置按钮便不可再用
                    cbxCOMPort.Enabled = false;
                    cbxBaudRate.Enabled = false;
                    cbxDataBits.Enabled = false;
                    cbxParity.Enabled = false;
                    cbxStopBits.Enabled = false;
                    rbnChar.Enabled = false;
                    rbnHex.Enabled = false;
                }
                catch (Exception)
                {
                    //打开串口失败后，相应标志位取消isSetProperty=false;isOpen=false;
                    MessageBox.Show("串口无效或已被占用！", "错误提示");
                }
            }
            else{
                try
                {
                sp.Close(); isOpen = false;
                isSetProperty = false;
                btnOpenCOM.Text = "打开串口";
                //关闭串口后，串口设置选项便可以继续使用
                cbxCOMPort.Enabled = true;
                cbxBaudRate.Enabled = true;
                cbxDataBits.Enabled = true;
                cbxParity.Enabled = true;
                cbxStopBits.Enabled = true;
                rbnChar.Enabled = true;
                rbnHex.Enabled = true;
                }
                catch (Exception)
                {
                    lblStatus.Text = "关闭串口时发生错误";
                }
            }
        }

        private void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            System.Threading.Thread.Sleep(100);//延时 100ms 等待接收完数据
                                               //this.Invoke 就是跨线程访问 ui 的方法，也是本文的范例
            this.Invoke((EventHandler)(delegate
            {
                if (isHex == false)
                {
                    tbxRecvData.Text += sp.ReadLine();
                }
                else
                {
                    Byte[] ReceivedData = new Byte[sp.BytesToRead]; //创建接收字节数组
                    sp.Read(ReceivedData, 0, ReceivedData.Length); //读取所接收到的数据
                    String RecvDataText = null;
                    for (int i = 0; i < ReceivedData.Length - 1; i++)
                    {
                        RecvDataText += ("0x" + ReceivedData[i].ToString("X2") + " ");
                    }
                    tbxRecvData.Text += RecvDataText;
                }
                sp.DiscardInBuffer();//丢弃接收缓冲区数据
            }));
        }
        private void btnCleanData_Click(object sender, EventArgs e)
        {
            tbxRecvData.Text = "";
            tbxSendData.Text = "";
        }

        private void btnBinFileOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = true;
            fileDialog.Title = "请选择文件";
            fileDialog.Filter = "所有文件(*.*)|*.bin";
            string file = "";
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                file = fileDialog.FileName;
                MessageBox.Show("已选择文件:" + file, "选择文件提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            openBinFile(file);
        }

        public void openBinFile(string file)
        {
            string Mytext = "";
            int file_len;
            int read_len;
            byte[] binchar = new byte[] { };

            FileStream Myfile = new FileStream(file, FileMode.Open, FileAccess.Read);
            BinaryReader binreader = new BinaryReader(Myfile);

            file_len = (int)Myfile.Length;//获取bin文件长度

            while (file_len > 0)
            {
                if (file_len / 256 > 0)//一次读取256字节
                    read_len = 256;
                else                   //不足256字节按实际长度读取
                    read_len = file_len % 256;

                binchar = binreader.ReadBytes(read_len);

                foreach (byte j in binchar)
                {
                    Mytext += j.ToString("X2");//以16进制 2位宽度显示
                    Mytext += " ";
                }

                file_len -= read_len;
            }
            tbxSendData.Text = Mytext;
            binreader.Close();
        }

        private void cbxCOMPort_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}

