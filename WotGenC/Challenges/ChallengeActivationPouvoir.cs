namespace WotGenC.Challenges
{
    public class ChallengeActivationPouvoir : Challenge
    {
        public ChallengeActivationPouvoir(params GameMode[] modes) : base(modes)
        {
            Intitule = "Activation de son pouvoir dès que possible";
        }
    }
}