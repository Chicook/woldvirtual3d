using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Generic;

namespace WoldVirtual3D.Viewer.Server
{
    public class WindowCapture : IDisposable
    {
        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool SetFocus(IntPtr hWnd);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;
        private const int SW_SHOWNOACTIVATE = 4;
        private const int GWL_STYLE = -16;
        private const uint WS_VISIBLE = 0x10000000;
        
        private const int WS_CAPTION = 0x00C00000;
        private const int WS_THICKFRAME = 0x00040000;
        private const int WS_SYSMENU = 0x00080000;
        private const int WS_MINIMIZEBOX = 0x00020000;
        private const int WS_MAXIMIZEBOX = 0x00010000;
        private const uint SWP_FRAMECHANGED = 0x0020;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const IntPtr HWND_TOP = (IntPtr)0;
        
        private const uint WM_KEYDOWN = 0x0100;
        private const uint WM_KEYUP = 0x0101;
        private const uint WM_CHAR = 0x0102;

        private Process? godotProcess;
        private IntPtr godotWindowHandle = IntPtr.Zero;
        private Control? parentControl;
        private System.Windows.Forms.Timer? positionMonitorTimer;

        public bool IsWindowAttached => godotWindowHandle != IntPtr.Zero;

        public async Task<bool> AttachGodotWindow(Process process, Control parentControl)
        {
            this.godotProcess = process;
            this.parentControl = parentControl;
            return await FindAndAttachWindow();
        }

        private async Task<bool> FindAndAttachWindow()
        {
            if (godotProcess == null || parentControl == null)
            {
                return false;
            }

            if (!parentControl.IsHandleCreated)
            {
                int waitAttempts = 0;
                while (!parentControl.IsHandleCreated && waitAttempts < 50)
                {
                    await Task.Delay(100);
                    waitAttempts++;
                }
            }

            if (!parentControl.IsHandleCreated)
            {
                System.Diagnostics.Debug.WriteLine("✗ Control padre no tiene handle después de esperar");
                return false;
            }

            int attempts = 0;
            const int maxAttempts = 150;

            while (attempts < maxAttempts && !godotProcess.HasExited)
            {
                attempts++;
                
                IntPtr windowHandle = FindWindowByProcessId(godotProcess.Id);
                
                if (windowHandle != IntPtr.Zero)
                {
                    System.Diagnostics.Debug.WriteLine($"[WindowCapture] ✓ Ventana de Godot encontrada en intento {attempts}");
                    ShowWindow(windowHandle, SW_HIDE);
                    
                    if (AttachToParent(windowHandle))
                    {
                        godotWindowHandle = windowHandle;
                        StartPositionMonitor();
                        System.Diagnostics.Debug.WriteLine("[WindowCapture] ✓ Ventana adjuntada exitosamente al visor");
                        return true;
                    }
                }

                int delay = attempts < 10 ? 100 : 200;
                await Task.Delay(delay);
            }

            System.Diagnostics.Debug.WriteLine($"⚠ No se encontró la ventana de Godot después de {attempts} intentos");
            return false;
        }

        private IntPtr FindWindowByProcessId(int processId)
        {
            IntPtr foundWindow = IntPtr.Zero;
            List<IntPtr> windows = new List<IntPtr>();

            EnumWindowsProc callback = (hWnd, lParam) =>
            {
                GetWindowThreadProcessId(hWnd, out uint windowProcessId);
                if (windowProcessId == processId)
                {
                    windows.Add(hWnd);
                    if (IsWindowVisible(hWnd))
                    {
                        foundWindow = hWnd;
                        return false;
                    }
                }
                return true;
            };

            EnumWindows(callback, IntPtr.Zero);

            if (foundWindow != IntPtr.Zero)
            {
                return foundWindow;
            }

            try
            {
                Process? process = Process.GetProcessById(processId);
                if (process != null && !process.HasExited)
                {
                    process.Refresh();
                    if (process.MainWindowHandle != IntPtr.Zero)
                    {
                        return process.MainWindowHandle;
                    }
                }
            }
            catch { }

            if (windows.Count > 0)
            {
                return windows[0];
            }

            return IntPtr.Zero;
        }

