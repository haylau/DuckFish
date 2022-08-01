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
            if (_chessboard.IsLegal(fromTile, toTile))
            {
                _chessboard.Move(fromTile, toTile);
                UpdateMove(e, toImg);
            }
            return;
        }
    }
}
