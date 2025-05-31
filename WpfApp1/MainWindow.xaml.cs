using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        // 定义 Windows 消息常量，用于处理非客户区交互
        private const int WM_NCHITTEST = 0x0084; // 非客户区命中测试消息，用于确定鼠标位置
        private const int WM_NCLBUTTONDOWN = 0x00A1; // 非客户区左键按下消息
        private const int WM_NCLBUTTONUP = 0x00A2; // 非客户区左键释放消息

        // 定义非客户区命中测试代码
        private const int HTMINBUTTON = 8; // 最小化按钮区域
        private const int HTMAXBUTTON = 9; // 最大化/还原按钮区域（触发 Snap Layouts）
        private const int HTCLOSE = 20; // 关闭按钮区域

        // 定义系统命令常量，用于窗口操作
        private const int SC_MINIMIZE = 0xF020; // 最小化窗口命令
        private const int SC_MAXIMIZE = 0xF030; // 最大化窗口命令
        private const int SC_RESTORE = 0xF120; // 还原窗口命令
        private const int SC_CLOSE = 0xF060; // 关闭窗口命令

        // 窗口的消息源句柄
        private HwndSource _hwndSource;

        // 构造函数
        public MainWindow()
        {
            InitializeComponent(); // 初始化 XAML 定义的 UI 组件
            Loaded += MainWindow_Loaded; // 订阅窗口加载事件
        }

        // 窗口加载事件处理
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 获取窗口的 Win32 句柄并创建消息源
            _hwndSource = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            // 注册消息钩子以处理 Windows 消息
            _hwndSource.AddHook(HwndSourceHook);
        }

        // 窗口关闭时清理资源
        protected override void OnClosed(EventArgs e)
        {
            // 移除消息钩子
            _hwndSource.RemoveHook(HwndSourceHook);
            // 调用基类关闭逻辑
            base.OnClosed(e);
        }

        // 标题栏区域的鼠标左键按下事件，用于拖动窗口
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 获取鼠标相对于窗口的坐标
            var hitPoint = e.GetPosition(this);
            // 仅在非按钮区域拖动窗口，避免干扰按钮点击
            if (!IsOverButton(MinButton, hitPoint) && !IsOverButton(MaxButton, hitPoint) &&
                !IsOverButton(CloseButton, hitPoint))
            {
                DragMove(); // 拖动窗口
            }
        }

        // 消息钩子函数，处理 Windows 消息
        private IntPtr HwndSourceHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                // 处理非客户区命中测试消息
                case WM_NCHITTEST:
                    // 获取鼠标相对于窗口的 WPF 坐标
                    var hitPoint = GetWpfMousePosition();
                    // 检查鼠标是否在最小化按钮上
                    if (IsOverButton(MinButton, hitPoint))
                    {
                        handled = true; // 标记消息已处理
                        return new IntPtr(HTMINBUTTON); // 返回最小化按钮区域代码
                    }

                    // 检查鼠标是否在最大化/还原按钮上
                    if (IsOverButton(MaxButton, hitPoint))
                    {
                        handled = true; // 标记消息已处理，确保触发 Snap Layouts
                        return new IntPtr(HTMAXBUTTON); // 返回最大化按钮区域代码
                    }

                    // 检查鼠标是否在关闭按钮上
                    if (IsOverButton(CloseButton, hitPoint))
                    {
                        handled = true; // 标记消息已处理
                        return new IntPtr(HTCLOSE); // 返回关闭按钮区域代码
                    }

                    break;

                // 处理非客户区左键按下消息
                case WM_NCLBUTTONDOWN:
                    int hitTest = wParam.ToInt32(); // 获取命中测试代码
                    if (hitTest == HTMINBUTTON)
                    {
                        // 最小化窗口
                        SendMessage(hwnd, WM_SYSCOMMAND, SC_MINIMIZE, 0);
                        handled = true; // 标记消息已处理
                    }
                    else if (hitTest == HTMAXBUTTON)
                    {
                        // 允许系统处理 Snap Layouts 的选择，不直接最大化
                        handled = false;
                    }
                    else if (hitTest == HTCLOSE)
                    {
                        // 关闭窗口
                        SendMessage(hwnd, WM_SYSCOMMAND, SC_CLOSE, 0);
                        handled = true; // 标记消息已处理
                    }

                    break;

                // 处理非客户区左键释放消息
                case WM_NCLBUTTONUP:
                    if (wParam.ToInt32() == HTMAXBUTTON)
                    {
                        // 如果未选择 Snap Layouts，执行最大化/还原
                        if (!IsSnapLayoutSelected())
                        {
                            SendMessage(hwnd, WM_SYSCOMMAND,
                                WindowState == WindowState.Maximized ? SC_RESTORE : SC_MAXIMIZE, 0);
                        }

                        handled = false; // 允许系统继续处理
                    }

                    break;
            }

            return IntPtr.Zero; // 默认返回值，表示未处理或处理完成
        }

        // 检查是否选择了 Snap Layouts 布局
        private bool IsSnapLayoutSelected()
        {
            // 简单判断：如果窗口状态不是正常或最大化，假设选择了 Snap Layouts
            return WindowState != WindowState.Normal && WindowState != WindowState.Maximized;
        }

        // 获取鼠标相对于窗口的 WPF 坐标
        private Point GetWpfMousePosition()
        {
            // 获取屏幕坐标
            GetCursorPos(out POINT screenPoint);
            // 转换为窗口客户区坐标
            ScreenToClient(_hwndSource.Handle, ref screenPoint);
            // 考虑 WPF 的 DPI 缩放
            var source = HwndSource.FromHwnd(_hwndSource.Handle);
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

        // 检查鼠标是否在指定按钮区域内
        private bool IsOverButton(FrameworkElement button, Point clientPoint)
        {
            // 获取按钮的 WPF 相对于窗口的位置
            var buttonPosInWindow = button.TransformToAncestor(this).Transform(new Point(0, 0));

            // 获取按钮的实际尺寸
            var buttonSize = new Size(button.ActualWidth, button.ActualHeight);

            // 构建按钮在窗口内的矩形区域
            var buttonRectInWindow = new Rect(buttonPosInWindow, buttonSize);

            // 检查传入的 clientPoint 是否在该区域内
            return buttonRectInWindow.Contains(clientPoint);
        }


        // 定义 Win32 API 结构和函数
        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X; // X 坐标
            public int Y; // Y 坐标
        }

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint); // 获取鼠标屏幕坐标

        [DllImport("user32.dll")]
        private static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint); // 屏幕坐标转客户区坐标

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam); // 发送消息

        private const uint WM_SYSCOMMAND = 0x0112; // 系统命令消息

        // 按钮点击事件处理
        private void MinButton_Click(object sender, RoutedEventArgs e)
        {
            // 最小化窗口
            SendMessage(_hwndSource.Handle, WM_SYSCOMMAND, SC_MINIMIZE, 0);
        }

        private void MaxButton_Click(object sender, RoutedEventArgs e)
        {
            // 切换最大化/还原状态
            SendMessage(_hwndSource.Handle, WM_SYSCOMMAND,
                WindowState == WindowState.Maximized ? SC_RESTORE : SC_MAXIMIZE, 0);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // 关闭窗口
            SendMessage(_hwndSource.Handle, WM_SYSCOMMAND, SC_CLOSE, 0);
        }
    }
}