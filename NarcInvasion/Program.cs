using System;
using System.Collections.Generic;
using System.Threading;

namespace NarcInvasion
{
    class Program
    {
        private static readonly ConsoleColor originalForegroundColor = Console.ForegroundColor;
        private static readonly int maxX = Console.WindowWidth - 1;
        private static readonly int maxY = Console.WindowHeight - 1;
        private static readonly Random random = new Random();
        private static readonly string soundPath = Environment.CurrentDirectory + @"\sound\";
        private static readonly string backgroundSound = soundPath + "Background.mp3";
        private static readonly string battleBackgroundSound = soundPath + "Endless-Storm.mp3";
        private static readonly int maxEnemyHealth = 4;
        private static readonly int dialogueBorderHeight = 7;
        private static readonly int dialogueTextX = 4;
        private static readonly int dialogueTextY = maxY - dialogueBorderHeight + 3;

        private static GameObject player;
        private static GameObject playerInMainGame;
        private static GameObject enemy;
        private static GameObject enemyInMainGame;
        private static GameObject bird;
        private static GameObject dialogueEnter;
        private static GameObject dialogueChoiceYes;
        private static GameObject dialogueChoiceNo;
        private static GameObject dialogueChoiceSelected;
        private static GameObject dialogueEmpty;
        private static GameObject enemyHealthBar;
        private static List<GameObject> persons;
        private static List<GameObject> trees;
        private static List<GameObject> dialogueBorderLines;
        private static List<GameObject> evilWords;
        private static List<GameObject> rockWalls;
        private static List<GameObject> blackHole;
        private static Queue<Dialogue> dialogue;
        private static GameState gameState;
        private static WMPLib.WindowsMediaPlayer soundPlayerBackground;
        private static WMPLib.WindowsMediaPlayer soundPlayerDialogue;
        private static int enemyHealth;
        private static bool isInitialDialogueWithBirdDone;
        private static bool isFirstBattleDone;
        private static Thread battleDialogueThread;
        private static Timer battleAnimationTimer;

        static void Main(string[] args)
        {
            SetConsoleProperties();
            
            InitObjects();
            DrawObjects();

            PlayBackgroundSound();
            StartInitialDialogue();
            HandleInput();

            RestoreConsoleProperties();
        }

        private static void SetConsoleProperties()
        {
            Console.CursorVisible = false;
        }

        private static void InitObjects()
        {
            player = new GameObject(AsciiArt.Person, maxX / 2 - 2, maxY / 2 - 2, ConsoleColor.White);
            bird = new GameObject(AsciiArt.Bird, maxX / 2 - 10, maxY / 2 - 4, ConsoleColor.Yellow);
            persons = new List<GameObject>();
            trees = new List<GameObject>();
            trees.Add(new GameObject(AsciiArt.Trees, (int)(0.6 * maxX), (int)(0.05 * maxY), ConsoleColor.DarkGreen));
            trees.Add(new GameObject(AsciiArt.Tree, (int)(0.1 * maxX), (int)(0.4 * maxY), ConsoleColor.DarkGreen));
            dialogue = new Queue<Dialogue>();

            isInitialDialogueWithBirdDone = false;
            isFirstBattleDone = false;
            gameState = GameState.MainGame;

            InitSoundPlayers();
            InitDialogueChoices();
            InitDialogueBorderLines();
            InitBirdDialogue();
            InitPersons();
        }

        private static void InitSoundPlayers()
        {
            soundPlayerDialogue = new WMPLib.WindowsMediaPlayer();
            soundPlayerDialogue.settings.volume = 40;  // volume can be between 0 and 100

            soundPlayerBackground = new WMPLib.WindowsMediaPlayer();
            soundPlayerBackground.settings.setMode("loop", true);
            soundPlayerBackground.settings.volume = 10;  // volume can be between 0 and 100
        }

