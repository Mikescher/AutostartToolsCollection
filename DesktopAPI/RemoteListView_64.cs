using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows;
using System.Drawing;
using Microsoft.Win32.SafeHandles;

namespace DesktopAPI
{
    class RemoteListView_64
    {
        const uint LVM_FIRST = 0x1000;
        const uint LVM_GETITEMCOUNT = LVM_FIRST + 0x4;
        const uint LVM_GETITEMTEXT = LVM_FIRST + 0x2d;
		const uint LVM_SETITEMPOSITION = LVM_FIRST + 15;
        const uint LVM_GETITEMPOSITION = LVM_FIRST + 16;
        const uint MEM_COMMIT = 0x1000;
        const uint MEM_RELEASE = 0x8000;
        const uint PAGE_READWRITE = 4;
        const uint PROCESS_VM_READ = 0x10;
        const uint PROCESS_VM_WRITE = 0x20;
        const uint PROCESS_VM_OPERATION = 0x8;
        const uint WM_GETTEXT = 0xd;
        const uint WM_GETTEXTLENGTH = 0xe;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		struct LV_ITEM_64
        {
            public uint mask;
            public int iItem;
            public int iSubItem;
            public uint state;
            public uint stateMask;
			public uint placeholder_1;
            public IntPtr pszText;
			public uint placeholder_2;
            public int cchTextMax;
            public int iImage;
            public IntPtr lParam;
            public int iIndent;
            public int iGroupId;
            public int cColumns;
            public IntPtr puColumns;
            public IntPtr piColFmt;
            public int iGroup;
        }

		#region External

        [DllImport("user32.dll", SetLastError = true)]
        static extern int SendMessage(IntPtr hWnd, uint message, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", SetLastError = true)]
        static extern int SendMessage(IntPtr hWnd, uint message, int wParam, StringBuilder lParam);
        [DllImport("user32.dll", SetLastError = true)]
        static extern int SendMessage(IntPtr hWnd, uint message, int wParam, IntPtr lParam);
        [DllImport("user32.dll", SetLastError = true, EntryPoint = "SendMessageA")]
        static extern IntPtr SendMessageA(IntPtr hWnd, uint message, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern SafeProcessHandle OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr VirtualAllocEx(SafeProcessHandle hProcess, IntPtr lpAddress, int dwSize, uint flAllocationType, uint flProtect);
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool VirtualFreeEx(SafeProcessHandle hProcess, IntPtr lpAddress, int dwSize, uint dwFreeType);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ReadProcessMemory(SafeProcessHandle hProcess, IntPtr lpBaseAddress, StringBuilder lpBuffer, int nSize, ref int bytesRead);
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ReadProcessMemory(SafeProcessHandle hProcess, IntPtr lpBaseAddress, ref Point lpBuffer, int nSize, ref int bytesRead);
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
		static extern bool ReadProcessMemory(SafeProcessHandle hProcess, IntPtr lpBaseAddress, ref LV_ITEM_64 lpBuffer, int nSize, ref int bytesRead);
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ReadProcessMemory(SafeProcessHandle hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer, int nSize, ref int bytesRead);
        [DllImport("kernel32.dll", EntryPoint = "ReadProcessMemory", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ReadProcessMemoryW(SafeProcessHandle hProcess, IntPtr lpBaseAddress, StringBuilder lpBuffer, int nSize, ref int bytesRead);
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool WriteProcessMemory(SafeProcessHandle hProcess, IntPtr lpBaseAddress, ref Point lpBuffer, int nSize, ref int lpNumberOfBytesWritten);
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
		static extern bool WriteProcessMemory(SafeProcessHandle hProcess, IntPtr lpBaseAddress, ref LV_ITEM_64 lpBuffer, int nSize, ref int lpNumberOfBytesWritten);

		#endregion

		public static IntPtr MakeLParam(int wLow, int wHigh)
		{
			return (IntPtr)(((short)wHigh << 16) | (wLow & 0xffff));
		}

		public static void SetPosition(int idx, IntPtr lvHandle, Point pos)
		{
			SendMessage(lvHandle, LVM_SETITEMPOSITION, (IntPtr)idx, MakeLParam(pos.x, pos.y));
		}

        public static LVItem GetItemTextAndLocation(int index, SafeProcessHandle hProcess, IntPtr lvHandle)
        {
			LV_ITEM_64 lvitem = new LV_ITEM_64() { cchTextMax = 260, iItem = index };
            IntPtr textPointer = IntPtr.Zero;
            IntPtr locationPointer = IntPtr.Zero;
            StringBuilder text = new StringBuilder(260);
            Point location = new Point();
            try
            {
                // Reserve some place for Location Structure
                locationPointer = VirtualAllocEx(hProcess, IntPtr.Zero, Marshal.SizeOf(location), MEM_COMMIT, PAGE_READWRITE);
                // And Reserve for text too. In this case we expect 260 char max
                textPointer = VirtualAllocEx(hProcess, IntPtr.Zero, 260, MEM_COMMIT, PAGE_READWRITE);
                lvitem.pszText = textPointer;
                IntPtr pLvItem = IntPtr.Zero;
                // Dummy variable
                int pref = 0;
                try
                {
                    // Ask for item location
                    SendMessage(lvHandle, LVM_GETITEMPOSITION, index, locationPointer);
                    // Read item location structure
                    bool boolResult = ReadProcessMemory(hProcess, locationPointer, ref location, Marshal.SizeOf(location), ref pref);
                    if (boolResult == false)
                        throw new Win32Exception();

                    // Reserve some place for LV_ITEM structue
                    pLvItem = VirtualAllocEx(hProcess, IntPtr.Zero, Marshal.SizeOf(lvitem), MEM_COMMIT, PAGE_READWRITE);
                    // Filling it by current data from our application memory
                    boolResult = WriteProcessMemory(hProcess, pLvItem, ref lvitem, Marshal.SizeOf(lvitem), ref pref);
                    if (boolResult == false)
                        throw new Win32Exception();
                    // Ask to fill our LV_ITEM's pszText to text of item
                    SendMessage(lvHandle, LVM_GETITEMTEXT, index, pLvItem);
                    // Reading this text to our string builder
                    boolResult = ReadProcessMemory(hProcess, textPointer, text, 260, ref pref);
                    if (boolResult == false)
                        throw new Win32Exception();	
                }
                finally
                {
                    if (!pLvItem.Equals(IntPtr.Zero))
                        VirtualFreeEx(hProcess, pLvItem, 0, MEM_RELEASE);
                }
            }
            finally
            {
                if (!textPointer.Equals(IntPtr.Zero))
                    VirtualFreeEx(hProcess, textPointer, 0, MEM_RELEASE);
                if (!locationPointer.Equals(IntPtr.Zero))
                    VirtualFreeEx(hProcess, locationPointer, 0, MEM_RELEASE);
            }
            return new LVItem(text.ToString(), location);
        }
    }
}
