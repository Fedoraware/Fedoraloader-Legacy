using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;

namespace Fedoraloader.Injection.Native
{
	public static class NativeWrapper
	{
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr OpenProcess(ProcessAccessFlags access, bool inheritHandle, int procId);

		[DllImport("kernel32", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern IntPtr CreateToolhelp32Snapshot([In] UInt32 dwFlags, [In] UInt32 th32ProcessID);

		[DllImport("kernel32", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern bool Process32First([In] IntPtr hSnapshot, ref ProcessEntry32 lppe);

		[DllImport("kernel32", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern bool Process32Next([In] IntPtr hSnapshot, ref ProcessEntry32 lppe);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern IntPtr LoadLibrary(string lpFileName);

		[DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
		public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

		[DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
		public static extern IntPtr memcpy(IntPtr dest, IntPtr src, UIntPtr count);

		[DllImport("kernel32.dll")]
		public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, Int32 nSize, out IntPtr lpNumberOfBytesWritten);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr GetModuleHandle(string lpModuleName);

		[DllImport("kernel32.dll", SetLastError = true)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		[SuppressUnmanagedCodeSecurity]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool CloseHandle(IntPtr hObject);

		[DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
		public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize, AllocationType flAllocationType, MemoryProtection flProtect);

		[DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool IsWow64Process([In] IntPtr process, [Out] out bool wow64Process);
	}
}