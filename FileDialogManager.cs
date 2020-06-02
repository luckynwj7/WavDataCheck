using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;

namespace WavDataCheck
{
    public static class FileDialogManager
    {
        public static void ShowFolderOpenDialog(System.Windows.Controls.TextBox resultTextBox)
        {
            //파일오픈창 생성 및 설정
            FolderBrowserDialog ofd = new System.Windows.Forms.FolderBrowserDialog();

            /*
            ofd.Title = "파일 오픈 예제창";
            ofd.FileName = "test";
            ofd.Filter = "모든 파일 (*.*) | *.*";
            */
            //파일 오픈창 로드
            DialogResult dr = ofd.ShowDialog();

            //OK버튼 클릭시
            if (dr == DialogResult.OK)
            {
                //File명과 확장자를 가지고 온다.
                //string fileName = ofd.SafeFileName;
                //File경로와 File명을 모두 가지고 온다.
                //string fileFullName = ofd.FileName;
                //File경로만 가지고 온다.
                //string filePath = fileFullName.Replace(fileName, "");

                //출력 예제용 로직
                //resultTextBox.Text = "File Name  : " + fileFullName;
                //label2.Text = "Full Name  : " + fileFullName;
                //label3.Text = "File Path  : " + filePath;
                resultTextBox.Text = ofd.SelectedPath;
            }
            //취소버튼 클릭시 또는 ESC키로 파일창을 종료 했을경우
            else if (dr == DialogResult.Cancel)
            {

            }
        }

        public static string ReturnFolderOpenDialog()
        {
            FolderBrowserDialog ofd = new System.Windows.Forms.FolderBrowserDialog();
            DialogResult dr = ofd.ShowDialog();
            if(dr == DialogResult.OK)
            {
                return ofd.SelectedPath;
            }
            else if (dr == DialogResult.Cancel)
            {
                return "";
            }
            return "";
        }
    }
}
