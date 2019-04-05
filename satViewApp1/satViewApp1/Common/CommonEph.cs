using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace satViewApp1.Common
{
    public class CommonEph
    {
        #region 变量定义

        public static bool flag1;
        public static bool flag2;
        public static bool flag3;

        //private static string eph_file_loc = "./brdm/brdm0440.19p";
        private static string eph_file_loc = "./brdm/brdm0140.18p";
        private static string alm_file_loc = "./alm/344.alm";

        public struct gtime_t /* time struct */
        {
            public long time;        /* time (s) expressed by standard time_t */
            public double sec;         /* fraction of second under 1 s */
        };

        public struct eceft
        {
            public double x;
            public double y;
            public double z;
            public double dx;
            public double dy;
            public double dz;
            public double tb;

            public double tmp;
        };

        public struct EphData
        {
            public ushort signalId;
            public ushort svId;
            public ushort state;
            public ushort valid;

            public uint week;
            public gtime_t toc;
            public gtime_t toe;
            public gtime_t tow;

            public double toes;
            public double af2;
            public double af1;
            public double af0;

            public double M0;
            public double delta_n;
            public double e;
            public double sqrt_A;
            public double OMEGA0;
            public double i0;
            public double omega;
            public double OMEGADot;
            public double IDOT;

            public double Cuc;
            public double Cus;
            public double Crc;
            public double Crs;
            public double Cic;
            public double Cis;

            public static double[] alpha = new double[4];
            public static double[] beta = new double[4];

            public int URA;
            public int health;
            public int l2CodeFlag;
            public int l2DataFlag;
            public double Tgd1;
            public double Tgd2;
            public double Tgd3;

            public byte IODE2;
            public byte IODE3;
            public ushort IODC;

            // intermediate parameters
            public double A;
            public double n;
            public double root_e;
            public double Ek;
            public double OMEGAt;
            public double OMEGAkDot;

            //L1C
            public uint top;
            public double delta_A;
            public double A_Dot;
            public double delta_n_Dot;
            public double delta_OMEGA_Dot;
            public byte URA0;
            public byte URA1;
            public byte URA2;
            public double ISCp;
            public double ISCd;
            public byte IntStatFlag;
        }

        public struct AlmData
        {
            public double toa;
            public double sqrtA;
            public double e;
            public double omega;
            public double M0;
            public double Omega0;
            public double OmegaDot;
            public double i0;
            public double a0;
            public double a1;
            public int wn;
        }

        public static EphData[] d_eph = new EphData[32];
        public static EphData[] d_ephBD = new EphData[34];

        public static AlmData[] d_alm_gps = new AlmData[32];
        public static AlmData[] d_alm_bds = new AlmData[32];

        #endregion

        #region 方法定义

        // 计算卫星位置
        public static eceft SatPosCal(int satNum, double tTime, EphData d_eph, int sys, int week)
        {
            double ei, ea, diff, ta, aol, delr, delal, delinc, r, inc;
            double la, xp, yp, bclk, tc, d_toc, d_toe;
            double satang;
            double az;
            eceft result;
            short m_times = 0;
            double Azimuth;		//Azimuth,Elevation 为方位角仰角的中间计算量
            double dot_E, dot_phi, dot_i, dot_r, dot_u, dot_X, dot_Y, dot_Wc;
            double cosE, sinE, cos2Phi, sin2Phi;
            double tr;		// 相对论效应
            double dot_tr;  // 相对论效应校正量的一阶导数
            double omegae = 7.2921151467E-5;
            double omegae_bds = 7.292115E-5;
            double omge;
            double mu_gps = 3.986005e14;
            double mu_bds = 3.986004418E14;
            double mu;
            double SIN_5 = -0.0871557427476582;
            double COS_5 = 0.9961946980917456;
            double xg, yg, zg, sino, coso;

            int d_wn = week - (int)d_eph.week + 1;

            d_toc = tTime - d_eph.toe.sec;
            d_toc = d_toc + d_wn * 604800;

            if (d_toc > 302400.0)
            {
                d_toc = d_toc - 604800.0;
            }
            else if (d_toc < -302400.0)
            {
                d_toc = d_toc + 604800.0;
            }

            //卫星时钟校正多项式，tgd双频校正
            bclk = d_eph.af0 + d_eph.af1 * d_toc + d_eph.af2 * d_toc * d_toc - d_eph.Tgd1;

            tc = tTime - bclk;

            //计算当前时间距星历参考时间的距离
            d_toe = tc - d_eph.toe.sec;
            d_toe = d_toe + d_wn * 604800;

            if (d_toe > 302400.0)
            {
                d_toe = d_toe - 604800.0;
            }
            else if (d_toe < -302400.0)
            {
                d_toe = d_toe + 604800.0;
            }

            switch (sys)
            {
                case 1: mu = mu_bds; omge = omegae_bds; break;
                default: mu = mu_gps; omge = omegae; break;
            }

            //改正平均角速度
            double wm = Math.Sqrt(mu) / (d_eph.sqrt_A * d_eph.sqrt_A * d_eph.sqrt_A) + d_eph.delta_n;

            ei = d_eph.M0 + d_toe * wm;
            ea = ei;

            //迭代求出偏近点角ea
            do
            {
                m_times++;
                if (m_times > 30)
                {
                    break;
                }

                diff = (ei - (ea - d_eph.e * Math.Sin(ea))) / (1 - d_eph.e * Math.Cos(ea));
                ea = ea + diff;
            } while (Math.Abs(diff) > 1.0e-12);

            cosE = Math.Cos(ea);
            sinE = Math.Sin(ea);
            dot_E = (wm + d_eph.delta_n) / (1 - d_eph.e * cosE);

            bclk = bclk - 4.442807633E-10 * d_eph.e * d_eph.sqrt_A * sinE;//相对论校正
            result.tb = bclk;//最终的卫星钟差

            ta = Math.Atan2(Math.Sqrt(1.00 - (d_eph.e * d_eph.e)) * sinE, cosE - d_eph.e);//真近点角

            aol = ta + d_eph.omega;   //升焦距角
            dot_phi = Math.Sqrt(1 - d_eph.e * d_eph.e) * dot_E / (1 - d_eph.e * cosE);

            cos2Phi = Math.Cos(2.0 * aol);
            sin2Phi = Math.Sin(2.0 * aol);


            //轨道的二次谐波校正
            delr = d_eph.Crc * cos2Phi + d_eph.Crs * sin2Phi;//半径校正值
            delal = d_eph.Cuc * cos2Phi + d_eph.Cus * sin2Phi;//纬度校正值
            delinc = d_eph.Cic * cos2Phi + d_eph.Cis * sin2Phi;//倾角校正值

            r = (d_eph.sqrt_A * d_eph.sqrt_A) * (1.00 - d_eph.e * cosE) + delr;  	//经校正的半径值
            aol = aol + delal;												//经校正的纬度值
            inc = d_eph.i0 + delinc + d_eph.IDOT * d_toe;			//经校正的纬度值

            xp = r * Math.Cos(aol);												//在轨道平面中的x位置
            yp = r * Math.Sin(aol);												//在轨道平面中的y位置

            if (sys == 1 && satNum <= 5) // sys == 1 : bds
            {
                la = d_eph.OMEGA0 + (d_eph.OMEGADot) * d_toe - omge * d_eph.toe.sec;//经校正的升交点经度

                xg = xp * Math.Cos(la) - yp * Math.Cos(inc) * Math.Sin(la);
                yg = xp * Math.Sin(la) + yp * Math.Cos(inc) * Math.Cos(la);
                zg = yp * Math.Sin(inc);

                sino = Math.Sin(omge * d_toe);
                coso = Math.Cos(omge * d_toe);

                result.x = xg * coso + yg * sino * COS_5 + zg * sino * SIN_5;
                result.y = -xg * sino + yg * coso * COS_5 + zg * coso * SIN_5;
                result.z = -yg * SIN_5 + zg * COS_5;

                result.dx = 0;
                result.dy = 0;
                result.dz = 0;
            }
            else
            {
                la = d_eph.OMEGA0 + (d_eph.OMEGADot - omge) * d_toe - omge * d_eph.toe.sec;//经校正的升交点经度

                result.x = xp * Math.Cos(la) - yp * Math.Cos(inc) * Math.Sin(la);							//ECEF中的x坐标
                result.y = xp * Math.Sin(la) + yp * Math.Cos(inc) * Math.Cos(la);							//ECEF中的y坐标
                result.z = yp * Math.Sin(inc);											//ECEF中的z坐标

                dot_u = (1 + 2 * (d_eph.Cus * cos2Phi - d_eph.Cuc * sin2Phi)) * dot_phi;
                //经校正的纬度变化率
                dot_r = d_eph.sqrt_A * d_eph.sqrt_A * d_eph.e * sinE * dot_E + 2 * (d_eph.Crs * cos2Phi - d_eph.Crc * sin2Phi) * dot_phi;
                //经校正的半径变化率
                dot_i = 2 * (d_eph.Cis * cos2Phi - d_eph.Cic * sin2Phi) * dot_phi + d_eph.IDOT;
                //经校正的倾角变化率

                dot_Wc = d_eph.OMEGADot - omge;

                dot_X = dot_r * Math.Cos(aol) - yp * dot_u;		//轨道平面内的x坐标变化率
                dot_Y = dot_r * Math.Sin(aol) + xp * dot_u;		//轨道平面内的y坐标变化率

                result.dx = dot_X * Math.Cos(la) - dot_Y * Math.Cos(inc) * Math.Sin(la) + result.z * Math.Sin(la) * dot_i
                                        - result.y * dot_Wc;
                result.dy = dot_X * Math.Sin(la) + dot_Y * Math.Cos(inc) * Math.Cos(la) - result.z * Math.Cos(la) * dot_i
                                        + result.x * dot_Wc;
                result.dz = dot_Y * Math.Sin(inc) + yp * Math.Cos(inc) * dot_i;
            }

            result.tmp = Math.Cos(la);
            return result;
        }

        // 计算卫星位置
        public static eceft SatPosCalAlm   (int satNum, double tTime, AlmData d_alm, int sys, int week)
        {
            double ei, ea, diff, ta, aol, delr, delal, delinc, r, inc;
            double la, xp, yp, bclk, tc, tk;
            double satang;
            double az;
            eceft result;
            short m_times = 0;
            double Azimuth;		//Azimuth,Elevation 为方位角仰角的中间计算量
            double dot_E, dot_phi, dot_i, dot_r, dot_u, dot_X, dot_Y, dot_Wc;
            double cosE, sinE, cos2Phi, sin2Phi;
            double tr;		// 相对论效应
            double dot_tr;  // 相对论效应校正量的一阶导数
            double omegae = 7.2921151467E-5;
            double omegae_bds = 7.292115E-5;
            double omge;
            double mu_gps = 3.986005e14;
            double mu_bds = 3.986004418E14;
            double mu;
            double SIN_5 = -0.0871557427476582;
            double COS_5 = 0.9961946980917456;
            double xg, yg, zg, sino, coso;
            int d_wn = week - d_alm.wn;
            double i0_d;

            tk = tTime - d_alm.toa;
            tk = tTime - d_alm.toa + (d_wn + 1) * 604800;

            if (tk > 302400.0)
            {
                tk = tk - 604800.0;
            }
            else if (tk < -302400.0)
            {
                tk = tk + 604800.0;
            }

            switch (sys)
            {
                case 1: mu = mu_bds; omge = omegae_bds; break;
                default: mu = mu_gps; omge = omegae; break;
            }

            //改正平均角速度
            double wm = Math.Sqrt(mu) / (d_alm.sqrtA * d_alm.sqrtA * d_alm.sqrtA);

            ei = d_alm.M0 + tk * wm;
            ea = ei;

            //迭代求出偏近点角ea
            do
            {
                m_times++;
                if (m_times > 30)
                {
                    break;
                }

                diff = (ei - (ea - d_alm.e * Math.Sin(ea))) / (1 - d_alm.e * Math.Cos(ea));
                ea = ea + diff;
            } while (Math.Abs(diff) > 1.0e-12);

            cosE = Math.Cos(ea);
            sinE = Math.Sin(ea);

            ta = Math.Atan2(Math.Sqrt(1.00 - (d_alm.e * d_alm.e)) * sinE, cosE - d_alm.e);//真近点角

            aol = ta + d_alm.omega;   //升焦距角

            r = (d_alm.sqrtA * d_alm.sqrtA) * (1.00 - d_alm.e * cosE);  	//经校正的半径值

            xp = r * Math.Cos(aol);												//在轨道平面中的x位置
            yp = r * Math.Sin(aol);												//在轨道平面中的y位置

            la = d_alm.Omega0 + (d_alm.OmegaDot - omge) * tk - omge * d_alm.toa;//经校正的升交点经度

            if (sys == 1 && satNum > 5)
            {
                i0_d = d_alm.i0 + 0.3 * 3.1415926;
            }
            else
            {
                i0_d = d_alm.i0;
            }
            result.x = xp * Math.Cos(la) - yp * Math.Cos(i0_d) * Math.Sin(la);							//ECEF中的x坐标
            result.y = xp * Math.Sin(la) + yp * Math.Cos(i0_d) * Math.Cos(la);							//ECEF中的y坐标
            result.z = yp * Math.Sin(i0_d);											//ECEF中的z坐标
            result.dx = 0;
            result.dy = 0;
            result.dz = 0;
            result.tb = 0;
            result.tmp = 0;

            return result;
        }

        public static void ReadEph()
        {
            int idx;

            //string strFilePath = "brdm2350.17p";
            string strFilePath = eph_file_loc;
            FileStream fs = new FileStream(strFilePath, FileMode.Open, FileAccess.Read);
            StreamReader read = new StreamReader(fs, Encoding.Default);
            string strReadline;
            while ((strReadline = read.ReadLine()) != null)
            {
                flag1 = (strReadline[0] == 'G' && strReadline[4] == '2' && strReadline[15] == '2' && strReadline[16] == '2');
                flag2 = (strReadline[0] == 'G' && strReadline[4] == '2' && strReadline[15] == '2' && strReadline[16] == '1');

                flag3 = (strReadline[0] == 'C' && strReadline[15] == '2' && strReadline[16] == '3');

                if (flag1 || flag2)
                {
                    idx = ushort.Parse(strReadline.Substring(1, 2)) - 1;
                    d_eph[idx].signalId = ushort.Parse(strReadline.Substring(1, 2));
                    d_eph[idx].af0 = double.Parse(strReadline.Substring(23, 19));
                    d_eph[idx].af1 = double.Parse(strReadline.Substring(42, 19));
                    d_eph[idx].af2 = double.Parse(strReadline.Substring(61, 19));

                    // second line
                    strReadline = read.ReadLine();
                    double tmp = double.Parse(strReadline.Substring(4, 19));
                    d_eph[idx].IODE2 = (byte)(tmp);
                    d_eph[idx].Crs = double.Parse(strReadline.Substring(23, 19));
                    d_eph[idx].delta_n = double.Parse(strReadline.Substring(42, 19));
                    d_eph[idx].M0 = double.Parse(strReadline.Substring(61, 19));

                    // third line
                    strReadline = read.ReadLine();
                    d_eph[idx].Cuc = double.Parse(strReadline.Substring(4, 19));
                    d_eph[idx].e = double.Parse(strReadline.Substring(23, 19));
                    d_eph[idx].Cus = double.Parse(strReadline.Substring(42, 19));
                    d_eph[idx].sqrt_A = double.Parse(strReadline.Substring(61, 19));

                    // fourth line
                    strReadline = read.ReadLine();
                    d_eph[idx].toe.sec = double.Parse(strReadline.Substring(4, 19));
                    d_eph[idx].Cic = double.Parse(strReadline.Substring(23, 19));
                    d_eph[idx].OMEGA0 = double.Parse(strReadline.Substring(42, 19));
                    d_eph[idx].Cis = double.Parse(strReadline.Substring(61, 19));

                    // fifth line
                    strReadline = read.ReadLine();
                    d_eph[idx].i0 = double.Parse(strReadline.Substring(4, 19));
                    d_eph[idx].Crc = double.Parse(strReadline.Substring(23, 19));
                    d_eph[idx].omega = double.Parse(strReadline.Substring(42, 19));
                    d_eph[idx].OMEGADot = double.Parse(strReadline.Substring(61, 19));

                    // sixth line
                    strReadline = read.ReadLine();
                    d_eph[idx].IDOT = double.Parse(strReadline.Substring(4, 19));
                    tmp = double.Parse(strReadline.Substring(23, 19));
                    d_eph[idx].l2CodeFlag = (int)(tmp);
                    tmp = double.Parse(strReadline.Substring(42, 19));
                    d_eph[idx].week = (uint)(tmp);
                    tmp = double.Parse(strReadline.Substring(61, 19));
                    d_eph[idx].l2DataFlag = (int)(tmp);

                    // seventh line
                    strReadline = read.ReadLine();
                    tmp = double.Parse(strReadline.Substring(4, 19));
                    d_eph[idx].URA = (int)(tmp);
                    tmp = double.Parse(strReadline.Substring(23, 19));
                    d_eph[idx].health = (int)(tmp);
                    d_eph[idx].Tgd1 = double.Parse(strReadline.Substring(42, 19));
                    tmp = double.Parse(strReadline.Substring(61, 19));
                    d_eph[idx].IODC = (ushort)(tmp);

                }

                if (flag3)
                {
                    idx = ushort.Parse(strReadline.Substring(1, 2)) - 1;
                    d_ephBD[idx].signalId = ushort.Parse(strReadline.Substring(1, 2));
                    d_ephBD[idx].af0 = double.Parse(strReadline.Substring(23, 19));
                    d_ephBD[idx].af1 = double.Parse(strReadline.Substring(42, 19));
                    d_ephBD[idx].af2 = double.Parse(strReadline.Substring(61, 19));

                    // second line
                    strReadline = read.ReadLine();
                    double tmp = double.Parse(strReadline.Substring(4, 19));
                    d_ephBD[idx].IODE2 = (byte)(tmp);
                    d_ephBD[idx].Crs = double.Parse(strReadline.Substring(23, 19));
                    d_ephBD[idx].delta_n = double.Parse(strReadline.Substring(42, 19));
                    d_ephBD[idx].M0 = double.Parse(strReadline.Substring(61, 19));

                    // third line
                    strReadline = read.ReadLine();
                    d_ephBD[idx].Cuc = double.Parse(strReadline.Substring(4, 19));
                    d_ephBD[idx].e = double.Parse(strReadline.Substring(23, 19));
                    d_ephBD[idx].Cus = double.Parse(strReadline.Substring(42, 19));
                    d_ephBD[idx].sqrt_A = double.Parse(strReadline.Substring(61, 19));

                    // fourth line
                    strReadline = read.ReadLine();
                    d_ephBD[idx].toe.sec = double.Parse(strReadline.Substring(4, 19));
                    d_ephBD[idx].Cic = double.Parse(strReadline.Substring(23, 19));
                    d_ephBD[idx].OMEGA0 = double.Parse(strReadline.Substring(42, 19));
                    d_ephBD[idx].Cis = double.Parse(strReadline.Substring(61, 19));

                    // fifth line
                    strReadline = read.ReadLine();
                    d_ephBD[idx].i0 = double.Parse(strReadline.Substring(4, 19));
                    d_ephBD[idx].Crc = double.Parse(strReadline.Substring(23, 19));
                    d_ephBD[idx].omega = double.Parse(strReadline.Substring(42, 19));
                    d_ephBD[idx].OMEGADot = double.Parse(strReadline.Substring(61, 19));

                    // sixth line
                    strReadline = read.ReadLine();
                    d_ephBD[idx].IDOT = double.Parse(strReadline.Substring(4, 19));
                    tmp = double.Parse(strReadline.Substring(23, 19));
                    d_ephBD[idx].l2CodeFlag = (int)(tmp);
                    tmp = double.Parse(strReadline.Substring(42, 19));
                    d_ephBD[idx].week = (uint)(tmp);
                    tmp = double.Parse(strReadline.Substring(61, 19));
                    d_ephBD[idx].l2DataFlag = (int)(tmp);

                    // seventh line
                    strReadline = read.ReadLine();
                    tmp = double.Parse(strReadline.Substring(4, 19));
                    d_ephBD[idx].URA = (int)(tmp);
                    tmp = double.Parse(strReadline.Substring(23, 19));
                    d_ephBD[idx].health = (int)(tmp);
                    d_ephBD[idx].Tgd1 = double.Parse(strReadline.Substring(42, 19));
                    tmp = double.Parse(strReadline.Substring(61, 19));
                    d_ephBD[idx].IODC = (ushort)(tmp);

                }
            }

            fs.Close();
            read.Close();
        }

        public static void ReadAlm()
        {
            int idx = 0;

            string strFilePath = alm_file_loc;
            FileStream fs = new FileStream(strFilePath, FileMode.Open, FileAccess.Read);
            StreamReader read = new StreamReader(fs, Encoding.Default);
            string strReadline;
            while ((strReadline = read.ReadLine()) != null)
            {
                if (strReadline.Length > 0)
                    flag1 = (strReadline[0] == '*');

                if (flag1)
                {
                    strReadline = read.ReadLine();
                    idx = ushort.Parse(strReadline.Substring(28, 2)) - 1;

                    strReadline = read.ReadLine();
                    strReadline = read.ReadLine();
                    d_alm_gps[idx].e = double.Parse(strReadline.Substring(27, 18));

                    strReadline = read.ReadLine();
                    d_alm_gps[idx].toa = double.Parse(strReadline.Substring(27, 10));

                    strReadline = read.ReadLine();
                    d_alm_gps[idx].i0 = double.Parse(strReadline.Substring(27, 13));

                    strReadline = read.ReadLine();
                    d_alm_gps[idx].OmegaDot = double.Parse(strReadline.Substring(27, 18));

                    strReadline = read.ReadLine();
                    d_alm_gps[idx].sqrtA = double.Parse(strReadline.Substring(27, 12));

                    strReadline = read.ReadLine();
                    d_alm_gps[idx].Omega0 = double.Parse(strReadline.Substring(27, 18));

                    strReadline = read.ReadLine();
                    d_alm_gps[idx].omega = double.Parse(strReadline.Substring(27, 12));

                    strReadline = read.ReadLine();
                    d_alm_gps[idx].M0 = double.Parse(strReadline.Substring(27, 18));

                    strReadline = read.ReadLine();
                    d_alm_gps[idx].a0 = double.Parse(strReadline.Substring(27, 18));

                    strReadline = read.ReadLine();
                    d_alm_gps[idx].a1 = double.Parse(strReadline.Substring(27, 18));

                    strReadline = read.ReadLine();
                    d_alm_gps[idx].wn = int.Parse(strReadline.Substring(28, 4));

                    flag1 = false;

                    //Console.WriteLine(d_alm_gps[idx].a0);
                }
            }

            fs.Close();
            read.Close();
        }

        public static void ReadAlmBD()
        {
            int idx;
            string strFilePath = "./alm/alm_bds_n.txt";
            FileStream fs = new FileStream(strFilePath, FileMode.Open, FileAccess.Read);
            StreamReader read = new StreamReader(fs, Encoding.Default);
            string strReadline;
            while ((strReadline = read.ReadLine()) != null)
            {
                idx = ushort.Parse(strReadline.Substring(0, 2)) - 1;
                d_alm_bds[idx].toa = double.Parse(strReadline.Substring(9, 6));
                d_alm_bds[idx].sqrtA = double.Parse(strReadline.Substring(16, 9));
                d_alm_bds[idx].e = double.Parse(strReadline.Substring(26, 11));
                d_alm_bds[idx].omega = double.Parse(strReadline.Substring(38, 12));
                d_alm_bds[idx].M0 = double.Parse(strReadline.Substring(51, 12));
                d_alm_bds[idx].Omega0 = double.Parse(strReadline.Substring(64, 12));
                d_alm_bds[idx].OmegaDot = double.Parse(strReadline.Substring(77, 12)) / 1e7;
                d_alm_bds[idx].i0 = double.Parse(strReadline.Substring(90, 12));
                d_alm_bds[idx].a0 = double.Parse(strReadline.Substring(103, 12)) / 1e3;
                d_alm_bds[idx].a1 = double.Parse(strReadline.Substring(116, 12)) / 1e10;
                d_alm_bds[idx].wn = int.Parse(strReadline.Substring(131, 5)) + 256 * 2;
            }
        }


        #endregion
    }
}
