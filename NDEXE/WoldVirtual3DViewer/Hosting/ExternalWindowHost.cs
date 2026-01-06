using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace WoldVirtual3DViewer.Hosting
{
    public partial class ExternalWindowHost : HwndHost
    {
        [LibraryImport("user32.dll", SetLastError = true, EntryPoint = "CreateWindowExW", StringMarshalling = StringMarshalling.Utf16)]
        private static partial IntPtr CreateWindowEx(int exStyle, string lpClassName, string lpWindowName, int dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

        [LibraryImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool DestroyWindow(IntPtr hWnd);

        [LibraryImport("user32.dll", SetLastError = true)]
        private static partial IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [LibraryImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, [MarshalAs(UnmanagedType.Bool)] bool bRepaint);

        private IntPtr _hwndHost;
        private IntPtr _externalHwnd;

        public IntPtr HostHandle => _hwndHost;
        public IntPtr ExternalHandle => _externalHwnd;

        public void AttachExternal(IntPtr externalHwnd)
        {
            if (externalHwnd == IntPtr.Zero || _hwndHost == IntPtr.Zero) return;
            _externalHwnd = externalHwnd;
            SetParent(_externalHwnd, _hwndHost);
            var s = RenderSize;
            MoveWindow(_externalHwnd, 0, 0, (int)Math.Max(0, s.Width), (int)Math.Max(0, s.Height), true);
        }

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            const int WS_CHILD = unchecked((int)0x40000000);
            const int WS_VISIBLE = unchecked((int)0x10000000);
            _hwndHost = CreateWindowEx(0, "STATIC", "", WS_CHILD | WS_VISIBLE, 0, 0, (int)RenderSize.Width, (int)RenderSize.Height, hwndParent.Handle, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            return new HandleRef(this, _hwndHost);
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            if (hwnd.Handle != IntPtr.Zero)
            {
                DestroyWindow(hwnd.Handle);
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            if (_externalHwnd != IntPtr.Zero)
            {
                var s = sizeInfo.NewSize;
                MoveWindow(_externalHwnd, 0, 0, (int)Math.Max(0, s.Width), (int)Math.Max(0, s.Height), true);
            }
        }
    }
}
