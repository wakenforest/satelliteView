using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace satViewApp1.View
{
    public partial class Form_LocInput : Form
    {
        //定义委托
        public delegate void MyDelegate();
        //定义事件
        public event MyDelegate MyEvent;

        public Form_LocInput()
        {
            InitializeComponent();
        }

        private void button_loc_reset_Click(object sender, EventArgs e)
        {
            this.textBox_loc_lat.Text = "";
            this.textBox_loc_lon.Text = "";
        }

        private void button_loc_submit_Click(object sender, EventArgs e)
        {
            Form1.loc_lon = System.Convert.ToDouble(this.textBox_loc_lon.Text);
            Form1.loc_lat = System.Convert.ToDouble(this.textBox_loc_lat.Text);

            if (MyEvent != null)
                MyEvent();//引发事件
            this.Close();
        }
    }
}
