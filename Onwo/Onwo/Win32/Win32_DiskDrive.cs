using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Threading.Tasks;

//using MyTv.Entities;

namespace Onwo.Win32
{
    public class Win32_DiskDrive
    {
        public static async Task<IList<Win32_DiskDrive>> GetLocalDrives()
        {
            var driveObjects = await Task.Run(()=> new ManagementObjectSearcher("select * from Win32_DiskDrive").Get()
                .Cast<ManagementBaseObject>()
                .ToArray());
            var driveTasks = driveObjects.Select(d => Task.Run(() => GetInfo(d))).ToArray();
            var driveArray = await Task.WhenAll(driveTasks).ConfigureAwait(false);
            return driveArray.ToList();
        }

        public static async Task<Win32_DiskDrive> GetInfo(ManagementBaseObject drive)
        {
            var result=new Win32_DiskDrive();
            result.Serial = drive["SerialNumber"]?.ToString().Trim() ?? "";
            result.InterfaceType = drive["InterfaceType"]?.ToString();
            result.IsUsb = string.Equals(result.InterfaceType, "usb", StringComparison.InvariantCultureIgnoreCase);
            result.Model = drive["Model"]?.ToString() ?? "";

            var sig = drive["Signature"];
            int signature = 0;
            if (sig is uint)
                signature = (int)(uint)sig;
            result.Signature = signature;

            bool nullSerial = string.IsNullOrEmpty(result.Serial);

            if (nullSerial && signature == 0)
                throw new Exception(
                    "Error: could not find a Serial or Signature for drive so there is no way to uniquely identify it.");

            uint numPartitions = (uint)(drive["Partitions"] ?? 0);
            //uint index = (uint) (drive["Index"] ?? 0);
            result.SizeInBytes = (long)(ulong)(drive["Size"] ?? 0);
            string str = $"associators of {{Win32_DiskDrive.DeviceID='{drive["DeviceID"]}'}} where AssocClass = Win32_DiskDriveToDiskPartition";
            var partitionCllection_w32 = await Task.Run(()=> new ManagementObjectSearcher(str).Get()
                .Cast<ManagementBaseObject>()
                .ToArray());
            var partitionTasks = partitionCllection_w32.Select(p => Task.Run(() => Win32_Partition.Create(p))).ToArray();
            result.Partitions = (await Task.WhenAll(partitionTasks).ConfigureAwait(false))
                .Where(win32_part=>win32_part!=null)
                .ToArray();
            result.NumPartitions = (uint)result.Partitions.Length;
            result.Partitions.ForEach(part => part.Drive = result);

            return result;
        }
        private Win32_DiskDrive() { }
        public string Serial;
        public string InterfaceType;
        public bool IsUsb;
        public string Model;
        public int Signature;
        public uint NumPartitions;
        public long SizeInBytes;
        public Win32_Partition[] Partitions;
    }
}
