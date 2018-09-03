namespace Djlastnight.Hid
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.InteropServices;
    using Djlastnight.Win32;
    using Djlastnight.Win32.Win32Hid;
    using Djlastnight.Win32.Win32RawInput;

    /// <summary>
    /// Provide some utility functions for raw input handling.
    /// </summary>
    internal static class RawInputHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rawInputHandle"></param>
        /// <param name="rawInput"></param>
        /// <param name="rawInputBuffer">Caller must free up memory on the pointer using Marshal.FreeHGlobal</param>
        /// <returns></returns>
        public static bool GetRawInputData(IntPtr rawInputHandle, ref RAWINPUT rawInput, ref IntPtr rawInputBuffer)
        {
            bool success = true;
            rawInputBuffer = IntPtr.Zero;

            try
            {
                uint size = 0;
                uint sizeOfHeader = (uint)Marshal.SizeOf(typeof(RAWINPUTHEADER));

                // Get the size of our raw input data.
                Win32.Win32RawInput.NativeMethods.GetRawInputData(rawInputHandle, Contants.RID_INPUT, IntPtr.Zero, ref size, sizeOfHeader);

                // Allocate a large enough buffer
                rawInputBuffer = Marshal.AllocHGlobal((int)size);

                // Now read our RAWINPUT data
                if (Win32.Win32RawInput.NativeMethods.GetRawInputData(rawInputHandle, Contants.RID_INPUT, rawInputBuffer, ref size, (uint)Marshal.SizeOf(typeof(RAWINPUTHEADER))) != size)
                {
                    return false;
                }

                // Cast our buffer
                rawInput = (RAWINPUT)Marshal.PtrToStructure(rawInputBuffer, typeof(RAWINPUT));
            }
            catch
            {
                Debug.WriteLine("GetRawInputData failed!");
                success = false;
            }

            return success;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceHandle"></param>
        /// <param name="deviceInfo"></param>
        /// <returns></returns>
        public static bool GetDeviceInfo(IntPtr deviceHandle, ref RID_DEVICE_INFO deviceInfo)
        {
            bool success = true;
            IntPtr deviceInfoBuffer = IntPtr.Zero;
            try
            {
                // Get Device Info
                uint deviceInfoSize = (uint)Marshal.SizeOf(typeof(RID_DEVICE_INFO));
                deviceInfoBuffer = Marshal.AllocHGlobal((int)deviceInfoSize);

                int res = Win32.Win32RawInput.NativeMethods.GetRawInputDeviceInfo(deviceHandle, RawInputDeviceInfoType.RIDI_DEVICEINFO, deviceInfoBuffer, ref deviceInfoSize);
                if (res <= 0)
                {
                    Debug.WriteLine("WM_INPUT could not read device info: " + Marshal.GetLastWin32Error().ToString());
                    return false;
                }

                // Cast our buffer
                deviceInfo = (RID_DEVICE_INFO)Marshal.PtrToStructure(deviceInfoBuffer, typeof(RID_DEVICE_INFO));
            }
            catch
            {
                Debug.WriteLine("GetRawInputData failed!");
                success = false;
            }
            finally
            {
                // Always executes, prevents memory leak
                Marshal.FreeHGlobal(deviceInfoBuffer);
            }

            return success;
        }

        /// <summary>
        /// Fetch pre-parsed data corresponding to HID descriptor for the given HID device.
        /// </summary>
        /// <param name="deviceHandle"></param>
        /// <returns></returns>
        public static IntPtr GetPreParsedData(IntPtr deviceHandle)
        {
            uint dataSize = 0;
            int result = Win32.Win32RawInput.NativeMethods.GetRawInputDeviceInfo(deviceHandle, RawInputDeviceInfoType.RIDI_PREPARSEDDATA, IntPtr.Zero, ref dataSize);
            if (result != 0)
            {
                Debug.WriteLine("Failed to get raw input pre-parsed data size: " + result + " : " + Marshal.GetLastWin32Error());
                return IntPtr.Zero;
            }

            IntPtr data = Marshal.AllocHGlobal((int)dataSize);
            result = Win32.Win32RawInput.NativeMethods.GetRawInputDeviceInfo(deviceHandle, RawInputDeviceInfoType.RIDI_PREPARSEDDATA, data, ref dataSize);
            if (result <= 0)
            {
                Debug.WriteLine("Failed to get raw input pre-parsed data: " + result + " : " + Marshal.GetLastWin32Error());
                return IntPtr.Zero;
            }

            return data;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public static string GetDeviceName(IntPtr device)
        {
            uint deviceNameSize = 256;
            int result = Win32.Win32RawInput.NativeMethods.GetRawInputDeviceInfo(device, RawInputDeviceInfoType.RIDI_DEVICENAME, IntPtr.Zero, ref deviceNameSize);
            if (result != 0)
            {
                return string.Empty;
            }

            // Size is the character count not byte count
            IntPtr deviceName = Marshal.AllocHGlobal((int)deviceNameSize * 2);
            try
            {
                result = Win32.Win32RawInput.NativeMethods.GetRawInputDeviceInfo(device, RawInputDeviceInfoType.RIDI_DEVICENAME, deviceName, ref deviceNameSize);
                if (result > 0)
                {
                    // -1 for NULL termination
                    return Marshal.PtrToStringAnsi(deviceName, result - 1);
                }

                return string.Empty;
            }
            finally
            {
                Marshal.FreeHGlobal(deviceName);
            }
        }

        public static IList<Device> GetDevices()
        {
            // Get our list of devices
            RAWINPUTDEVICELIST[] ridList = null;
            uint deviceCount = 0;
            int res = Win32.Win32RawInput.NativeMethods.GetRawInputDeviceList(ridList, ref deviceCount, (uint)Marshal.SizeOf(typeof(RAWINPUTDEVICELIST)));
            if (res == -1)
            {
                // Just give up then
                return null;
            }

            ridList = new RAWINPUTDEVICELIST[deviceCount];
            res = Win32.Win32RawInput.NativeMethods.GetRawInputDeviceList(ridList, ref deviceCount, (uint)Marshal.SizeOf(typeof(RAWINPUTDEVICELIST)));
            if (res != deviceCount)
            {
                // Just give up then
                return null;
            }

            var devs = new List<Device>();
            // For each our device add a node to our treeview
            foreach (RAWINPUTDEVICELIST device in ridList)
            {
                // Try create our HID device.
                try
                {
                    devs.Add(new Device(device.hDevice));
                }
                catch
                {
                    continue;
                }
            }

            return devs;
        }
    }
}