        private static void InitDialogueChoices()
        {
            string enterText = "Enter";
            string yes = "[Yes]";
            string no = " No ";
            
            string emptyText = "";
            for (int i = 0; i < (maxX - 1 - 4) * 2; i++) emptyText += " ";
            
            dialogueEnter = new GameObject(enterText, maxX - 2 - enterText.Length, maxY - 1, ConsoleColor.DarkYellow);
            dialogueChoiceYes = new GameObject(yes, maxX / 4 - yes.Length / 2, maxY - 2, ConsoleColor.White);
            dialogueChoiceNo = new GameObject(no, maxX * 3 / 4 - no.Length, maxY - 2, ConsoleColor.Yellow);
            dialogueChoiceSelected = dialogueChoiceYes;
            dialogueEmpty = new GameObject(Wrap(emptyText), dialogueTextX, dialogueTextY, ConsoleColor.White);
        }

        private static string Wrap(string text)
        {
            int size = maxX - 4;
            if (text.Length > size)
            {
                int spaceIndex = text.Substring(0, size).LastIndexOf(" ");
                int splitIndex = spaceIndex > -1 ? spaceIndex : size;
                text = text.Substring(0, splitIndex) + Environment.NewLine + Environment.NewLine + text.Substring(splitIndex);
            }
            return text;
        }

        private static void InitDialogueBorderLines()
        {
            int textHeight = dialogueBorderHeight - 2;
            int verticalLineHeight = textHeight;
            int horizontalLineX = 0;
            int horizontalLine1Y = maxY - dialogueBorderHeight + 1;
            int horizontalLine2Y = maxY;
            int verticalLineY = horizontalLine1Y + 1;
            int verticalLine1X = 1;
            int verticalLine2X = maxX - 1;

            string horizontalLine1 = "";
            string horizontalLine2 = "";
            string verticalLine = "";
            for (int i = 0; i < maxX; i++) { horizontalLine1 += "."; }
            for (int i = 0; i < maxX; i++) { horizontalLine2 += "."; }
            for (int i = 0; i < verticalLineHeight; i++) { verticalLine += ":" + Environment.NewLine; }

            dialogueBorderLines = new List<GameObject>();
            dialogueBorderLines.Add(new GameObject(horizontalLine1, horizontalLineX, horizontalLine1Y, ConsoleColor.DarkGray));
            dialogueBorderLines.Add(new GameObject(horizontalLine2, horizontalLineX, horizontalLine2Y, ConsoleColor.DarkGray));
            dialogueBorderLines.Add(new GameObject(verticalLine, verticalLine1X, verticalLineY, ConsoleColor.DarkGray));
            dialogueBorderLines.Add(new GameObject(verticalLine, verticalLine2X, verticalLineY, ConsoleColor.DarkGray));
        }

        private static void InitBirdDialogue()
        {
            Answer canYouHelpAnswer = new Answer((o) => { isInitialDialogueWithBirdDone = true; Draw.DrawObjects(persons); }, (o) => InitBirdDialogue());
            dialogue.Enqueue(new Dialogue(Wrap("3p: Hello! Can you help me?"), dialogueTextX, dialogueTextY, ConsoleColor.Yellow, soundPath + "hello.mp3"));
            dialogue.Enqueue(new Dialogue(Wrap("mc: Wow... a bird that can speak. What do you need help with?"), dialogueTextX, dialogueTextY, ConsoleColor.White));
            dialogue.Enqueue(new Dialogue(Wrap("3p: A terrible mental disease has started to appear on my planet. I came here because the disease seems to have already spread very widely among humans here :("), dialogueTextX, dialogueTextY, ConsoleColor.Yellow, soundPath + "a-terrible-disease.mp3"));
            dialogue.Enqueue(new Dialogue(Wrap("3p: Can you help me learn more about the disease by speaking to the humans in the park?"), dialogueTextX, dialogueTextY, ConsoleColor.Yellow, canYouHelpAnswer, soundPath + "can-you-help.mp3"));
        }

