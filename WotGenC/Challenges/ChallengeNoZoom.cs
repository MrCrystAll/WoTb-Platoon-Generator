namespace WotGenC.Challenges
{
    public class ChallengeNoZoom : Challenge
    {
        public ChallengeNoZoom(params GameMode[] modes) : base(modes)
        {
            Intitule = "Interdiction de zoomer/Unbind du zoom";
        }
    }
}