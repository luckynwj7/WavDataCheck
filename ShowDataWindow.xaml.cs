using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Documents.DocumentStructures;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WavDataCheck
{
    /// <summary>
    /// ShowDataWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ShowDataWindow : Window
    {
        private DataTable viewingDataTable;
        public DataTable ViewingDataTable
        {
            get { return viewingDataTable; }
            set { viewingDataTable = value; }
        }
        private static ShowDataWindow thisWin;

        public static ShowDataWindow GetShowDataWindow(DataTable inputData)
        {
            if(thisWin is null)
            {
                thisWin = new ShowDataWindow(inputData);
                return thisWin;
            }
            else
            {
                return thisWin;
            }
        }
        private ShowDataWindow(DataTable inputData)
        {
            InitializeComponent();
            this.viewingDataTable = inputData;
            UpdateShowData(viewingDataTable);
        }

        public void UpdateShowData(DataTable inputData)
        {
            dataListSt.Children.Clear();
            this.viewingDataTable = inputData;
            for (int rowCount = 0; rowCount < this.viewingDataTable.Rows.Count; rowCount++)
            {
                DataListAdd(viewingDataTable.Rows[rowCount], rowCount + 1);
            }
        }

        private void DataListAdd(DataRow row, int index)
        {
            StackPanel listSt = new StackPanel();
            listSt.Orientation = Orientation.Horizontal;

            // 인덱스
            TextBlock txt1 = new TextBlock();
            txt1.Width = 52;
            txt1.Text = index.ToString();
            txt1.TextAlignment = TextAlignment.Center;

            Border bor1 = new Border();
            bor1.BorderThickness = new Thickness(1, 1, 1, 1);
            bor1.BorderBrush = Brushes.Black;
            bor1.Child = txt1;

            // 음성파일
            TextBlock txt2 = new TextBlock();
            txt2.Width = 152;
            txt2.Text = row[0].ToString();
            txt2.TextAlignment = TextAlignment.Center;

            Border bor2 = new Border();
            bor2.BorderThickness = new Thickness(1, 1, 1, 1);
            bor2.BorderBrush = Brushes.Black;
            bor2.Child = txt2;

            // 전사
            TextBlock txt3 = new TextBlock();
            txt3.Width = 152;
            txt3.Text = row[1].ToString();
            txt3.TextAlignment = TextAlignment.Center;

            Border bor3 = new Border();
            bor3.BorderThickness = new Thickness(1, 1, 1, 1);
            bor3.BorderBrush = Brushes.Black;
            bor3.Child = txt3;
            
            // 시스템 인식
            TextBlock txt4 = new TextBlock();
            txt4.Width = 152;
            txt4.Text = row[2].ToString();
            txt4.TextAlignment = TextAlignment.Center;

            Border bor4 = new Border();
            bor4.BorderThickness = new Thickness(1, 1, 1, 1);
            bor4.BorderBrush = Brushes.Black;
            bor4.Child = txt4;

            // 선택 버튼
            Button selBtn = new Button();
            selBtn.Width = 52;
            selBtn.Content = "선택";
            selBtn.Click += OnSelBtnClick;


            listSt.Children.Add(bor1);
            listSt.Children.Add(bor2);
            listSt.Children.Add(bor3);
            listSt.Children.Add(bor4);
            listSt.Children.Add(selBtn);


            // 최종 추가
            this.dataListSt.Children.Add(listSt);
        }

        private void OnSelBtnClick(object sender, RoutedEventArgs e)
        {
            App.DataMediaWin.PrevIndex = App.DataMediaWin.CurrentIndex;
            App.DataMediaWin.CurrentIndex = Int32.Parse(((((sender as Button).Parent as StackPanel).Children[0] as Border).Child as TextBlock).Text) - 1;
            App.DataMediaWin.ScrollViewFlag = true; // 스크롤 뷰 안내려가게 조절
            App.DataMediaWin.WindowStatusUpdate(App.DataMediaWin.CurrentIndex);
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        public void HumanResultAdjust(int index, string inputStr)
        {
            (((dataListSt.Children[index] as StackPanel).Children[2] as Border).Child as TextBlock).Text = inputStr;
            MoveScrollView(index);
            SignCurrentIndex();
        }
        public void MoveScrollView(int index)
        {
            int viewIndex = 0;
            if (index < 20)
            {
                viewIndex = 0;
            }
            else if (index >= dataListSt.Children.Count-20)
            {
                viewIndex = dataListSt.Children.Count - 1;
            }
            else
            {
                viewIndex = index + 20;
            }
            (((dataListSt.Children[viewIndex] as StackPanel).Children[2] as Border).Child as TextBlock).BringIntoView();
        }

        public void SignCurrentIndex()
        {
            // 이전 및 현재 보고 있는 칸에 색칠하기
            int curIndex = App.DataMediaWin.CurrentIndex;
            int prevIndex = App.DataMediaWin.PrevIndex;
            foreach(StackPanel listSt in dataListSt.Children)
            {
                int indexNum = Int32.Parse(((listSt.Children[0] as Border).Child as TextBlock).Text) - 1;
                Brush inputBrush = Brushes.Black;
                if (indexNum == App.DataMediaWin.CurrentIndex)
                {
                    // 현재 보고 있는 데이터
                    inputBrush = Brushes.Red;
                }
                else if(indexNum == App.DataMediaWin.PrevIndex)
                {
                    // 이전에 보고 있었던 데이터
                    inputBrush = Brushes.Yellow;
                }
                else
                {
                    // 그 외 나머지
                    inputBrush = null;
                }
                for (int childCount = 0; childCount < 4; childCount++)
                {
                    (listSt.Children[childCount] as Border).Background = inputBrush;
                }
            }
        }
    }
}
