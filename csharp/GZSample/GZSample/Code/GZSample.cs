using ICSharpCode.SharpZipLib.GZip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SharpZipLib
{
    public class GZSample
    {
        //-------------------------------------------------------------------------
        public static void WriteFile(string filePath, byte[] bytes)
        {
            try
            {
                using (FileStream fs = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                {
                    fs.Write(bytes, 0, bytes.Length);
                    fs.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("WriteFile: " + ex.Message);
            }
        }
        //-------------------------------------------------------------------------
        public static byte[] ReadFile(string filePath)
        {
            var buffer = new byte[0];
            try
            {
                using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    int length = (int)fs.Length;
                    buffer = new byte[length];
                    int count;
                    int sum = 0;
                    while ((count = fs.Read(buffer, sum, length - sum)) > 0)
                    {
                        sum += count;
                    }
                    fs.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ReadFile: " + ex.Message);
            }

            return buffer;
        }
        //-------------------------------------------------------------------------
        public static byte[] CompressGZIP(byte[] buf)
        {
            if (null == buf || 0 == buf.Length)
            {

                Console.WriteLine("CompressGZIP buf is nil");
                return buf;
            }

            byte[] bufCompress = buf;
            try
            {
                MemoryStream ms = new MemoryStream();
                GZipOutputStream outStream = new GZipOutputStream(ms);
                outStream.Write(buf, 0, buf.Length);
                outStream.Flush();
                outStream.Finish();

                bufCompress = ms.GetBuffer();
                Array.Resize(ref bufCompress, (int)outStream.Length);

                outStream.Close();
                ms.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("CompressGZIP 错误:" + ex.Message);
            }

            return bufCompress;
        }
        //-------------------------------------------------------------------------
        public static string DeCompressGZIP(byte[] bufCompress)
        {
            if (null == bufCompress || 0 == bufCompress.Length)
            {

                Console.WriteLine("DeCompressGZIP bufCompress is nil");
                return string.Empty;
            }

            MemoryStream msDe = null;
            GZipInputStream inStream = null;
            MemoryStream msCom = null;
            long currentIndex = 0;
            try
            {
                msDe = new MemoryStream();
                msDe.Write(bufCompress, 0, bufCompress.Length);
                //msDe.Flush();
                msDe.Seek(0, SeekOrigin.Begin);

                inStream = new GZipInputStream(msDe);

                // 一次读取 8KB
                int nSize = 1024 * 8;
                byte[] bufCom = new byte[nSize];
                msCom = new MemoryStream();


                while (true)
                {
                    int numRead = inStream.Read(bufCom, 0, nSize);
                    if (numRead <= 0)
                    {
                        break;
                    }
                    // 写进去
                    msCom.Write(bufCom, 0, numRead);
                    currentIndex += numRead;
                    if (numRead < nSize)
                    {
                        break;
                    }
                }

                byte[] bufDeCompress = msCom.GetBuffer();

                return Encoding.UTF8.GetString(bufDeCompress);
            }
            catch (Exception ex)
            {
                Console.WriteLine("DeCompressGZIP 异常(1/2),可能ZIP出错 可能ZIP文件解压后大小刚好是缓冲区的N倍:" + ex.Message);
                Console.WriteLine("DeCompressGZIP 异常(2/2),bufCompress.Length:" + bufCompress.Length + ",currentIndex:" + currentIndex);

                if (null != msCom)
                {
                    byte[] bufDeCompress = msCom.GetBuffer();
                    return Encoding.UTF8.GetString(bufDeCompress);
                }
            }
            finally
            {
                if (null != inStream)
                {
                    inStream.Close();
                }
                if (null != msDe)
                {
                    msDe.Close();
                }
                if (null != msCom)
                {
                    msCom.Close();
                }
            }

            return string.Empty;

        }
        //-------------------------------------------------------------------------
        public static string CompressGZIPBase64(string strValue, out bool bSuccess)
        {
            bSuccess = false;
            if (true == string.IsNullOrEmpty(strValue))
            {

                Console.WriteLine("CompressGZIPBase64 strValue is nil");
                return string.Empty;
            }

            string str = strValue;
            try
            {
                byte[] buf = Encoding.UTF8.GetBytes(strValue);
                MemoryStream ms = new MemoryStream();
                GZipOutputStream outStream = new GZipOutputStream(ms);
                outStream.Write(buf, 0, buf.Length);
                outStream.Flush();
                outStream.Finish();

                byte[] bufCompress = ms.GetBuffer();
                //说明：过滤掉尾部无效0数据
                Array.Resize(ref bufCompress, (int)outStream.Length);

                // 0X1F 0X8B 0X08 0X00
                // 31 139 8 0
                if (0x1F == bufCompress[0] && 0x8B == bufCompress[1]/* && 0x08 == bufCompress[2] && 0x00 == bufCompress[3]*/)
                {
                    str = Convert.ToBase64String(bufCompress);
                    bSuccess = true;
                }
                else
                {
                    Console.WriteLine("CompressGZIPBase64 ERROR:" + bufCompress[0] + "," + bufCompress[1] + "," + bufCompress[2] + "," + bufCompress[3]);
                }

                //LoggerHelper.Error("CompressGZIPBase64 0 :" + bufCompress[0]);
                //LoggerHelper.Error("CompressGZIPBase64 1 :" + bufCompress[1]);
                //LoggerHelper.Error("CompressGZIPBase64 2 :" + bufCompress[2]);
                //LoggerHelper.Error("CompressGZIPBase64 3 :" + bufCompress[3]);
                // byte[] bufCompress2 = Convert.FromBase64String(str);

                outStream.Close();
                ms.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("CompressGZIPBase64 错误:" + ex.Message);
            }


            return str;
        }
        //-------------------------------------------------------------------------
        public static string DeCompressGZIPBase64(string strValue)
        {
            if (true == string.IsNullOrEmpty(strValue))
            {
                Console.WriteLine("DeCompressGZIPBase64 strValue is nil");
                return string.Empty;
            }

            string str = strValue;
            MemoryStream msDe = null;
            GZipInputStream inStream = null;
            MemoryStream msCom = null;
            try
            {
                byte[] bufCompress = Convert.FromBase64String(strValue);
                msDe = new MemoryStream();
                msDe.Write(bufCompress, 0, bufCompress.Length);
                //msDe.Flush();
                msDe.Seek(0, SeekOrigin.Begin);

                inStream = new GZipInputStream(msDe);

                // 一次读取 8KB
                int nSize = 1024 * 8;
                byte[] bufCom = new byte[nSize];
                msCom = new MemoryStream();

                long currentIndex = 0;

                while (true)
                {
                    int numRead = inStream.Read(bufCom, 0, nSize);
                    if (numRead <= 0)
                    {
                        break;
                    }
                    // 写进去
                    msCom.Write(bufCom, 0, numRead);
                    currentIndex += numRead;
                    if (numRead < nSize)
                    {
                        break;
                    }
                }

                byte[] bufDeCompress = msCom.GetBuffer();
                str = Encoding.UTF8.GetString(bufDeCompress);


            }
            catch (Exception ex)
            {
                Console.WriteLine("DeCompressGZIPBase64 异常,可能ZIP出错 可能ZIP文件解压后大小刚好是缓冲区的N倍:" + ex.Message);

                if (null != msCom)
                {
                    byte[] bufDeCompress = msCom.GetBuffer();
                    str = Encoding.UTF8.GetString(bufDeCompress);
                }

            }
            finally
            {
                if (null != inStream)
                {
                    inStream.Close();
                }
                if (null != msDe)
                {
                    msDe.Close();
                }
                if (null != msCom)
                {
                    msCom.Close();
                }
            }

            return str;

        }
        //-------------------------------------------------------------------------
        public static void CompressGZIPDemo()
        {
            Console.WriteLine("CompressGZIPDemo:");
            var gzipBytes = CompressGZIP(Encoding.UTF8.GetBytes("csharp"));
            Console.WriteLine("gzContent:" + Encoding.UTF8.GetString(gzipBytes));

            var outPath = System.Environment.CurrentDirectory + "/../../Res/gz/csharp.gz";
            WriteFile(outPath, gzipBytes);
            //-------------------------------------------------------------------------
            var fileBytes = ReadFile(outPath);
            var unzipContent = DeCompressGZIP(fileBytes);
            Console.WriteLine("ungzContent:" + unzipContent);
            WriteFile(outPath, gzipBytes);
            Console.WriteLine("========================================");
        }
        //-------------------------------------------------------------------------
        public static void CompressGZIPBase64Demo()
        {
            Console.WriteLine("CompressGZIPBase64Demo:");
            var success = false;
            var gzipContent = CompressGZIPBase64("csharp base64", out success);
            Console.WriteLine("gzBase64Content=" + gzipContent);

            var outPath = System.Environment.CurrentDirectory + "/../../Res/gz/csharp_base64.gz";
            WriteFile(outPath, Encoding.UTF8.GetBytes(gzipContent));
            //-------------------------------------------------------------------------
            var fileBytes = ReadFile(outPath);
            var unzipContent = DeCompressGZIPBase64(Encoding.UTF8.GetString(fileBytes));
            Console.WriteLine("ungzBase64Content:" + unzipContent);
            Console.WriteLine("========================================");
        }
        //-------------------------------------------------------------------------
        public static void DeCompressGZIPDemo()
        {
            Console.WriteLine("DeCompressGZIPDemo:");
            var filePath = "F:/js.gz";
            var buffer = ReadFile(filePath);
            Console.WriteLine("gzContent:" + Encoding.UTF8.GetString(buffer));

            var unzipContent = DeCompressGZIP(buffer);
            Console.WriteLine("ungzContent:" + unzipContent);

            Console.WriteLine("========================================");
        }
        //-------------------------------------------------------------------------
        public static void DeCompressGZIPBase64Demo()
        {
            Console.WriteLine("DeCompressGZIPBase64Demo:");
            var filePath = "F:/js_base64.gz";
            var buffer = ReadFile(filePath);
            Console.WriteLine("gzBase64Content:" + Encoding.UTF8.GetString(buffer));

            var unzipContent = DeCompressGZIPBase64(Encoding.UTF8.GetString(buffer));
            Console.WriteLine("ungzBase64Content:" + unzipContent);
            Console.WriteLine("========================================");
        }
        //-------------------------------------------------------------------------
        public static void Main(string[] args)
        {
            CompressGZIPDemo();
            CompressGZIPBase64Demo();
            DeCompressGZIPDemo();
            DeCompressGZIPBase64Demo();

            Console.Read();
        }
    }
}
