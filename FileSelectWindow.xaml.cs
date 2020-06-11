using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WavDataCheck
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FileSelectWindow : Window
    {
        private static FileSelectWindow fileSelectWinObj;
        public static FileSelectWindow FileSelectWinObj
        {
            get 
            {
                if(fileSelectWinObj is null)
                {
                    fileSelectWinObj = new FileSelectWindow();
                    return fileSelectWinObj;
                }
                else
                {
                    return fileSelectWinObj;
                }
            }
        }


        private string serverName;
        private MediaFileRepository myFileRep;

        private FileSelectWindow()
        {
            InitializeComponent();
            this.Title = StringResources.appTitle;
        }

        private void checkFileFindButton_Click(object sender, RoutedEventArgs e)
        {
            FileDialogManager.ShowFolderOpenDialog(checkFileTextBox);
        }

        private void checkTestStartButton_Click(object sender, RoutedEventArgs e)
        {
            if(checkFileTextBox.Text == "")
            {
                MessageBox.Show("경로를 입력해주세요.");
            }
            else if (!(IsServerName(checkFileTextBox.Text)))
            {
                MessageBox.Show("서버 파일 경로가 올바르지 않습니다.");
            }
            else
            {
                myFileRep = new MediaFileRepository(checkFileTextBox.Text,serverName);
                if (myFileRep.FindFlag) // txt또는 csv파일이 있을 경우에만 실행
                {
                    myFileRep.SortDataTable("systemResult");
                    App.DataMediaWin = DataMediaWindow.DataMediaWinObj;

                    App.DataMediaWin.ResultFilePath = JobEventHandler.MakeServerDir(checkFileTextBox.Text, serverName); // 파일 생성 및 저장 좌표 재정의
                    if (App.DataMediaWin.ResultFilePath != "0")//오류가 아닐시에
                    {
                        JobEventHandler.InputDataSave(myFileRep.MyDataTable, App.DataMediaWin.ResultFilePath); // 파일 저장

                        App.DataMediaWin.MediaFilePath = App.DataMediaWin.ResultFilePath.Replace("\\" + serverName + ".csv","");
                        Console.WriteLine("미디어 경로 : " + App.DataMediaWin.MediaFilePath);
                        App.DataMediaWin.ServerName = this.serverName;
                        App.DataMediaWin.MyFileRep = this.myFileRep;

                        App.DataMediaWin.Show();
                        App.DataMediaWin.WindowStart();
                        this.Hide();
                    }
                }

            }
        }

        private bool IsServerName(string filePath)
        {
            int slash;
            int endslash=0;
            bool oneCount = false;
            for (slash = filePath.Length - 1; slash >= 0; slash--)
            {
                if (filePath[slash] == '\\' && oneCount)
                {
                    break;
                }
                else if(filePath[slash] == '\\')
                {
                    if (ServerCheck(filePath.Substring(slash + 1)))
                    {
                        return true;
                    }
                    endslash = slash;
                    oneCount = true;
                }

            }
            if(ServerCheck(filePath.Substring(slash + 1, endslash - slash - 1)))
            {
                return true;
            }
            return false;
        }

        private bool ServerCheck(string subStr)
        {
            foreach (string server in StringResources.serverNameList)
            {
                if (server.Equals(subStr))
                {
                    this.serverName = subStr;
                    return true;
                }
            }
            return false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            System.Environment.Exit(0);
        }

        private void eixtAppBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Environment.Exit(0);
        }
    }
}
