using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowControlTest
{
    public partial class Form1 : Form
    {
        // 자식핸들 리스트
        internal List<IntPtr> handles = new List<IntPtr>();

        // enumchildWindow에 콜백함수 인자로 넣을 델리게이트와 그 델리게이트로 지정될 함수
        internal delegate bool CallBackProc(IntPtr hWnd, IntPtr parameter);
        internal bool EnumWindowProc(IntPtr handle, IntPtr lParam)
        {
            handles.Add(handle);
            return true;
        }

        // 주어진 부모핸들 아래의 모든 child창을 탐색한다. 이때 탐색후의 처리에 대해선
        // 콜백 함수에게 맡기며 물론 이 콜백함수는 유저가 커스텀한다
        // 이 콜백함수의 첫번째 인자는 child의 핸들, 두번째는 파라메터다.
        [DllImport("user32.dll")]
        private static extern bool EnumChildWindows(IntPtr parent, CallBackProc callback, IntPtr lParam);

        

        // EntryPoint는 함수진입점인데 이미 이 함수로 진입했다는 뜻으로 이후 아래에 정의해줄 함수의 이름은 맘대로 해도 된다
        // 여기선 그대로 FindWindowEx로 썼지만 FWE 라던가.. mymy 라던가로 써도 FindWindowEX로 작동한다는 뜻이다
        [DllImport("user32.dll", EntryPoint = "FindWindowEx", CharSet = CharSet.Auto)]
        static extern IntPtr FindWindowEx(IntPtr hwndParent, int hwndChildAfter, string lpszClass, string lpszWindow);

        
        const int GW_HWNDFIRST = 0; // 젤 첨음
        const int GW_HWNDLAST = 1;  // 마지막
        const int GW_HWNDNEXT = 2;  // 바로 다음
        const int GW_HWNDPREV = 3;  // 바로 이전
        const int GW_OWNER = 4;     // 부모
        const int GW_CHILD = 5;     // 첫번째 자식
        // 클래스나 캡션명을 정확히 알수 없을때 트리구조를 이용해서 핸들을 가져오는 함수
        // 두번째 인자가 그 대상을 정하는 상수다. 상수의 정의는 위에 있다. 접근은 다음과 같이 조합해서 하면 정확한 위치의 핸들을 가져올수 있다
        // int hwnd_first=GetWindow(hwnd_main,GW_CHILD); //첫번째 자식의 핸들값->바로 자식의 핸들값을 가져오므로
        // int hwnd_second = GetWindow(hwnd_first, GW_HWNDNEXT); //첫번째 아래에 있는 핸들값 바로 두번째 핸들값
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindow(IntPtr hWnd, int uCmd);

        [DllImport("user32.dll")]
        public static extern void GetWindowText(IntPtr handle, StringBuilder resultWindowText, int maxTextCapacity);

        // SendMessage의 메세지들은 명령어들에 가까운데 그중 WM_GETTEXT 는 말그대로 TEXT를 받아오는 메세지다
        // 윈도우의 캡션을 받아오며 없을시엔 아무것도 받아오지 않는다
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int msg, int Param, System.Text.StringBuilder text);
        const int WM_GETTEXT = 0x000D;
        const int WM_GETTEXTLENGTH = 0x000E;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // 이름으로 찾은 프로세스들
            Process[] p = Process.GetProcessesByName("Aurora4StarEdit");

            /*
            if(p.GetLength(0) != 0)
            {
                IntPtr mHandle = p[0].MainWindowHandle;
                IntPtr cHandle = FindWindowEx(mHandle, 0, "MDIClient", "");
                cHandle = FindWindowEx(cHandle, 0, "Afx:00400000:b:00010005:00000006:000108B3", "");
                StringBuilder sb = new StringBuilder();
                GetWindowText(cHandle, sb, 100);
                richTextBox1.Text = cHandle + " : " + sb.ToString();
            }
            */

            StringBuilder sb = new StringBuilder(255);
            if (p.GetLength(0) != 0)
            {
                IntPtr mainHandle = p[0].MainWindowHandle;
                IntPtr firstChildHandle = FindWindowEx(mainHandle, 0, "MDIClient", "");

                SendMessage(firstChildHandle, WM_GETTEXT, sb.Capacity, sb);
                MessageBox.Show(sb.ToString());

//                EnumChildWindows(firstChildHandle, EnumWindowProc, default(IntPtr));  // MDIClient 의 자식윈도우한테서만 받아온다
                EnumChildWindows(mainHandle, EnumWindowProc, default(IntPtr));   // mainHandle의 모든 자식윈도우의 핸들을 받아온다
            }

            string result = string.Join("\n", handles);
            richTextBox1.AppendText(result);

            foreach(IntPtr h in handles)
            {
                SendMessage(h, WM_GETTEXT, sb.Capacity, sb);
                richTextBox1.AppendText(sb.ToString() + "\n");
            }
        }
    }
}
