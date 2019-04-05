using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Threading;
using satViewApp1.View;
using satViewApp1.Common;

namespace satViewApp1
{

    [System.Runtime.InteropServices.ComVisibleAttribute(true)]
    public partial class Form1 : Form
    {
        private const int tnum = 128;   //总卫星数
        private const int sat_inview = 16;
        private int beidouNum = 16;

        public static string[] satNameArray = { "GEO-1", "GEO-2", "GEO-3","GEO-4", "GEO-5", "IGSO-1",
        "IGSO-2", "IGSO-3", "IGSO-4", "IGSO-5", "MEO-3", "MEO-4", "IGSO-6","MEO-6","tmp","tmp","tmp"}; 

        public struct satlla
        {
            public double lat;
            public double lon;
            public double alt;
        }

        satlla[] lla = new satlla[sat_inview];

        // 手动设定的用户位置
        public static double loc_lon;
        public static double loc_lat;

        satViewApp1.MyFocus my_focus = new satViewApp1.MyFocus();

        public Form1()
        {
            InitializeComponent();
            InitializeGrid();
            this.WindowState = FormWindowState.Maximized;

            for (int i = 0; i < sat_inview; i++)
            {
                lla[i] = new satlla();
            }
        }

        private void InitializeGrid()
        {
            //dataGridView1.Columns.Clear(); //清除所有Column
            AddColumn("PRN",60);
            AddColumn("x",115);
            AddColumn("y", 115);
            AddColumn("z", 115);
            AddColumn("方位", 95);
            AddColumn("俯仰", 95);
            dataGridView1.RowHeadersVisible = false; //隱藏RowHeader
            
            dataGridView1.RowsDefaultCellStyle.BackColor = Color.AliceBlue;
            dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.Azure;
            //dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.Beige;

            // 禁止用户改变DataGridView1的所有列的列宽
            dataGridView1.AllowUserToResizeColumns = false;
            //禁止用户改变DataGridView1所有行的行高
            dataGridView1.AllowUserToResizeRows = false;

            for (int i = 0; i < 16; i++)
            {
                AddRow();
            }
            //dataGridView1.AutoResizeColumns(); //調整寬度
        }

        public void AddColumn(string strHeader, int width)
        {
            DataGridViewColumn column = new DataGridViewTextBoxColumn();
            column.HeaderText = strHeader;
            column.Width = width;
            dataGridView1.Columns.Add(column);
        }

        public void AddRow()
        {
            List<object> lstObj = new List<object>();
            lstObj.Add("");
            lstObj.Add("");
            lstObj.Add("");
            lstObj.Add("");
            lstObj.Add("");
            lstObj.Add("");

            dataGridView1.Rows.Add(lstObj.ToArray());//將Object Array填入DataRow
        }

        // 更新表格中的卫星数据
        public void UpdateGrid(SatInfo[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                dataGridView1[0, i].Value = data[i].prn;
                dataGridView1[1, i].Value = data[i].x;
                dataGridView1[2, i].Value = data[i].y;
                dataGridView1[3, i].Value = data[i].z;
                dataGridView1[4, i].Value = data[i].az;
                dataGridView1[5, i].Value = data[i].el;
            }
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            //InvokeTestMethod("Tom", "Hello");

            object[] objects = new object[2];
            //当前纬度
            objects[0] = loc_lat;
            //当前经度
            objects[1] = loc_lon;
            //传值给html中的mapInit函数
            wbShow.Document.InvokeScript("mapInit", objects);
        }

        private void InvokeTestMethod(String name, String address)
        {
            if (wbShow.Document != null)
            {
                Object[] objArray = new Object[2];
                objArray[0] = (Object)name;
                objArray[1] = (Object)address;
                wbShow.Document.InvokeScript("test", objArray);
            }
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
            wbShow.ScriptErrorsSuppressed = true;
            string path = Path.Combine(Application.StartupPath, "baiduMap.html");
            //string path = Path.Combine(Application.StartupPath, "bmap_demo.html");
            //string path = Path.Combine(Application.StartupPath, "bar_demo.html");
            wbShow.Navigate(path);
            wbShow.ObjectForScripting = this;

            wbShowBottom.ScriptErrorsSuppressed = true;
            string path_bottome = Path.Combine(Application.StartupPath, "line-stack.html");
            //string path_bottome = Path.Combine(Application.StartupPath, "bar_demo.html");
            wbShowBottom.Navigate(path_bottome);
        }

