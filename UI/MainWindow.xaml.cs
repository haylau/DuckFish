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
            _chessboard.SetBoard(); // starts as random color
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
        private void UpdateMove(DragEventArgs from, object to)
        {
            UpdateImage(to);

            if (from.Data.GetData(DataFormats.Serializable) is not Image fromImg) return;
            string? fromTile = fromImg.Tag.ToString();
            if (fromTile == null) return;
            UpdateImage(fromImg);
        }

        private void UpdateImage(object sender)
        {
            if (sender is not Image img) return;
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

        private bool IsMoveable(object sender, MouseEventArgs e)
        {
            if (sender is not Image img) return false;
            string? tile = img.Tag.ToString();
            if (tile == null) return false;
            return e.LeftButton == MouseButtonState.Pressed && _chessboard.IsMoveable(tile);
        }

        private bool IsLegal(DragEventArgs e, string tile)
        {
            if (e.Data.GetData(DataFormats.Serializable) is not Image fromImg) return false;
            string? fromTile = fromImg.Tag.ToString();
            if (fromTile == null) return false;

            if (_chessboard.IsLegal(fromTile, tile))
            {
                _chessboard.Move(fromTile, tile);
                return true;
            }

            return false;
        }

        // Begin drag/drop hell as I couldn't find a better way to do this

        private void PieceMove(object sender, MouseEventArgs e)
        {
            if (sender is not Image img) return;
            if (IsMoveable(sender, e))
            {
                DragDrop.DoDragDrop(img, new DataObject(DataFormats.Serializable, img), DragDropEffects.Move);
            }
        }

        #region DROPEVENT
        // Begin DropEvent DragDrop

        // Row 8
        #region DROPEVENT_8
        private void a8_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "a8"))
            {
                UpdateMove(e, tile_a8);
            }
        }
        private void b8_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "b8"))
            {
                UpdateMove(e, tile_b8);
            }

        }
        private void c8_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "c8"))
            {
                UpdateMove(e, tile_c8);
            }
        }
        private void d8_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "d8"))
            {
                UpdateMove(e, tile_d8);
            }

        }
        private void e8_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "e8"))
            {
                UpdateMove(e, tile_e8);
            }
        }
        private void f8_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "f8"))
            {
                UpdateMove(e, tile_f8);
            }

        }
        private void g8_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "g8"))
            {
                UpdateMove(e, tile_g8);
            }
        }
        private void h8_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "h8"))
            {
                UpdateMove(e, tile_h8);
            }

        }
        #endregion DROPEVENT_8

        // Row 7
        #region DROPEVENT_7
        private void a7_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "a7"))
            {
                UpdateMove(e, tile_a7);
            }
        }

        private void b7_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "b7"))
            {
                UpdateMove(e, tile_b7);
            }

        }
        private void c7_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "c7"))
            {
                UpdateMove(e, tile_c7);
            }
        }

        private void d7_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "d7"))
            {
                UpdateMove(e, tile_d7);
            }

        }
        private void e7_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "e7"))
            {
                UpdateMove(e, tile_e7);
            }
        }

        private void f7_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "f7"))
            {
                UpdateMove(e, tile_f7);
            }

        }
        private void g7_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "g7"))
            {
                UpdateMove(e, tile_g7);
            }
        }

        private void h7_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "h7"))
            {
                UpdateMove(e, tile_h7);
            }
        }
        #endregion DROPEVENT_7

        // Row 6
        #region DROPEVENT_6
        private void a6_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "a6"))
            {
                UpdateMove(e, tile_a6);
            }
        }

        private void b6_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "b6"))
            {
                UpdateMove(e, tile_b6);
            }

        }
        private void c6_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "c6"))
            {
                UpdateMove(e, tile_c6);
            }
        }

        private void d6_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "d6"))
            {
                UpdateMove(e, tile_d6);
            }

        }
        private void e6_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "e6"))
            {
                UpdateMove(e, tile_e6);
            }
        }

        private void f6_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "f6"))
            {
                UpdateMove(e, tile_f6);
            }

        }
        private void g6_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "g6"))
            {
                UpdateMove(e, tile_g6);
            }
        }

        private void h6_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "h6"))
            {
                UpdateMove(e, tile_h6);
            }

        }
        #endregion DROPEVENT_6

        // Row 5
        #region DROPEVENT_5

        private void a5_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "a5"))
            {
                UpdateMove(e, tile_a5);
            }
        }

        private void b5_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "b5"))
            {
                UpdateMove(e, tile_b5);
            }

        }
        private void c5_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "c5"))
            {
                UpdateMove(e, tile_c5);
            }
        }

        private void d5_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "d5"))
            {
                UpdateMove(e, tile_d5);
            }

        }
        private void e5_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "e5"))
            {
                UpdateMove(e, tile_e5);
            }
        }

        private void f5_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "f5"))
            {
                UpdateMove(e, tile_f5);
            }

        }
        private void g5_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "g5"))
            {
                UpdateMove(e, tile_g5);
            }
        }

        private void h5_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "h5"))
            {
                UpdateMove(e, tile_h5);
            }

        }
        #endregion DROPEVENT_5

        // Row 4
        #region DROPEVENT_4

        private void a4_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "a4"))
            {
                UpdateMove(e, tile_a4);
            }
        }

        private void b4_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "b4"))
            {
                UpdateMove(e, tile_b4);
            }

        }
        private void c4_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "c4"))
            {
                UpdateMove(e, tile_c4);
            }
        }

        private void d4_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "d4"))
            {
                UpdateMove(e, tile_d4);
            }

        }
        private void e4_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "e4"))
            {
                UpdateMove(e, tile_e4);
            }
        }

        private void f4_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "f4"))
            {
                UpdateMove(e, tile_f4);
            }

        }
        private void g4_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "g4"))
            {
                UpdateMove(e, tile_g4);
            }
        }

        private void h4_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "h4"))
            {
                UpdateMove(e, tile_h4);
            }

        }
        #endregion DROPEVENT_4

        // Row 3
        #region DROPEVENT_3

        private void a3_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "a3"))
            {
                UpdateMove(e, tile_a3);
            }
        }

        private void b3_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "b3"))
            {
                UpdateMove(e, tile_b3);
            }

        }
        private void c3_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "c3"))
            {
                UpdateMove(e, tile_c3);
            }
        }

        private void d3_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "d3"))
            {
                UpdateMove(e, tile_d3);
            }

        }
        private void e3_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "e3"))
            {
                UpdateMove(e, tile_e3);
            }
        }

        private void f3_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "f3"))
            {
                UpdateMove(e, tile_f3);
            }

        }
        private void g3_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "g3"))
            {
                UpdateMove(e, tile_g3);
            }
        }

        private void h3_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "h3"))
            {
                UpdateMove(e, tile_h3);
            }

        }
        #endregion DROPEVENT_3

        // Row 2
        #region DROPEVENT_2

        private void a2_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "a2"))
            {
                UpdateMove(e, tile_a2);
            }
        }

        private void b2_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "b2"))
            {
                UpdateMove(e, tile_b2);
            }

        }
        private void c2_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "c2"))
            {
                UpdateMove(e, tile_c2);
            }
        }

        private void d2_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "d2"))
            {
                UpdateMove(e, tile_d2);
            }

        }
        private void e2_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "e2"))
            {
                UpdateMove(e, tile_e2);
            }
        }

        private void f2_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "f2"))
            {
                UpdateMove(e, tile_f2);
            }

        }
        private void g2_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "g2"))
            {
                UpdateMove(e, tile_g2);
            }
        }

        private void h2_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "h2"))
            {
                UpdateMove(e, tile_h2);
            }

        }

        #endregion DROPEVENT_2

        // Row 1
        #region DROPEVENT_1

        private void a1_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "a1"))
            {
                UpdateMove(e, tile_a1);
            }
        }

        private void b1_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "b1"))
            {
                UpdateMove(e, tile_b1);
            }

        }
        private void c1_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "c1"))
            {
                UpdateMove(e, tile_c1);
            }
        }

        private void d1_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "d1"))
            {
                UpdateMove(e, tile_d1);
            }

        }
        private void e1_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "e1"))
            {
                UpdateMove(e, tile_e1);
            }
        }

        private void f1_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "f1"))
            {
                UpdateMove(e, tile_f1);
            }

        }
        private void g1_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "g1"))
            {
                UpdateMove(e, tile_g1);
            }
        }

        private void h1_Drop(object sender, DragEventArgs e)
        {
            if (IsLegal(e, "h1"))
            {
                UpdateMove(e, tile_h1);
            }

        }

        #endregion DROPEVENT_1

        #endregion DROPEVENT

    }
}
