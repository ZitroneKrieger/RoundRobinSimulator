using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoundRobinSimulator
{
    public class SchachMatch
    {
        private Player _schwarz;
        private Player _weiß;
        private decimal? _resultatWeiß;
        private decimal? _resultatSchwarz;

        public decimal? ResultatWeiß { get => _resultatWeiß; set => _resultatWeiß = value; }
        public decimal? ResultatSchwarz { get => _resultatSchwarz; set => _resultatSchwarz = value; }
        public Player Weiß { get => _weiß; set => _weiß = value; }
        public Player Schwarz { get => _schwarz; set => _schwarz = value; }
    }
}