        private static void InitPersons()
        {
            Person person1 = new Person(AsciiArt.Person, (int)(0.05 * maxX), (int)(0.1 * maxY), ConsoleColor.DarkGray, true);
            Person person2 = new Person(AsciiArt.Person, (int)(0.7 * maxX), (int)(0.5 * maxY), ConsoleColor.DarkGray, true);
            Person person3 = new Person(AsciiArt.Person, (int)(0.3 * maxX), (int)(0.6 * maxY), ConsoleColor.DarkGray);
            Person person4 = new Person(AsciiArt.Person, (int)(0.1 * maxX), (int)(0.1 * maxY), ConsoleColor.DarkGray);
            
            Answer personIsNarc = new Answer((o) => HandleIsPersonNarc(o), (o) => HandleIsPersonNarc(o), dialogueChoiceYes);
            Answer personIsNoNarc = new Answer((o) => HandleIsPersonNarc(o), (o) => HandleIsPersonNarc(o), dialogueChoiceNo);
            Dialogue doesPersonHaveAProblemNarc = new Dialogue(Wrap("3p: Does this person seem to have a problem?"), dialogueTextX, dialogueTextY, bird.Color, personIsNarc, soundPath + "this-person-has-a-problem.mp3");
            Dialogue doesPersonHaveAProblemNoNarc = new Dialogue(Wrap("3p: Does this person seem to have a problem?"), dialogueTextX, dialogueTextY, bird.Color, personIsNoNarc, soundPath + "this-person-has-a-problem.mp3");

            person1.Dialogue.Enqueue(new Dialogue(Wrap("Person: *talking to someone else* \"Why are you being so difficult, get over it already!\""), dialogueTextX, dialogueTextY, person1.Color, soundPath + "why-so-difficult.mp3"));
            person1.Dialogue.Enqueue(doesPersonHaveAProblemNarc);
            person2.Dialogue.Enqueue(new Dialogue(Wrap("Person: Everyone notices me when I enter a room. They know that they’ll never be as successful as me."), dialogueTextX, dialogueTextY, person2.Color, soundPath + "everyone-notices-me.mp3"));
            person2.Dialogue.Enqueue(doesPersonHaveAProblemNarc);
            person3.Dialogue.Enqueue(new Dialogue(Wrap("Person: Isn't it nice to help people without expecting anything in return."), dialogueTextX, dialogueTextY, person3.Color, soundPath + "help-without-return.mp3"));
            person3.Dialogue.Enqueue(doesPersonHaveAProblemNoNarc);
            person4.Dialogue.Enqueue(new Dialogue(Wrap("Person: ..."), dialogueTextX, dialogueTextY, person4.Color));

            persons.Add(person1);
            persons.Add(person2);
            persons.Add(person3);
            persons.Add(person4);
        }

        private static void DrawObjects()
        {
            Draw.DrawObject(player);
            Draw.DrawObject(bird);
            Draw.DrawObjects(trees);
        }

        private static void PlayBackgroundSound()
        {
            Sound.Play(soundPlayerBackground, backgroundSound);
        }

        private static void StartInitialDialogue()
        {
            DrawDialogue(dialogue.Peek());
            Thread.Sleep(1000);
            PlayDialogueSound(dialogue.Peek());
            gameState = GameState.Dialogue;
        }

        private static void DrawDialogue(Dialogue dialogue)
        {
            Draw.DrawObject(dialogue);
            Draw.DrawObject(dialogueEnter);
            Draw.DrawObjects(dialogueBorderLines);
            DrawDialogueChoices(dialogue);
        }

        private static void DrawDialogueChoices(Dialogue dialogue)
        {
            if (dialogue.Answer != null)
            {
                Draw.DrawObject(dialogueChoiceYes);
                Draw.DrawObject(dialogueChoiceNo);
            }
        }

        private static void PlayDialogueSound(Dialogue dialogue)
        {
            if (dialogue.SoundPath != null)
            {
                Sound.Stop(soundPlayerDialogue);
                Sound.Play(soundPlayerDialogue, dialogue.SoundPath);
            }
        }

        static void HandleInput()
        {
            ConsoleKeyInfo input;
            while (gameState != GameState.GameOver)
            {
                input = Console.ReadKey(true);
                if (gameState == GameState.MainGame) HandleInputMainGame(input);
                else if (gameState == GameState.Dialogue) HandleInputDialogue(input);
                else if (gameState == GameState.Battle) HandleInputBattle(input);
            }
        }

        private static void HandleInputMainGame(ConsoleKeyInfo input)
        {
            if (input.Key == ConsoleKey.LeftArrow && player.X > 1)
                MovePlayer(Direction.Left);
            else if (input.Key == ConsoleKey.RightArrow && player.X < maxX - player.Width)
                MovePlayer(Direction.Right);
            else if (input.Key == ConsoleKey.UpArrow && player.Y > 0)
                MovePlayer(Direction.Up);
            else if (input.Key == ConsoleKey.DownArrow && player.Y <= maxY - player.Height)
                MovePlayer(Direction.Down);
        }

