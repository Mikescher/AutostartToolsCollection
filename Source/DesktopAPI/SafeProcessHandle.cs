using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DesktopAPI
{
	public sealed class SafeProcessHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		[DllImport("kernel32.dll", SetLastError = true)]
		static extern bool CloseHandle(IntPtr handle);
		public SafeProcessHandle() : base(true) { }
		public SafeProcessHandle(IntPtr handle) : base(true) { base.SetHandle(handle); }
		protected override bool ReleaseHandle() { return CloseHandle(base.handle); }
	}
}