        private void wbShowBottom_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }

        // JS中点击或移动用户坐标点，触发该函数，
        public void MySetFocus(string message)
        {
            string myloc;
            myloc = my_focus.SetLocation(message);
            this.label2.Text = myloc;
        }

        // 开始计算
        private void button1_Click(object sender, EventArgs e)
        {
            SatInfo[] sat_list = new SatInfo[tnum];

            // 测试数据
            SatInfo[] dataDemo = new SatInfo[sat_inview];

            double[] tmp = new double[3];
            double[] tmplla = new double[3];
            tmp[0] = 1e3; tmp[1] = 2e3; tmp[2] = 3e3;
            CommonEph.eceft ecef_tmp = new CommonEph.eceft();


            CommonEph.ReadEph();  // 读取星历，放入d_eph[]和d_ephBD[]中
            int satNum = 1;
            

            for (int i = 0; i < sat_inview; i++)
            {

                dataDemo[i] = new SatInfo();
                satNum = i+1;

                CommonEph.EphData eph = CommonEph.d_ephBD[satNum - 1];   // 下面要使用的星历
                ecef_tmp = dataDemo[0].CalSat(satNum, eph, 1);
                tmp[0] = ecef_tmp.x; tmp[1] = ecef_tmp.y; tmp[2] = ecef_tmp.z;

                SatInfo.ecef2pos(tmp, tmplla);
                tmplla[0] = tmplla[0] * 180 / Math.PI;
                tmplla[1] = tmplla[1] * 180 / Math.PI;
                lla[i].lat = tmplla[0];
                lla[i].lon = tmplla[1];
                lla[i].alt = tmplla[2];

                dataDemo[i].setSatInfo(i, 0, tmplla, tmp);
            }

            UpdateGrid(dataDemo);
            Convert2JS();
        }

        void Convert2JS()
        {
            //MessageBox.Show("位置传递过来啦！");
            object[] objects = new object[sat_inview * 2];

            for(int i=0; i< sat_inview;i++)
            {
                objects[0] = lla[i].lon;
                objects[1] = lla[i].lat;
                objects[2] = satNameArray[i];
                wbShow.Document.InvokeScript("setSatPos", objects);
            }
            //传值给html中的setSatPos函数
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //string logPath = "C:\\01-05-research\\_satelliteView\\satelliteView\\satViewApp1\\satViewApp1";
            string logPath = "\\brdm";
            //初始化一个OpenFileDialog类 
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.InitialDirectory = Application.StartupPath + logPath;//设置打开路径的目录

            //判断用户是否正确的选择了文件 
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show(fileDialog.FileName);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Console.WriteLine("!!!!");
            //Thread thread = new Thread(() =>
            {
                WebClient client = new WebClient();
                Uri uri = new Uri("ftp://cddis.gsfc.nasa.gov/pub/gps/data/campaign/mgex/daily/rinex3/2016/brdm/brdm1460.16p.Z");

                // Specify a progress notification handler.
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressCallback);
                // Specify that the DownloadFileCallback method gets called
                // when the download completes.
                client.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadFileCallback2);
                client.DownloadFileAsync(uri, @".\afile.z");
            }//);
             //thread.Start();

        }

        public void DownloadProgressCallback(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar1.Value = int.Parse((e.ProgressPercentage).ToString());

            //// Displays the operation identifier, and the transfer progress.
            //Console.WriteLine("{0}    downloaded {1} of {2} bytes. {3} % complete...",
            //    (string)e.UserState,
            //    e.BytesReceived,
            //    e.TotalBytesToReceive,
            //    e.ProgressPercentage);
        }

        void DownloadFileCallback2(object sender, AsyncCompletedEventArgs c)
        {
            MessageBox.Show("星历下载结束");
        }

        internal class MyWebClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri address)
            {
                FtpWebRequest req = (FtpWebRequest)base.GetWebRequest(address);
                req.UsePassive = false;
                return req;
            }
        }

        void f_MyEvent()
        {
            //MessageBox.Show("已单击f窗体按钮");
            object[] objects = new object[2];
            //当前纬度
            objects[0] = loc_lat;
            //当前经度
            objects[1] = loc_lon;
            //传值给html中的setPos函数
            wbShow.Document.InvokeScript("setPos", objects);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Form_LocInput f = new Form_LocInput();
            f.Show();
            f.MyEvent += new Form_LocInput.MyDelegate(f_MyEvent);//监听f窗体事件
        }
    }
}
