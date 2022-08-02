using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Engine;

namespace UI
{

    public partial class MainWindow : INotifyPropertyChanged
    {

        private readonly Board _chessboard = new();

        public MainWindow()
        {
            _chessboard.EnableDebug(); // Testing
            _chessboard.SelectColor(Piece.White); // locks to white
            _chessboard.SetBoard(); // normally starts as random color
            DataContext = this;
            InitializeComponent();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void Image_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateImage(sender);
        }

        private void ClearBoardColors()
        {
            int rows = board.Rows;
            int columns = board.Columns;

            foreach (var children in board.Children)
            {
                if (children is UniformGrid grid)
                {
                    int index = board.Children.IndexOf(grid);

                    int row = index / columns;  // divide
                    int column = index % columns;  // modulus

                    var lightTile = App.Current.Resources["LightTile"].ToString();
                    var darkTile = App.Current.Resources["DarkTile"].ToString();

                    if (row % 2 == 0)
                    {
                        if (column % 2 == 0)
                        {
                            grid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(lightTile));
                        }
                        else
                        {
                            grid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(darkTile));
                        }
                    }
                    else
                    {
                        if (column % 2 != 0)
                        {
                            grid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(lightTile));
                        }
                        else
                        {
                            grid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(darkTile));
                        }
                    }
                }
            }
        }
        private void ReloadBoard()
        {
            foreach (UniformGrid grid in board.Children)
            {
                foreach (Image img in grid.Children)
                {
                    UpdateImage(img);
                }
            }
        }
        private void UpdateImage(object sender)
        {
            if (sender is Image img)
            {
                string? tile = img.Tag.ToString();
                if (tile == null) return;
                int piece = _chessboard.GetTile(tile);
                switch (piece)
                {
                    case Piece.Empty:
                        {
                            img.Source = new BitmapImage(new Uri(@"pack://application:,,,/Assets/Empty.png", UriKind.Absolute));
                            break;
                        }
                    case Piece.White + Piece.Pawn:
                        {
                            img.Source = new BitmapImage(new Uri(@"pack://application:,,,/Assets/Pawn_White.png", UriKind.Absolute));
                            break;
                        }
                    case Piece.White + Piece.Knight:
                        {
                            img.Source = new BitmapImage(new Uri(@"pack://application:,,,/Assets/Knight_White.png", UriKind.Absolute));
                            break;
                        }
                    case Piece.White + Piece.Bishop:
                        {
                            img.Source = new BitmapImage(new Uri(@"pack://application:,,,/Assets/Bishop_White.png", UriKind.Absolute));
                            break;
                        }
                    case Piece.White + Piece.Rook:
                        {
                            img.Source = new BitmapImage(new Uri(@"pack://application:,,,/Assets/Rook_White.png", UriKind.Absolute));
                            break;
                        }
                    case Piece.White + Piece.Queen:
                        {
                            img.Source = new BitmapImage(new Uri(@"pack://application:,,,/Assets/Queen_White.png", UriKind.Absolute));
                            break;
                        }
                    case Piece.White + Piece.King:
                        {
                            img.Source = new BitmapImage(new Uri(@"pack://application:,,,/Assets/King_White.png", UriKind.Absolute));
                            break;
                        }
                    case Piece.Black + Piece.Pawn:
                        {
                            img.Source = new BitmapImage(new Uri(@"pack://application:,,,/Assets/Pawn_Black.png", UriKind.Absolute));
                            break;
                        }
                    case Piece.Black + Piece.Knight:
                        {
                            img.Source = new BitmapImage(new Uri(@"pack://application:,,,/Assets/Knight_Black.png", UriKind.Absolute));
                            break;
                        }
                    case Piece.Black + Piece.Bishop:
                        {
                            img.Source = new BitmapImage(new Uri(@"pack://application:,,,/Assets/Bishop_Black.png", UriKind.Absolute));
                            break;
                        }
                    case Piece.Black + Piece.Rook:
                        {
                            img.Source = new BitmapImage(new Uri(@"pack://application:,,,/Assets/Rook_Black.png", UriKind.Absolute));
                            break;
                        }
                    case Piece.Black + Piece.Queen:
                        {
                            img.Source = new BitmapImage(new Uri(@"pack://application:,,,/Assets/Queen_Black.png", UriKind.Absolute));
                            break;
                        }
                    case Piece.Black + Piece.King:
                        {
                            img.Source = new BitmapImage(new Uri(@"pack://application:,,,/Assets/King_Black.png", UriKind.Absolute));
                            break;
                        }
                }
            }
        }
        private void UpdateMove(object sender, DragEventArgs e, int flag)
        {
            if (sender is not Image toImg) return;
            if (e.Data.GetData(DataFormats.Serializable) is not Image fromImg) return;
            // #TODO handle capture rendering
            if (flag != Move.Flag.None)
            {
                ReloadBoard();
            }
            else
            {
                UpdateImage(toImg);
                UpdateImage(fromImg);
            }
            ClearBoardColors();

            if (fromImg.Tag.ToString() is not string fromTile) return;
            if (toImg.Tag.ToString() is not string toTile) return;
            int fromIdx = _chessboard.TileToIndex(fromTile);
            int toIdx = _chessboard.TileToIndex(fromTile);

            if (toImg.Parent is not UniformGrid toGrid) return;
            if (fromImg.Parent is not UniformGrid fromGrid) return;

            var lightTile = App.Current.Resources["PrevMoveLight"].ToString();
            var darkTile = App.Current.Resources["PrevMoveDark"].ToString();
            int fromRow = fromIdx / 8;
            int fromCol = fromIdx % 8;
            int toRow = toIdx / 8;
            int toCol = toIdx % 8;

            // set From
            if (fromRow % 2 == 0)
            {
                if (fromCol % 2 == 0)
                {
                    fromGrid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(lightTile));
                }
                else
                {
                    fromGrid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(darkTile));
                }
            }
            else
            {
                if (fromCol % 2 != 0)
                {
                    fromGrid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(lightTile));
                }
                else
                {
                    fromGrid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(darkTile));
                }
            }
            // set To
            if (toRow % 2 == 0)
            {
                if (toCol % 2 == 0)
                {
                    toGrid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(lightTile));
                }
                else
                {
                    toGrid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(darkTile));
                }
            }
            else
            {
                if (toCol % 2 != 0)
                {
                    toGrid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(lightTile));
                }
                else
                {
                    toGrid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(darkTile));
                }
            }
        }
        private void PieceMove(object sender, MouseEventArgs e)
        {
            if (sender is not Image img) return;
            if (img.Tag.ToString() is not string tile) return;
            if (e.LeftButton == MouseButtonState.Pressed && _chessboard.IsMoveable(tile))
            {
                DragDrop.DoDragDrop(img, new DataObject(DataFormats.Serializable, img), DragDropEffects.Move);
            }
        }

        private void PieceDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.Serializable) is not Image fromImg) return;
            if (sender is not UniformGrid grid) return;
            if (grid.Children[0] is not Image toImg) return;
            if (fromImg.Tag.ToString() is not string fromTile) return;
            if (toImg.Tag.ToString() is not string toTile) return;
            int flag = _chessboard.MoveFlag(fromTile, toTile);
            // #TODO ask for promotion type if promotion flag
            if (_chessboard.IsLegal(fromTile, toTile, flag))
            {
                _chessboard.PlayerMove(fromTile, toTile, flag);
                UpdateMove(toImg, e, flag);

            }
            return;
        }
    }
}
