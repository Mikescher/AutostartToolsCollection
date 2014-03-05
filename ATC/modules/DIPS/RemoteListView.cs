using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows;
using System.Drawing;
using Microsoft.Win32.SafeHandles;

namespace ATC.modules.DIPS
{
    class RemoteListView
    {
        const uint LVM_FIRST = 0x1000;
        const uint LVM_GETITEMCOUNT = LVM_FIRST + 0x4;
        const uint LVM_GETITEMTEXT = LVM_FIRST + 0x2d;
        const uint LVM_GETITEMPOSITION = LVM_FIRST + 16;
        const uint MEM_COMMIT = 0x1000;
        const uint MEM_RELEASE = 0x8000;
        const uint PAGE_READWRITE = 4;
        const uint PROCESS_VM_READ = 0x10;
        const uint PROCESS_VM_WRITE = 0x20;
        const uint PROCESS_VM_OPERATION = 0x8;
        const uint WM_GETTEXT = 0xd;
        const uint WM_GETTEXTLENGTH = 0xe;

		#region External

		[DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool EnumChildWindows(IntPtr hWndParent, EnumChildProcDelegate enumProcDelegate, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern IntPtr FindWindow(string className, string windowName);
        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowThreadProcessId(IntPtr hwnd, ref int lpdwProcessId);
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern int GetClassName(IntPtr hWnd, StringBuilder className, int bufferSize);

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

		#endregion

		public static LVItem[] GetDesktopListView()
		{
			return GetListView("Progman", "Program Manager", "SysListView32", "FolderView");
		}

		private delegate bool EnumChildProcDelegate(IntPtr hWnd, IntPtr lParam);
        public static LVItem[] GetListView(string windowClass, string windowCaption, string listViewClass, string listViewCaption)
        {
            LVItem[] lvArray = new LVItem[0];

            // Searching for Parent Window
            IntPtr hParent = FindWindow(windowClass, windowCaption);
            if (hParent.Equals(IntPtr.Zero))
                if (Marshal.GetLastWin32Error() != 0)
                    throw new Win32Exception();
                else
                {
                    string msg = null;
                    msg = "The application window is not open, or the caption and class are incorrect.";
                    throw new ArgumentException(msg);
                }

            // Searching for ListView using Delegate and EnumChildWindows
            IntPtr Handle = IntPtr.Zero;
            EnumChildWindows(hParent, 
                new EnumChildProcDelegate(delegate(IntPtr hWnd, IntPtr lParam)
                {
                    int length = SendMessage(hWnd, WM_GETTEXTLENGTH, IntPtr.Zero, IntPtr.Zero);
                    StringBuilder captionBuilder = new StringBuilder(length + 1);
                    if (length > 0)
                    {
                        int result = SendMessage(hWnd, WM_GETTEXT, captionBuilder.Capacity, captionBuilder);
                    }
                    if (captionBuilder.ToString().Equals(listViewCaption))
                    {
                        StringBuilder classBuilder = new StringBuilder(256);
                        int result = GetClassName(hWnd, classBuilder, classBuilder.Capacity - 1);
                        if (classBuilder.ToString().Equals(listViewClass))
                        {
                            Handle = hWnd;
                            return false;
                        }
                    }
                    return true;
                }), IntPtr.Zero);
            if (Handle.Equals(IntPtr.Zero))
                if (Marshal.GetLastWin32Error() == 0)
                    throw new Win32Exception();
                else
                {
                    string msg = null;
                    msg = "This listview does not exist, or class name and or caption are incorrect";
                    throw new ArgumentException(msg);
                }

            // Get ProcessId of selected Window
            int id = -1;
            GetWindowThreadProcessId(hParent, ref id);
            if (id == -1)
                throw new ArgumentException("Could not find the process");

            // Opening Process
            SafeProcessHandle hprocess = null;
			
            try
            {
                hprocess = OpenProcess(PROCESS_VM_OPERATION | PROCESS_VM_READ | PROCESS_VM_WRITE, false, id);
                if (hprocess == null && Marshal.GetLastWin32Error() == 0)
                    throw new Win32Exception();

                // Getting Items Count
                int itemCount = SendMessage(Handle, LVM_GETITEMCOUNT, IntPtr.Zero, IntPtr.Zero);
                // Filling Items
                lvArray = new LVItem[itemCount];
                for (int i = 0; i <= itemCount - 1; i++)
                    lvArray[i] = GetItemTextAndLocation(i, hprocess, Handle);
            }
            finally
            {
                if (hprocess != null)
                {
                    hprocess.Close();
                    hprocess.Dispose();
                }
            }
            return lvArray;
        }

		private static LVItem GetItemTextAndLocation(int index, SafeProcessHandle hProcess, IntPtr lvHandle)
		{
			LVItem r = RemoteListView_32.GetItemTextAndLocation(index, hProcess, lvHandle);

			// 64bit handling
			if (string.IsNullOrEmpty(r.Name))
			{
				LVItem r2 = RemoteListView_64.GetItemTextAndLocation(index, hProcess, lvHandle);
				if (string.IsNullOrEmpty(r2.Name))
					return r;
				else
					return r2;
			}
			else
				return r;
		}
    }
}
