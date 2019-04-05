using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace satViewApp1
{
    [System.Runtime.InteropServices.ComVisible(true)]
    public class MyFocus
    {
        public double lat;
        public double lon;
        public string country;
        public string province;
        public string city;
        public string district;

        public string SetLocation (string loc)
        {
            string[] loc_split;
            loc_split = loc.Split(',');
            this.lat = Convert.ToDouble(loc_split[0]);
            this.lon = Convert.ToDouble(loc_split[1]);
            this.province = loc_split[2];
            this.city = loc_split[3];
            this.district = loc_split[4];

            //// debug
            //string result = "经度为：" + Convert.ToString(this.lon) + ","+ this.city;
            //MessageBox.Show(result);

            string result = "经度：" + Convert.ToString(this.lon) + "," +
                "纬度：" + Convert.ToString(this.lat) + "\n\r" +
                this.province + " " + this.city + " " + this.district;

            return result;

        }

        //无返回值，有参数
        public void ShowSomething(String msg)
        {
            MessageBox.Show("Called from " + msg);
        }

        //有返回值，有参数
        public string returnSomething(String msg)
        {
            msg += "From C#";
            return msg;
        }
        //直接获取变量
        public string a = "A";
    }
}
