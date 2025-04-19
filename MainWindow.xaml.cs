using JogoDaVelha.Classes;
using JogoDaVelha.Enums;
using JogoDaVelha.Game;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace JogoDaVelha
{





    public partial class MainWindow : Window
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private Thread thread1;
        private Thread thread2;
        private Thread thread3;
        private Thread thread4;
        private Thread thread5;
        public MainWindow()
        {
            InitializeComponent();
            IniciaTabelaJogo();
            CriarAnimação();
            _gameState.AçãoJogada += AçãoJogada;
            _gameState.FimJogo += FimJogo;
            _gameState.RecomeçarJogo += RecomeçarJogo;

        }



        private readonly Dictionary<Player, ImageSource> _images = new()
           {
               { Player.X, new BitmapImage(new Uri("pack://application:,,,/Arquivos/X15.png")) },
               { Player.O, new BitmapImage(new Uri("pack://application:,,,/Arquivos/O15.png")) }
           };

        private readonly Image[,] ControleImagem = new Image[3, 3];
        private readonly GameState _gameState = new();

        private readonly Dictionary<Player, ObjectAnimationUsingKeyFrames> animação = new()
        {
            {Player.X, new ObjectAnimationUsingKeyFrames()},
            {Player.O, new ObjectAnimationUsingKeyFrames()}
        };


        private readonly DoubleAnimation FadeOut = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = new Duration(TimeSpan.FromSeconds(0.5)),

        };


        private readonly DoubleAnimation FadeIn = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = new Duration(TimeSpan.FromSeconds(0.5)),

        };


        private async Task HandleFadeOut(UIElement element)
        {
            element.BeginAnimation(UIElement.OpacityProperty, FadeOut);
            await Task.Delay(500);
            element.Visibility = Visibility.Hidden;
        }

        private async Task HandleFadeIn(UIElement element)
        {
            element.Visibility = Visibility.Visible;
            element.BeginAnimation(UIElement.OpacityProperty, FadeIn);
            await Task.Delay(500);

        }


        private void CriarAnimação()
        {
            try
            {
                _semaphore.Wait();

                thread1 = new Thread(CriarQuadrosAnimacao);
                thread1.Name = "ThreadCriarQuadrosAnimacao";
                thread1.Start();
                Thread.Yield();



            }
            finally
            {
                _semaphore.Release();
            }


        }

        private void CriarQuadrosAnimacao()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                animação[Player.X].Duration = new Duration(TimeSpan.FromSeconds(0.25));
                animação[Player.O].Duration = new Duration(TimeSpan.FromSeconds(0.25));

                for (int i = 0; i < 16; i++)
                {
                    Uri Xuri = new Uri($"pack://application:,,,/Arquivos/X{i}.png");
                    BitmapImage imagemX = new BitmapImage(Xuri);
                    DiscreteObjectKeyFrame keyframeX = new DiscreteObjectKeyFrame(imagemX);
                    animação[Player.X].KeyFrames.Add(keyframeX);


                    Uri Ouri = new Uri($"pack://application:,,,/Arquivos/O{i}.png");
                    BitmapImage imagemO = new BitmapImage(Ouri);
                    DiscreteObjectKeyFrame keyframeO = new DiscreteObjectKeyFrame(imagemO);
                    animação[Player.O].KeyFrames.Add(keyframeO);
                }
            });
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
            ControleImagem[linha, coluna].BeginAnimation(Image.SourceProperty, animação[jogadorAtual]);
            ImagemTurno.Source = _images[_gameState.JogadorAtual];


        }


        private async Task MudarTelaFimDeJogo(string texto, ImageSource imagem)
        {

            Resultado.Text = texto;
            ImagemVencedor.Source = imagem;
            await Task.Delay(1000);
            await Task.WhenAll(
             HandleFadeOut(Tabuleiro),
             HandleFadeOut(GridJogo),
             HandleFadeOut(PainelTurno)
         );

            await HandleFadeIn(PainelVencedor);



        }


        private async void FimJogo(GameResult resultado)
        {

            await MostrarLinha(resultado.Info);
            await Task.Delay(1000);


            if (resultado.Ganhador == Player.Nenhum)
            {
                await MudarTelaFimDeJogo("Empate!", null);

            }
            else
            {

                await MudarTelaFimDeJogo("Vencedor: ", _images[resultado.Ganhador]);

                switch (resultado.Ganhador)
                {
                    case Player.X:
                        _gameState.ScoreX++;
                        break;
                    case Player.O:
                        _gameState.ScoreO++;
                        break;
                }

                ScoreX.Text = _gameState.ScoreX.ToString();
                ScoreO.Text = _gameState.ScoreO.ToString();
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

            if (info is null) return (new Point(0, 0), new Point(0, 0));



            if (info?.TipoVitória == WinType.Linha)
            {
                double y = info.Numero * squareSize + margin;
                return (new Point(0, y), new Point(GridJogo.Width, y));
            }
            else if (info.TipoVitória == WinType.Coluna)
            {
                double x = info.Numero * squareSize + margin;
                return (new Point(x, 0), new Point(x, GridJogo.Height));

            }
            else if (info.TipoVitória == WinType.Diagonal)
            {
                return (new Point(0, 0), new Point(GridJogo.Width, GridJogo.Height));
            }
            else
            {
                return (new Point(GridJogo.Width, 2), new Point(0, GridJogo.Height));
            }


        }


        private async Task MostrarLinha(WinInfo info)
        {
            await Task.Delay(1000);
            (Point start, Point end) = CalcularLinha(info);
            Linha.X1 = start.X;
            Linha.Y1 = start.Y;

            Linha.X2 = end.X;
            Linha.Y2 = end.Y;


            DoubleAnimation AnimaçãoLinhaX = new DoubleAnimation
            {
                From = end.X,
                To = start.X,
                Duration = new Duration(TimeSpan.FromSeconds(0.5)),

            };

            DoubleAnimation AnimaçãoLinhaY = new DoubleAnimation
            {
                From = end.Y,
                To = start.Y,
                Duration = new Duration(TimeSpan.FromSeconds(0.5)),

            };





            Linha.Visibility = Visibility.Visible;

            Linha.BeginAnimation(Line.X1Property, AnimaçãoLinhaX);
            Linha.BeginAnimation(Line.Y1Property, AnimaçãoLinhaY);

            await Task.Delay(AnimaçãoLinhaX.Duration.TimeSpan);


        }


        private void LimparTabuleiro()
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    ControleImagem[i, j].BeginAnimation(Image.SourceProperty, null);
                    ControleImagem[i, j].Source = null;
                }
            }
        }

        private async void ReiniciarTela()
        {
            Linha.Visibility = Visibility.Hidden;
            await HandleFadeOut(PainelVencedor);

            await Task.WhenAll(HandleFadeIn(GridJogo),
            HandleFadeIn(Tabuleiro),
            HandleFadeIn(PainelTurno));



        }

        private void Jogo_MouseDown(object sender, MouseButtonEventArgs e)
        {

            try
            {
                _semaphore.Wait();

                thread2 = new Thread(ProcessarJogadaAutomatica);



                double squareWidth = GridJogo.Width / 3;
                Point posicao = e.GetPosition(GridJogo);

                int linha = (int)(posicao.Y / squareWidth);
                int coluna = (int)(posicao.X / squareWidth);

                if (_gameState.JogadorAtual == Player.X)
                {
                    _gameState.Jogada(linha, coluna);
                }
                else
                {
                    return;
                }

                thread2.Name = "ThreadProcessarJogadaAutomatica";

                thread2.Start();
            }
            finally
            {
                _semaphore.Release();
            }


        }

        private async void ProcessarJogadaAutomatica()
        {
            await _semaphore.WaitAsync();
            Thread.Sleep(1000);

            List<Tuple<int, int>> jogadasPossiveis = new List<Tuple<int, int>>();


            try
            {
                Tuple<int, int>? JogadaCritica = null;

                thread3 = new Thread(() =>
                {
                    JogadaCritica = EncontrarJogadaCritica(_gameState.TabelaJogo, Player.X);

                    if (JogadaCritica is not null)
                    {
                        (int linhas, int colunas) = JogadaCritica;

                        jogadasPossiveis.Add(new Tuple<int, int>(linhas, colunas));


                    }
                });

                thread3.Name = "ThreadEncontrarJogadaCritica";

                thread3.Start();
                thread3.Join();


                Tuple<int, int>? melhorJogada = null;

                thread4 = new Thread(() =>
                {

                    int melhorPontuacao = int.MinValue;

                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            if (_gameState.TabelaJogo[i, j] == Player.Nenhum)
                            {

                                _gameState.TabelaJogo[i, j] = Player.O;

                                int pontuacao = 0;


                                pontuacao = Minimax(_gameState.TabelaJogo, false);


                                _gameState.TabelaJogo[i, j] = Player.Nenhum;



                                if (pontuacao > melhorPontuacao)
                                {
                                    melhorPontuacao = pontuacao;
                                    melhorJogada = new Tuple<int, int>(i, j);
                                }
                            }
                        }
                    }

                    



                    if (melhorJogada != null)
                    {
                        jogadasPossiveis.Add(melhorJogada);


                    }



                });


                thread5 = new Thread(() =>
                {

                    if (jogadasPossiveis.Count == 0)
                    {
                        return;
                    }
                    else
                    {
                        Random random = new Random();
                        int index = random.Next(jogadasPossiveis.Count);
                        melhorJogada = jogadasPossiveis[index];
                    }

                    (int l, int c) = melhorJogada;
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _gameState.Jogada(l, c);
                    });
                });

                thread4.Name = "ThreadAlgoritmoEscolhaJogada";
                thread5.Name = "ThreadEscolherJogada";

                thread4.Start();

                thread4.Join();
                thread5.Start();
                thread5.Join();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private int Minimax(Player[,] tabela, bool isMaximizing)
        {

            if (VerificarVitoria(tabela, Player.O)) return 15;
            if (VerificarVitoria(tabela, Player.X)) return 10;
            if (_gameState.TabelaCheia()) return 0;

            if (isMaximizing)
            {
                int melhorPontuacao = int.MinValue;

                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (tabela[i, j] == Player.Nenhum)
                        {

                            tabela[i, j] = Player.O;


                            int pontuacao = Minimax(tabela, false);


                            tabela[i, j] = Player.Nenhum;


                            melhorPontuacao = Math.Max(melhorPontuacao, pontuacao);
                        }
                    }
                }

                return melhorPontuacao;
            }
            else
            {
                int melhorPontuacao = int.MaxValue;

                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (tabela[i, j] == Player.Nenhum)
                        {

                            tabela[i, j] = Player.X;


                            int pontuacao = Minimax(tabela, true);


                            tabela[i, j] = Player.Nenhum;


                            melhorPontuacao = Math.Min(melhorPontuacao, pontuacao);
                        }
                    }
                }

                return melhorPontuacao;
            }
        }

        private bool VerificarVitoria(Player[,] tabela, Player jogador)
        {

            for (int i = 0; i < 3; i++)
            {
                if (tabela[i, 0] == jogador && tabela[i, 1] == jogador && tabela[i, 2] == jogador) return true;
                if (tabela[0, i] == jogador && tabela[1, i] == jogador && tabela[2, i] == jogador) return true;
            }

            if (tabela[0, 0] == jogador && tabela[1, 1] == jogador && tabela[2, 2] == jogador) return true;
            if (tabela[0, 2] == jogador && tabela[1, 1] == jogador && tabela[2, 0] == jogador) return true;

            return false;
        }

        private Tuple<int, int>? EncontrarJogadaCritica(Player[,] Tabela, Player jogador)
        {
            for (int i = 0; i < 3; i++)
            {
                // Verifica linhas
                if (Tabela[i, 0] == jogador && Tabela[i, 1] == jogador && Tabela[i, 2] == Player.Nenhum)
                    return new Tuple<int, int>(i, 2);
                if (Tabela[i, 0] == jogador && Tabela[i, 2] == jogador && Tabela[i, 1] == Player.Nenhum)
                    return new Tuple<int, int>(i, 1);
                if (Tabela[i, 1] == jogador && Tabela[i, 2] == jogador && Tabela[i, 0] == Player.Nenhum)
                    return new Tuple<int, int>(i, 0);

                // Verifica colunas
                if (Tabela[0, i] == jogador && Tabela[1, i] == jogador && Tabela[2, i] == Player.Nenhum)
                    return new Tuple<int, int>(2, i);
                if (Tabela[0, i] == jogador && Tabela[2, i] == jogador && Tabela[1, i] == Player.Nenhum)
                    return new Tuple<int, int>(1, i);
                if (Tabela[1, i] == jogador && Tabela[2, i] == jogador && Tabela[0, i] == Player.Nenhum)
                    return new Tuple<int, int>(0, i);
            }

            // Verifica diagonais
            if (Tabela[0, 0] == jogador && Tabela[1, 1] == jogador && Tabela[2, 2] == Player.Nenhum)
                return new Tuple<int, int>(2, 2);
            if (Tabela[0, 0] == jogador && Tabela[2, 2] == jogador && Tabela[1, 1] == Player.Nenhum)
                return new Tuple<int, int>(1, 1);
            if (Tabela[1, 1] == jogador && Tabela[2, 2] == jogador && Tabela[0, 0] == Player.Nenhum)
                return new Tuple<int, int>(0, 0);

            if (Tabela[0, 2] == jogador && Tabela[1, 1] == jogador && Tabela[2, 0] == Player.Nenhum)
                return new Tuple<int, int>(2, 0);
            if (Tabela[0, 2] == jogador && Tabela[2, 0] == jogador && Tabela[1, 1] == Player.Nenhum)
                return new Tuple<int, int>(1, 1);
            if (Tabela[1, 1] == jogador && Tabela[2, 0] == jogador && Tabela[0, 2] == Player.Nenhum)
                return new Tuple<int, int>(0, 2);

            return null;
        }



        private void Button_Click(object sender, RoutedEventArgs e)
        {

            if (_gameState.JogoEmAndamento)
            {
                return;
            }
            _gameState.Recomeçar();
        }


    }
}