namespace WotGenC.Challenges
{
    public class ChallengeImmobility : Challenge
    {
        public ChallengeImmobility(params GameMode[] modes) : base(modes)
        {
            Intitule = "Immobilité pendant un certain temps";
        }
    }
}