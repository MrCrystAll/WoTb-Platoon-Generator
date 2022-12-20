using System;
using System.Collections.Generic;
using System.Linq;
using WotGenC.Missions;

namespace WotGenC.Challenges
{
    public class ChallengeSetter
    {
        public void WillThereBeAChallenge(List<Player> players, List<Challenge> challenges)
        {
            for (int i = 0; i < players.Count; i++)
            {
                Player j = players[i];
                j.Challenge = null;
            }
            
            Random random = new Random();

            //Nombre de joueurs à avoir un challenge
            int nbPlayers = random.Next(1, players.Count + 1);
            
            //Les joueurs qui vont être choisis
            List<Player> selected = new List<Player>(nbPlayers);

            //Liste d'entiers de 0 au nombre de joueurs - 1
            List<int> entiers = new List<int>(players.Count);

            //On remplit le tableau
            for (int i = 0; i < players.Count; i++)
            {
                entiers.Add(i);
            }
            
            for (int i = 0; i < nbPlayers; i++)
            {
                int j = random.Next(0, entiers.Count);
                selected.Add(players[entiers[j]]);
                entiers.Remove(j);
            }


            foreach (var player in selected)
            {
                //Choix du challenge
                var challengesSorted = challenges.Where(x => x.AvailablesModes.Contains(Settings.GameMode)).ToList();
                int choice = random.Next(0, challengesSorted.Count);
                player.Challenge = challengesSorted[choice];
            }
        }
    }
}