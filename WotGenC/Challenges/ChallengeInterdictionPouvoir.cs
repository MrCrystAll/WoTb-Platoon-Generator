namespace WotGenC.Challenges
{
    public class ChallengeInterdictionPouvoir : Challenge
    {
        public ChallengeInterdictionPouvoir(params GameMode[] modes) : base(modes)
        {
            Intitule = "Interdiction d'utiliser son pouvoir";
        }
    }
}