        private static void MovePlayer(Direction direction)
        {
            player.Move(direction);
            bool noCollision = !IsCollisionPlayerWithObjects();
            player.Move(GetOppositeDirection(direction));
            if (noCollision)
            {
                Draw.UndrawObject(player);
                player.Move(direction);
                Draw.DrawObject(player);
            } 
            if (gameState == GameState.MainGame)
            {
                player.Move(direction);
                DrawDialogueIfCollisionPlayerWithPersonInMainGame();
                player.Move(GetOppositeDirection(direction));
            }
        }

        private static bool IsCollisionPlayerWithObjects()
        {
            if (gameState == GameState.MainGame)
                return IsCollision(player, trees) || IsCollision(player, persons) || IsCollision(player, bird);
            else if (gameState == GameState.Battle)
                return IsCollision(player, enemy) || IsCollision(player, enemyHealthBar) 
                    || IsCollision(player, blackHole) || IsCollision(player, rockWalls);
            else
                return false;
        }

        private static void DrawDialogueIfCollisionPlayerWithPersonInMainGame()
        {
            foreach (GameObject person in persons)
            {
                if (IsCollision(player, person))
                {
                    enemyInMainGame = person;
                    foreach (Dialogue d in person.Dialogue) { dialogue.Enqueue(d); }
                    DrawDialogue();
                }
            }
            if (!isInitialDialogueWithBirdDone && IsCollision(player, bird))
            {
                DrawDialogue();
            }
        }

        public static Direction GetOppositeDirection(Direction direction)
        {
            if (direction == Direction.Up) return Direction.Down;
            if (direction == Direction.Down) return Direction.Up;
            if (direction == Direction.Left) return Direction.Right;
            if (direction == Direction.Right) return Direction.Left;
            return Direction.Right;
        }

        private static bool IsCollision(GameObject object1, IEnumerable<GameObject> objects)
        {
            foreach (GameObject gameObject in objects)
            {
                if (IsCollision(object1, gameObject))
                {
                    return true;
                }
            }
            return false;
        }

        private static GameObject GetCollisionObject(GameObject object1, IEnumerable<GameObject> objects)
        {
            foreach (GameObject gameObject in objects)
            {
                if (IsCollision(object1, gameObject))
                {
                    return gameObject;
                }
            }
            return null;
        }

        private static bool IsCollision(GameObject object1, GameObject object2)
        {
            return (Math.Abs(object1.XCenter - object2.XCenter) < object1.Width / 2 + object2.Width / 2)
                && (Math.Abs(object1.YCenter - object2.YCenter) < object1.Height / 2 + object2.Height / 2);
        }

        private static void DrawDialogue()
        {
            if (dialogue.Count > 1) DrawNextDialogue(); 
            else if (dialogue.Count == 1) UndrawLastDialogueAndDoChosenAction();
        }

        private static void DrawNextDialogue()
        {
            ClearDialogue();
            Dialogue previousDialogue = dialogue.Dequeue();
            Dialogue currentDialogue = dialogue.Peek();
            DrawDialogue(currentDialogue);
            PlayDialogueSound(currentDialogue);
            gameState = GameState.Dialogue;
        }

        private static void ClearDialogue()
        {
            Draw.DrawObject(dialogueEmpty);
            Draw.UndrawObject(dialogueEnter);
            Draw.UndrawObjects(dialogueBorderLines);
        }

        private static void UndrawLastDialogueAndDoChosenAction()
        {
            ClearDialogue();
            Dialogue currentDialogue = dialogue.Peek();
            gameState = GameState.MainGame;
            DoDialogueAnswerAction(currentDialogue);
        }

        private static void DoDialogueAnswerAction(Dialogue dialogue)
        {
            if (dialogue != null && dialogue.Answer != null)
            {
                if (dialogueChoiceSelected == dialogueChoiceYes)
                {
                    dialogue.Answer.Yes(dialogue.Answer.CorrectAnswer);
                }
                else
                {
                    dialogue.Answer.No(dialogue.Answer.CorrectAnswer);
                }
                dialogueChoiceSelected = dialogueChoiceYes;
                dialogueChoiceYes.Text = "[Yes]";
                dialogueChoiceNo.Text = " No ";
                dialogueChoiceYes.Color = ConsoleColor.White;
                dialogueChoiceNo.Color = ConsoleColor.Yellow;
            }
        }

