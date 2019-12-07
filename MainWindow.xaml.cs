using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Threading;

namespace _8_puzzle
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        //chiều dài rộng mỗi ô
        int width = 80;
        int height;//chiều dài tự mở rộng theo tiwr lệ của chiều rộng

        int startX = 65;
        int startY = 80;


        int numberPuzzle = 3;
        Image[] sourceImg;
        int[,] A;

        //lưu vị trí khoảng trống
        int IBase;
        int JBase;

        //vị trí khung preview
        int PosiPreviewX = 450;
        int PosiPreviewY = 80;
        string uri;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dt.Interval = TimeSpan.FromSeconds(1);
            dt.Tick += dtTicker;
            countMinute = 3;
            countSecond = 0;

            sourceImg = new Image[numberPuzzle * numberPuzzle];
            A = new int[numberPuzzle, numberPuzzle];
            uri = "Images/1.png";
            var source = new BitmapImage(new Uri("Images/1.png", UriKind.Relative));
            height = (int)(source.Height / (source.Width / width));
            previewImage.Source = source;
            Canvas.SetLeft(previewImage, PosiPreviewX);
            Canvas.SetTop(previewImage, PosiPreviewY);
            // Choose();
            CutImage(source);
            RanDom();
            Load_Interface();
        }

        List<Image> cropImages = new List<Image>();
        private void CutImage(BitmapImage source)
        {
            int dem = 0;
            for (int i = 0; i < numberPuzzle; i++)
            {
                for (int j = 0; j < numberPuzzle; j++)
                {
                    if (!((i == numberPuzzle - 1) && (j == numberPuzzle - 1)))
                    {
                        var h = (int)source.Height / numberPuzzle;
                        var w = (int)source.Width / numberPuzzle;
                        var rect = new Int32Rect(j * w, i * h, w, h);
                        CroppedBitmap cropBitmap = new CroppedBitmap(source,
                            rect);

                        var cropImage = new Image();
                        cropImage.Stretch = Stretch.Fill;
                        cropImage.Width = width;
                        cropImage.Height = height;
                        cropImage.Source = cropBitmap;
                        cropImages.Add(cropImage);
                        canvas.Children.Add(cropImage);
                        Canvas.SetLeft(cropImage, startX + j * (cropImage.Width + 2));
                        Canvas.SetTop(cropImage, startY + i * (cropImage.Height + 2));

                        //lưu lại vào mảng để load khi random()
                        dem++;
                        sourceImg[dem] = cropImage;
                        A[i, j] = dem;

                        //add event
                        //cropImage.MouseLeftButtonDown += CropImage_MouseLeftButtonDown;
                        //cropImage.PreviewMouseLeftButtonUp += CropImage_PreviewMouseLeftButtonUp;
                        cropImage.Tag = new Tuple<int, int>(i, j);
                        //cropImage.MouseLeftButtonUp
                    }
                }
            }
        }


        bool _isDragging = false;
        Image _selectedBitmap = null;
        Point _lastPosition;
        Point _startPosition;
        private void CropImage_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            //Image img=sender as Image();
            var tuple = _selectedBitmap.Tag as Tuple<int, int>;
            int a = tuple.Item1;
            int b = tuple.Item2;
            var position = e.GetPosition(this);

            int x = (int)(position.X - startX) / (width + 2) * (width + 2) + startX;
            int y = (int)(position.Y - startY) / (height + 2) * (height + 2) + startY;
            int j = (x - startX) / width;
            int i = (y - startY) / height;

            if (!(i == IBase && j == JBase) || x > startX + numberPuzzle * width || y > startY + numberPuzzle * height || !((a == IBase && (b - JBase == 1 || b - JBase == -1)) || (b == JBase && (a - IBase == 1 || a - IBase == -1)))) //cần thêm điều kiện có nằm gần cục màu trắng không
            {
                x = (int)(_startPosition.X - startX) / (width + 2) * (width + 2) + startX;
                y = (int)(_startPosition.Y - startY) / (height + 2) * (height + 2) + startY;
            }
            else
            {
                Swap(ref A[IBase, JBase], ref A[a, b]);
                _selectedBitmap.Tag = new Tuple<int, int>(IBase, JBase);
                JBase = b;
                IBase = a;
            }

            Canvas.SetLeft(_selectedBitmap, x);
            Canvas.SetTop(_selectedBitmap, y);

            if (Win())
            {
                dt.Stop();
                MessageBox.Show("you won!");
            }
        }

        private void CropImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
            _selectedBitmap = sender as Image;
            _lastPosition = e.GetPosition(this);
            _startPosition = e.GetPosition(this);
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            var position = e.GetPosition(this);

            int i = ((int)position.Y - startY) / height;
            int j = ((int)position.X - startX) / width;

            if (_isDragging)
            {
                var dx = position.X - _lastPosition.X;
                var dy = position.Y - _lastPosition.Y;

                var lastLeft = Canvas.GetLeft(_selectedBitmap);
                var lastTop = Canvas.GetTop(_selectedBitmap);
                Canvas.SetLeft(_selectedBitmap, lastLeft + dx);
                Canvas.SetTop(_selectedBitmap, lastTop + dy);

                _lastPosition = position;
            }
        }


        public void RanDom()
        {
            int n = numberPuzzle * numberPuzzle;
            int[] b = new int[n];

            for (int i = 0; i < n; i++)
            {
                b[i] = i + 1;
            }

            Random ran = new Random();

            for (int i = 2; i < n - 2; i++)
            {
                int soRan = ran.Next(2, n - 2);
                int temp = b[soRan];
                Swap(ref b[soRan], ref b[soRan - 1]);
                Swap(ref b[soRan], ref b[soRan + 1]);
                Swap(ref b[soRan], ref b[soRan - 2]);
                Swap(ref b[soRan], ref b[soRan + 2]);
            }

            int dem = 0;
            for (int i = 0; i < numberPuzzle; i++)
            {
                for (int j = 0; j < numberPuzzle; j++)
                {
                    A[i, j] = b[dem++];
                }
            }
        }

        public void Swap(ref int a, ref int b)
        {
            int temp = a;
            a = b;
            b = temp;
        }


        public void Load_Interface()
        {
            for (int i = 0; i < numberPuzzle; i++)
            {
                for (int j = 0; j < numberPuzzle; j++)
                {
                    if (A[i, j] != numberPuzzle * numberPuzzle)
                    {
                        sourceImg[A[i, j]].Tag = new Tuple<int, int>(i, j);
                        Canvas.SetLeft(sourceImg[A[i, j]], startX + j * (sourceImg[A[i, j]].Width + 2));
                        Canvas.SetTop(sourceImg[A[i, j]], startY + i * (sourceImg[A[i, j]].Height + 2));
                    }
                    else
                    {
                        IBase = i;
                        JBase = j;
                    }
                }
            }
        }

        public bool Win()
        {
            int dem = 1;
            for (int i = 0; i < numberPuzzle; i++)
            {
                for (int j = 0; j < numberPuzzle; j++)
                {
                    if (A[i, j] != dem++)
                        return false;
                }
            }
            return true;
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            RanDom();
            dt.Stop();
            Load_Interface();

            // Disable event
            foreach (Image cropImage in cropImages)
            {
                cropImage.MouseLeftButtonDown -= CropImage_MouseLeftButtonDown;
                cropImage.PreviewMouseLeftButtonUp -= CropImage_PreviewMouseLeftButtonUp;
            }
            upBtn.Click -= Control_Click;
            downBtn.Click -= Control_Click;
            leftBtn.Click -= Control_Click;
            rightBtn.Click -= Control_Click;

            countSecond = 0;
            second.Content = "0" + countSecond.ToString();
            countMinute = 3;
            minute.Content = countMinute.ToString();
        }

        private void ChooseImage_Click(object sender, RoutedEventArgs e)
        {
            dt.Stop();
            if (Choose())
            {
                RanDom();
                Load_Interface();
            }
            //else
            //    dt.Start();
        }

        public bool Choose()
        {
            var screen = new OpenFileDialog();
            screen.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            if (screen.ShowDialog() == true)
            {
                canvas.Children.Clear();
                uri = screen.FileName;
                var source = new BitmapImage(
                    new Uri(screen.FileName, UriKind.Absolute));
                height = (int)(source.Height / (source.Width / width));
                //previewImage.Width = 250;
                //previewImage.Height = 250;
                previewImage.Source = source;
                //Canvas.SetLeft(previewImage, PosiPreviewX);
                //Canvas.SetTop(previewImage, PosiPreviewY);
                // Bat dau cat thanh 9 manh
                CutImage(source);
                return true;
            }
            return false;
        }

        public void moveControl(int a, int b)
        {
            var img = sourceImg[A[a, b]];
            var tuple = img.Tag as Tuple<int, int>;
            int i = tuple.Item1;
            int j = tuple.Item2;
            Swap(ref A[i, j], ref A[IBase, JBase]);

            Canvas.SetLeft(img, startX + JBase * (width + 2));
            Canvas.SetTop(img, startY + IBase * (height + 2));

            img.Tag = new Tuple<int, int>(IBase, JBase);
            IBase = i;
            JBase = j;
        }

        private void Control_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn.Content.Equals("Up") && IBase < 2)
            {
                moveControl(IBase + 1, JBase);
            }

            if (btn.Content.Equals("Down") && IBase > 0)
            {
                moveControl(IBase - 1, JBase);
            }

            if (btn.Content.Equals("Left") && JBase < 2)
            {
                moveControl(IBase, JBase + 1);
            }
            if (btn.Content.Equals("Right") && JBase > 0)
            {
                moveControl(IBase, JBase - 1);
            }

            if (Win())
            {
                dt.Stop();
                MessageBox.Show("You Won!");
            }

        }

        DispatcherTimer dt = new DispatcherTimer();
        //DispatcherTimer dt1 = new DispatcherTimer();

        int countMinute;
        int countSecond;
        private void dtTicker(object sender, EventArgs e)
        {

            string s = countSecond.ToString();
            if (countSecond < 10)
            {
                s = "0" + s;
            }
            minute.Content = countMinute.ToString();
            second.Content = s;
            if (countMinute == 0)
            {
                if (countSecond == 0)
                {
                    dt.Stop();
                    MessageBox.Show("Game Over!");

                }
            }
            if (countSecond == 0)
            {
                countMinute--;
                countSecond = 60;
            }
            countSecond--;
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            //RanDom();
            //Load_Interface();

            // Add event
            foreach(Image cropImage in cropImages)
            {
                cropImage.MouseLeftButtonDown += CropImage_MouseLeftButtonDown;
                cropImage.PreviewMouseLeftButtonUp += CropImage_PreviewMouseLeftButtonUp;
            }
            upBtn.Click += Control_Click;
            downBtn.Click += Control_Click;
            leftBtn.Click += Control_Click;
            rightBtn.Click += Control_Click;

            if (countMinute == 0 && countSecond == 0)
            {
                countMinute = 3;
                countSecond = 0;
            }
            dt.Start();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            dt.Stop();
            var screen = new FileName();
            StreamWriter Wr = null;
            if (screen.ShowDialog() == true)
            {
                if (!screen.filename.ToString().Equals(""))
                {
                    Wr = new StreamWriter(screen.filename);

                    //ghi dòng đầu phút
                    Wr.WriteLine($"{ countMinute}");
                    //dòng kế tiếp là giây
                    Wr.WriteLine($"{countSecond}");

                    //lưu lại đường dẫn
                    Wr.WriteLine(uri);

                    //Lưu ma trận biểu diễn
                    //lưu ma trận biểu diễn
                    for (int i = 0; i < numberPuzzle; i++)
                    {
                        for (int j = 0; j < numberPuzzle; j++)
                        {
                            Wr.Write($"{A[i, j]} ");

                            if (j == numberPuzzle - 1)
                            {
                                Wr.Write(" ");
                            }
                        }
                        Wr.WriteLine("");
                    }

                    Wr.Close();
                    MessageBox.Show("Save successfully!");
                }
            }

        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            var screen = new OpenFileDialog();
            screen.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            if (screen.ShowDialog() == true)
            {
                var filename = screen.FileName;

                var Rr = new StreamReader(filename);

                //đọc dòng đầu lấy minute
                countMinute = int.Parse(Rr.ReadLine());
                minute.Content = countMinute.ToString();

                //đọc dòng kế tiếp lấy second
                countSecond = int.Parse(Rr.ReadLine());
                second.Content = countSecond.ToString();

                //đọc link
                uri = Rr.ReadLine();
                canvas.Children.Clear();
                var source = new BitmapImage(new Uri(uri, UriKind.Absolute));
                height = (int)(source.Height / (source.Width / width));
                CutImage(source);

                //đọc ma trận biểu diễn
                for (int i = 0; i < numberPuzzle; i++)
                {
                    var token = Rr.ReadLine().Split(new string[] { " " }, StringSplitOptions.None);
                    for (int j = 0; j < numberPuzzle; j++)
                    {
                        A[i, j] = int.Parse(token[j]);
                    }
                }
                previewImage.Source = source;
                Load_Interface();
            }
        }
    }
}
