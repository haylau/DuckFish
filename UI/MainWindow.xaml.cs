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
            _chessboard.setBoard(Board.debug);
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
                        break;
                    }
                case Piece.White + Piece.Pawn:
                    {
                        break;
                    }
                case Piece.White + Piece.Knight:
                    {
                        break;
                    }
                case Piece.White + Piece.Bishop:
                    {
                        break;
                    }
                case Piece.White + Piece.Rook:
                    {
                        break;
                    }
                case Piece.White + Piece.Queen:
                    {
                        break;
                    }
                case Piece.White + Piece.King:
                    {   
                        img.Source = new BitmapImage(new Uri(@"pack://application:,,,/Assets/King_White.png", UriKind.Absolute));
                        break;
                    }
                case Piece.Black + Piece.Pawn:
                    {
                        break;
                    }
                case Piece.Black + Piece.Knight:
                    {
                        break;
                    }
                case Piece.Black + Piece.Bishop:
                    {
                        break;
                    }
                case Piece.Black + Piece.Rook:
                    {
                        break;
                    }
                case Piece.Black + Piece.Queen:
                    {
                        break;
                    }
                case Piece.Black + Piece.King:
                    {
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
