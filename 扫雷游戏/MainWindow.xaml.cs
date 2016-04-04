//*******************************************************************
//
//      文件名（File Name）：          MainWindow.xaml 
//
//      功能描述（Description）：      扫雷游戏的内层逻辑    
//
//      数据表（Tables）：             nothing
//                            
//      作者（Author）：               MH
//
//      日期（Create Date）：          2014.8.5
//
//
//*******************************************************************
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
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace 扫雷游戏
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            timer.Interval = TimeSpan.FromMilliseconds(1000);//设置为1秒
            timer.Tick += timer_Tick;
        }

        

        private int GameSize = 9;//方块数
        private int MineNum = 10;//地雷的数目
        private const int HAVEMINE = -1;//表示有地雷
        private int[,] Blocks;//内部表示方块状态的数组，-1表示有地雷，其他数字表示周围地雷的个数
        private Button[,] Btns;//管理Buttons的数组
        private const int COVER = 0;
        private const int UNCOVER = -1;
        private const int FLAG = 1;
        private int[,] Turn;//表示该方块有没有翻开，-1表示翻开，0表示没有翻开，1表示插上了红旗
        private int Time = 0;//游戏所用时间
        private int LeftNumOfMine;//剩余地雷的数量
        private bool IsFirstBlock = true;//是不是被第一个点击的方块，用来开计时器
        private DispatcherTimer timer = new DispatcherTimer();

        /// <summary>
        /// 在Blocks数组中随机放置地雷标记
        /// </summary>
        private void LayMines()
        {
            Random r = new Random();
            for (int i = 0; i < MineNum; )
            {
                int row = r.Next(0, GameSize);
                int col = r.Next(0, GameSize);
                if (Blocks[row, col] != HAVEMINE)
                {
                    Blocks[row, col] = HAVEMINE;
                    i++;
                }
                else
                {
                    continue;
                }
            }
        }

        /// <summary>
        /// 计算一个方块周围的地雷数
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        private int CalAroundBlock(int row,int col)
        {
            int num_around = 0;//周围的地雷数
            int minrow = (row == 0) ? 0 : (row - 1);//得出最小行数
            int maxrow = (row == GameSize - 1) ? (GameSize - 1) : (row + 1);
            int mincol = (col == 0) ? 0 : col - 1;
            int maxcol = (col == GameSize - 1) ? (GameSize - 1) : (col + 1);
            for (int i = minrow; i <= maxrow; i++)
            {
                for (int j = mincol; j <= maxcol; j++)
                {
                    if (Blocks[i, j] == HAVEMINE)
                        num_around++;
                }
            }
            return num_around;
        }

        /// <summary>
        /// 设置一个方块周围的地雷数
        /// </summary>
        private void SetAroundNumbers()
        {
            for (int i = 0; i < GameSize; i++)
            {
                for (int j = 0; j < GameSize; j++)
                {
                    if (Blocks[i, j] == HAVEMINE)
                        continue;
                    else
                    {
                        Blocks[i, j] = CalAroundBlock(i, j);
                    }
                }
            }
        }

        /// <summary>
        /// 点到四周没有地雷的方块的扩展
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        private void ExpandBlankBlock(int row,int col)
        {
            int minrow = (row == 0) ? 0 : (row - 1);//得出最小行数
            int maxrow = (row == GameSize - 1) ? (GameSize - 1) : (row + 1);
            int mincol = (col == 0) ? 0 : col - 1;
            int maxcol = (col == GameSize - 1) ? (GameSize - 1) : (col + 1);
            for (int i = minrow; i <= maxrow; i++)
            {
                for (int j = mincol; j <= maxcol; j++)
                {
                    if(Turn[i,j]==COVER)
                    {
                        Btns[i, j].Visibility = Visibility.Hidden;
                        Turn[i, j] = UNCOVER;
                        if (CalAroundBlock(i, j) == 0)
                            ExpandBlankBlock(i, j);
                    }
                }
            }

        }


        /// <summary>
        /// 放置标签，内容根据已经生成的方块状态来设置
        /// </summary>
        private void LayLabels()
        {
            for (int i = 0; i < GameSize; i++)
            {
                for (int j = 0; j < GameSize; j++)
                {
                    Label lbl = new Label();
                    lbl.SetResourceReference(Label.TemplateProperty, "myLable");
                    lbl.FontSize = 37;
                    lbl.FontWeight = FontWeights.Bold;
                    lbl.BorderBrush = new SolidColorBrush(Colors.Black);
                    lbl.BorderThickness = new Thickness(1.4, 1.4, 1.4, 1.4);
                    lbl.HorizontalContentAlignment = HorizontalAlignment.Center;
                    gridBoard.Children.Add(lbl);
                    lbl.SetValue(Grid.RowProperty, i);
                    lbl.SetValue(Grid.ColumnProperty, j);
                    if(Blocks[i,j]==HAVEMINE)
                    {
                        lbl.Content = new Image { Source = new BitmapImage(new Uri("Images/mine.png",UriKind.Relative)) };
                    }
                    else if(Blocks[i,j]>0)
                    {
                        lbl.Padding = new Thickness(1,-8,1,8);
                        lbl.Content = Blocks[i, j].ToString();
                        if (Blocks[i, j] == 1)
                            lbl.Foreground = new SolidColorBrush(Colors.Blue);
                        else if(Blocks[i, j] == 2)
                            lbl.Foreground = new SolidColorBrush(Colors.Green);
                        else if (Blocks[i, j] == 3)
                            lbl.Foreground = new SolidColorBrush(Colors.Green);
                    }
                }
            }
        }

        /// <summary>
        /// 放置按钮
        /// </summary>
        private void LayBtns()
        {
            for (int i = 0; i < GameSize; i++)
            {
                for (int j = 0; j < GameSize; j++)
                {
                    Button btn = new Button();
                    btn.Margin = new Thickness(0.3);
                    if(j!=GameSize-1&&i!=GameSize-1)
                    {
                        btn.Effect = new DropShadowEffect { Direction = -45, Opacity = 0.6, BlurRadius = 0, ShadowDepth=6};
                    }
                    btn.Name = "Block" + i.ToString() + "_" + j.ToString();
                    btn.SetResourceReference(Button.TemplateProperty, "myButton");
                    btn.Click += new RoutedEventHandler(btn_Click);
                    btn.Background = new SolidColorBrush(Color.FromRgb(97,138,230));
                    btn.MouseRightButtonUp += new MouseButtonEventHandler(btn_MouseRightButtonUp);
                    gridBoard.Children.Add(btn);
                    btn.SetValue(Grid.RowProperty, i);
                    btn.SetValue(Grid.ColumnProperty, j);
                    Btns[i, j] = btn;
                }
            }
        }

        /// <summary>
        /// 判断游戏结果
        /// </summary>
        /// <returns></returns>
        private bool JudgeGameResult()
        {
            for (int i = 0; i < GameSize; i++)
            {
                for (int j = 0; j < GameSize; j++)
                {
                    if(Blocks[i,j]==HAVEMINE)//存在地雷的方块未标上红旗
                    {
                        if (Turn[i, j] != FLAG)
                            return false;
                    }
                    else//不是地雷的方块未翻开
                    {
                        if (Turn[i, j] == COVER)
                            return false;
                    }
                }
            }
            return true;
        }

        private void btn_Click(object sender, RoutedEventArgs e)
        {
            if(IsFirstBlock)
            {
                timer.Start();
                IsFirstBlock = false;
            }

            Button btn = (Button)sender;
            int i = Convert.ToInt32(btn.Name[5].ToString());
            int j = Convert.ToInt32(btn.Name[7].ToString());
            if (Turn[i, j] != FLAG)//没有插上红旗
            {
                if (Blocks[i, j] == 0)
                {
                    ExpandBlankBlock(i, j);
                    if(JudgeGameResult())
                    {
                        timer.Stop();
                        MessageBox.Show("你赢了！");
                        Clear();
                        Time = 0;
                        lblTime.Content = Time.ToString();
                        IsFirstBlock = true;
                    }
                }
                else if (Blocks[i, j] > 0)
                {
                    Turn[i, j] = UNCOVER;
                    btn.Visibility = Visibility.Hidden;
                    if (JudgeGameResult())
                    {
                        timer.Stop();
                        MessageBox.Show("你赢了！");
                        Clear();
                        Time = 0;
                        lblTime.Content = Time.ToString();
                        IsFirstBlock = true;
                    }
                }
                else
                {
                    btn.Visibility = Visibility.Hidden;
                    ShowMines();
                    timer.Stop();
                    MessageBox.Show("你输了!");
                    Clear();
                    Time = 0;
                    lblTime.Content = Time.ToString();
                    IsFirstBlock = true;
                }
            }
        }

        private void btn_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Button btn = (Button)sender;
            int i = Convert.ToInt32(btn.Name[5].ToString());
            int j = Convert.ToInt32(btn.Name[7].ToString());
            if(Turn[i,j]!=FLAG)
            {
                Turn[i, j] = FLAG;
                btn.Content = new Image { Source = new BitmapImage(new Uri("Images/flag.png", UriKind.Relative)) };
                LeftNumOfMine--;
                lblLeftMines.Content = LeftNumOfMine.ToString();
                if(JudgeGameResult())
                {
                    timer.Stop();
                    MessageBox.Show("你赢了！");
                    Clear();
                    Time = 0;
                    lblTime.Content = Time.ToString();
                    IsFirstBlock = true;
                }
            }
            else
            {
                Turn[i, j] = COVER;
                btn.Content = null;
                LeftNumOfMine++;
                lblLeftMines.Content = LeftNumOfMine.ToString();
            }
        }

        /// <summary>
        /// 把地雷全部显示出来
        /// </summary>
        private void ShowMines()
        {
            for(int i=0;i<GameSize;i++)
            {
                for (int j = 0; j < GameSize; j++)
                {
                    if(Blocks[i,j]==HAVEMINE)
                    {
                        Btns[i, j].Visibility = Visibility.Hidden;
                    }
                }
            }
        }

        /// <summary>
        /// 计时器的事件处理方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_Tick(object sender, EventArgs e)
        {
            Time++;
            lblTime.Content = Time.ToString();
        }

        /// <summary>
        /// 初始化游戏
        /// </summary>
        private void InitGame()
        {
            Blocks = new int[GameSize, GameSize];
            Turn = new int[GameSize, GameSize];
            Btns = new Button[GameSize, GameSize];
            LeftNumOfMine = MineNum;
            LayMines();
            SetAroundNumbers();
            LayLabels();
            LayBtns();
        }
        

        /// <summary>
        /// 清理游戏界面
        /// </summary>
        private void Clear()
        {
            gridBoard.Children.Clear();
            this.mnu.Focus();
            
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MnuAbout_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("本游戏由MH童鞋开发！");
        }

        private void MnuNewGame_Click(object sender, RoutedEventArgs e)
        {
            InitGame();
        }


        private void New_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            InitGame();
        }

        


    }
}
