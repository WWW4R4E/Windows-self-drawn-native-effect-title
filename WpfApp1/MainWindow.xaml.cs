using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Shell;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private const int WM_NCHITTEST = 0x0084;
        private const int WM_NCMOUSEMOVE = 0x00A0;
        private const int WM_NCLBUTTONDOWN = 0x00A1;
        private const int WM_HOTKEY = 0x0312;
        private const int HTMINBUTTON = 8;
        private const int HTMAXBUTTON = 9;
        private const int HTCLOSE = 20;
        private const int SC_MINIMIZE = 0xF020;
        private const int SC_MAXIMIZE = 0xF030;
        private const int SC_RESTORE = 0xF120;
        private const int SC_CLOSE = 0xF060;
        private const int HOTKEY_ID = 0x0001;
        private const uint MOD_WIN = 0x0008; // Windows 键
        private const uint VK_Z = 0x5A; // Z 键

        private HwndSource _hwndSource;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _hwndSource = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            _hwndSource.AddHook(HwndSourceHook);

            // 注册 Win + Z 快捷键
            RegisterHotKey(_hwndSource.Handle, HOTKEY_ID, MOD_WIN, VK_Z);
        }

        protected override void OnClosed(EventArgs e)
        {
            UnregisterHotKey(_hwndSource.Handle, HOTKEY_ID);
            _hwndSource.RemoveHook(HwndSourceHook);
            base.OnClosed(e);
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 只在标题栏非按钮区域拖动窗口
            var hitPoint = e.GetPosition(this);
            if (!IsOverButton(MinButton, hitPoint) && !IsOverButton(MaxButton, hitPoint) &&
                !IsOverButton(CloseButton, hitPoint))
            {
                DragMove();
            }
        }

        private IntPtr HwndSourceHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_NCHITTEST:
                    var hitPoint = GetWpfMousePosition();
                    if (IsOverButton(MinButton, hitPoint))
                    {
                        handled = true;
                        return new IntPtr(HTMINBUTTON);
                    }

                    if (IsOverButton(MaxButton, hitPoint))
                    {
                        handled = false; 
                        return new IntPtr(HTMAXBUTTON);
                    }
                

                    if (IsOverButton(CloseButton, hitPoint))
                    {
                        handled = true;
                        return new IntPtr(HTCLOSE);
                    }

                    // 默认返回 HTCAPTION 以允许拖动
                    if (hitPoint.Y < 32) // 标题栏高度
                    {
                        handled = false;
                        return new IntPtr(2); // HTCAPTION
                    }

                    break;

                case WM_NCMOUSEMOVE:
                    if (wParam.ToInt32() == HTMAXBUTTON)
                    {
                        // 转发消息以触发 Snap Layouts
                        PostMessage(hwnd, WM_NCMOUSEMOVE, wParam, lParam);
                        handled = false; // 确保系统继续处理
                    }

                    break;

                case WM_NCLBUTTONDOWN:
                    int hitTest = wParam.ToInt32();
                    if (hitTest == HTMINBUTTON)
                    {
                        SendMessage(hwnd, WM_SYSCOMMAND, SC_MINIMIZE, 0);
                        handled = true;
                    }
                    else if (hitTest == HTMAXBUTTON)
                    {
                        SendMessage(hwnd, WM_SYSCOMMAND,
                            WindowState == WindowState.Maximized ? SC_RESTORE : SC_MAXIMIZE, 0);
                        handled = true;
                    }
                    else if (hitTest == HTCLOSE)
                    {
                        SendMessage(hwnd, WM_SYSCOMMAND, SC_CLOSE, 0);
                        handled = true;
                    }

                    break;

                case WM_HOTKEY:
                    if (wParam.ToInt32() == HOTKEY_ID)
                    {
                        SendMessage(hwnd, WM_SYSCOMMAND,
                            WindowState == WindowState.Maximized ? SC_RESTORE : SC_MAXIMIZE, 0);
                        handled = true;
                    }

                    break;
            }

            return IntPtr.Zero;
        }

private Point GetWpfMousePosition()
{
    GetCursorPos(out POINT screenPoint);
    var hwnd = new WindowInteropHelper(this).Handle;
    ScreenToClient(hwnd, ref screenPoint);

    // 将 Win32 客户区坐标转为 WPF 坐标（考虑 DPI 缩放）
    var source = HwndSource.FromHwnd(hwnd);
    if (source != null)
    {
        var matrix = source.CompositionTarget?.TransformFromDevice;
        if (matrix.HasValue)
        {
            return matrix.Value.Transform(new Point(screenPoint.X, screenPoint.Y));
        }
    }

    return new Point(screenPoint.X, screenPoint.Y);
}


        private bool IsOverButton(FrameworkElement button, Point clientPoint)
        {
            // 获取按钮在窗口中的相对位置
            var transform = button.TransformToAncestor(this);
            var positionInWindow = transform.Transform(new Point(0, 0));

            // 构建按钮在窗口客户区中的矩形区域
            var buttonRectInWindow = new Rect(positionInWindow, new Size(button.ActualWidth, button.ActualHeight));

            Console.WriteLine($"Button: {positionInWindow}, Size: {button.ActualWidth}x{button.ActualHeight}");
            Console.WriteLine($"Mouse: {clientPoint}");
            Console.WriteLine($"Contains: {buttonRectInWindow.Contains(clientPoint)}");

            return buttonRectInWindow.Contains(clientPoint);
        }


        // Win32 API 声明
        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const uint WM_SYSCOMMAND = 0x0112;

        // 按钮点击事件
        private void MinButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessage(_hwndSource.Handle, WM_SYSCOMMAND, SC_MINIMIZE, 0);
        }

        private void MaxButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessage(_hwndSource.Handle, WM_SYSCOMMAND,
                WindowState == WindowState.Maximized ? SC_RESTORE : SC_MAXIMIZE, 0);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessage(_hwndSource.Handle, WM_SYSCOMMAND, SC_CLOSE, 0);
        }
    }
}