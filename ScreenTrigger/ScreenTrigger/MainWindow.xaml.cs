using PInvoke;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Windows.Foundation;

namespace ScreenTrigger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NotifyIcon notifyIcon;
        private List<string> names = new List<string> { "LVision" };

        public MainWindow()
        {
            InitializeComponent();

            notifyIcon = new NotifyIcon();
            notifyIcon.Text = "ScreenTrigger";
            notifyIcon.Icon = new System.Drawing.Icon("L_Letter.ico");
            notifyIcon.Visible = true;
            notifyIcon.MouseDoubleClick += NotifyIcon_MouseDoubleClick;
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add("Exit");
            menu.Items[0].Click += MenuStrip_Click;
            notifyIcon.ContextMenuStrip = menu;
        }

        private void MenuStrip_Click(object? sender, EventArgs e)
        {
            notifyIcon.Dispose();
            Environment.Exit(Environment.ExitCode);
            System.Windows.Application.Current.Shutdown();
        }

        private void NotifyIcon_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            this.Visibility = Visibility.Visible;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            e.Cancel = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Hide();

            //ManagementEventWatcher startWatch = new ManagementEventWatcher(
            //    new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"));
            //startWatch.EventArrived += new EventArrivedEventHandler(startWatch_EventArrived);
            //startWatch.Start();
            Automation.AddAutomationEventHandler(WindowPattern.WindowOpenedEvent, AutomationElement.RootElement, TreeScope.Subtree, (sender, e) =>
            {
                AutomationElement src = sender as AutomationElement;
                if (src != null)
                {
                    Console.WriteLine("Class : " + src.Current.ClassName);
                    Console.WriteLine("Title : " + src.Current.Name);
                    Console.WriteLine("Handle: " + src.Current.NativeWindowHandle);

                    Process p = Process.GetProcessById(src.Current.ProcessId);

                    foreach (var name in names)
                    {
                        if (p.ProcessName.Contains(name))
                        {
                            var allScreens = Screen.AllScreens.ToList();
                            if (allScreens.Count == 1)
                                return;

                            var screenOfChoice = allScreens[allScreens.Count - 1];
                            var screenOfOrigin = allScreens[0];

                            int xDiff = screenOfChoice.WorkingArea.Left - screenOfOrigin.WorkingArea.Left;
                            int yDiff = screenOfChoice.WorkingArea.Top - screenOfOrigin.WorkingArea.Top;

                            GetWindowRect(new IntPtr(src.Current.NativeWindowHandle), out RECT rect);
                            double xRatio = (double)screenOfChoice.WorkingArea.Width / screenOfOrigin.WorkingArea.Width;
                            double yRatio = (double)screenOfChoice.WorkingArea.Height / screenOfOrigin.WorkingArea.Height;

                            if (screenOfChoice.Bounds.Contains(new System.Drawing.Rectangle(rect.left, rect.top,
                                rect.right - rect.left, rect.bottom - rect.top)))
                                continue;
                            MoveWindow(new IntPtr(src.Current.NativeWindowHandle),
                                (int)((rect.left - screenOfOrigin.WorkingArea.Left) * xRatio + screenOfChoice.WorkingArea.Left),
                                (int)((rect.top - screenOfOrigin.WorkingArea.Top) * yRatio + screenOfChoice.WorkingArea.Top),
                                (int)((rect.right - rect.left) * xRatio),
                                (int)((rect.bottom - rect.top) * yRatio), false);
                            //SetWindowPos(new IntPtr(src.Current.NativeWindowHandle), IntPtr.Zero, screenOfChoice.WorkingArea.Left, screenOfChoice.WorkingArea.Top, 0, 0, SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOZORDER);
                        }
                    }
                }
            });
        }

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        [Flags]
        public enum SetWindowPosFlags : uint
        {

            /// <summary>
            ///     If the calling thread and the thread that owns the window are attached to different input queues, the system posts the request to the thread that owns the window. This prevents the calling thread from blocking its execution while other threads process the request.
            /// </summary>
            SWP_ASYNCWINDOWPOS = 0x4000,

            /// <summary>
            ///     Prevents generation of the WM_SYNCPAINT message.
            /// </summary>
            SWP_DEFERERASE = 0x2000,

            /// <summary>
            ///     Draws a frame (defined in the window's class description) around the window.
            /// </summary>
            SWP_DRAWFRAME = 0x0020,

            /// <summary>
            ///     Applies new frame styles set using the SetWindowLong function. Sends a WM_NCCALCSIZE message to the window, even if the window's size is not being changed. If this flag is not specified, WM_NCCALCSIZE is sent only when the window's size is being changed.
            /// </summary>
            SWP_FRAMECHANGED = 0x0020,

            /// <summary>
            ///     Hides the window.
            /// </summary>
            SWP_HIDEWINDOW = 0x0080,

            /// <summary>
            ///     Does not activate the window. If this flag is not set, the window is activated and moved to the top of either the topmost or non-topmost group (depending on the setting of the hWndInsertAfter parameter).
            /// </summary>
            SWP_NOACTIVATE = 0x0010,

            /// <summary>
            ///     Discards the entire contents of the client area. If this flag is not specified, the valid contents of the client area are saved and copied back into the client area after the window is sized or repositioned.
            /// </summary>
            SWP_NOCOPYBITS = 0x0100,

            /// <summary>
            ///     Retains the current position (ignores X and Y parameters).
            /// </summary>
            SWP_NOMOVE = 0x0002,

            /// <summary>
            ///     Does not change the owner window's position in the Z order.
            /// </summary>
            SWP_NOOWNERZORDER = 0x0200,

            /// <summary>
            ///     Does not redraw changes. If this flag is set, no repainting of any kind occurs. This applies to the client area, the nonclient area (including the title bar and scroll bars), and any part of the parent window uncovered as a result of the window being moved. When this flag is set, the application must explicitly invalidate or redraw any parts of the window and parent window that need redrawing.
            /// </summary>
            SWP_NOREDRAW = 0x0008,

            /// <summary>
            ///     Same as the SWP_NOOWNERZORDER flag.
            /// </summary>
            SWP_NOREPOSITION = 0x0200,

            /// <summary>
            ///     Prevents the window from receiving the WM_WINDOWPOSCHANGING message.
            /// </summary>
            SWP_NOSENDCHANGING = 0x0400,

            /// <summary>
            ///     Retains the current size (ignores the cx and cy parameters).
            /// </summary>
            SWP_NOSIZE = 0x0001,

            /// <summary>
            ///     Retains the current Z order (ignores the hWndInsertAfter parameter).
            /// </summary>
            SWP_NOZORDER = 0x0004,

            /// <summary>
            ///     Displays the window.
            /// </summary>
            SWP_SHOWWINDOW = 0x0040,

        }
    }
}