        private static void HandleInputDialogue(ConsoleKeyInfo input)
        {
            if (input.Key == ConsoleKey.Enter)
                DrawDialogue();
            else if ((input.Key == ConsoleKey.LeftArrow || input.Key == ConsoleKey.RightArrow) && IsDialogueChoiceDisplayed())
                ChangeDialogueChoiceSelected();
        }

        private static bool IsDialogueChoiceDisplayed()
        {
            return gameState == GameState.Dialogue && dialogue.Peek().Answer != null;
        }

        private static void ChangeDialogueChoiceSelected()
        {
            if (dialogueChoiceSelected == dialogueChoiceYes)
            {
                dialogueChoiceSelected = dialogueChoiceNo;
                dialogueChoiceNo.Text = "[No]";
                dialogueChoiceYes.Text = " Yes ";
                dialogueChoiceYes.Color = ConsoleColor.Yellow;
                dialogueChoiceNo.Color = ConsoleColor.White;
            }
            else
            {
                dialogueChoiceSelected = dialogueChoiceYes;
                dialogueChoiceNo.Text = " No ";
                dialogueChoiceYes.Text = "[Yes]";
                dialogueChoiceYes.Color = ConsoleColor.White;
                dialogueChoiceNo.Color = ConsoleColor.Yellow;
            }
            Draw.DrawObject(dialogueChoiceYes);
            Draw.DrawObject(dialogueChoiceNo);
        }

        private static void HandleInputBattle(ConsoleKeyInfo input)
        {
            if (input.Key == ConsoleKey.Enter)
                PlaceRockWall();
            else if (input.Key == ConsoleKey.LeftArrow && player.X > 1)
                MovePlayer(Direction.Left);
            else if (input.Key == ConsoleKey.RightArrow && player.X < maxX - player.Width)
                MovePlayer(Direction.Right);
            else if (input.Key == ConsoleKey.UpArrow && player.Y > 1)
                MovePlayer(Direction.Up);
            else if (input.Key == ConsoleKey.DownArrow && player.Y <= maxY - player.Height - dialogueBorderHeight + 1)
                MovePlayer(Direction.Down);
        }

        private static void PlaceRockWall()
        {
            if (rockWalls.Count == 2)
            {
                GameObject rockWall = rockWalls[0];
                Draw.UndrawObject(rockWall);
                rockWalls.RemoveAt(0);
            }
            rockWalls.Add(new GameObject(AsciiArt.RockWall, player.X + player.Width + 1, player.Y + 1, ConsoleColor.DarkGray));
            Draw.DrawObjects(rockWalls);
        }

        private static void HandleIsPersonNarc(GameObject correctAnswer)
        {
            if (correctAnswer == dialogueChoiceYes)
            {
                if (dialogueChoiceSelected == correctAnswer)
                {
                    dialogue.Enqueue(new Dialogue(Wrap("3p: You are right! The person wasn't able to drain mental energy from you!"), dialogueTextX, dialogueTextY, ConsoleColor.Yellow, soundPath + "right-no-energy-drained.mp3"));
                }
                else
                {
                    dialogue.Enqueue(new Dialogue(Wrap("3p: You are wrong! The person drained your mental energy!"), dialogueTextX, dialogueTextY, ConsoleColor.Yellow, soundPath + "wrong-energy-drained.mp3"));
                }
                Answer answer = new Answer((o) => StartBattle(), (o) => { });
                dialogue.Enqueue(new Dialogue(Wrap("3p: Would you like to try to cure them by killing the evil in them in battle?"), dialogueTextX, dialogueTextY, ConsoleColor.Yellow, answer, soundPath + "cure-them.mp3"));
            }
            else
            {
                if (dialogueChoiceSelected == correctAnswer)
                {
                    dialogue.Enqueue(new Dialogue(Wrap("3p: You are right! This person isn't evil!"), dialogueTextX, dialogueTextY, ConsoleColor.Yellow, soundPath + "right-no-evil.mp3"));
                }
                else
                {
                    dialogue.Enqueue(new Dialogue(Wrap("3p: You are wrong! But don't worry, this person isn't evil!"), dialogueTextX, dialogueTextY, ConsoleColor.Yellow, soundPath + "wrong-no-evil.mp3"));
                }
            }
            DrawDialogue();
        }

