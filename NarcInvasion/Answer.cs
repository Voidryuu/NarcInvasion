namespace NarcInvasion
{
    public class Answer
    {
        public delegate void YesAction(GameObject o);
        public delegate void NoAction(GameObject o);

        public YesAction Yes { get; }
        public NoAction No { get; }

        public GameObject CorrectAnswer { get; set; }

        public Answer(YesAction yesAction, NoAction noAction, GameObject correctAnswer)
        {
            Yes = yesAction;
            No = noAction; 
            CorrectAnswer = correctAnswer;
        }

        public Answer(YesAction yesAction, NoAction noAction) : this(yesAction, noAction, null) { }
    }
}