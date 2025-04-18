using JogoDaVelha.Classes;
using JogoDaVelha.Enums;
using JogoDaVelha.Game;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;

namespace JogoDaVelha
{
    
    public partial class MainWindow : Window
    {
            
           private readonly Dictionary<Player, ImageSource> _images = new()
           {
               { Player.X, new BitmapImage(new Uri("pack://application:,,,/Arquivos/X15.png")) },
               { Player.O, new BitmapImage(new Uri("pack://application:,,,/Arquivos/O15.png")) }
           };

           private readonly Image[,] ControleImagem = new Image[3, 3];
           private readonly GameState _gameState = new();

        private readonly 
   


        public MainWindow()
        {
            InitializeComponent();
            IniciaTabelaJogo();

            _gameState.AçãoJogada += AçãoJogada;
            _gameState.FimJogo += FimJogo;
            _gameState.RecomeçarJogo += RecomeçarJogo;
        }


        private void IniciaTabelaJogo()
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Image controle = new Image();
                    GridJogo.Children.Add(controle);
                    ControleImagem[i, j] = controle;
                }
            }
        }

        private void AçãoJogada(int linha, int coluna)
        {
            Player jogadorAtual = _gameState.TabelaJogo[linha, coluna];
            ControleImagem[linha, coluna].Source = _images[jogadorAtual];
            ImagemTurno.Source = _images[_gameState.JogadorAtual];
        }


        private void  MudarTelaFimDeJogo(string texto, ImageSource imagem)
        {
            Tabuleiro.Visibility = Visibility.Hidden;
            GridJogo.Visibility = Visibility.Hidden;
            Resultado.Text = texto;
            ImagemVencedor.Source = imagem;
            PainelVencedor.Visibility = Visibility.Visible;
        }


        private async void FimJogo(GameResult resultado)
        {
            
            MostrarLinha(resultado.Info);
            await Task.Delay(2000);

            if (resultado.Ganhador == Player.Nenhum)
            {
                MudarTelaFimDeJogo("Empate!", null);

            } else
            {
                MudarTelaFimDeJogo("Vencedor: ", _images[resultado.Ganhador]);
            }
        }


        private void RecomeçarJogo()
        {
            ReiniciarTela();
            ImagemTurno.Source = _images[_gameState.JogadorAtual];
            LimparTabuleiro();
        }

        private (Point, Point) CalcularLinha(WinInfo info)
        {
            double squareSize = GridJogo.Width / 3;
            double margin = squareSize / 2; 

            if(info.TipoVitória == WinType.Linha)
            {
              double y = info.Numero * squareSize + margin;
              return (new Point(0, y), new Point(GridJogo.Width, y));
            }
            else if (info.TipoVitória == WinType.Coluna)
            {
               double x = info.Numero * squareSize + margin;
                return (new Point(x, 0), new Point(x, GridJogo.Height)); 

            } else if(info.TipoVitória == WinType.Diagonal)
            {
                return (new Point(0, 0), new Point(GridJogo.Width, GridJogo.Height));
            } else
            {
                return (new Point(GridJogo.Width, 2), new Point(0, GridJogo.Height));
            }
          

        }


        private async void MostrarLinha(WinInfo info)
        {
            await Task.Delay(1000);
            (Point start, Point end) = CalcularLinha(info);
            Linha.X1 = start.X;
            Linha.Y1 = start.Y;
            
            Linha.X2 = end.X;
            Linha.Y2 = end.Y;

            
            Linha.Visibility = Visibility.Visible;

           
        }


        private void LimparTabuleiro()
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    ControleImagem[i, j].Source = null;
                }
            }
        }

        private void ReiniciarTela()
        {
            Tabuleiro.Visibility = Visibility.Visible;
            GridJogo.Visibility = Visibility.Visible;
            PainelVencedor.Visibility = Visibility.Hidden;
            Linha.Visibility = Visibility.Hidden;
        }

        private void Jogo_MouseDown(object sender, MouseButtonEventArgs e)
        {
            double squareWidth = GridJogo.Width / 3;
            Point posicao = e.GetPosition(GridJogo);

            int linha = (int)(posicao.Y / squareWidth);
            int coluna = (int)(posicao.X / squareWidth);

            _gameState.Jogada(linha, coluna); 
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _gameState.Recomeçar();
        }
    }
}