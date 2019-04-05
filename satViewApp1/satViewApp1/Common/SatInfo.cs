using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using satViewApp1.Common;

namespace satViewApp1
{
    public class SatInfo
    {
        public int prn;
        public int sys;    //0:Beidou, 1:GPS, 2:Galileo, 3:Glonass, 4:QZSS
        public double x;
        public double y;
        public double z;
        public double az;
        public double el;

        public static double FE_WGS84 = (1.0 / 298.257223563);
        public static double RE_WGS84 = 6378137.0;

        private CommonEph.eceft sat_res = new CommonEph.eceft();

        public void setSatInfo(int prn, int sys, double[] satPos, double[] usrPos)
        {
            this.prn = prn;
            this.sys = sys;
            this.x = satPos[0];
            this.y = satPos[1];
            this.z = satPos[2];
            double[] azel = new double[2];
            azel = CalAZEL(usrPos, satPos);
            this.az = azel[0];
            this.el = azel[1];
        }

        public CommonEph.eceft CalSat(int satNum, CommonEph.EphData eph, int sys)
        {
            double tTime = 30000;
            sat_res = CommonEph.SatPosCal(satNum, tTime, eph, sys, (int) eph.week);
            return sat_res;
        }

        public static double norm(double[] a, int n)
        {
            return Math.Sqrt(dot(a, a, n));
        }

        public static double dot(double[] a, double[] b, int n)
        {
            double c = 0.0;
            while (--n >= 0)
            {
                c += a[n] * b[n];
            }
            return c;
        }

        public static void ecef2pos(double[] r, double[] pos)
        {
            double e2 = FE_WGS84 * (2.0 - FE_WGS84), z, zk = 0.0, v = RE_WGS84, sinp;
            double r2 = dot(r, r, 2);
            z = r[2];
            while (Math.Abs(z - zk) >= 1E-4)
            {
                zk = z;
                sinp = z / Math.Sqrt(r2 + z * z);
                v = RE_WGS84 / Math.Sqrt(1.0 - e2 * sinp * sinp);
                z = r[2] + v * e2 * sinp;
            }
            pos[0] = r2 > 1E-12 ? Math.Atan(z / Math.Sqrt(r2)) : (r[2] > 0.0 ? Math.PI / 2.0 : -Math.PI / 2.0);
            pos[1] = r2 > 1E-12 ? Math.Atan2(r[1], r[0]) : 0.0;
            pos[2] = Math.Sqrt(r2 + z * z) - v;
        }

        public double[] CalAZEL(double[] usrPos, double[] satPos)
        {
            double[] azel = new double[2];
            return azel;
        }
    }
}
