using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static GmGard.Models.UserQuest;

namespace GmGard.Models
{
    public class GameChoiceDescriptor
    {
        private UserQuest User;

        public GameChoiceDescriptor(UserQuest user)
        {
            User = user;
        }

        private struct GameChoice
        {
            public int position;
            public int length;
            public Dictionary<uint, string> choice;
        }

        private static readonly GameChoice Act2Choice = new GameChoice
        {
            position = 0,
            length = 1,
            choice = new Dictionary<uint, string>{ { 0, "no" }, { 1, "yes" } }
        };

        private static readonly GameChoice Act3Choice = new GameChoice
        {
            position = 1,
            length = 2,
            choice = new Dictionary<uint, string>
            {
                { 1, "stay" },
                { 2, "run" },
                { 3, "attack" },
            }
        };

        private static readonly GameChoice Act4Choice = new GameChoice
        {
            position = 3,
            length = 2,
            choice = new Dictionary<uint, string>
            {
                { 1, "left" },
                { 2, "right" },
                { 3, "unknow" },
            }
        };

        private static readonly GameChoice Act5Choice = new GameChoice
        {
            position = 5,
            length = 2,
            choice = new Dictionary<uint, string>
            {
                { 1, "peace" },
                { 2, "mo" },
                { 3, "gan" },
            }
        };

        private static readonly GameChoice Act5bChoice = new GameChoice
        {
            position = 7,
            length = 2,
            choice = new Dictionary<uint, string>
            {
                { 1, "cross" },
                { 2, "wear" },
                { 3, "think" },
            }
        };

        private static readonly GameChoice Act6Choice = new GameChoice
        {
            position = 9,
            length = 1,
            choice = new Dictionary<uint, string> { { 0, "false" }, { 1, "true" } }
        };

        private string GetChoice(GameChoice game)
        {
            if (User.GameChoices == null)
            {
                return null;
            }
            uint val = TakeBit(User.GameChoices, game.position, game.length);
            string c = null;
            game.choice.TryGetValue(val, out c);
            return c;
        }

        private bool SetChoice(string choice, GameChoice game)
        {
            if (!game.choice.ContainsValue(choice))
            {
                return false;
            }
            var pair = game.choice.Single(c => c.Value == choice);
            BitArray ba = new BitArray(User.GameChoices ?? new byte[1]);
            SetBit(ba, game.position, game.length, pair.Key);
            var newGameChoices = new byte[(ba.Length + 7) / 8];
            ba.CopyTo(newGameChoices, 0);
            User.GameChoices = newGameChoices;
            return true;
        }

        public string GetAct2Choice() => GetChoice(Act2Choice);
        public bool SetAct2Choice(string choice) => SetChoice(choice, Act2Choice);
        public string GetAct3Choice() => GetChoice(Act3Choice);
        public bool SetAct3Choice(string choice) => SetChoice(choice, Act3Choice);
        public string GetAct4Choice() => GetChoice(Act4Choice);
        public bool SetAct4Choice(string choice) => SetChoice(choice, Act4Choice);
        public string GetAct5Choice() => GetChoice(Act5Choice);
        public bool SetAct5Choice(string choice) => SetChoice(choice, Act5Choice);
        public string GetAct5bChoice() => GetChoice(Act5bChoice);
        public bool SetAct5bChoice(string choice) => SetChoice(choice, Act5bChoice);
        public string GetAct6Choice() => GetChoice(Act6Choice);
        public bool SetAct6Choice(string choice) => SetChoice(choice, Act6Choice);

        public void GenerateQuestions()
        {
            BitArray ba = new BitArray(User.GameChoices ?? new byte[3]);
            Random rnd = new Random();
            int[] questionIndices = Enumerable.Range(1, 10).OrderBy(_ => rnd.Next()).Take(3).ToArray();
            SetBit(ba, 10, 4, (uint)questionIndices[0]);
            SetBit(ba, 14, 4, (uint)questionIndices[1]);
            SetBit(ba, 18, 4, (uint)questionIndices[2]);
            var newGameChoices = new byte[(ba.Length + 7) / 8];
            ba.CopyTo(newGameChoices, 0);
            User.GameChoices = newGameChoices;
        }

        public int GetQuestion(int i)
        {
            return (int)TakeBit(User.GameChoices, 10 + i * 4, 4);
        }

        public int? GetCurrentQuestion()
        {
            switch (User.Progress)
            {
                case GameProgress.Act6Q1:
                    return GetQuestion(0);
                case GameProgress.Act6Q2:
                    return GetQuestion(1);
                case GameProgress.Act6Q3:
                    return GetQuestion(2);
            }
            return null;
        }

        private readonly char[] answers = new char[] { 'A', 'C', 'A', 'A', 'A', 'C', 'C', 'B', 'A', 'B' };

        public bool CheckAnswer(int i, char choice)
        {
            int questionIndex = GetQuestion(i);
            return answers[questionIndex - 1] == choice;
        }

        static private uint TakeBit(byte[] bits, int start, int length)
        {
            if (length > 32)
            {
                throw new OverflowException();
            }
            int end = start + length;
            BitArray ba = new BitArray(bits);
            if (ba.Length < end)
            {
                return 0;
            }
            uint val = 0;
            for (int i = end - 1; i >= start; --i)
            {
                val = ((val << 1) | (ba.Get(i) ? 1u : 0));
            }
            return val;
        }

        static private void SetBit(BitArray ba, int start, int length, uint val)
        {
            int end = start + length;
            if (ba.Length < end)
            {
                ba.Length = end;
            }
            for (int i = start; i < end; ++i)
            {
                ba.Set(i, (val & 1) == 1);
                val = val >> 1;
            }
        }
    }
}
