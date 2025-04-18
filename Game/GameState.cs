using JogoDaVelha.Classes;
using JogoDaVelha.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JogoDaVelha.Game
{
    public class GameState
    {
        public Player[,] TabelaJogo { get; private set; }

        public Player JogadorAtual { get; set; } 

        public int Turnos { get; set; }

        public bool JogoEmAndamento { get; set; }


        public event Action<int, int> AçãoJogada;
        public event Action<GameResult> FimJogo;
        public event Action RecomeçarJogo;

        public GameState()
        {
            TabelaJogo = new Player[3, 3];
            JogadorAtual = Player.X;
            Turnos = 0;
            JogoEmAndamento = true;
        }

        private bool JogadaValida(int linha, int coluna)
        {
            if (!JogoEmAndamento) return false; 

            if(TabelaJogo[linha, coluna] != Player.Nenhum) return false;
            

            return true;
        }

        private bool TabelaCheia()
        {
            if (Turnos == 9) return true;
            return false;
        }

        private void TrocaJogador()
        {
            JogadorAtual = JogadorAtual == Player.X ? Player.O : Player.X;
        }


        private bool VerificaGanhador((int, int)[] squares, Player player)
        {
            foreach((int linha, int coluna) in squares)
            {
                if (TabelaJogo[linha, coluna] != player)
                {
                    return false;
                }
            }

            return true;
        }

        private bool JogadaVencedora(int linha, int coluna, out WinInfo info)
        {
            (int, int)[] l = new[] { (linha, 0), (linha, 1), (linha, 2) };
            (int, int)[] c = new[] { (0, coluna), (1, coluna), (2, coluna) };
            (int, int)[] diag = new[] { (0, 0), (1, 1), (2, 2) };
            (int, int)[] diag2 = new[] { (0, 2), (1, 1), (2, 0) };

            if(VerificaGanhador(l, JogadorAtual)) {
                info = new WinInfo()
                {
                    TipoVitória = WinType.Linha,
                    Numero = linha
                };
                return true;
            }

            if(VerificaGanhador(c, JogadorAtual)) {
                info = new WinInfo()
                {
                    TipoVitória = WinType.Coluna,
                    Numero = coluna
                };
                return true;
            }

            if (VerificaGanhador(diag, JogadorAtual)) {
                info = new WinInfo()
                {
                    TipoVitória = WinType.Diagonal,
                    Numero = 0
                };
                return true;
            }

            if (VerificaGanhador(diag2, JogadorAtual)) {
                info = new WinInfo()
                {
                    TipoVitória = WinType.AntiDiagonal,
                    Numero = 0
                };
                return true;
            }

            info = null;
            return false;

        }


        public bool JogadaFinal(int linha, int coluna, out GameResult resultado)
        {
            if(JogadaVencedora(linha, coluna, out WinInfo info))
            {
                resultado = new GameResult()
                {
                    Ganhador = JogadorAtual,
                    Info = info
                };
                JogoEmAndamento = false;
                return true;
            }

            if(TabelaCheia())
            {
                resultado = new GameResult()
                {
                    Ganhador = Player.Nenhum,
                    Info = null
                };
                return true;
            }

            resultado = null;
            return false;
        }


        public Jogada(int linha, int coluna)
        {
            if (!JogadaValida(linha, coluna)) return;

            TabelaJogo[linha, coluna] = JogadorAtual;
            Turnos++;

            if(JogadaFinal(linha, coluna, out GameResult resultado))
            {
                JogoEmAndamento = false;
                   
                AçãoJogada?.Invoke(linha, coluna);

                FimJogo?.Invoke(resultado);
            } else
            {
                AçãoJogada?.Invoke(linha, coluna);

                TrocaJogador();

            }

                
        }


        public void Recomeçar()
        {
            TabelaJogo = new Player[3, 3];
            JogadorAtual = Player.X;
            Turnos = 0;
            JogoEmAndamento = true;
            RecomeçarJogo?.Invoke();
        }

    }
}
