using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Controls.Primitives;

namespace WotGenC.Missions
{
    public class MissionSetter
    {

        public void WillThereBeAMission(List<Player> players)
        {
            List<int> choices = new List<int>();

            //6 zeros => 60% mission dégats
            for(int i = 0; i < 6; i++)
            {
                choices.Add(0);
            }

            //4 uns => 40% mission protection
            for (int i = 0; i < 4; i++)
            {
                choices.Add(1);
            }


            Random random = new Random();

            //Nombre de joueurs à avoir une mission
            int nbPlayers = random.Next(1, players.Count);
            
            //Les joueurs qui vont être choisis
            List<Player> Selected = new List<Player>(nbPlayers);

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
                Selected.Add(players[entiers[j]]);
                entiers.Remove(j);
            }


            foreach (var player in Selected)
            {
                //Choix de la mission
                int choice = random.Next(0, choices.Count);

                Mission mission = null;
                //Mission dégats
                if (choices[choice] == 0)
                {
                    //Tier VII
                    if(Settings.Tier == Tier.VII)
                    {
                        //Dégats : Entre 1300 et 1800
                        //Kills : Entre 1 et 3
                        mission = new MissionDegats(random.Next(1300, 1800), random.Next(1, 4));
                    }

                    //Tier VIII
                    else if (Settings.Tier == Tier.VIII)
                    {
                        //Dégats : Entre 1800 et 2300
                        //Kills : Entre 1 et 3
                        mission = new MissionDegats(random.Next(1800, 2300), random.Next(1, 4));
                    }

                    //Tier IX
                    else if (Settings.Tier == Tier.IX)
                    {
                        //Dégats : Entre 2300 et 2800
                        //Kills : Entre 2 et 4
                        mission = new MissionDegats(random.Next(2300, 2800), random.Next(2, 5));
                    }

                    //Tier X
                    else if (Settings.Tier == Tier.X)
                    {
                        //Dégats : Entre 2800 et 3300
                        //Kills : Entre 2 et 4
                        mission = new MissionDegats(random.Next(2800, 3300), random.Next(2, 5));
                    }
                }

                else
                {
                    mission = new MissionProtect(player);
                }
                player.Mission = mission;
            }
        }
    }   
}
