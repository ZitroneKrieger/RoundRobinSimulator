using RoundRobinSimulator;
using System.Security.Cryptography.X509Certificates;

internal class StatistcsObject
{
    public List<Player> playerList;
    public StatistcsObject(List<Player> players)
    {
        this.playerList = players;
    }
    public StatistcsObject()
    {
    }


    public void AddToPlayerByNameIfWasWhite(string playername, bool istWeißGewesen)
    {
        var player = playerList.Where(x => x.Name == playername).First();
        if(istWeißGewesen)
        {
            player.WeißGewesen++;
        }
        else
        {
            player.SchwarzGewesen++;
        }
    }

    public void AddAllStanding(List<Tuple<Player, Player>> roundParings)
    {
        for(int i = 0; i < roundParings.Count; i++)
        {
            AddToPlayerByNameIfWasWhite(roundParings[i].Item1.Name, false);
            AddToPlayerByNameIfWasWhite(roundParings[i].Item2.Name, true);
        }
    }

    public void InitAllPlayersSeiten()
    {
        foreach(var player in playerList)
        {
            player.InitSeitenGewesen();
        }
    }
}