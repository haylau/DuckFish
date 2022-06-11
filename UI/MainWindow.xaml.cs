using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
using Engine;

namespace UI
{

    public partial class MainWindow : INotifyPropertyChanged
    {

        private Board _chessboard = new Board();
        public Board ChessBoard
        {
            get { return _chessboard; }
            set
            {
                _chessboard = value;
                // RaisePropetyChanged("M");
            }
        }

        public MainWindow()
        {
            _chessboard.setBoard(Board.start);
            DataContext = this;
            InitializeComponent();
        }

        private void Image_Loaded(object sender, RoutedEventArgs e)
        {
            Image? img = sender as Image;
            if (img == null) return;
            string? tile = img.Tag.ToString();
            if (tile == null) return;
            int piece = _chessboard.getTile(tile);
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

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
