using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

using System.Xml;
using System.Diagnostics;
using SqlServer;

namespace Common
{
    public class CommonService
    {
        /// <summary>
        /// 转全角的函数(SBC case)
        /// </summary>
        /// <param name="input">任意字符串</param>
        /// <returns>全角字符串</returns>
        ///<remarks>
        ///全角空格为12288,半角空格为32
        ///其他字符半角(33-126)与全角(65281-65374)的对应关系是：均相差65248
        ///</remarks>        
        public string ToSBC(string input)
        {
            //半角转全角：
            char[] c = input.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == 32)
                {
                    c[i] = (char)12288;
                    continue;
                }
                if (c[i] < 127)
                    c[i] = (char)(c[i] + 65248);
            }
            return new string(c);
        }

        /// <summary>
        /// 获取本机IP
        /// </summary>
        /// <returns></returns>
        public static List<string> GetLocIPAddress()
        {
            List<string> locAddress = new List<string>();
            try
            {
                string HostName = Dns.GetHostName();
                IPHostEntry MyEntry = Dns.GetHostEntry(HostName);

                foreach (IPAddress ipadd in MyEntry.AddressList)
                {
                    if (!ipadd.IsIPv6LinkLocal)
                    {
                        locAddress.Add(ipadd.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                MyLog.WriteLog(ex);
            }
            return locAddress;
        }



        /// <summary>
        /// 解压ZIP
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        private static byte[] DecompressZIP(byte[] data)
        {
            try
            {
                MemoryStream ms = new MemoryStream(data);
                Stream zipStream = null;
                zipStream = new GZipStream(ms, CompressionMode.Decompress);
                byte[] dc_data = null;
                dc_data = EtractBytesFormStream(zipStream, data.Length);
                return dc_data;
            }
            catch
            {
                return null;
            }
        }
        private static byte[] EtractBytesFormStream(Stream zipStream, int dataBlock)
        {
            try
            {
                byte[] data = null;
                int totalBytesRead = 0;
                while (true)
                {
                    Array.Resize(ref data, totalBytesRead + dataBlock + 1);
                    int bytesRead = zipStream.Read(data, totalBytesRead, dataBlock);
                    if (bytesRead == 0)
                    {
                        break;
                    }
                    totalBytesRead += bytesRead;
                }
                Array.Resize(ref data, totalBytesRead);
                return data;
            }
            catch
            {
                return null;
            }
        }


        /// <summary>
        /// Byte数组转换为图像
        /// </summary>
        public static System.Drawing.Image byteArrayToImage(byte[] byteArrayIn)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn);
            System.Drawing.Image returnImage = System.Drawing.Image.FromStream(ms);
            return returnImage;
        }


        /// <summary>
        /// 找出ILIST中符合条件的第一个实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tList"></param>
        /// <param name="PropertyName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T FindFirst<T>(IList<T> tList, string PropertyName, object value)
        {
            System.Reflection.PropertyInfo[] propertyInfoList = typeof(T).GetProperties();

            foreach (T t in tList)
            {
                foreach (System.Reflection.PropertyInfo property in propertyInfoList)
                {
                    if (property.Name == PropertyName)
                    {
                        object valueT = property.GetValue(t, null);
                        if (value.Equals(valueT))
                        {
                            return t;
                        }
                    }
                }
            }
            return default(T);
        }

        #region 分组
        public static Dictionary<Tkey, IList<T>> GroupBy<Tkey, T>(IList<T> lis, string groupby)
        {
            System.Reflection.PropertyInfo[] myPropertyInfo = typeof(T).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            PropertyInfo pi = null;
            foreach (PropertyInfo p in myPropertyInfo)
            {
                if (p.Name.Equals(groupby))
                {
                    pi = p;
                    break;
                }
            }
            if (pi == null)
            {
                throw new Exception("无效属性" + groupby);
            }
            Dictionary<Tkey, IList<T>> dic = new Dictionary<Tkey, IList<T>>();
            foreach (T t in lis)
            {
                Tkey o = (Tkey)pi.GetValue(t, null);
                if (!dic.ContainsKey(o))
                {
                    IList<T> li = new List<T>();
                    li.Add(t);
                    dic.Add(o, li);
                }
                else
                {
                    dic[o].Add(t);
                }
            }
            return dic;
        }
        #endregion
        #region 总计
        public static decimal Sum<T>(IList<T> Lis, string SumBy)
        {
            if (Lis == null)
            {
                return 0;
            }
            System.Reflection.PropertyInfo[] myPropertyInfo = typeof(T).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            PropertyInfo pi = null;
            foreach (PropertyInfo p in myPropertyInfo)
            {
                if (p.Name.Equals(SumBy))
                {
                    pi = p;
                    break;
                }
            }
            if (pi == null)
            {
                throw new Exception("无效属性" + SumBy);
            }
            decimal total = 0;
            foreach (T t in Lis)
            {
                decimal v = Convert.ToDecimal(pi.GetValue(t, null));
                total += v;
            }
            return total;
        }
        #endregion
        #region 快速排序
        public static void QuickSort<T>(IList<T> ints, string orderBy)
        {
            System.Reflection.PropertyInfo[] myPropertyInfo = typeof(T).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            PropertyInfo pi = null;
            foreach (PropertyInfo p in myPropertyInfo)
            {
                if (p.Name.Equals(orderBy))
                {
                    pi = p;
                    break;
                }
            }
            if (pi == null)
            {
                throw new Exception("无效属性" + orderBy);
            }
            QuickSort<T>(ints, 0, ints.Count - 1, pi, new DeleCompareAB<T>(CompareAMoreThenB));
        }
        public static void QuickSortDesc<T>(IList<T> ints, string orderBy)
        {
            System.Reflection.PropertyInfo[] myPropertyInfo = typeof(T).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            PropertyInfo pi = null;
            foreach (PropertyInfo p in myPropertyInfo)
            {
                if (p.Name.Equals(orderBy))
                {
                    pi = p;
                    break;
                }
            }
            if (pi == null)
            {
                throw new Exception("无效属性" + orderBy);
            }
            QuickSort<T>(ints, 0, ints.Count - 1, pi, new DeleCompareAB<T>(CompareALessThenB));
        }
        static void QuickSort<T>(IList<T> ints, int i, int j, PropertyInfo pi, DeleCompareAB<T> CompareAB)
        {
            T tmp;
            int start = i, end = j;
            bool add = true;

            while (i < j)
            {
                if (add)
                {
                    if (CompareAB(ints, i, j, pi))

                    //if ((ints[i]) > ints[j])
                    {
                        tmp = ints[i];
                        ints[i] = ints[j];
                        ints[j] = tmp;
                        add = false;
                        j--;
                    }
                    else
                        i++;
                }
                else
                {
                    if (CompareAB(ints, i, j, pi))
                    {
                        tmp = ints[i];
                        ints[i] = ints[j];
                        ints[j] = tmp;
                        add = true;
                        i++;
                    }
                    else
                        j--;
                }

            }
            if (i > start + 1)
            {
                QuickSort(ints, start, i - 1, pi, CompareAB);

            } if (i < end - 1)
            {
                QuickSort(ints, i + 1, end, pi, CompareAB);
            }
        }
        delegate bool DeleCompareAB<T>(IList<T> ints, int i, int j, PropertyInfo pi);
        /// <summary>
        /// 第i个T的pi的值大于第j个T的pi的值?
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ints"></param>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <param name="pi"></param>
        /// <returns></returns>
        private static bool CompareAMoreThenB<T>(IList<T> ints, int i, int j, PropertyInfo pi)
        {
            if (pi.PropertyType == typeof(DateTime) || pi.PropertyType == typeof(DateTime?))
            {
                return Convert.ToDateTime(pi.GetValue(ints[i], null)) > Convert.ToDateTime(pi.GetValue(ints[j], null));
            }
            else if (pi.PropertyType == typeof(int) || pi.PropertyType == typeof(decimal) || pi.PropertyType == typeof(double) || pi.PropertyType == typeof(float))
            {
                return Convert.ToDouble(pi.GetValue(ints[i], null)) > Convert.ToDouble(pi.GetValue(ints[j], null));
            }
            else
                return string.Compare((string)pi.GetValue(ints[i], null), (string)pi.GetValue(ints[j], null)) > 0;
        }
        /// <summary>
        /// 第j个T的pi的值大于第i个T的pi的值?
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ints"></param>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <param name="pi"></param>
        /// <returns></returns>
        private static bool CompareALessThenB<T>(IList<T> ints, int i, int j, PropertyInfo pi)
        {
            if (pi.PropertyType == typeof(DateTime) || pi.PropertyType == typeof(DateTime?))
            {
                return Convert.ToDateTime(pi.GetValue(ints[i], null)) < Convert.ToDateTime(pi.GetValue(ints[j], null));
            }
            else if (pi.PropertyType == typeof(int) || pi.PropertyType == typeof(decimal) || pi.PropertyType == typeof(double) || pi.PropertyType == typeof(float))
            {
                return Convert.ToDouble(pi.GetValue(ints[i], null)) < Convert.ToDouble(pi.GetValue(ints[j], null));
            }
            else
                return string.Compare((string)pi.GetValue(ints[i], null), (string)pi.GetValue(ints[j], null)) < 0;
        }
        #endregion

        /// <summary>
        /// 是否包含英文字母
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool ContainLetters(string str)
        {
            bool result = false;
            string newStr = str.ToUpper();
            char[] arr = str.ToCharArray();
            foreach (char c in arr)
            {
                if (Char.IsLetter(c))
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// 修改配置文件
        /// </summary>
        /// <param name="AppKey"></param>
        /// <param name="AppValue"></param>
        public static void OptConfig(string AppKey, string AppValue)
        {
            try
            {
                Assembly Asm = Assembly.GetExecutingAssembly();
                XmlDocument xmlDoc = new XmlDocument();

                string m_strFullPath = Asm.Location.Substring(0, (Asm.Location.LastIndexOf("//") + 1)) + "CarSecretary.exe.config";
                xmlDoc.Load(m_strFullPath);

                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(m_strFullPath);
                XmlNode xNode;
                XmlElement xElem1;
                XmlElement xElem2;
                xNode = xDoc.SelectSingleNode("//appSettings");
                xElem1 = (XmlElement)xNode.SelectSingleNode("//add[@key='" + AppKey + "']");
                if (xElem1 != null)
                {
                    xElem1.SetAttribute("value", AppValue);
                }
                else
                {
                    xElem2 = xDoc.CreateElement("add");
                    xElem2.SetAttribute("key", AppKey);
                    xElem2.SetAttribute("value", AppValue);
                    xNode.AppendChild(xElem2);
                }
                xDoc.Save(m_strFullPath);
            }
            catch (System.NullReferenceException NullEx)
            {
                throw NullEx;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public static string GetMachineCode()
        {
            string tmpUint32 = "";
            string tmpUint32_1 = string.Empty;
            try
            {
                System.Management.ManagementObjectSearcher cmicWmi = new System.Management.ManagementObjectSearcher("SELECT   *   FROM   Win32_DiskDrive");
                foreach (System.Management.ManagementObject cmicWmiObj in cmicWmi.Get())
                {
                    tmpUint32 = cmicWmiObj["signature"].ToString();
                    break;
                }

            }
            catch (Exception ex)
            {
               // CarLog.WriteLog(ex);


            }
            //获取cpu序列号   
            try
            {
                System.Management.ManagementObjectSearcher Wmi = new System.Management.ManagementObjectSearcher("SELECT   *   FROM   Win32_Processor");

                foreach (System.Management.ManagementObject WmiObj in Wmi.Get())
                {
                    tmpUint32_1 = WmiObj["ProcessorId"].ToString();
                    break;
                }
            }
            catch (Exception ex)
            {
                //CarLog.WriteLog(ex);


            }
            string mechinecodeGetFromNet = tmpUint32_1 + tmpUint32;
            return mechinecodeGetFromNet;
        }

        public static string GetHostNameProcessID()
        {
            string HostName = System.Net.Dns.GetHostName();
            Process pro = Process.GetCurrentProcess();
            string UserProgressID = HostName + pro.Id;
            return UserProgressID;
        }


      

        /// <summary>
        /// 转半角的函数(DBC case)
        /// </summary>
        /// <param name="input">任意字符串</param>
        /// <returns>半角字符串</returns>
        ///<remarks>
        ///全角空格为12288,半角空格为32
        ///其他字符半角(33-126)与全角(65281-65374)的对应关系是：均相差65248
        ///</remarks>
        public string ToDBC(string input)
        {
            char[] c = input.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == 12288)
                {
                    c[i] = (char)32;
                    continue;
                }
                if (c[i] > 65280 && c[i] < 65375)
                    c[i] = (char)(c[i] - 65248);
            }
            return new string(c);
        }



   


        /// <summary>
        /// 获取参数值
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetParamValue(string name)
        {
            string sql = "SELECT param_value FROM SP_Params WHERE param_name=@paramname";
            SqlParameter[] pas = new SqlParameter[]
            {
            new SqlParameter("@paramname",SqlDbType.VarChar,150)
            };
            pas[0].Value = name;
            object rows = SqlHelper.ExecuteScalar(sql, pas);
            if (rows != System.DBNull.Value)
            {
                return rows.ToString();
            }
            else
            {
                return " ";
            }
        }

        /// <summary>
        /// 设置参数名
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool SetParamName(string name, string value)
        {
            string sql = "UPDATE SP_Params Set param_value=@paramvalue WHERE param_name=@paramname";
            SqlParameter[] pas = new SqlParameter[]
            {
            new SqlParameter("@paramvalue",SqlDbType.Text),
            new SqlParameter("@paramname",SqlDbType.VarChar,150)
            };
            pas[0].Value = value;
            pas[1].Value = name;
            int rows = SqlHelper.ExecuteNonQuery(sql, pas);
            return (rows > 0);
        }


        public static void CloseProcess(string StrNameID)
        {
            System.Diagnostics.Process[] CloseID = System.Diagnostics.Process.GetProcessesByName(StrNameID);

            if (CloseID.Length != 0)
            {
                for (int i = 0; i < CloseID.Length; i++)
                {
                    if (CloseID[i].Responding && !CloseID[i].HasExited)
                    {
                        System.Console.WriteLine("指定进程存在而且正在响应中...正在关闭.");
                        CloseID[i].CloseMainWindow();
                        if (!CloseID[i].HasExited)
                        {
                            System.Console.WriteLine("由于特别原因无法关闭进程,现在强制关闭!!!");
                            CloseID[i].Kill();
                        }
                    }
                    else
                    {
                        System.Console.WriteLine("指定进程存在但无法响应...正在强制关闭!");
                        CloseID[i].Kill();
                    }
                }
            }
            else
                System.Console.WriteLine("指定进程不存在无法关闭!请确认输入正确.");
        }


        /// <param name="v">要进行处理的数据</param>
        /// <param name="x">保留的小数位数</param>
        /// <returns>四舍五入后的结果</returns>
        public static double Round(double v, int x)
        {
            string value = v.ToString("f" + x.ToString());
            return Convert.ToDouble(value);
            //double vt = Math.Pow(10, x);
            //double vx = v * vt;
            //vx += 0.5;
            //return (Math.Floor(vx) / vt);
        }

        /// <summary>
        /// 四舍五入,精确到毛
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static decimal Round(decimal v)
        {
            string vstr = v.ToString("0.00");
            string[] xx = vstr.Split('.');
            int _intPart = int.Parse(xx[0]);

            int _0X = int.Parse(vstr.Substring(vstr.Length - 1));
            int _X0 = int.Parse(vstr.Substring(vstr.Length - 2, 1));
            if (_0X >= 5)
            {
                _X0 += 1;
            }
            if (_X0 > 9)
            {
                _intPart += 1;
                _X0 = 0;
            }
            return decimal.Parse(_intPart + "." + _X0 + "0");
        }

        #region 根据SQL获取ILIST<T>

        public static object ConvertDBNull(object objValue, Type targetType)
        {
            object targetValue;
            if (objValue == DBNull.Value)
            {
                switch (targetType.Name)
                {
                    case "Byte":
                    case "Int16":
                    case "Int32":
                    case "Decimal":
                        targetValue = 0;
                        break;
                    case "Single":
                        targetValue = 0F;
                        break;
                    case "Double":
                        targetValue = 0.0;
                        break;
                    case "String":
                        targetValue = string.Empty;
                        break;
                    case "Boolean":
                        targetValue = false;
                        break;
                    default:
                        targetValue = 0;
                        break;
                }
            }
            else
            {
                targetValue = objValue;
            }

            return targetValue;
        }

        public delegate string DeleObjectName2ColumnName(System.Reflection.PropertyInfo property);
        public static IList<T> GetManyTListBySQL<T>(string sql, DeleObjectName2ColumnName ObjectName2ColumnName, params SqlParameter[] parms)
        {
            IList<T> lists = new List<T>();

            using (SqlDataReader rdr = SqlHelper.ExecuteReader(sql, parms))
            {
                DataView dv = rdr.GetSchemaTable().DefaultView;
                while (rdr.Read())
                {
                    T objectT = (T)Activator.CreateInstance(typeof(T));

                    SetProperties(rdr, objectT, ObjectName2ColumnName, dv);


                    lists.Add(objectT);
                }
            }

            return lists;
        }
        public static IList<T> GetManyTListBySQL<T>(string sql, params SqlParameter[] parms)
        {
            IList<T> lists = new List<T>();

            using (SqlDataReader rdr = SqlHelper.ExecuteReader(sql, parms))
            {
                DataView dv = rdr.GetSchemaTable().DefaultView;
                while (rdr.Read())
                {
                    T objectT = (T)Activator.CreateInstance(typeof(T));

                    SetProperties(rdr, objectT, ObjectName2ColumnName, dv);


                    lists.Add(objectT);
                }
            }

            return lists;
        }
        static string ObjectName2ColumnName(System.Reflection.PropertyInfo property)
        {
            string columnName;
            switch (property.Name)
            {
                default:
                    columnName = property.Name;
                    break;
            }
            return columnName;
        }
        public static void SetProperties(System.Data.IDataReader dataReader, object Obj, DeleObjectName2ColumnName ObjectName2ColumnName, DataView dv)
        {
            System.Reflection.PropertyInfo[] propertyInfoList = Obj.GetType().GetProperties();

            string columnName;
            foreach (System.Reflection.PropertyInfo property in propertyInfoList)
            {
                if (property.CanWrite)
                {

                    columnName = ObjectName2ColumnName(property);

                    object objValue = null;

                    try
                    {
                        dv.RowFilter = "ColumnName='" + columnName + "'";
                        if (dv.Count > 0)
                            objValue = dataReader[columnName];
                    }
                    catch
                    {
                        objValue = null;
                    }

                    if (objValue != null && objValue != DBNull.Value)
                    {
                        objValue = ConvertDBNull(objValue, property.PropertyType);


                        if (property.PropertyType == typeof(decimal) &&
                            objValue.GetType() != typeof(decimal))
                        {
                            property.SetValue(Obj, Convert.ToDecimal(objValue), null);
                        }
                        else if (property.PropertyType == typeof(int) &&
                                objValue.GetType() != typeof(int))
                        {
                            property.SetValue(Obj, Convert.ToInt32(objValue), null);
                        }
                        else if (property.PropertyType == typeof(bool) && objValue.GetType() != typeof(bool))
                        {
                            property.SetValue(Obj, Convert.ToBoolean(objValue), null);
                        }
                        else
                        {
                            property.SetValue(Obj, objValue, null);
                        }
                    }
                }
            }

        }
        #endregion


    }
}
