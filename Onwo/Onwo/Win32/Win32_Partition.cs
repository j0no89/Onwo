using System;
using System.Management;
using System.Threading.Tasks;

//using MyTv.Entities;

namespace Onwo.Win32
{
    public class Win32_Partition
    {
        public static async Task<Win32_Partition> Create(ManagementBaseObject partition)
        {
            var result=new Win32_Partition();

            var str = $"associators of {{Win32_DiskPartition.DeviceID='{partition["DeviceID"]}'}} where AssocClass= Win32_LogicalDiskToPartition";
            var searcher = new ManagementObjectSearcher(str).Get();
            int cnt = searcher.Count;
            if (cnt == 0)
                return null;
            foreach (var logical in searcher)
            {
                str = $"select * from Win32_LogicalDisk where Name='{logical["Name"]}'";
                var volumeEnumerator = new ManagementObjectSearcher(str).Get().GetEnumerator();
                volumeEnumerator.MoveNext();

                ManagementObject volume = (ManagementObject) volumeEnumerator.Current;
                result.VolumeSize = (long)(ulong)volume["Size"];
                result.FreeSpace = (long)(ulong)volume["FreeSpace"];
                result.VolumeName = (string)volume["VolumeName"] ?? "";
                string driveStr = (string)volume["Name"];
                result.DriveChar = driveStringToChar(driveStr);
                result.Index = (int) (uint) (partition["Index"] ?? -1);
                result.Serial = volume["VolumeSerialNumber"].ToString();
                result.StartOffset = (long) (ulong) partition["StartingOffset"];
                
                object ser = logical["VolumeSerialNumber"];
                string serStr = ser?.ToString() ?? "";
            }
            return result;

        }
        private static char driveStringToChar(String driveString)
        {
            if (string.IsNullOrEmpty(driveString))
                throw new Exception("Error: partition does not have a drive letter");
            if (driveString.Length > 1 && driveString[1] != ':')
                throw new Exception("Error: unexpected drive name string");

            char driveChar = driveString[0];
            if (!char.IsLetter(driveChar))
                throw new Exception("Error: unexpected drive name string");
            return char.ToUpper(driveChar);
        }
        private Win32_Partition() { }
        public long VolumeSize;
        public long FreeSpace;
        public string VolumeName;
        public char DriveChar;
        public int Index;
        public string Serial;
        public long StartOffset;
        public Win32_DiskDrive Drive;
    }
}