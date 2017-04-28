// Knight's Tour using Warnsdorff's Rule for 8x8 chessboard
// https://en.wikipedia.org/wiki/Knight%27s_tour#Warnsdorf.27s_rule

using knights_tour;
using Knights_Tour.Adorner;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Knights_Tour
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Image _knight;

        private const short BoardLeft = 30;
        private const short BoardTop = 30;
        private const short CellWidth = 60;
        private const short CellHeight = 60;

        private int _stepsCount;
        private bool _canMove;
        private LogicHelper _logicHelper;
        private int _timerInterval = 800;

        //2D Array of positioins on chessboard 
        private int[,] _positions;

        private Point _dropPosition;

        //Statusbar texts
        String _statusBarInitialText = "Drag and drop the knight to set it to the new position.";
        String _statusBarRunTimeText = "To reset the tour, please press Ctrl+R...";
        

        public MainWindow()
        {
            InitializeComponent();
            InitializeKnightDropTarget();
        }

        private void InitializeKnightDropTarget()
        {
            _logicHelper = new LogicHelper();
            _canMove = false;
            _stepsCount = 0;
            _dropPosition = new Point(30, 30);
            _positions = new int[64, 2];
            _knight = new Knight();

            CreateDragTarget(_knight);            
            SetInitialPositionValues();
            UpdateStatusBarText(_statusBarInitialText);
        }        

        private void CreateDragTarget(Image dragTarget)
        {
            try
            {
                Canvas canvas = dragTarget.Parent as Canvas;
                if (canvas != null)
                {
                    canvas.Children.Remove(dragTarget);
                }
                DragDropHelper.SetIsDragSource(_knight, true);
                DragDropHelper.SetDropTarget(_knight, "knightDropTarget");

                ImageDragDropAdorner imageAdorner = new ImageDragDropAdorner();
                DragDropHelper.SetDragDropControl(_knight, imageAdorner);
                DragDropHelper.SetAdornerLayer(_knight, "adornerLayer");

                Canvas.SetLeft(_knight, _dropPosition.X);
                Canvas.SetTop(_knight, _dropPosition.Y);
                knightDropTarget.Children.Add(_knight);
                DataContext = _knight;
                DragDropHelper.ItemDropped += new EventHandler<DragDropEventArgs>(DragDropHelper_ItemDropped);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.InnerException.ToString());
            }
        }

        void DragDropHelper_ItemDropped(object sender, DragDropEventArgs e)
        {
            _dropPosition =  GetDropPosition(sender);

            Visual visual = e.Content as Visual;
            Image dragTarget = (Image)DragDropHelper.FindAncestor(typeof(Image), visual);
            CreateDragTarget(dragTarget);

            SetInitialPositionValues();
        }

        private Point GetDropPosition(object obj)
        {
            Point dropPosition = new Point();

            ImageDragDropAdorner adorner = (ImageDragDropAdorner)obj;
            
            double imageLeft = Canvas.GetLeft(adorner);
            double imageRight = Canvas.GetRight(adorner);
            double imageTop = Canvas.GetTop(adorner);
            double imageBottom = Canvas.GetBottom(adorner);

            double newLeft = Math.Truncate(imageLeft / CellWidth) * CellWidth + BoardLeft;
            double newRight = Math.Truncate(imageLeft / CellWidth) * (CellWidth + 1);
            double newTop = Math.Truncate(imageTop / CellHeight) * CellHeight + BoardTop;
            double newBottom = Math.Truncate(imageTop / CellHeight) * (CellHeight + 1);

            if (imageLeft < newLeft + CellWidth / 2)
            {
                dropPosition.X = newLeft;
            }
            else if (imageLeft > newLeft + CellWidth / 2)
            {
                dropPosition.X = newRight;
            }

            if (imageTop < newTop + CellHeight / 2)
            {
                dropPosition.Y = newTop;
            }
            else if (imageTop > newTop + CellHeight / 2)
            {
                dropPosition.Y = newBottom;
            }

            return dropPosition;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Tick += new EventHandler(TimerTick);

            timer.Start();
        }

        void TimerTick(object sender, EventArgs e)
        {
            if (_canMove)
            {
                DispatcherTimer timer = sender as DispatcherTimer;
                if (timer != null)
                {
                    timer.Interval = new TimeSpan(0, 0, 0, 0, _timerInterval);
                }

                //Check the number of steps, finish if it is 63
                if (_stepsCount > 62)
                {
                    _canMove = false;
                    return;
                }
                //Delete the Knight from the current position on the board
                knightDropTarget.Children.Remove(_knight);
                //Draw a circle for graphical representation of current step
                Ellipse ellipse = new Ellipse
                {
                    Height = CellHeight/2,
                    Width = CellWidth/2,
                    Margin = new Thickness(CellWidth/4),
                    Fill = new SolidColorBrush(Colors.Black)
                };
                //Textblock for current step number representation
                TextBlock currentPosition = new TextBlock
                {
                    Height = CellHeight / 2,
                    Width = CellWidth / 2,
                    Margin = new Thickness(CellWidth / 4),
                    Background = new SolidColorBrush(Colors.Transparent),
                    TextAlignment = TextAlignment.Center,
                    FontSize = 20,
                    Foreground = new SolidColorBrush(Colors.White),
                    Text = _stepsCount.ToString()
                };
                // Set the circle with the number of the current position on the board
                SetOnBoard(ellipse, _positions[_stepsCount, 0], _positions[_stepsCount, 1]);
                SetOnBoard(currentPosition, _positions[_stepsCount, 0], _positions[_stepsCount, 1]);
                _stepsCount++;
                // Set the Knight on the new position on the board
                SetOnBoard(_knight, _positions[_stepsCount, 0], _positions[_stepsCount, 1]);
            }
        }

        #region Keyboard events
        //Keyboard shortcuts' events
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl))
            {
                switch (e.Key)
                {
                    //handle G key
                    case Key.G:
                    StartTour();
                    break;
                    //handle R key
                    case Key.R:
                    ResetBoard();
                    break;                   
                }
            }
            //handle F1 key
            if (e.Key == Key.F1)
            {    
                MenuItem_Help_Click(sender, e);
            }
        }
        #endregion        

        #region Menu Items 
        //Menu bar items click events
        private void MenuItem_Start_Click(object sender, RoutedEventArgs e)
        {
            StartTour();
        }

        private void MenuItem_Reset_Click(object sender, RoutedEventArgs e)
        {
            ResetBoard();
        }

        private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuItem_Options_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Visibility visibality = Options.Visibility;
            if (visibality == Visibility.Visible)
            {
                Options.Visibility = Visibility.Collapsed;
                MenuItemOptions.Header = "Show options...";
            }
            else
            {
                Options.Visibility = Visibility.Visible;
                MenuItemOptions.Header = "Hide options...";
            }
        }

        private void MenuItem_Help_Click(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            string appRoot = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string filePath = System.IO.Path.Combine(appRoot, "Help.chm");
            if (!System.IO.File.Exists(filePath))
            {
                // The path to the Help folder.
                string directory = System.IO.Path.Combine(appRoot, "../../Help");
                // The path to the Help file.
                filePath = System.IO.Path.Combine(directory, "Help.chm");
            }
            // Launch the Help file.
            if (System.IO.File.Exists(filePath))
            {
                Process.Start(filePath);
            } else
            {
                MessageBox.Show("File not found!");
            }
            
        }

        private void MenuItem_About_Click(object sender, RoutedEventArgs e)
        {
            using (AboutBox box = new AboutBox())
            {
                box.ShowDialog();
            }
        }
        #endregion

        #region Utilities
        private void SetOnBoard(UIElement element, int horizontal, int vertical)
        {
            Canvas.SetLeft(element, BoardLeft + horizontal * CellWidth);
            Canvas.SetTop(element, BoardTop + vertical * CellHeight);
            knightDropTarget.Children.Add(element);
        }

        private void EnableUIControls(bool status)
        {
            MenuItemStart.IsEnabled = status;
            HorizontalPosition.IsEnabled = status;
            VerticalPosition.IsEnabled = status;
            StartButton.IsEnabled = status;
        }

        private void StartTour()
        {
            if (_canMove == false)
            {
                EnableUIControls(false);
                _canMove = true;
                _positions = _logicHelper.GeneratePositions((int)_dropPosition.X / CellWidth, (int)_dropPosition.Y / CellHeight);
                UpdateStatusBarText(_statusBarRunTimeText);
            }
        }

        private void ResetBoard()
        {

            System.Collections.ObjectModel.Collection<UIElement> childrenToRemove = new System.Collections.ObjectModel.Collection<UIElement>();
            foreach (UIElement uielement in knightDropTarget.Children)
            {
                if ((uielement.GetType() == typeof(TextBlock)) || uielement.GetType() == typeof(Ellipse))
                {
                    childrenToRemove.Add(uielement);
                }
            }
            foreach (UIElement textBlock in childrenToRemove)
            {
                knightDropTarget.Children.Remove(textBlock);
            }

            knightDropTarget.Children.Remove(_knight);
            InitializeKnightDropTarget();
            EnableUIControls(true);
        }

        private void UpdateStatusBarText(string text)
        {
            StatusBarText.Text = text;
        }

        private void slSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _timerInterval = 1000 - (int)slSpeed.Value * 100;
        }

        private void SetInitialPositionValues()
        {
            HorizontalPosition.SelectedIndex = (int)(_dropPosition.X - BoardLeft) / CellWidth;
            VerticalPosition.SelectedIndex = VerticalPosition.Items.Count - 1 - (int)(_dropPosition.Y - BoardTop) / CellHeight;
        }

        private void VerticalPosition_SelectionChanged(object sender, RoutedEventArgs e)
        {
            ComboBox cmbBox = sender as ComboBox;
            if (cmbBox != null)
            {
                int verticalIndex = cmbBox.Items.Count - 1 - cmbBox.SelectedIndex;
                _dropPosition.Y = verticalIndex * CellHeight + BoardTop;

                knightDropTarget.Children.Remove(_knight);
                SetOnBoard(_knight, HorizontalPosition.SelectedIndex, verticalIndex);
            }
        }

        private void HorizontalPosition_SelectionChanged(object sender, RoutedEventArgs e)
        {
            ComboBox cmbBox = sender as ComboBox;
            if (cmbBox != null)
            {
                int horizontalIndex = cmbBox.SelectedIndex;
                _dropPosition.X = horizontalIndex * CellWidth + BoardLeft;

                knightDropTarget.Children.Remove(_knight);
                SetOnBoard(_knight, horizontalIndex, VerticalPosition.Items.Count - 1 - VerticalPosition.SelectedIndex);
            }
        }
        #endregion
    }
}

