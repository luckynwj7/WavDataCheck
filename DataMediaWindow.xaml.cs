using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WavDataCheck
{
    /// <summary>
    /// Window1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DataMediaWindow : Window
    {
        private static DataMediaWindow dataMediaWinObj;
        public static DataMediaWindow DataMediaWinObj
        {
            get
            {
                if (dataMediaWinObj is null)
                {
                    dataMediaWinObj = new DataMediaWindow();
                    return dataMediaWinObj;
                }
                else
                {
                    return dataMediaWinObj;
                }
            }
        }
        private string mediaFilePath;
        public string MediaFilePath
        {
            get { return mediaFilePath; }
            set { mediaFilePath = value; }
        }
        private string resultFilePath;
        public string ResultFilePath
        {
            get { return resultFilePath; }
            set { resultFilePath = value; }
        }

        private string serverName;
        public string ServerName
        {
            get { return serverName; }
            set { serverName = value; }
        }

        private MediaFileRepository myFileRep;
        public MediaFileRepository MyFileRep
        {
            get { return myFileRep; }
            set { myFileRep = value; }
        }
        private MediaPlayer myMediaPlayer;

        private int currentJobRow = 0; // 현재 작업하는 행
        private int fullJobCount = 0; // 전체 파일 수(고정)
        private int currentIndex = 0; // 현재 보고 있는 파일
        public int CurrentIndex
        {
            get { return currentIndex; }
            set { currentIndex = value; }
        }
        private int prevIndex; //바로 이전에 보고 있었던 파일
        public int PrevIndex
        {
            get { return prevIndex; }
            set { prevIndex = value; }
        }
        private bool scrollViewFlag; // ShowWindow의 스크롤 뷰 조절 변수
        public bool ScrollViewFlag
        {
            get { return scrollViewFlag; }
            set { scrollViewFlag = value; }
        }
        private int autoModeActivateFlag; // 자동 실행을 조정해주는 변수. 0-한번씩, 1-하나 반복, 2- 자동모드

        private ShowDataWindow showWin;

        private int realFileCount;

        public delegate void timeTick(); //프로그래스바
        DispatcherTimer ticks = new DispatcherTimer();

        private DataMediaWindow()
        {
            InitializeComponent();
            currentJobRow = 0;
            currentIndex = 0;
            realFileCount = 0;
            autoModeActivateFlag = 0;
            myMediaPlayer = new MediaPlayer();
            saveAndCheckBtn.Content += "\n(압축하지 않음. 검수전용)"; //버튼 컨텐츠
        }
        public void WindowStart()
        {
            showWin = ShowDataWindow.GetShowDataWindow(myFileRep.MyDataTable);
            showWin.Show();


            autoModeActivateFlag = 0;
            prevIndex = -1; // 이전 인덱스 초기값. -1로 하여 거절하게 만듬
            scrollViewFlag = false; //스크롤 뷰 초기값

            serverNameTxt.Text = serverName; // 서버 이름
            fullJobCount = myFileRep.MyDataTable.Rows.Count;
            fullJobTxt.Text = fullJobCount.ToString();

            myMediaPlayer.Volume = volumeSlider.Value;
            volumeTxt.Text = (volumeSlider.Value * 100).ToString();

            myMediaPlayer.SpeedRatio = speedSlider.Value;
            speedTxt.Text = (speedSlider.Value * 100).ToString();
            CalCurrentJobRow();

            currentIndex = CalcLastJob();
            if (currentIndex >= fullJobCount)
            {
                currentIndex = 0;
            }

            realFileCount = CalcFileCount(myFileRep.MyDataTable.Rows.Count); // 출력 파일 개수
            showWin.UpdateShowData(myFileRep.MyDataTable); // 검증 시 데이터 출력이 갱신되지 않는 현상
            WindowStatusUpdate(currentIndex);
        }
        private int CalcFileCount(int inputCount)
        {
            foreach (DataRow row in myFileRep.MyDataTable.Rows)
            {
                if (row[1].ToString().Equals("0"))
                {
                    inputCount--;
                }
            }
            return inputCount;
        }
        private int CalcLastJob()
        {
            int returnCount = 0;
            foreach (DataRow row in myFileRep.MyDataTable.Rows)
            {
                if (row[1].ToString() != "")
                {
                    returnCount++;
                }
                else
                {
                    break;
                }
            }
            return returnCount;
        }

        private void CalCurrentJobRow()
        {
            foreach (DataRow row in myFileRep.MyDataTable.Rows)
            {
                if (row[1].ToString() != "")
                {
                    currentJobRowPlus();
                }
            }
        }

        public void WindowStatusUpdate(int jobIndex)
        {
            myMediaPlayer.MediaOpened -= loadedMusic; // 원래 있던 미디어 쓰레드를 지움
            myMediaPlayer.MediaEnded -= endedMusic; // 원래 있던 미디어 쓰레드를 지움

            fullFileTxt.Text = realFileCount.ToString(); // 전체 파일 개수

            indexTxt.Text = (jobIndex + 1).ToString(); // 인덱스
            fileNameTxt.Text = MyFileRep.MyDataTable.Rows[jobIndex][0].ToString(); //파일 이름

            systemResultTxt.Text = MyFileRep.MyDataTable.Rows[jobIndex][2].ToString(); //시스템 인식
            if (MyFileRep.MyDataTable.Rows[jobIndex][1].ToString() == "")
            {
                humanResultTxt.Text = JobEventHandler.PredictResult(systemResultTxt.Text); // 전사 예측 입력
                overlapJobTxt.Text = "";
            }
            else
            {
                humanResultTxt.Text = MyFileRep.MyDataTable.Rows[jobIndex][1].ToString(); // 이미 전사가 있을 경우
                overlapJobTxt.Text = "작업을 완료한 행 입니다.";
            }
            UpdateJobRate();
            humanResultTxt.Focus();
            humanResultTxt.Select(0, humanResultTxt.Text.Length);

            //스크롤 뷰 이동
            if (ScrollViewFlag)
            {
                scrollViewFlag = false;
            }
            else
            {
                showWin.MoveScrollView(jobIndex);
            }
            //보고 있는 인덱스 색칠
            showWin.SignCurrentIndex();

            //파일 이름으로 미디어 재생
            myMediaPlayer.Open(new Uri(mediaFilePath + "\\" + fileNameTxt.Text));
            myMediaPlayer.MediaOpened += loadedMusic;
            myMediaPlayer.MediaEnded += endedMusic;
            myMediaPlayer.Play();
        }

        public void UpdateJobRate()
        {
            jobCountTxt.Text = currentJobRow.ToString(); // 현재 작업량
            jobRateTxt.Text = ((double)((double)currentJobRow / (double)fullJobCount * 100.0)).ToString("F1"); // 진행률
            nmgJobTxt.Text = (fullJobCount - currentJobRow).ToString();
        }

        private void prevFileBtn_Click(object sender, RoutedEventArgs e)
        {
            CurrentIndexMinus();
        }

        private void nextFileBtn_Click(object sender, RoutedEventArgs e)
        {
            CurrentIndexPlus();
        }

        private void submitBtn_Click(object sender, RoutedEventArgs e)
        {
            SubmitAct();
        }


        private void delRowBtn_Click(object sender, RoutedEventArgs e)
        {
            DelRowAct();
        }

        private void CurrentIndexPlus()
        {
            prevIndex = currentIndex;
            currentIndex++;
            if (currentIndex > myFileRep.MyDataTable.Rows.Count - 1)
            {
                currentIndex = 0;
            }
            WindowStatusUpdate(currentIndex);
        }
        private void CurrentIndexMinus()
        {
            prevIndex = currentIndex;
            currentIndex--;
            if (currentIndex < 0)
            {
                currentIndex = myFileRep.MyDataTable.Rows.Count - 1;
            }
            WindowStatusUpdate(currentIndex);
        }

        private void moveIndexBtn_Click(object sender, RoutedEventArgs e)
        {
            MoveIndexEvent();
        }
        private void MoveIndexEvent()
        {
            int inputNum = 1;
            try
            {
                inputNum = Int32.Parse(moveIndexTxt.Text);
            }
            catch (System.FormatException ex)
            {
                MessageBox.Show("숫자만 입력해 주세요.");
                moveIndexTxt.Text = "";
                return;
            }
            if (inputNum < 1 || inputNum > myFileRep.MyDataTable.Rows.Count)
            {
                MessageBox.Show("인덱스 범위가 초과되었습니다. 없는 행입니다.");
                moveIndexTxt.Text = "";
            }
            else
            {
                prevIndex = currentIndex;
                currentIndex = inputNum - 1;
                WindowStatusUpdate(currentIndex);
                moveIndexTxt.Text = "";
            }
        }

        private void endZipBtn_Click(object sender, RoutedEventArgs e)
        {
            myMediaPlayer.Pause();
            if (jobRateTxt.Text != "100.0")
            {
                if ((MessageBox.Show("진행률이 100% 미만입니다. 그래도 압축하시겠습니까?", "압축", MessageBoxButton.YesNo)) == MessageBoxResult.No)
                {
                    myMediaPlayer.Play();
                    return;
                }
            }
            string savePath = FileDialogManager.ReturnFolderOpenDialog();
            if (savePath != "" && savePath != mediaFilePath)
            {
                JobEventHandler.EndingJob(myFileRep.MyDataTable, mediaFilePath);
                MyFileRep.SortDataTable("fileName");
                JobEventHandler.InputDataSave(myFileRep.MyDataTable, resultFilePath);
                JobEventHandler.CompressZipByIonic(mediaFilePath, savePath + "\\" + serverName + ".zip");
                MessageBox.Show("압축 완료");

                this.Hide();

                // 검증 시 데이터 값이 초기화 되지 않는 현상
                currentIndex = 0;
                fullJobCount = 0;
                currentJobRow = 0;
                // END

            }
            else if (savePath == mediaFilePath)
            {
                MessageBox.Show("미디어 파일 경로와 저장 경로를 다르게 해주세요.");
            }
            else if (savePath == "")
            {

            }
            else
            {
                MessageBox.Show("경로 오류");
            }
        }

        private void musicPauseBtn_Click(object sender, RoutedEventArgs e)
        {
            myMediaPlayer.Pause();
        }

        private void musicStopBtn_Click(object sender, RoutedEventArgs e)
        {
            myMediaPlayer.Stop();
            myMediaPlayer.Play();
        }

        private void musicPlayBtn_Click(object sender, RoutedEventArgs e)
        {
            if (progressTimeTxt.Text == totalTimeTxt.Text)
            {
                myMediaPlayer.Stop();
            }
            myMediaPlayer.Play();
        }

        private void loadedMusic(object sender, EventArgs e)
        {
            progressSlider.Maximum = myMediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds;
            totalTimeTxt.Text = myMediaPlayer.NaturalDuration.TimeSpan.ToString("mm':'ss':'ff");
            ticks.Interval = TimeSpan.FromMilliseconds(1);
            ticks.Tick += ticks_Tick;
            ticks.Start();
        }

        void ticks_Tick(object sender, object e)
        {
            TimeSpan newTimeSpan = myMediaPlayer.Position;
            progressSlider.Value = newTimeSpan.TotalMilliseconds;
            progressTimeTxt.Text = newTimeSpan.ToString("mm':'ss':'ff");
        }
        private void endedMusic(object sender, EventArgs e)
        {
            if (autoModeActivateFlag==2)
            {
                SubmitAct();
            }
            else if(autoModeActivateFlag==1)
            {
                myMediaPlayer.Stop();
                myMediaPlayer.Play();
            }
            else
            {
                // 오토모드 없음
            }
        }

        private void progressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TimeSpan newTimeSpan = myMediaPlayer.Position;
            progressTimeTxt.Text = newTimeSpan.ToString("mm':'ss':'ff");
        }

        private void progressSlider_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            myMediaPlayer.Position = new TimeSpan(0, 0, 0, 0, (int)progressSlider.Value);
            TimeSpan newTimeSpan = myMediaPlayer.Position;
            progressTimeTxt.Text = newTimeSpan.ToString("mm':'ss':'ff");
        }

        private void volumeSlider_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            myMediaPlayer.Volume = volumeSlider.Value;
            volumeTxt.Text = (volumeSlider.Value * 100).ToString("F");
        }

        private void speedSlider_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            //myMediaPlayer.SpeedRatio = speedSlider.Value; //배속은 관련 문제 해결 후 적용할 것
            speedTxt.Text = (speedSlider.Value * 100).ToString("F2");
        }

        private void SubmitAct()
        {
            if (HumanJobCheck())
            {
                return;
            }
            // 제출하기
            showWin.HumanResultAdjust(currentIndex, humanResultTxt.Text);
            MyFileRep.MyDataTable.Rows[currentIndex][1] = humanResultTxt.Text;
            CurrentIndexPlus();
            currentJobRowPlus();
            WindowStatusUpdate(currentIndex);
            JobEventHandler.InputDataSave(myFileRep.MyDataTable, resultFilePath);
        }
        private void DelRowAct()
        {
            //행 및 파일 삭제하기
            /*
            FileInfo fileDel = new FileInfo(mediaFilePath + "\\" + MyFileRep.MyDataTable.Rows[currentIndex][0]);
            fileDel.Delete();
            DataRow dr = MyFileRep.MyDataTable.Rows[currentIndex];
            dr.Delete();
            currentJobRow++;
            WindowStatusUpdate(currentIndex);
            JobEventHandler.InputDataSave(myFileRep.MyDataTable, resultFilePath);
            */
            // 퍼포먼스 문제로 닫아둠
            showWin.HumanResultAdjust(currentIndex, "0");
            MyFileRep.MyDataTable.Rows[currentIndex][1] = "0";
            realFileCount--;
            CurrentIndexPlus();
            currentJobRowPlus();
            WindowStatusUpdate(currentIndex);
            JobEventHandler.InputDataSave(myFileRep.MyDataTable, resultFilePath);
        }

        private bool HumanJobCheck()
        {
            string inputText = humanResultTxt.Text;
            if (inputText.Length >= 2)
            {
                if (inputText.Length >= 4 && (inputText.Substring(0, 4).Equals("/o/f") || inputText.Substring(0, 4).Equals("/f/o")))
                {
                    if (inputText.Length < 5)
                    {
                        return false; // 입력완료
                    }
                    else
                    {
                        inputText = inputText.Substring(4);
                    }
                }
                else if (inputText.Substring(0, 2).Equals("/o"))
                {
                    if (inputText.Length < 3)
                    {
                        Console.WriteLine(inputText.Length);
                        MessageBox.Show("/o 뒤에 아무것도 입력되지 않았습니다.");
                        return true;
                    }
                    else
                    {
                        inputText = inputText.Substring(2);
                    }
                }
                else if (inputText.Substring(0, 2).Equals("/f"))
                {
                    if (inputText.Length < 3)
                    {
                        return false; // 입력완료
                    }
                    else
                    {
                        inputText = inputText.Substring(2);
                    }
                }
            }

            // 전사 작업이 제대로 되는지 (영어나 숫자) 체크
            if (inputText == "")
            {
                MessageBox.Show("아무것도 입력되지 않았습니다.");
                return true;
            }
            if (inputText.Contains("/o") || inputText.Contains("/f"))
            {
                MessageBox.Show("/o 또는 /f는 맨 앞에 입력해주세요.");
                return true;
            }

            string temp = Regex.Replace(inputText, @"[^가-힣]", "");
            if (temp.Length < 1)
            {
                MessageBox.Show("한글만 입력해 주세요.");
                return true;
            }
            string resultTemp = "";
            foreach (char all in inputText)
            {
                bool hanFlag = true;
                foreach (char han in temp)
                {
                    Console.WriteLine("비교 : " + all + "/" + han);
                    if (all == han)
                    {
                        hanFlag = false;
                        break;
                    }

                    if (han == temp[temp.Length - 1])
                    {
                        hanFlag = true;
                    }
                }
                if (all != ' ' && all != '?' && hanFlag)
                {
                    resultTemp += all;
                }

            }

            Console.WriteLine("결과 : " + resultTemp);
            if (resultTemp.Length > 0)
            {
                MessageBox.Show("한글만 입력해 주세요.");
                return true;
            }

            return false;
        }

        private void humanResultTxt_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (humanResultTxt.Text == "0")
                {
                    DelRowAct();
                }
                else
                {
                    SubmitAct();
                }
            }
        }

        private void showDataBtn_Click(object sender, RoutedEventArgs e)
        {
            showWin.UpdateShowData(myFileRep.MyDataTable);
            showWin.Show();
        }

        private void currentJobRowPlus()
        {
            if (currentJobRow < fullJobCount)
            {
                currentJobRow++;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("저장하지 않고 종료하면 파일 및 csv파일 상태가 정리되지 않습니다. 그래도 종료하시겠습니까?", "WARNING", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                System.Environment.Exit(0);
            }
            else
            {
                e.Cancel = true;
                return;
            }

        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            string inputText = humanResultTxt.Text;
            int selectIndex = humanResultTxt.SelectionStart; // 포커스를 맞춰줄 인덱스
            // 토글 방식으로 /f/o 진행
            if (e.Key == Key.F1)
            {
                if (inputText.Length >= 2)
                {

                    if (inputText.Length >= 4 && (inputText.Substring(0, 4).Equals("/f/o") || inputText.Substring(0, 4).Equals("/o/f")))
                    {
                        if (inputText.Substring(0, 2).Equals("/o"))
                        {
                            inputText = inputText.Substring(2);
                        }
                        else
                        {
                            inputText = "/f" + inputText.Substring(4);
                        }
                        selectIndex -= 2;
                    }
                    else if (inputText.Substring(0, 2).Equals("/o"))
                    {
                        inputText = inputText.Substring(2);
                        selectIndex -= 2;
                    }
                    else if (inputText.Substring(0, 2).Equals("/f"))
                    {
                        inputText = "/o" + inputText;
                        selectIndex += 2;
                    }
                    else
                    {
                        inputText = "/o" + inputText;
                        selectIndex += 2;
                    }
                    if (selectIndex < 0)
                    {
                        selectIndex = 0;
                    }
                    humanResultTxt.Text = inputText;
                    humanResultTxt.SelectionStart = selectIndex;
                }
                else
                {
                    selectIndex = humanResultTxt.SelectionStart;
                    humanResultTxt.Text = "/o" + inputText;
                    humanResultTxt.SelectionStart = selectIndex + 2;
                }

            }
            if (e.Key == Key.F2)
            {
                if (inputText.Length >= 2)
                {
                    if (inputText.Length >= 4 && (inputText.Substring(0, 4).Equals("/f/o") || inputText.Substring(0, 4).Equals("/o/f")))
                    {
                        if (inputText.Substring(0, 2).Equals("/f"))
                        {
                            inputText = inputText.Substring(2);
                        }
                        else
                        {
                            inputText = "/o" + inputText.Substring(4);
                        }
                        selectIndex -= 2;
                    }
                    else if (inputText.Substring(0, 2).Equals("/f"))
                    {
                        inputText = inputText.Substring(2);
                        selectIndex -= 2;
                    }
                    else if (inputText.Substring(0, 2).Equals("/o"))
                    {
                        inputText = "/f" + inputText;
                        selectIndex += 2;
                    }
                    else
                    {
                        inputText = "/f" + inputText;
                        selectIndex += 2;
                    }
                    if (selectIndex < 0)
                    {
                        selectIndex = 0;
                    }
                    humanResultTxt.Text = inputText;
                    humanResultTxt.SelectionStart = selectIndex;
                }
                else
                {
                    selectIndex = humanResultTxt.SelectionStart;
                    humanResultTxt.Text = "/f" + inputText;
                    humanResultTxt.SelectionStart = selectIndex + 2;
                }
            }

            if (e.Key == Key.F5)
            {
                myMediaPlayer.Stop();
                myMediaPlayer.Play();
            }
            if (e.Key == Key.F11)
            {
                CurrentIndexMinus();
            }
            if (e.Key == Key.F12)
            {
                CurrentIndexPlus();
            }
            if(e.Key == Key.F6)
            {
                AutoModeAct();
            }
        }

        private void moveIndexTxt_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MoveIndexEvent();
            }
        }

        private void saveAndCheckBtn_Click(object sender, RoutedEventArgs e)
        {
            myMediaPlayer.Pause();
            // 검증 시 데이터 값이 초기화 되지 않는 현상
            currentIndex = 0;
            fullJobCount = 0;
            currentJobRow = 0;
            // END

            JobEventHandler.EndingJob(myFileRep.MyDataTable, mediaFilePath);
            MyFileRep.SortDataTable("fileName");
            JobEventHandler.InputDataSave(myFileRep.MyDataTable, resultFilePath);
            MessageBox.Show("작업 완료");
            this.Hide();
            showWin.Hide();
            App.FileSelectWin.Show();
            App.FileSelectWin.checkFileTextBox.Text = "";
        }

        private void autoModeBtn_Click(object sender, RoutedEventArgs e)
        {
            AutoModeAct();
        }

        private void AutoModeAct()
        {
            if (autoModeActivateFlag==0)
            {
                LotateStartModeActivate();
            }
            else if(autoModeActivateFlag==1)
            {
                AutoModeActivate();
            }
            else if (autoModeActivateFlag == 2)
            {
                OneStartModeActivate();
            }
        }

        private void AutoModeActivate()
        {
            myMediaPlayer.Pause();
            autoModeActivateFlag = 2;
            autoModeImage.Source = JobEventHandler.GetImageFromResource(Properties.Resources.lotateAutoImage);
            if (totalTimeTxt.Text == progressTimeTxt.Text)
            {
                SubmitAct();
            }
            myMediaPlayer.Play();
        }
        private void LotateStartModeActivate()
        {
            myMediaPlayer.Pause();
            autoModeActivateFlag = 1;
            autoModeImage.Source = JobEventHandler.GetImageFromResource(Properties.Resources.loateStartImage);
            myMediaPlayer.Play();

        }
        private void OneStartModeActivate()
        {
            myMediaPlayer.Pause();
            autoModeActivateFlag = 0;
            autoModeImage.Source = JobEventHandler.GetImageFromResource(Properties.Resources.oneStartImage);
            myMediaPlayer.Play();
        }
    }
}