        private static void StartBattle()
        {
            InitBattle();
        }

        private static void InitBattle()
        {
            gameState = GameState.Battle;
            Console.Clear();
            PlayBackgroundSoundBattle();
            Sound.Stop(soundPlayerDialogue);

            evilWords = new List<GameObject>();
            rockWalls = new List<GameObject>();

            int blackHoleBottomHeight = 4;
            blackHole = new List<GameObject>();
            blackHole.Add(new GameObject(AsciiArt.BlackHoleTop, maxX - 9, 0, ConsoleColor.White));
            blackHole.Add(new GameObject(AsciiArt.BlackHoleBottom, maxX - 9, maxY - blackHoleBottomHeight - dialogueBorderHeight + 1, ConsoleColor.White));
            Draw.DrawObjects(blackHole);

            playerInMainGame = new GameObject(player.Text, player.X, player.Y, player.Color);
            player.X = 5;
            player.Y = maxY / 2 - player.Height / 2;
            Draw.DrawObject(player);
            
            enemy = new GameObject(AsciiArt.Person, maxX - 20, maxY / 2 - 2, ConsoleColor.Red);
            Draw.DrawObject(enemy);

            enemyHealth = 3;
            string healthBarText = GetHealthBarText(enemyHealth, enemy);
            enemyHealthBar = new GameObject(healthBarText, enemy.XCenter - healthBarText.Length / 2, enemy.Y + enemy.Height + 1, ConsoleColor.DarkRed);
            Draw.DrawObject(enemyHealthBar);
            battleAnimationTimer = new Timer(BattleAnimation, null, 1000, 300);
            if (!isFirstBattleDone)
            {
                battleDialogueThread = new Thread(() => DrawBattleInstructionsDialogue());
                battleDialogueThread.Start();
            }
        }

        private static void PlayBackgroundSoundBattle()
        {
            Sound.Play(soundPlayerBackground, battleBackgroundSound);
        }

        private static string GetHealthBarText(int health, GameObject person)
        {
            string healthbar = "";
            for (int i = 0; i < health*4; i++) healthbar += "?";
            return healthbar;
        }

        private static void BattleAnimation(object state)
        {
            // add evilwords to the list and move them to the left
            if (random.Next(2) == 1)
            {
                GameObject newEvilWord = new GameObject(AsciiArt.EvilWord, maxX - 24, random.Next(0, maxY - dialogueBorderHeight), ConsoleColor.Red);
                evilWords.Add(newEvilWord);
            }
            Draw.UndrawObjects(evilWords);
            foreach (GameObject evilWord in evilWords) evilWord.X -= 1;
            evilWords.RemoveAll(evilWord => evilWord.X + evilWord.Width < 0);
            Draw.DrawObjects(evilWords);
            Draw.DrawObject(enemy); // undrawing the evilwords might undraw the enemy
            Draw.DrawObject(enemyHealthBar); // undrawing the evilwords might undraw the healthbar

            // move the player to the right
            Draw.UndrawObject(player);
            MovePlayer(Direction.Right);
            Draw.DrawObject(player);
            if (player.X + player.Width > maxX) {
                battleAnimationTimer.Dispose();
                GameOver();
                return;
            }

            // check collision evilwords
            GameObject evilWordInCollisionWithPlayer = GetCollisionObject(player, evilWords);
            if (evilWordInCollisionWithPlayer != null) {
                evilWords.Remove(evilWordInCollisionWithPlayer);
                Draw.UndrawObject(evilWordInCollisionWithPlayer);
                Draw.DrawObject(player); // undrawing the evilword might undraw the player
                if (enemyHealth < maxEnemyHealth) UpdateEnemyHealth(+1);
            }
            List<GameObject> rockWallsInCollision = new List<GameObject>();
            foreach (GameObject rockWall in rockWalls)
            {
                GameObject evilWordInCollisionWithRockWall = GetCollisionObject(rockWall, evilWords);
                if (evilWordInCollisionWithRockWall != null)
                {
                    evilWords.Remove(evilWordInCollisionWithRockWall);
                    Draw.UndrawObject(evilWordInCollisionWithRockWall);
                    rockWallsInCollision.Add(rockWall);
                    Draw.UndrawObject(rockWall);
                    UpdateEnemyHealth(-1);
                    if (enemyHealth == 0)
                    {
                        gameState = GameState.MainGame;
                        battleAnimationTimer.Dispose();
                        Thread battleEndThread = new Thread(() => DrawBattleEnd());
                        battleEndThread.Start();
                        Thread.Sleep(5000);
                        isFirstBattleDone = true;
                        persons.Remove(enemyInMainGame);
                        InitMainGameAfterBattle();
                        return;
                    }
                }
            }
            rockWalls.RemoveAll(rockWall => rockWallsInCollision.Contains(rockWall));
        }

