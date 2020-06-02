using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace WavDataCheck
{
    public class MediaFileRepository
    {
        private string filePath;
        private string serverName;
        private string metaDataFile;
        private int readMode; //txt면 0, csv면1
        private DataTable myDataTable;
        public DataTable MyDataTable
        {
            get { return myDataTable; }
        }

        private bool findFlag; // 메타 데이터 파일을 찾았음을 의미
        public bool FindFlag
        {
            get { return findFlag; }
        }
        public MediaFileRepository(string filePath, string serverName)
        {
            this.filePath = filePath;
            this.serverName = serverName;
            myDataTable = new DataTable();
            myDataTable.Columns.Add("fileName", typeof(string));
            myDataTable.Columns.Add("humanResult", typeof(string));
            myDataTable.Columns.Add("systemResult", typeof(string));
            FindMetaData();
            if (findFlag)
            {
                ReadMetaData();
                FileMatchCheck();
            }
            ShowDataTable();
        }

        private void FindMetaData()
        {
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(filePath);
            if (di.GetFiles("*.txt").Length > 0)
            {
                metaDataFile = di.GetFiles("*.txt")[0].Name;
                readMode = 0;
                findFlag = true;
            }
            else if (di.GetFiles("*.csv").Length > 0)
            {
                metaDataFile = di.GetFiles("*.csv")[0].Name;
                readMode = 1;
                findFlag = true;
            }
            else
            {
                MessageBox.Show("txt 혹은 csv파일이 없습니다.");
                findFlag = false;
            }
        }

        private void ReadMetaData()
        {
            string result = "";
            if (readMode == 0) //txt읽기
            {
                result = System.IO.File.ReadAllText(filePath + "\\" + metaDataFile); 
            }
            else if (readMode == 1) //csv읽기
            {
                StreamReader sr = new StreamReader(filePath + "\\" + metaDataFile, Encoding.GetEncoding("euc-kr"));
                result = sr.ReadToEnd(); 
                sr.Close();
                Console.WriteLine(result);
            }
            AddDataTable(result);
        }

        private void AddDataTable(string dataContent)
        {
            string[] words = dataContent.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            DataRow row = null;
            foreach (string word in words)
            {
                if (readMode == 0)
                {
                    string[] inwords = word.Split('\t');
                    if(inwords[0] != "")
                    {
                        row = myDataTable.NewRow();
                        row["fileName"] = inwords[0];
                        if (inwords.Length > 1)
                        {
                            row["systemResult"] = inwords[1];
                        }
                        else
                        {
                            row["systemResult"] = "";
                        }
                        row["humanResult"] = "";
                        myDataTable.Rows.Add(row);
                    }
                    
                }

                else if (readMode == 1)
                {
                    string[] inwords = word.Split(',');
                    if (inwords[0] != "" && inwords[0] != "음성파일")
                    {
                        row = myDataTable.NewRow();
                        row["fileName"] = inwords[0];
                        row["humanResult"] = inwords[1];
                        row["systemResult"] = inwords[2];
                        myDataTable.Rows.Add(row);
                    }
                }
            }
        }

        private void FileMatchCheck()
        {
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(filePath);
            bool matchCheck = false;
            int delCount = 0;
            int overlapCount = 0;

            // 파일 일치 검사
            foreach (FileInfo realName in di.GetFiles("*.wav"))
            {
                foreach (DataRow dataRow in myDataTable.Rows)
                {
                    matchCheck = false;
                    if (realName.Name.ToString() == dataRow[0].ToString())
                    {
                        matchCheck = true;
                        break;
                    }
                }
                if (matchCheck == false)
                {
                    FileInfo fileDel = new FileInfo(filePath + "\\" + realName);
                    fileDel.Delete();
                    delCount++;
                }
                
            }
            if (delCount > 0)
            {
                MessageBox.Show("불일치 파일 " + delCount + "개를 삭제했습니다.");
            }
            delCount = 0;
            // 행 일치 및 중복 검사
            int rowCount;
            for (rowCount = 0;rowCount<myDataTable.Rows.Count; rowCount++)
            {
                foreach(FileInfo realName in di.GetFiles("*.wav"))
                {
                    matchCheck = false;
                    if (realName.Name.ToString() == myDataTable.Rows[rowCount][0].ToString())
                    {
                        matchCheck = true;
                        break;
                    }
                    
                }
                if (matchCheck == false)
                {
                    DataRow dr = myDataTable.Rows[rowCount];
                    dr.Delete();
                    delCount++;
                    rowCount--;
                }

                // 중복 행 검사
                string temp = myDataTable.Rows[rowCount][0].ToString();
                int innerCount;
                for (innerCount=0 ; innerCount < myDataTable.Rows.Count ; innerCount++)
                {
                    if (innerCount != rowCount && temp==myDataTable.Rows[innerCount][0].ToString())
                    {
                        DataRow dr = myDataTable.Rows[innerCount];
                        dr.Delete();
                        rowCount--;
                        overlapCount++;
                    }
                }
            }
            if (delCount > 0)
            {
                MessageBox.Show("불일치 행 " + delCount + "개를 삭제했습니다.");
            }
            if (overlapCount > 0)
            {
                MessageBox.Show("중복행 " + delCount + "개를 삭제했습니다.");
            }
        }

        private void ShowDataTable()
        {
            Console.WriteLine("결과출력");
            foreach(DataRow row in myDataTable.Rows)
            {
                Console.WriteLine(row["fileName"] + " : " + row["humanResult"] + " : " + row["systemResult"]);
            }
        }

        public void SortDataTable(string sortColName)
        {
            DataView dv = new DataView(myDataTable);
            dv.Sort = sortColName + " ASC";
            myDataTable = dv.ToTable();
        }

    }
}
