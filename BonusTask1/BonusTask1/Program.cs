using System.Text.Json;
using System.Text.RegularExpressions;
namespace BonusTask1
{
    internal class Program
    {
        const string fname = "bank.txt";
        const string rules = "rules.txt";
        const string leaderBoard = "leaderBoard.txt";
        public class Question
        {
            public string QuestionText { get; }
            public int Answer { get; }

            public Question(string QuestionText, int Answer)
            {
                this.QuestionText = QuestionText;
                this.Answer = Answer;
            }
        }

        public class Player
        {
            public string Name { get; }
            public int Points { get; private set; }

            public Player(string Name, int Points)
            {
                this.Name = Name;
                this.Points = Points;
            }
            public void AddPoints()
            {
                ++Points;
            }

            public void ChangePoints(int points)
            {
                if (points < 0)
                    throw new ArgumentException("количество баллов не должно быть отрицательным");
                Points = points;
            }

            public override string ToString()
            {
                switch (Points)
                {
                    case 1:
                        return $"{Name} - {Points} балл";

                    case 2 or 3 or 4:
                        return $"{Name} - {Points} балла";

                    default:
                        return $"{Name} - {Points} баллов";

                }

            }

        }
        /// <summary>
        /// Печать вопроса Question
        /// </summary>
        public static void PrintQuestion(int num, Question q)
        {
            Console.Write($"Вопрос {num}: ");
            Console.WriteLine(q.QuestionText);
        }
        /// <summary>
        /// Заполнение списка объектов типа Question
        /// </summary>
        /// <returns></returns>
        public static List<Question> FillQuestions()
        {
            List<Question> questions = new List<Question>();
            string[] lines = File.ReadAllLines(fname);
            for (int i = 0; i < lines.Count(); ++i)
            {
                string line = lines[i];
                string question = Regex.Match(line, @".+(?=\?)").Value;
                string answerString = Regex.Match(line, @"\b([а-яА-Я\s]+)=>True\b").Groups[1].Value;

                int numberOfRightAnswer = -1;

                question = question + "? ";
                int ansNum = 1;

                question = question + "(укажите только номер правильного ответа)\n";

                foreach (Match m in Regex.Matches(line, @"\b([а-яА-Я\s]+)(?=\=)\b"))
                {
                    if (m.Value == answerString)
                        numberOfRightAnswer = ansNum;
                    question = question + $"{ansNum})" + m.Groups[1].Value + " ";
                    ++ansNum;
                }

                questions.Add(new Question(question, numberOfRightAnswer));
            }
            return questions;
        }
        /// <summary>
        /// Печать правил
        /// </summary>
        public static void PrintRules()
        {
            try
            {
                Console.WriteLine(File.ReadAllText(rules));
                Console.WriteLine();
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine("Файл не найден, поэтому правил пока нет: " + e.Message);
            }

        }
        /// <summary>
        /// Реакция на правильный ответ
        /// </summary>
        public static void PrintKindResponse()
        {
            string[] kindResponses = new string[] { "Правильно!", "Да у вас талант!",
            "Так держать!", "Поздравляю, ответ верный!", "Вы лучше всех!"};
            Random r = new Random();
            int ind = r.Next(0, kindResponses.Count());
            Console.WriteLine(kindResponses[ind]);
        }

        /// <summary>
        /// Реакция на неправильный ответ
        /// </summary>
        public static void PrintAggressiveResponse()
        {
            string[] aggressiveResponses = new string[] {"Победы вам не видать.", "Неправильно.", "Мимо.", "Не-а.",
                "Может уже пора заканчивать игру?", "Поздравляю! Ваш ответ неправильный." };
            Random r = new Random();
            int ind = r.Next(0, aggressiveResponses.Count());
            Console.WriteLine(aggressiveResponses[ind]);
        }

        /// <summary>
        /// Обновление таблицы лидеров путём добавления игрока
        /// Если не существует - будет создана
        /// </summary>
        /// <param name="path"></param>
        public static void UpdateLeaderBoardFile(string path, Player pl)
        {
            try
            {
                if (!File.Exists(path))
                    File.Create(path).Close();

                List<Player> playersList = new List<Player>();

                using (StreamReader sr = new StreamReader(path))
                {
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        Player p = JsonSerializer.Deserialize<Player>(line);
                        playersList.Add(p);
                    }
                }
                //для каждого игрока будем хранить лучший результат
                bool foundPl = false;
                foreach (Player p in playersList)
                {
                    if (p.Name == pl.Name) // будем считать, что имена, записанные в разном регистре - различны
                    {
                        foundPl = true;
                        if (p.Points < pl.Points)
                        {
                            p.ChangePoints(pl.Points);
                            break;
                        }
                    }
                }
                //добавляем в первый раз
                if (foundPl == false)
                    playersList.Add(pl);


                File.WriteAllText(path, string.Empty);

                playersList = playersList.OrderBy(x => -x.Points).ToList();
                using (StreamWriter sw = new StreamWriter(path, true))
                {

                    foreach (Player p in playersList)
                    {
                        string jsonString = JsonSerializer.Serialize<Player>(p);
                        sw.WriteLine(jsonString);
                    }
                }
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine("Файл не найден: " + e.Message);
            }
        }

        /// <summary>
        /// Печать топ-10 игроков по количеству баллов
        /// </summary>
        /// <param name="path"></param>
        public static void PrintTop10Players(string path)
        {
            try
            {
                Console.WriteLine("Топ-10 игроков:");
                List<Player> top10Players = new List<Player>();
                using (StreamReader sr = new StreamReader(path))
                {

                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        Player pl = JsonSerializer.Deserialize<Player>(line);
                        top10Players.Add(pl);
                        if (top10Players.Count() == 10)
                            break;
                    }
                }
                foreach (Player p in top10Players)
                    Console.WriteLine(p);
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine("Файл не найден: " + e.Message);
            }
        }

        static void Main(string[] args)
        {

            List<Question> questions = FillQuestions();
            PrintRules();

            Console.Write("Представьтесь: ");
            string name = Console.ReadLine();
            while (name == "" || name.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries).Count() == 0)
            {
                Console.Write("Представьтесь: ");
                name = Console.ReadLine();
            }
            name = name.Trim();
            name = string.Join(' ', name.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries));
            Console.WriteLine($"Удачи, {name}!");

            Player pl = new Player(name, 0);

            name = null;

            for (int num = 1; num <= questions.Count(); ++num)
            {
                PrintQuestion(num, questions[num - 1]);

                int answer = -1;
                while (true)
                {
                    Console.Write("Введите ответ:");
                    string input = Console.ReadLine();
                    if (int.TryParse(input, out int tempAns) && int.Parse(input) <= 4 && int.Parse(input) >= 1)
                    {
                        answer = tempAns;
                        break;
                    }
                }

                if (answer == questions[num - 1].Answer)
                {
                    PrintKindResponse();
                    pl.AddPoints();
                }
                else
                    PrintAggressiveResponse();

            }


            switch (pl.Points)
            {
                case 1:
                    Console.WriteLine($"{pl.Name}, вы набрали {pl.Points} балл");
                    break;
                case 2 or 3 or 4:
                    Console.WriteLine($"{pl.Name}, вы набрали {pl.Points} балла");
                    break;
                default:
                    Console.WriteLine($"{pl.Name}, вы набрали {pl.Points} баллов");
                    break;
            }
            UpdateLeaderBoardFile(leaderBoard, pl);
            PrintTop10Players(leaderBoard);
        }
    }
}