        private bool AttachToParent(IntPtr windowHandle)
        {
            try
            {
                if (parentControl == null || !parentControl.IsHandleCreated)
                {
                    return false;
                }

                ShowWindow(windowHandle, SW_HIDE);
                HideWindowTitleBar(windowHandle);
                
                bool setParentResult = SetParent(windowHandle, parentControl.Handle);
                if (!setParentResult)
                {
                    return false;
                }

                Thread.Sleep(50);

                if (parentControl.Width > 0 && parentControl.Height > 0)
                {
                    SetWindowPos(windowHandle, HWND_TOP, 0, 0, parentControl.Width, parentControl.Height, 
                        SWP_NOZORDER | SWP_NOACTIVATE);
                    MoveWindow(windowHandle, 0, 0, parentControl.Width, parentControl.Height, false);
                }

                ShowWindow(windowHandle, SW_SHOW);
                Thread.Sleep(50);
                HideWindowTitleBar(windowHandle);

                if (parentControl.InvokeRequired)
                {
                    parentControl.Invoke((MethodInvoker)delegate
                    {
                        parentControl.Invalidate();
                        parentControl.Update();
                    });
                }
                else
                {
                    parentControl.Invalidate();
                    parentControl.Update();
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[WindowCapture] ✗ Error al adjuntar ventana: {ex.Message}");
                return false;
            }
        }

        public void ResizeWindow(int width, int height)
        {
            if (godotWindowHandle != IntPtr.Zero && width > 0 && height > 0)
            {
                SetWindowPos(godotWindowHandle, HWND_TOP, 0, 0, width, height, 
                    SWP_NOZORDER | SWP_NOACTIVATE);
                MoveWindow(godotWindowHandle, 0, 0, width, height, true);
                HideWindowTitleBar(godotWindowHandle);
            }
        }

        public void SetFocusToGodotWindow()
        {
            if (godotWindowHandle != IntPtr.Zero)
            {
                SetFocus(godotWindowHandle);
            }
        }

        public bool SendKeyToGodot(Keys key, bool keyDown = true)
        {
            if (godotWindowHandle == IntPtr.Zero)
            {
                return false;
            }

            uint message = keyDown ? WM_KEYDOWN : WM_KEYUP;
            IntPtr wParam = (IntPtr)(int)key;
            return PostMessage(godotWindowHandle, message, wParam, IntPtr.Zero);
        }

        private void StartPositionMonitor()
        {
            if (positionMonitorTimer != null)
            {
                return;
            }

            positionMonitorTimer = new System.Windows.Forms.Timer
            {
                Interval = 500
            };
            positionMonitorTimer.Tick += PositionMonitorTimer_Tick;
            positionMonitorTimer.Start();
        }

        private void PositionMonitorTimer_Tick(object? sender, EventArgs e)
        {
            if (godotWindowHandle == IntPtr.Zero || parentControl == null || !parentControl.IsHandleCreated)
            {
                return;
            }

            try
            {
                if (GetWindowRect(godotWindowHandle, out RECT rect))
                {
                    Point screenPos = new Point(rect.Left, rect.Top);
                    Point clientPos = parentControl.PointToClient(screenPos);

                    if (clientPos.X != 0 || clientPos.Y != 0)
                    {
                        SetWindowPos(godotWindowHandle, HWND_TOP, 0, 0, parentControl.Width, parentControl.Height, 
                            SWP_NOZORDER | SWP_NOACTIVATE);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[WindowCapture] Error en monitor de posición: {ex.Message}");
            }
        }

        private void HideWindowTitleBar(IntPtr windowHandle)
        {
            try
            {
                int currentStyle = GetWindowLong(windowHandle, GWL_STYLE);
                int newStyle = currentStyle & ~WS_CAPTION & ~WS_THICKFRAME & ~WS_SYSMENU & ~WS_MINIMIZEBOX & ~WS_MAXIMIZEBOX;
                SetWindowLong(windowHandle, GWL_STYLE, newStyle);
                SetWindowPos(windowHandle, IntPtr.Zero, 0, 0, 0, 0, 
                    SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[WindowCapture] Error al ocultar barra de título: {ex.Message}");
            }
        }

        public void Detach()
        {
            if (positionMonitorTimer != null)
            {
                positionMonitorTimer.Stop();
                positionMonitorTimer.Dispose();
                positionMonitorTimer = null;
            }

            if (godotWindowHandle != IntPtr.Zero)
            {
                try
                {
                    SetParent(godotWindowHandle, IntPtr.Zero);
                }
                catch { }
                godotWindowHandle = IntPtr.Zero;
            }
        }

        public void Dispose()
        {
            Detach();
        }
    }
}

