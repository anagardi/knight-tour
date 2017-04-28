using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Controls;

namespace Knights_Tour
{
    public class DragDropHelper
    {
        #region Private Members
        private Point _initialMousePosition;
        private static Point _targetPosition;
        private Point _delta;
        private Point _scrollTarget;
        private Point _dropPosition;
        private UIElement _dropTarget;
        private Rect _dropBoundingBox;
        private bool _mouseCaptured;
        private object _draggedData;
        private static Canvas _topCanvas;
        private Canvas _adornerLayer;
        private DragDropAdornerBase _adorner;

        private static DragDropHelper _instance;
        private static DragDropHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DragDropHelper();
                }
                return _instance;
            }
        }

        #endregion

        #region Attached Properties
        public static bool GetIsDragSource(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsDragSourceProperty);
        }
        public static void SetIsDragSource(DependencyObject obj, bool value)
        {
            obj.SetValue(IsDragSourceProperty, value);
        }
        public static UIElement GetDragDropControl(DependencyObject obj)
        {
            return (UIElement)obj.GetValue(DragDropControlProperty);
        }
        public static void SetDragDropControl(DependencyObject obj, UIElement value)
        {
            obj.SetValue(DragDropControlProperty, value);
        }
        public static string GetDropTarget(DependencyObject obj)
        {
            return (string)obj.GetValue(DropTargetProperty);
        }
        public static void SetDropTarget(DependencyObject obj, string value)
        {
            obj.SetValue(DropTargetProperty, value);
        }
        public static string GetAdornerLayer(DependencyObject obj)
        {
            return (string)obj.GetValue(AdornerLayerProperty);
        }
        public static void SetAdornerLayer(DependencyObject obj, string value)
        {
            obj.SetValue(AdornerLayerProperty, value);
        }

        public static readonly DependencyProperty IsDragSourceProperty =
            DependencyProperty.RegisterAttached("IsDragSource", typeof(bool), typeof(DragDropHelper), new UIPropertyMetadata(false, IsDragSourceChanged));

        public static readonly DependencyProperty DragDropControlProperty =
            DependencyProperty.RegisterAttached("DragDropControl", typeof(UIElement), typeof(DragDropHelper), new UIPropertyMetadata(null));

        public static readonly DependencyProperty AdornerLayerProperty =
             DependencyProperty.RegisterAttached("AdornerLayer", typeof(string), typeof(DragDropHelper), new UIPropertyMetadata(null));

        public static readonly DependencyProperty DropTargetProperty =
            DependencyProperty.RegisterAttached("DropTarget", typeof(string), typeof(DragDropHelper), new UIPropertyMetadata(string.Empty));


        private static void IsDragSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var dragSource = obj as UIElement;
            if (dragSource != null)
            {
                if (Object.Equals(e.NewValue, true))
                {
                    dragSource.PreviewMouseLeftButtonDown += Instance.DragSource_PreviewMouseLeftButtonDown;
                    dragSource.PreviewMouseLeftButtonUp += Instance.DragSource_PreviewMouseLeftButtonUp;
                    dragSource.PreviewMouseMove += Instance.DragSource_PreviewMouseMove;
                }
                else
                {
                    dragSource.PreviewMouseLeftButtonDown -= Instance.DragSource_PreviewMouseLeftButtonDown;
                    dragSource.PreviewMouseLeftButtonUp -= Instance.DragSource_PreviewMouseLeftButtonUp;
                    dragSource.PreviewMouseMove -= Instance.DragSource_PreviewMouseMove;
                }
            }
        }


        #endregion

        #region Utilities
        public static FrameworkElement FindAncestor(Type ancestorType, Visual visual)
        {
            while (visual != null && !ancestorType.IsInstanceOfType(visual))
            {
                visual = (Visual)VisualTreeHelper.GetParent(visual);
            }
            return visual as FrameworkElement;
        }

        public static bool IsMovementBigEnough(Point initialMousePosition, Point currentPosition)
        {
            return (Math.Abs(currentPosition.X - initialMousePosition.X) >= SystemParameters.MinimumHorizontalDragDistance ||
                 Math.Abs(currentPosition.Y - initialMousePosition.Y) >= SystemParameters.MinimumVerticalDragDistance);
        }          
        #endregion

        #region Drag Handlers
        private void DragSource_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                _initialMousePosition = e.GetPosition(_topCanvas);

                Visual visual = e.OriginalSource as Visual;

                _topCanvas = (Canvas)DragDropHelper.FindAncestor(typeof(Canvas), visual);                          

                string adornerLayerName = GetAdornerLayer(sender as DependencyObject);
                _adornerLayer = (Canvas)_topCanvas.FindName(adornerLayerName);

                string dropTargetName = GetDropTarget(sender as DependencyObject);

                _dropTarget = (UIElement)_topCanvas.FindName(dropTargetName);

                _draggedData = (sender as FrameworkElement).DataContext;

            }
            catch (Exception exc)
            {
                Console.WriteLine("Exception in DragDropHelper: " + exc.InnerException.ToString());
            }
        }

        // Drag = mouse down + move by a certain amount
        private void DragSource_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!_mouseCaptured && _draggedData != null)
            {
                // Only drag when user moved the mouse by a reasonable amount               
                {
                    _adorner = (DragDropAdornerBase)GetDragDropControl(sender as DependencyObject);
                    _adorner.DataContext = _draggedData;
                    _adorner.Opacity = 0.7;

                    _adornerLayer.Visibility = Visibility.Visible;
                    _adornerLayer.Children.Add(_adorner);
                    _mouseCaptured = Mouse.Capture(_adorner);

                    int horizontalshift = _initialMousePosition.X > 510 ? 0 : 30;
                    int verticalshift = _initialMousePosition.Y > 510 ? 0 : 30;

                    _targetPosition.X = Math.Truncate((_initialMousePosition.X - horizontalshift) / 60) * 60 + horizontalshift;
                    _targetPosition.Y = Math.Truncate((_initialMousePosition.Y - verticalshift) / 60) * 60 + verticalshift;

                    Canvas.SetLeft(_adorner, _initialMousePosition.X);
                    Canvas.SetTop(_adorner, _initialMousePosition.Y);

                    _adornerLayer.PreviewMouseMove += new MouseEventHandler(_adorner_MouseMove);
                    _adornerLayer.PreviewMouseUp += new MouseButtonEventHandler(_adorner_MouseUp);
                }
            }
        }

        private void _adorner_MouseMove(object sender, MouseEventArgs e)
        {
            Point currentPoint = e.GetPosition(_topCanvas);

            _delta = new Point(_initialMousePosition.X - currentPoint.X, _initialMousePosition.Y - currentPoint.Y);
            _scrollTarget = new Point(_targetPosition.X - _delta.X, _targetPosition.Y - _delta.Y);

            Canvas.SetLeft(_adorner, _scrollTarget.X);
            Canvas.SetTop(_adorner, _scrollTarget.Y);

            _adorner.AdornerDropState = DropState.CannotDrop;

            if (_dropTarget != null)
            {
                GeneralTransform t = _dropTarget.TransformToVisual(_adornerLayer);
                _dropBoundingBox = t.TransformBounds(new Rect(0, 0, _dropTarget.RenderSize.Width, _dropTarget.RenderSize.Height));

                if (e.GetPosition(_adornerLayer).X > _dropBoundingBox.Left &&
                    e.GetPosition(_adornerLayer).X < _dropBoundingBox.Right &&
                    e.GetPosition(_adornerLayer).Y > _dropBoundingBox.Top &&
                    e.GetPosition(_adornerLayer).Y < _dropBoundingBox.Bottom)
                {
                    _adorner.AdornerDropState = DropState.CanDrop;
                }
            }
        }

        private void _adorner_MouseUp(object sender, MouseEventArgs e)
        {
            switch (_adorner.AdornerDropState)
            {
                case DropState.CanDrop:
                try
                {
                    ((StoryMovementTarget)_adorner.Resources["canDrop"]).Completed += (s, args) =>
                    {
                        _adornerLayer.Children.Clear();
                        _adornerLayer.Visibility = Visibility.Hidden;
                        _dropPosition = (e.GetPosition(_topCanvas));                      

                    };
                    ((StoryMovementTarget)_adorner.Resources["canDrop"]).Begin(_adorner);

                    if (ItemDropped != null)
                        ItemDropped(_adorner, new DragDropEventArgs(_draggedData));
                    
                }
                catch (Exception exc)
                {
                    MessageBox.Show("Exception in DragDropHelper: " + exc.InnerException.ToString());
                }
                break;
                case DropState.CannotDrop:
                try
                {
                    StoryMovementTarget sb = _adorner.Resources["cannotDrop"] as StoryMovementTarget;
                    DoubleAnimation aniX = sb.Children[0] as DoubleAnimation;
                    aniX.To = _delta.X;
                    DoubleAnimation aniY = sb.Children[1] as DoubleAnimation;
                    aniY.To = _delta.Y;
                    sb.Completed += (s, args) =>
                    {
                        _adornerLayer.Children.Clear();
                        _adornerLayer.Visibility = Visibility.Collapsed;
                    };
                    sb.Begin(_adorner);
                }
                catch (Exception exc)
                {
                    MessageBox.Show("Exception in DragDropHelper: " + exc.InnerException.ToString());
                }
                break;
            }

            _draggedData = null;
            _adornerLayer.PreviewMouseMove -= new MouseEventHandler(_adorner_MouseMove);
            _adornerLayer.PreviewMouseUp -= new MouseButtonEventHandler(_adorner_MouseUp);

            if (_adorner != null)
            {
                _adorner.ReleaseMouseCapture();
            }
            _adorner = null;
            _mouseCaptured = false;
        }
        

        private void DragSource_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _draggedData = null;
            _mouseCaptured = false;

            if (_adorner != null)
            {
                _adorner.ReleaseMouseCapture();
            }
        }

        #endregion

        #region Events
        public static event EventHandler<DragDropEventArgs> ItemDropped;
        #endregion
    }

    public class DragDropEventArgs : EventArgs
    {
        public object Content;
        public DragDropEventArgs() { }
        public DragDropEventArgs(object content)
        {
            Content = content;
        }
    }
}
