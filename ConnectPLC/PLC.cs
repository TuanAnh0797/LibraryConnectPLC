using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TALibrary
{
    public class PLC
    {
        byte[] TemplateRead = { 0x50, 0x00, 0x00, 0xff, 0xff, 0x03, 0x00, 0x0C, 0x00, 0x00, 0x00, 0x01, 0x04, 0x01, 0x00 };
        byte[] TemplateWrite = { 0x50, 0x00, 0x00, 0xff, 0xff, 0x03, 0x00, 0x0D, 0x00, 0x00, 0x00, 0x01, 0x14, 0x01, 0x00 };
        byte[] TempleteWritePath1 = { 0x50, 0x00, 0x00, 0xff, 0xff, 0x03, 0x00 };
        byte[] TempleteWritePath3 = { 0x00, 0x00, 0x01, 0x14, 0x00, 0x00 };
        public PLC()
        {
          
        }
        public async Task<bool> ReadBit(NetworkStream stream, int TimeOut, string typedevice, int device)
        {
            byte[] DataReciveFromPLC = new byte[100];
            byte[] CmdSendPLC = new byte[21];
            byte[] TemplateRead = { 0x50, 0x00, 0x00, 0xff, 0xff, 0x03, 0x00, 0x0C, 0x00, 0x00, 0x00, 0x01, 0x04, 0x01, 0x00 };
            byte[] Device = BitConverter.GetBytes(device);
            byte[] TypeDevice = Converttextdevicetohexdevice(typedevice);
            byte[] NumberByte = BitConverter.GetBytes(1);
            Buffer.BlockCopy(TemplateRead, 0, CmdSendPLC, 0, TemplateRead.Length);
            Buffer.BlockCopy(Device, 0, CmdSendPLC, TemplateRead.Length, 3);
            Buffer.BlockCopy(TypeDevice, 0, CmdSendPLC, TemplateRead.Length + 3, 1);
            Buffer.BlockCopy(NumberByte, 0, CmdSendPLC, TemplateRead.Length + 4, 2);
            CancellationTokenSource CancellationToken = new CancellationTokenSource();
            Task writeTask = stream.WriteAsync(CmdSendPLC, 0, CmdSendPLC.Length, CancellationToken.Token);
            if (await Task.WhenAny(writeTask, Task.Delay(TimeOut, CancellationToken.Token)) != writeTask)
            {
                CancellationToken.Cancel();
                throw new TimeoutException($"{typedevice}{device} Error Trigger Exist Send timed out.");
            }
            await writeTask;
            Task<int> readTask = stream.ReadAsync(DataReciveFromPLC, 0, DataReciveFromPLC.Length, CancellationToken.Token);
            if (await Task.WhenAny(readTask, Task.Delay(TimeOut, CancellationToken.Token)) != readTask)
            {
                CancellationToken.Cancel();
                throw new TimeoutException($"{typedevice}{device} Error Trigger Exist Send timed out.");
            }
            int bytesRead = await readTask;
            if (DataReciveFromPLC[9] == 0 && DataReciveFromPLC[10] == 0)
            {
                int rs = BitConverter.ToInt32(DataReciveFromPLC, 11);
                if (rs != 0)
                {
                    return true;
                }
            }
            return false;
        }
        public async Task<object> ReadData(NetworkStream stream, int TimeOut, string typedevice, int device,int numberbyte, string datatype)
        {
            byte[] DataReciveFromPLC = new byte[100];
            byte[] CmdReadDataPLC = new byte[21];
            byte[] Device = BitConverter.GetBytes(device);
            byte[] TypeDevice = Converttextdevicetohexdevice(typedevice);
            byte[] NumberByte = BitConverter.GetBytes(numberbyte);
            Buffer.BlockCopy(TemplateRead, 0, CmdReadDataPLC, 0, TemplateRead.Length);
            Buffer.BlockCopy(Device, 0, CmdReadDataPLC, TemplateRead.Length, 3);
            Buffer.BlockCopy(TypeDevice, 0, CmdReadDataPLC, TemplateRead.Length + 3, 1);
            Buffer.BlockCopy(NumberByte, 0, CmdReadDataPLC, TemplateRead.Length + 4, 2);
            CancellationTokenSource CancellationToken = new CancellationTokenSource();
            Task writeTask = stream.WriteAsync(CmdReadDataPLC, 0, CmdReadDataPLC.Length, CancellationToken.Token);
            if (await Task.WhenAny(writeTask, Task.Delay(TimeOut, CancellationToken.Token)) != writeTask)
            {
                CancellationToken.Cancel();
                throw new TimeoutException($"{typedevice}{device} Error Trigger Exist Send timed out.");
            }
            await writeTask;
            Task<int> readTask = stream.ReadAsync(DataReciveFromPLC, 0, DataReciveFromPLC.Length, CancellationToken.Token);
            if (await Task.WhenAny(readTask, Task.Delay(TimeOut, CancellationToken.Token)) != readTask)
            {
                CancellationToken.Cancel();
                throw new TimeoutException($"{typedevice}{device} Error Trigger Exist Send timed out.");
            }
            int bytesRead = await readTask;
            if (DataReciveFromPLC[9] == 0 && DataReciveFromPLC[10] == 0)
            {
                if (datatype.ToUpper() == "FLOAT")
                {
                    byte[] buff1 = new byte[] { DataReciveFromPLC[11], DataReciveFromPLC[12], DataReciveFromPLC[13], DataReciveFromPLC[14] };
                    float fl = BitConverter.ToSingle(buff1, 0);
                    return fl;
                }
                else if (datatype.ToUpper() == "DEC")
                {
                    byte[] buff1 = new byte[] { DataReciveFromPLC[11], DataReciveFromPLC[12], DataReciveFromPLC[13], DataReciveFromPLC[14] };
                    int Dec = BitConverter.ToInt32(buff1, 0);
                    return Dec;
                }
                else
                {
                    string datastr = Encoding.ASCII.GetString(DataReciveFromPLC, 11, findnull(DataReciveFromPLC)).Trim('\0').Trim('\r').Trim('\n');
                }
            }
            return null;
        }

        public async Task WriteBit(NetworkStream stream, int TimeOut, string typedevice, int device)
        {
            byte[] DataReciveFromPLC = new byte[100];
            byte[] On = { 0x10 };
            byte[] CmdSendPLC = new byte[22];
            byte[] Device = BitConverter.GetBytes(device);
            byte[] TypeDevice = Converttextdevicetohexdevice(typedevice);
            byte[] NumberofDeviceClear = { 0x01, 0x00 };
            Buffer.BlockCopy(TemplateWrite, 0, CmdSendPLC, 0, TemplateWrite.Length);
            Buffer.BlockCopy(Device, 0, CmdSendPLC, TemplateWrite.Length, 3);
            Buffer.BlockCopy(TypeDevice, 0, CmdSendPLC, TemplateWrite.Length + 3, 1);
            Buffer.BlockCopy(NumberofDeviceClear, 0, CmdSendPLC, TemplateWrite.Length + 4, 2);
            Buffer.BlockCopy(On, 0, CmdSendPLC, TemplateWrite.Length + 6, 1);

            CancellationTokenSource CancellationToken = new CancellationTokenSource();
            Task writeTask = stream.WriteAsync(CmdSendPLC, 0, CmdSendPLC.Length, CancellationToken.Token);
            if (await Task.WhenAny(writeTask, Task.Delay(TimeOut, CancellationToken.Token)) != writeTask)
            {
                CancellationToken.Cancel();
                throw new TimeoutException($"{typedevice}{device} Error Trigger Exist Send timed out.");
            }
            await writeTask;
            Task<int> readTask = stream.ReadAsync(DataReciveFromPLC, 0, DataReciveFromPLC.Length, CancellationToken.Token);
            if (await Task.WhenAny(readTask, Task.Delay(TimeOut, CancellationToken.Token)) != readTask)
            {
                CancellationToken.Cancel();
                throw new TimeoutException($"{typedevice}{device} Error Trigger Exist Send timed out.");
            }
            await readTask;

        }
        public async Task WriteASCII(NetworkStream stream, int TimeOut, string Devicestr, int HeaderDeviceint, int NumberofDeviceint, string datawrite)
        {
            byte[] DataReciveFromPLC = new byte[100];
            StringBuilder strb = new StringBuilder();
            byte[] HeaderDevice = BitConverter.GetBytes(HeaderDeviceint);
            byte[] Device = Converttextdevicetohexdevice(Devicestr);
            byte[] NumberofDevice = BitConverter.GetBytes(NumberofDeviceint);
            byte[] datawritetoplc;

            if (datawrite.Length / 2 != NumberofDeviceint)
            {
                strb.Clear();
                strb.Append(datawrite);
                int lenghtdatawrite = datawrite.Length;
                for (int i = 0; i < NumberofDeviceint * 2 - lenghtdatawrite; i++)
                {
                    strb.Append("\u0000");
                }
                datawrite = strb.ToString();
                datawritetoplc = Encoding.ASCII.GetBytes(datawrite);
            }
            else
            {
                datawritetoplc = Encoding.ASCII.GetBytes(datawrite);
            }
            byte[] path2 = BitConverter.GetBytes(TempleteWritePath3.Length + 6 + datawritetoplc.Length);
            byte[] CmdSendPLC = new byte[21 + datawritetoplc.Length];
            Buffer.BlockCopy(TempleteWritePath1, 0, CmdSendPLC, 0, TempleteWritePath1.Length);
            Buffer.BlockCopy(path2, 0, CmdSendPLC, 7, 2);
            Buffer.BlockCopy(TempleteWritePath3, 0, CmdSendPLC, 9, TempleteWritePath3.Length);
            Buffer.BlockCopy(HeaderDevice, 0, CmdSendPLC, 9 + TempleteWritePath3.Length, 3);
            Buffer.BlockCopy(Device, 0, CmdSendPLC, 12 + TempleteWritePath3.Length, 1);
            Buffer.BlockCopy(NumberofDevice, 0, CmdSendPLC, 13 + TempleteWritePath3.Length, 2);
            Buffer.BlockCopy(datawritetoplc, 0, CmdSendPLC, 15 + TempleteWritePath3.Length, datawritetoplc.Length);

            CancellationTokenSource CancellationToken = new CancellationTokenSource();
            Task writeTask = stream.WriteAsync(CmdSendPLC, 0, CmdSendPLC.Length, CancellationToken.Token);
            if (await Task.WhenAny(writeTask, Task.Delay(TimeOut, CancellationToken.Token)) != writeTask)
            {
                CancellationToken.Cancel();
                throw new TimeoutException($"{Devicestr}{HeaderDeviceint} Error Trigger Exist Send timed out.");
            }
            await writeTask;
            Task<int> readTask = stream.ReadAsync(DataReciveFromPLC, 0, DataReciveFromPLC.Length, CancellationToken.Token);
            if (await Task.WhenAny(readTask, Task.Delay(TimeOut, CancellationToken.Token)) != readTask)
            {
                CancellationToken.Cancel();
                throw new TimeoutException($"{Devicestr}{HeaderDeviceint} Error Trigger Exist Send timed out.");
            }
            await readTask;
        }
        public async Task WriteDEC(NetworkStream stream, int TimeOut, string Devicestr, int HeaderDeviceint, int datawrite)
        {
            byte[] DataReciveFromPLC = new byte[100];
            byte[] HeaderDevice = BitConverter.GetBytes(HeaderDeviceint);
            byte[] Device = Converttextdevicetohexdevice(Devicestr);
            byte[] NumberofDevice = BitConverter.GetBytes(2);
            byte[] datawritetoplc;
            datawritetoplc = BitConverter.GetBytes(datawrite);
            byte[] path2 = BitConverter.GetBytes(TempleteWritePath3.Length + 6 + datawritetoplc.Length);
            byte[] CmdSendPLC = new byte[21 + datawritetoplc.Length];
            Buffer.BlockCopy(TempleteWritePath1, 0, CmdSendPLC, 0, TempleteWritePath1.Length);
            Buffer.BlockCopy(path2, 0, CmdSendPLC, 7, 2);
            Buffer.BlockCopy(TempleteWritePath3, 0, CmdSendPLC, 9, TempleteWritePath3.Length);
            Buffer.BlockCopy(HeaderDevice, 0, CmdSendPLC, 9 + TempleteWritePath3.Length, 3);
            Buffer.BlockCopy(Device, 0, CmdSendPLC, 12 + TempleteWritePath3.Length, 1);
            Buffer.BlockCopy(NumberofDevice, 0, CmdSendPLC, 13 + TempleteWritePath3.Length, 2);
            Buffer.BlockCopy(datawritetoplc, 0, CmdSendPLC, 15 + TempleteWritePath3.Length, datawritetoplc.Length);
            CancellationTokenSource CancellationToken = new CancellationTokenSource();
            Task writeTask = stream.WriteAsync(CmdSendPLC, 0, CmdSendPLC.Length, CancellationToken.Token);
            if (await Task.WhenAny(writeTask, Task.Delay(TimeOut, CancellationToken.Token)) != writeTask)
            {
                CancellationToken.Cancel();
                throw new TimeoutException($"{Devicestr}{HeaderDeviceint} Error Trigger Exist Send timed out.");
            }
            await writeTask;
            Task<int> readTask = stream.ReadAsync(DataReciveFromPLC, 0, DataReciveFromPLC.Length, CancellationToken.Token);
            if (await Task.WhenAny(readTask, Task.Delay(TimeOut, CancellationToken.Token)) != readTask)
            {
                CancellationToken.Cancel();
                throw new TimeoutException($"{Devicestr}{HeaderDeviceint} Error Trigger Exist Send timed out.");
            }
            await readTask;
        }
        public async Task WriteFloat(NetworkStream stream, int TimeOut, string Devicestr, int HeaderDeviceint, int datawrite)
        {
            byte[] DataReciveFromPLC = new byte[100];
            byte[] HeaderDevice = BitConverter.GetBytes(HeaderDeviceint);
            byte[] Device = Converttextdevicetohexdevice(Devicestr);
            byte[] NumberofDevice = BitConverter.GetBytes(2);
            byte[] datawritetoplc;
            datawritetoplc = BitConverter.GetBytes(datawrite);
            byte[] path2 = BitConverter.GetBytes(TempleteWritePath3.Length + 6 + datawritetoplc.Length);
            byte[] CmdSendPLC = new byte[21 + datawritetoplc.Length];
            Buffer.BlockCopy(TempleteWritePath1, 0, CmdSendPLC, 0, TempleteWritePath1.Length);
            Buffer.BlockCopy(path2, 0, CmdSendPLC, 7, 2);
            Buffer.BlockCopy(TempleteWritePath3, 0, CmdSendPLC, 9, TempleteWritePath3.Length);
            Buffer.BlockCopy(HeaderDevice, 0, CmdSendPLC, 9 + TempleteWritePath3.Length, 3);
            Buffer.BlockCopy(Device, 0, CmdSendPLC, 12 + TempleteWritePath3.Length, 1);
            Buffer.BlockCopy(NumberofDevice, 0, CmdSendPLC, 13 + TempleteWritePath3.Length, 2);
            Buffer.BlockCopy(datawritetoplc, 0, CmdSendPLC, 15 + TempleteWritePath3.Length, datawritetoplc.Length);
            CancellationTokenSource CancellationToken = new CancellationTokenSource();
            Task writeTask = stream.WriteAsync(CmdSendPLC, 0, CmdSendPLC.Length, CancellationToken.Token);
            if (await Task.WhenAny(writeTask, Task.Delay(TimeOut, CancellationToken.Token)) != writeTask)
            {
                CancellationToken.Cancel();
                throw new TimeoutException($"{Devicestr}{HeaderDeviceint} Error Trigger Exist Send timed out.");
            }
            await writeTask;
            Task<int> readTask = stream.ReadAsync(DataReciveFromPLC, 0, DataReciveFromPLC.Length, CancellationToken.Token);
            if (await Task.WhenAny(readTask, Task.Delay(TimeOut, CancellationToken.Token)) != readTask)
            {
                CancellationToken.Cancel();
                throw new TimeoutException($"{Devicestr}{HeaderDeviceint} Error Trigger Exist Send timed out.");
            }
            await readTask;
        }
        public static byte[] Converttextdevicetohexdevice(string namedevice)
        {
            byte[] bytereturn = null;
            byte[] X = { 0x9C };
            byte[] Y = { 0x9D };
            byte[] M = { 0x90 };
            byte[] L = { 0x92 };
            byte[] B = { 0xA0 };
            byte[] D = { 0xA8 };
            byte[] W = { 0xB4 };
            byte[] ZR = { 0xB0 };

            Dictionary<string, byte[]> data = new Dictionary<string, byte[]>();
            data.Add("X", X);
            data.Add("Y", Y);
            data.Add("M", M);
            data.Add("L", L);
            data.Add("B", B);
            data.Add("D", D);
            data.Add("W", W);
            data.Add("ZR", ZR);

            foreach (var item in data)
            {
                if (namedevice == item.Key)
                {
                    bytereturn = item.Value;
                }
            }
            return bytereturn;
        }
        public int findnull(byte[] input)
        {
            int index = 0;
            for (int i = 11; i < input.Length; i++)
            {

                if (input[i] < 33 || input[i] > 126)
                {
                    return index;
                }
                index++;
            }
            return 80;
        }
    }
    public class Db_connect
    {
        public static string connection_string = "";
        public static DataTable Exc_GetTable(string query_object, CommandType type, params object[] obj)
        {
            using (SqlConnection conn = new SqlConnection(connection_string))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(query_object, conn);
                cmd.CommandType = type;
                SqlCommandBuilder.DeriveParameters(cmd);
                for (int i = 1; i <= obj.Length; i++)
                {
                    cmd.Parameters[i].Value = obj[i - 1];
                }
                SqlDataAdapter dap = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                dap.Fill(ds);
                conn.Close();
                return ds.Tables[0];
            }
        }
        public static object Exc_GetScalra(string query_object, CommandType type, params object[] obj)
        {
            using (SqlConnection conn = new SqlConnection(connection_string))
            {
                Object data;
                conn.Open();
                SqlCommand cmd = new SqlCommand(query_object, conn);
                cmd.CommandType = type;
                SqlCommandBuilder.DeriveParameters(cmd);
                for (int i = 1; i <= obj.Length; i++)
                {
                    cmd.Parameters[i].Value = obj[i - 1];
                }
                data = cmd.ExecuteScalar();
                conn.Close();
                return data;
            }
        }
        public static int Exc_NonQuery(string query_object, CommandType type, params object[] obj)
        {
            using (SqlConnection conn = new SqlConnection(connection_string))
            {
                int data;
                conn.Open();
                SqlCommand cmd = new SqlCommand(query_object, conn);
                cmd.CommandType = type;
                SqlCommandBuilder.DeriveParameters(cmd);
                for (int i = 1; i <= obj.Length; i++)
                {
                    cmd.Parameters[i].Value = obj[i - 1];
                }
                data = cmd.ExecuteNonQuery();
                conn.Close();
                return data;
            }
        }
        public static void Exc_BulkCoppy(string NameTableDatabase,DataTable datatable)
        {
            using (SqlConnection connection = new SqlConnection(connection_string))
            {
                connection.Open();
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                {
                    bulkCopy.DestinationTableName = NameTableDatabase;
                    bulkCopy.WriteToServer(datatable);
                }
                connection.Close();
            }
        }

    }
   

}