        private static void GameOver()
        {
            gameState = GameState.GameOver;
            Dialogue dialogue = new Dialogue(Wrap("Game Over! The narc has devoured your soul :("), dialogueTextX, dialogueTextY, ConsoleColor.Red);
            Draw.DrawObject(dialogue);
            Draw.DrawObjects(dialogueBorderLines);
        }

        private static void UpdateEnemyHealth(int addition)
        {
            Draw.UndrawObject(enemyHealthBar);
            enemyHealth += addition;
            enemyHealthBar.Text = GetHealthBarText(enemyHealth, enemy);
            Draw.DrawObject(enemyHealthBar);
        }

        private static void DrawBattleEnd()
        {
            ClearDialogue();
            Dialogue dialogue = new Dialogue(Wrap("Narc: Nooo... I can't die!!! *explodes*"), dialogueTextX, dialogueTextY, ConsoleColor.Red, soundPath + "i-cant-die.mp3");
            Draw.DrawObject(dialogue);
            Draw.DrawObjects(dialogueBorderLines);
            PlayDialogueSound(dialogue);
        }

        private static void InitMainGameAfterBattle()
        {
            PlayBackgroundSound();
            Console.Clear();
            player = playerInMainGame;
            DrawObjects();
            Draw.DrawObjects(persons);
            int nrOfNarcs = CountNrOfNarcs();
            if (nrOfNarcs == 0)
            {
                dialogue.Enqueue(new Dialogue(Wrap("3p: You have cured all evil in the world and thanks to you i learned how to save my planet! *ascends in a spaceship*"), dialogueTextX, dialogueTextY, ConsoleColor.Yellow, soundPath + "you-saved-the-world.mp3"));
                DrawDialogue();
                gameState = GameState.GameOver;
            }
        }

        private static int CountNrOfNarcs()
        {
            int nrOfNarcs = 0;
            foreach (Person person in persons)
            {
                foreach (Dialogue d in person.Dialogue)
                {
                    if (d.Answer != null && d.Answer.CorrectAnswer == dialogueChoiceYes)
                    {
                        nrOfNarcs++;
                    }
                }
            }
            return nrOfNarcs;
        }

        private static void DrawBattleInstructionsDialogue()
        {
            Thread.Sleep(2000);
            Dialogue dialogue = new Dialogue(Wrap("3p: Be careful to not get absorbed by the black hole of their insecure ego!"), dialogueTextX, dialogueTextY, ConsoleColor.Yellow, soundPath + "be-careful.mp3");
            Draw.DrawObject(dialogue);
            Draw.DrawObjects(dialogueBorderLines);
            PlayDialogueSound(dialogue);
            Thread.Sleep(8000);
            ClearDialogue();

            Thread.Sleep(5000);
            if (gameState == GameState.Battle)
            {
                dialogue = new Dialogue(Wrap("3p: You can press Enter to make walls of rocks that kill their evil words!"), dialogueTextX, dialogueTextY, ConsoleColor.Yellow, soundPath + "press-enter.mp3");
                Draw.DrawObject(dialogue);
                Draw.DrawObjects(dialogueBorderLines);
                PlayDialogueSound(dialogue);
                Thread.Sleep(10000);
                ClearDialogue();
            }
        }

        private static void RestoreConsoleProperties()
        {
            Console.CursorVisible = true;
            Console.ForegroundColor = originalForegroundColor; 
            Console.SetCursorPosition(0, maxY);
        }
    }
}
