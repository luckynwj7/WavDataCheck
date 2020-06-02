using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace WavDataCheck
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        private static FileSelectWindow fileSelectWin;
        public static FileSelectWindow FileSelectWin
        {
            get { return fileSelectWin; }
        }
        private static DataMediaWindow dataMediaWin;
        public static DataMediaWindow DataMediaWin
        {
            get { return dataMediaWin; }
            set { dataMediaWin = value; }
        }
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                Process[] myProcesses = Process.GetProcessesByName("WavDataCheck");
                if (myProcesses.Length > 1)
                {
                    MessageBox.Show("프로그램이 이미 실행중입니다");
                    System.Environment.Exit(0);
                }
            }
            catch (Exception ex)
            {

            }
            fileSelectWin = FileSelectWindow.FileSelectWinObj;
            fileSelectWin.Show();
        }
    }
}
