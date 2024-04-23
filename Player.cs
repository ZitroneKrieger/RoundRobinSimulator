using System.Runtime.CompilerServices;

namespace RoundRobinSimulator
{
    public class Player
    {


        public string Name { get; set; }
        public int Elo { get; set; }
        public int WeißGewesen { get; set; }
        public int SchwarzGewesen { get; set; }
        public List<decimal> Results { get; set; } = new List<decimal>();

        public Player(Player item)
        {
            Name = item.Name;
            Elo = item.Elo;
        }

        public Player()
        {
        }
        public void InitSeitenGewesen()
        {
            WeißGewesen = 0;
            SchwarzGewesen = 0;
        }
        public void AddWin()
        {
            decimal win = 1;
            Results.Add(win);
        }
        public void AddLose()
        {
            decimal lose = 0;
            Results.Add(lose);
        }
        public void AddDraw()
        {
            Results.Add(0.5M);
        }
        public decimal GetResult()
        {
            return Results.Sum();
        }

    }
}