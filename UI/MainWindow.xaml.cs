using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;
using Engine;

namespace UI
{
    public partial class MainWindow : INotifyPropertyChanged
    {
        private readonly Board _chessboard = new();

        public MainWindow()
        {
            _chessboard.SetAIMovGen("random");
            _chessboard.SelectColor(Piece.White); // locks to white
            _chessboard.SetBoard(); // normally starts as random color
            DataContext = this;
            InitializeComponent();
        }

        private void dispatcherTimer_Tick(object? sender, EventArgs e)
        {
            // let the ai play itself :)
            bool AIOnly = false;
            if (AIOnly)
            {
                if (!_chessboard.Checkmate)
                {
                    _chessboard.OpponentMove();
                    ReloadBoardColors();
                    ReloadBoardPieces();
                }
            }
            // Forcing the CommandManager to raise the RequerySuggested event
            CommandManager.InvalidateRequerySuggested();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = TimeSpan.FromTicks(1000);
            dispatcherTimer.Start();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void ReloadMoveIndicators()
        {
            foreach (UniformGrid uniformGrid in board.Children)
            {
                foreach (Grid grid in uniformGrid.Children)
                {
                    if (grid.Children[1] is not Ellipse indicator) return;
                    indicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Transparent"));
                }
            }
        }
        private void ReloadBoardColors()
        {
            int rows = board.Rows;
            int columns = board.Columns;

            foreach (UniformGrid uniformGrid in board.Children)
            {
                {
                    int index = board.Children.IndexOf(uniformGrid);

                    int row = index / columns;
                    int column = index % columns;

                    var lightTile = App.Current.Resources["LightTile"].ToString();
                    var darkTile = App.Current.Resources["DarkTile"].ToString();

                    if (row % 2 == 0)
                    {
                        if (column % 2 == 0)
                        {
                            uniformGrid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(lightTile));
                        }
                        else
                        {
                            uniformGrid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(darkTile));
                        }
                    }
                    else
                    {
                        if (column % 2 != 0)
                        {
                            uniformGrid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(lightTile));
                        }
                        else
                        {
                            uniformGrid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(darkTile));
                        }
                    }
                }
            }
        }
        private void ReloadBoardPieces()
        {
            foreach (UniformGrid uniformGrid in board.Children)
            {
                foreach (Grid grid in uniformGrid.Children)
                {
                    foreach (var child in grid.Children)
                    {
                        if (child is not Image img) continue;
                        UpdateImage(img);
                    }

                }
            }
        }
        private void UpdateMoveIndicators(Grid fromGrid)
        {
            if (fromGrid.Children[0] is not Image img) return;
            if (img.Tag.ToString() is not string tile) return;

            foreach (int target in _chessboard.GetLegalTargets(tile))
            {
                if (board.Children[target] is not UniformGrid uniformGrid) return;
                if (uniformGrid.Children[0] is not Grid grid) return;
                if (grid.Children[1] is not Ellipse indicator) return;
                var fillColor = App.Current.Resources["MovePreview"].ToString();
                indicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(fillColor));
            }
        }
        private void UpdateImage(object sender)
        {
            if (sender is Image img)
            {
                if (img.Tag.ToString() is not string tile) return;
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
        private void UpdateMoveHighlight(string fromTile, string toTile)
        {
            UpdateMoveHighlight(_chessboard.TileToIndex(fromTile), _chessboard.TileToIndex(toTile));
        }
        private void UpdateMoveHighlight(int fromIdx, int toIdx)
        {

            ReloadBoardColors();

            var lightTile = App.Current.Resources["PrevMoveLight"].ToString();
            var darkTile = App.Current.Resources["PrevMoveDark"].ToString();
            int fromRow = fromIdx / 8;
            int fromCol = fromIdx % 8;
            int toRow = toIdx / 8;
            int toCol = toIdx % 8;

            if (board.Children[fromIdx] is not UniformGrid fromUniformGrid) return;
            if (board.Children[toIdx] is not UniformGrid toUniformGrid) return;

            // set From
            if (fromRow % 2 == 0)
            {
                if (fromCol % 2 == 0)
                {
                    fromUniformGrid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(lightTile));
                }
                else
                {
                    fromUniformGrid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(darkTile));
                }
            }
            else
            {
                if (fromCol % 2 != 0)
                {
                    fromUniformGrid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(lightTile));
                }
                else
                {
                    fromUniformGrid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(darkTile));
                }
            }
            // set To
            if (toRow % 2 == 0)
            {
                if (toCol % 2 == 0)
                {
                    toUniformGrid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(lightTile));
                }
                else
                {
                    toUniformGrid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(darkTile));
                }
            }
            else
            {
                if (toCol % 2 != 0)
                {
                    toUniformGrid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(lightTile));
                }
                else
                {
                    toUniformGrid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(darkTile));
                }
            }
        }

        private void HighlightPiece(int tile)
        {
            if (board.Children[tile] is not UniformGrid uniformGrid) return;
            uniformGrid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(App.Current.Resources["CheckHighlight"].ToString()));
        }
        private void PieceMove(object sender, MouseEventArgs e)
        {
            if (sender is not Image img) return;
            if (img.Parent is not Grid grid) return;
            if (img.Tag.ToString() is not string tile) return;
            if (e.LeftButton == MouseButtonState.Pressed && _chessboard.IsMoveable(tile))
            {
                UpdateMoveIndicators(grid);
                DragDrop.DoDragDrop(img, new DataObject(DataFormats.Serializable, img), DragDropEffects.Move);
            }
            ReloadMoveIndicators();
        }

        private void PieceDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.Serializable) is not Image fromImg) return;

            if (sender is not UniformGrid uniformGrid) return;
            if (uniformGrid.Children[0] is not Grid grid) return;
            if (grid.Children[0] is not Image toImg) return;
            if (fromImg.Tag.ToString() is not string fromTile) return;
            if (toImg.Tag.ToString() is not string toTile) return;
            int flag;
            if (_chessboard.IsPromotion(fromTile, toTile))
            {
                // #TODO prompt promotion type
                flag = _chessboard.MoveFlag(fromTile, toTile, Engine.Piece.Queen); // defaults queen until promotion prompt 
            }
            else
            {
                flag = _chessboard.MoveFlag(fromTile, toTile);
            }
            if (_chessboard.IsLegal(fromTile, toTile, flag))
            {
                _chessboard.PlayerMove(fromTile, toTile, flag);
                ReloadBoardPieces();
                UpdateMoveHighlight(fromTile, toTile);
                if (_chessboard.InCheck)
                {
                    HighlightPiece(_chessboard.KingTile); // opponent in check
                    if (_chessboard.Checkmate) { return; } // #TODO handle player has won  
                    _chessboard.OpponentMove();
                    ReloadBoardColors();
                }
                else
                {
                    _chessboard.OpponentMove();
                }
                List<int> prevMove = _chessboard.GetPrevMove();
                ReloadBoardPieces();
                UpdateMoveHighlight(prevMove[0], prevMove[1]);
                if (_chessboard.InCheck)
                {
                    HighlightPiece(_chessboard.KingTile); // player in check
                    if (_chessboard.Checkmate) { return; } // #TODO handle player has won  
                }
            }
            return;
        }
    }
}
