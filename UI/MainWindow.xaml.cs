using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
        private int _time = 0;
        // Engine Mode
        bool AIOnly;
        bool PlayerOnly;

        // Depth Testing Variables
        private bool runDepthTest; // if enabled create Debug Folder in source directory
        private bool runDepthTestSetup;
        private int _depthIdx = 1;
        private int _depthMax;
        private List<int> expected = new();
        // End Depth Testing Variables

        public MainWindow()
        {
            // debug flags
            this.AIOnly = false;
            this.PlayerOnly = false;
            this.runDepthTest = false;
            runDepthTestSetup = runDepthTest;

            if (PlayerOnly)
            {
                _chessboard.SetAIMovGen("disable");
            }
            else
            {
                // _chessboard.SetAIMovGen("random");
                _chessboard.SetAIMovGen("engine");
            }

            _chessboard.SelectColor("white"); // locks to white
            // _chessboard.SelectColor("black"); // locks to black
            _chessboard.SetBoard(); // Sets to STARTPOS
            DataContext = this;
            InitializeComponent();
        }
        private void dispatcherTimer_Tick(object? sender, EventArgs e)
        {
            ++_time;

            if (AIOnly || (!PlayerOnly && _chessboard.PlayerTurn != _chessboard.CurrentTurn))
            {
                _chessboard.OpponentMove(5);
                List<int> prevMove = _chessboard.GetPrevMove();
                ReloadBoardPieces();
                UpdateMoveHighlight(prevMove[0], prevMove[1]);
                Thread.Sleep(500);
                if (_chessboard.InCheck)
                {
                    CheckPiece(_chessboard.KingTile); // player in check
                    if (_chessboard.Checkmate) { return; } // #TODO handle player has won  
                }
            }

            if (runDepthTestSetup)
            {
                MoveDepthCounter.Open();
                runDepthTestSetup = false;
                depthlog.Inlines.Add("Running Depth Test...\n");
                _chessboard.SetAIMovGen("disable");
                _chessboard.SelectColor("white");
                _chessboard.SetBoard(); //resets board orientation
                int position = 1;
                _depthMax = 6;
                switch (position)
                {
                    case 1:
                        {
                            // rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1
                            _chessboard.SetBoard(Board.STARTPOS);
                            int[] vals = { 20, 400, 8902, 197281, 4865609, 119060324 };
                            expected.AddRange(vals);
                            break;
                        }
                    case 2:
                        {
                            // r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 
                            _chessboard.SetBoard(Board.DEPTHTEST_2);
                            int[] vals = { 48, 2039, 97862, 4085603, 193690690 }; // pos 2
                            if (_depthMax > 5) _depthMax = 5; // dont run this past 5
                            expected.AddRange(vals);
                            break;
                        }
                    case 3:
                        {
                            // 8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 
                            _chessboard.SetBoard(Board.DEPTHTEST_3);
                            int[] vals = { 14, 191, 2812, 43238, 674624, 11030083 };
                            expected.AddRange(vals);
                            break;
                        }
                    case 4:
                        {
                            // r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1
                            _chessboard.SetBoard(Board.DEPTHTEST_4);
                            int[] vals = { 6, 264, 9467, 422333, 15833292, 706045033 };
                            expected.AddRange(vals);
                            break;
                        }
                    case 5:
                        {
                            // 1k6/1b6/8/8/7R/8/8/4K2R b K - 0 1  
                            _chessboard.SetBoard(Board.DEPTHTEST_5);
                            int[] vals = { 13, 284, 3529, 85765, 1063513, 27535380 };
                            expected.AddRange(vals);
                            break;
                        }
                    default:
                        {
                            _chessboard.SetBoard("r3k2r/p1ppqpb1/bn2pnp1/3PN1B1/1p2P3/2N2Q1p/PPP1BPPP/R3K2R b KQkq - 1 1");
                            _depthMax = 1;
                            int[] vals = { 14, 191, 2812, 43238, 674624, 11030083, 0 };
                            expected.AddRange(vals);
                            break;
                        }
                }
            }
            if (runDepthTest && _depthIdx <= _depthMax && _time > 3)
            {
                MoveDepthCounter.logDepth = _depthMax;
                Run run = new();
                var watch = System.Diagnostics.Stopwatch.StartNew();
                int moveCount = MoveDepthCounter.CountMoves(_depthIdx, _chessboard);
                watch.Stop();
                run.Text += "Found " + moveCount + " moves at depth " + _depthIdx + " after " + watch.ElapsedMilliseconds + "ms\n";
                if (expected[_depthIdx - 1] != 0)
                {
                    run.Text += "Expected " + expected[_depthIdx - 1];
                    if (expected[_depthIdx - 1] == moveCount) run.Text += " ✔\n";
                    else run.Text += " ☒\n";
                }
                depthlog.Inlines.Add(run);
                ++_depthIdx;
                ReloadBoardPieces();
            }
            else if (runDepthTest && _depthIdx > _depthMax)
            {
                runDepthTest = false;
                MoveDepthCounter.Close();
                depthlog.Inlines.Add("Depth Test Complete!\n");
            }
            // Forcing the CommandManager to raise the RequerySuggested event
            CommandManager.InvalidateRequerySuggested();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
            dispatcherTimer.Start();
            ReloadBoardColors();
            ReloadBoardPieces();
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
        private void CheckPiece(int tile)
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
                // make player's move
                _chessboard.PlayerMove(fromTile, toTile, flag);
                ReloadBoardPieces();
                UpdateMoveHighlight(fromTile, toTile);
                // check gamestate 
                if (_chessboard.InCheck)
                {
                    CheckPiece(_chessboard.KingTile); // opponent in check
                    if (_chessboard.Checkmate) { return; } // #TODO handle player has won  
                }
            }
            return;
        }
    }
}
