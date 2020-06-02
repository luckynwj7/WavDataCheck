using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WavDataCheck
{
    public static class JobEventHandler
    {
        public static string MakeServerDir(string filePath, string serverNum)
        {
            int slash = 0;
            string resultPath;
            for (int i = filePath.Length - 1; i >= 0; i--)
            {
                if (filePath[i] == '\\')
                {
                    slash = i + 1;
                    break;
                }
            }

            if(filePath.Substring(slash) == serverNum)
            {
                resultPath = filePath + "\\" + serverNum + ".csv";
            }
            else
            {
                //폴더가 한 번 더 감싼 구조
                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(filePath);
                Console.WriteLine("나온 경로 : " + filePath.Remove(slash) + "\\");
                foreach(FileInfo item in di.GetFiles())
                {
                    if (item.Name.Substring(item.Name.Length - 4) == ".wav")
                    {
                        File.Move(filePath + "\\" + item.Name, filePath.Remove(slash) + "\\" + item.Name);
                    }
                } 
                DirectoryInfo delDir = new DirectoryInfo(filePath);
                delDir.Delete(true);
                resultPath = filePath.Remove(slash) + serverNum + ".csv";
            }
            try
            {
                StreamWriter sw = new StreamWriter(@resultPath, true, Encoding.GetEncoding("euc-kr"));
                sw.Close();
            }
            catch(Exception e)
            {
                MessageBox.Show("파일 경로가 알맞지 않습니다.");
                return "0";
            }
            ClearDir(resultPath.Replace("\\" + serverNum + ".csv",""));
            return resultPath;
        }

        public static void InputDataSave(DataTable inputData, string savePath)
        {
            string result = "음성파일,전사,시스템 인식\n";
            foreach (DataRow row in inputData.Rows)
            {
                result += (row[0].ToString() + "," + row[1].ToString() + "," + row[2].ToString() + "\n");
            }
            result = result.Remove(result.Length - 1);
            System.IO.File.WriteAllText(savePath, result, Encoding.GetEncoding("euc-kr")); //최종파일로 저장
        }

        public static void ClearDir(string filePath)
        {
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(filePath);
            foreach(FileInfo item in di.GetFiles())
            {
                string temp = item.Name.Substring(item.Name.Length - 4);
                if(temp!=".csv" && temp != ".wav")
                {
                    item.Delete();
                }
            }
        }

        public static void CompressZipByIonic(string sourcePath, string zipPath)
        {
            var filelist = GetFileList(sourcePath, new List<String>());
            using (var zip = new Ionic.Zip.ZipFile())
            {
                foreach (string file in filelist)
                {
                    string path = file.Substring(sourcePath.Length + 1);
                    zip.AddEntry(path, File.ReadAllBytes(file));
                }
                zip.Save(zipPath);
            }
        }

        public static List<String> GetFileList(String rootPath, List<String> fileList)
        {
            if (fileList == null)
            {
                return null;
            }
            var attr = File.GetAttributes(rootPath);
            // 해당 path가 디렉토리이면
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                var dirInfo = new DirectoryInfo(rootPath);
                // 하위 모든 디렉토리는
                foreach (var dir in dirInfo.GetDirectories())
                {
                    // 재귀로 통하여 list를 취득한다.
                    GetFileList(dir.FullName, fileList);
                }
                // 하위 모든 파일은
                foreach (var file in dirInfo.GetFiles())
                {
                    // 재귀를 통하여 list를 취득한다.
                    GetFileList(file.FullName, fileList);
                }
            }
            // 해당 path가 파일이면 (재귀를 통해 들어온 경로)
            else
            {
                var fileInfo = new FileInfo(rootPath);
                // 리스트에 full path를 저장한다.
                fileList.Add(fileInfo.FullName);
            }
            return fileList;
        }

        public static void EndingJob(DataTable myData, string filePath)
        {
            // 행삭제 및 파일 삭제
            FileInfo fileDel = null;
            int rowCount;
            for (rowCount = 0; rowCount < myData.Rows.Count; rowCount++)
            {
                if (myData.Rows[rowCount][1].ToString() == "0")
                {
                    fileDel = new FileInfo(filePath + "\\" + myData.Rows[rowCount][0].ToString());
                    fileDel.Delete();
                    myData.Rows[rowCount].Delete();
                    rowCount--;
                }
            }
        }

        public static string PredictResult(string input)
        {
            if (input.Equals("아니오"))
            {
                return "아니요";
            }
            else if (input.Equals("아니야"))
            {
                return "아니요";
            }
            else if (input.Equals("안녕"))
            {
                return "아니요";
            }
            else if (input.Equals("5월"))
            {
                return "아니요";
            }
            else if (input.Equals("어디야"))
            {
                return "아니요";
            }
            else if (input.Equals("아니어라"))
            {
                return "아니요";
            }
            else if (input.Equals("예약"))
            {
                return "예";
            }
            else if (input.Equals("여행"))
            {
                return "예";
            }
            else if (input.Equals("왜"))
            {
                return "예";
            }
            else if(input.Contains("기아의 전화가 전자식인지"))
            {
                return "0";
            }
            else if (input.Contains("지났습니다"))
            {
                return "0";
            }
            else if (input.Contains("이번"))
            {
                return "0";
            }
            else if (input.Contains("일번"))
            {
                return "0";
            }
            else if (input.Contains("남기시려면"))
            {
                return "0";
            }
            else if (input.Contains("바랍니다"))
            {
                return "0";
            }
            else if (input.Contains("음성 녹음은"))
            {
                return "0";
            }
            else if (input.Contains("이걸로 주세요"))
            {
                return "0";
            }
            else if (input.Contains("기획"))
            {
                return "0";
            }
            else if (input.Contains("감사합니다"))
            {
                return "0";
            }
            else if (input.Contains("주세요"))
            {
                return "0";
            }
            return input;
        }
    }
}
