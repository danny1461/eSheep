using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace eSheep
{
    class SheepManager
    {
        public LinkedList<Rectangle> windows = new LinkedList<Rectangle>();
        public LinkedList<Sheep> sheep = new LinkedList<Sheep>();
        private int nextSheepId = 0;
        public int NextSheepId
        {
            get
            {
                return nextSheepId++;
            }
        }

        public IntPtr dropboxWindowHandle;

        #region Get Windows' Locations and Size
        delegate bool EnumWindowsCallback(IntPtr hWnd, int lParam);

        [DllImport("user32")]
        static extern int EnumWindows(EnumWindowsCallback x, int y);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);

        [DllImport("user32.dll")]
        static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, [Out] StringBuilder lpString, int nMaxCount);

        /*[DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);
        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        private struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public ShowWindowCommands showCmd;
            public System.Drawing.Point ptMinPosition;
            public System.Drawing.Point ptMaxPosition;
            public System.Drawing.Rectangle rcNormalPosition;
        }
        private enum ShowWindowCommands : int
        {
            Hide = 0,
            Normal = 1,
            Minimized = 2,
            Maximized = 3,
        }*/
        /*WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
        placement.length = Marshal.SizeOf(placement);
        GetWindowPlacement(hWnd, ref placement);
        if (placement.showCmd == ShowWindowCommands.Maximized)
            return true;*/

        public void GetWindowLocations()
        {
            windows.Clear();
            EnumWindows(this.EnumWindowsCallbackFunc, 0);
            if (Environment.OSVersion.Version.Major == 6)
            {
                // Windows 10
                var commonTaskBarLocation = new Rectangle(
                    0,
                    Screen.PrimaryScreen.WorkingArea.Height,
                    SystemInformation.VirtualScreen.Width,
                    SystemInformation.VirtualScreen.Height - Screen.PrimaryScreen.WorkingArea.Height
                );
                windows.AddLast(commonTaskBarLocation);
            }


            LinkedListNode<Rectangle> node1 = windows.Last;
            LinkedListNode<Rectangle> node2;
            LinkedListNode<Rectangle> nextNode;

            SetWindowPos(dropboxWindowHandle, 2, 0, 0, 1000, 600, 0);

            while (node1 != windows.First)
            {
                nextNode = node1.Previous;
                node2 = nextNode;
                while (node2 != null)
                {
                    if (node2.Value.Contains(node1.Value))
                    {
                        windows.Remove(node1);
                        break;
                    }
                    else if (node1.Value.Top > node2.Value.Top && node1.Value.Top < node2.Value.Bottom)
                    {
                        if (node1.Value.X > node2.Value.X && node1.Value.X < node2.Value.Right)
                        {
                            nextNode = windows.AddBefore(node1, new Rectangle(node2.Value.Right, node1.Value.Top, node1.Value.Right - node2.Value.Right, node1.Value.Height));
                            windows.Remove(node1);
                            break;
                        }
                        else if (node1.Value.Right < node2.Value.Right && node1.Value.Right > node2.Value.X)
                        {
                            nextNode = windows.AddBefore(node1, new Rectangle(node1.Value.Location, new Size(node2.Value.X - node1.Value.X, node1.Value.Height)));
                            windows.Remove(node1);
                            break;
                        }
                        else if (node1.Value.X < node2.Value.X && node1.Value.Right > node2.Value.Right)
                        {
                            windows.AddBefore(node1, new Rectangle(node1.Value.Location, new Size(node2.Value.X - node1.Value.X, node1.Value.Height)));
                            nextNode = windows.AddBefore(node1, new Rectangle(node2.Value.Right, node1.Value.Top, node1.Value.Right - node2.Value.Right, node1.Value.Height));
                            windows.Remove(node1);
                            break;
                        }
                    }
                    node2 = node2.Previous;
                }
                node1 = nextNode;
            }
        }
        private bool EnumWindowsCallbackFunc(IntPtr hWnd, int lParam)
        {
            if (!IsWindowVisible(hWnd))
                return true;
            StringBuilder sb = new StringBuilder(GetWindowTextLength(hWnd)+1);
            if (GetWindowText(hWnd, sb, sb.Capacity) <= 0)
                return true;
            String text = sb.ToString();
            if (text == "SheepRepresentation")
                return true;
            RECT rect = new RECT();
            GetWindowRect(hWnd, ref rect);
            Rectangle rectangle = new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
            if (rectangle.Top <= 50)
                return true;
            if (rectangle == SystemInformation.VirtualScreen)
                return true;
            if (text == "Start")
                rectangle.Width = SystemInformation.VirtualScreen.Width;
            if (text == "Dropbox")
                dropboxWindowHandle = hWnd;
            windows.AddLast(rectangle);
            return true;
        }
        #endregion

        public void AddSheep(bool offScreen = false)
        {
            sheep.AddLast(new Sheep(this, offScreen));
        }
        public void RemoveSheep()
        {
            sheep.Last.Value.Delete();
            sheep.RemoveLast();
        }
        public void Update()
        {
            // Update Window Locations
            GetWindowLocations();

            // Update All Sheep
            LinkedListNode<Sheep> node = sheep.First;
            while (node != null)
            {
                node.Value.Update();
                node = node.Next;
            }

            Application.DoEvents();
        }
    }
}
