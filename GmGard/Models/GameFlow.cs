using GmGard.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Entity;

namespace GmGard.Models
{
    public abstract class GameFlow
    {
        protected UserQuest User { get; set; }
        protected UserQuest.GameProgress CurrentProgress { get; set; }
        protected UserQuest.GameProgress PreviousProgress { get; set; }

        public virtual bool CheckPrecondition()
        {
            return User.Progress == PreviousProgress && !User.IsDead;
        }

        public virtual bool ProcessFlow()
        {
            User.Progress = CurrentProgress;
            return true;
        }

        public virtual object JsonResponse()
        {
            return new object { };
        }

        protected GameChoiceDescriptor GetChoiceDescriptor()
        {
            return new GameChoiceDescriptor(User);
        }
    }

    public abstract class ActStartFlow : GameFlow
    {
        public ActStartFlow(UserQuest user)
        {
            User = user;
        }

        public override object JsonResponse()
        {
            return new {
                isdead = User.IsDead,
                deathcount = User.DeathCount,
                points = User.user.Points
            };
        }
    }

    public abstract class ActChooseFlow : GameFlow
    {
        protected string Choice { get; set; }
        protected GameChoiceDescriptor Descriptor { get; set; }

        protected abstract bool SetChoice();

        public ActChooseFlow(UserQuest user, string choice)
        {
            User = user;
            Choice = choice;
            Descriptor = GetChoiceDescriptor();
        }

        public override bool ProcessFlow()
        {
            if (!SetChoice())
            {
                return false;
            }
            User.Progress = CurrentProgress;
            return true;
        }

        public override object JsonResponse()
        {
            return new
            {
                success = true,
                isdead = User.IsDead,
                deathcount = User.DeathCount
            };
        }
    }

    public class Act1BeforeChoose : ActStartFlow
    {
        public Act1BeforeChoose(UserQuest user)
            : base(user)
        {
            CurrentProgress = UserQuest.GameProgress.Act1BeforeChoose;
            PreviousProgress = UserQuest.GameProgress.None;
        }
    }

    public class Act2Start : ActStartFlow
    {
        public Act2Start(UserQuest user)
            : base(user)
        {
            CurrentProgress = UserQuest.GameProgress.Act2Start;
            PreviousProgress = UserQuest.GameProgress.Act1AfterChoose;
        }

        public override bool ProcessFlow()
        {
            if (User.Profession == UserQuest.UserProfession.Shiro)
            {
                // 打死白学家
                User.SetDead();
            }
            return base.ProcessFlow();
        }
    }

    public class Act3Start : ActStartFlow
    {
        private bool _isRandom = false;

        public Act3Start(UserQuest user)
            : base(user)
        {
            CurrentProgress = UserQuest.GameProgress.Act3Start;
            PreviousProgress = UserQuest.GameProgress.Act2AfterChoose;
        }

        public override bool ProcessFlow()
        {
            var choice = GetChoiceDescriptor().GetAct2Choice();
            // Random Event
            int seed = new Random().Next(100);
            _isRandom = seed >= 30 && seed < 50;
            if (_isRandom) // 20% range
            {
                User.SetDead();
            }
            else if ((User.Profession == UserQuest.UserProfession.Otokonoko || User.Profession == UserQuest.UserProfession.Futa) && choice == "yes")
            {
                // 伪娘, 扶她职业：选否 存活
                User.SetDead();
            }
            else if (User.Profession == UserQuest.UserProfession.Loli && choice == "no")
            {
                // 萝莉职业：选是 存活
                User.SetDead();
            }
            return base.ProcessFlow();
        }

        public override object JsonResponse()
        {
            return new
            {
                isdead = User.IsDead,
                deathcount = User.DeathCount,
                israndom = _isRandom,
                points = User.user.Points
            };
        }
    }

    public class Act4Start : ActStartFlow
    {
        public Act4Start(UserQuest user)
            : base(user)
        {
            CurrentProgress = UserQuest.GameProgress.Act4Start;
            PreviousProgress = UserQuest.GameProgress.Act3AfterChoose;
        }

        public override bool ProcessFlow()
        {
            string choice = GetChoiceDescriptor().GetAct3Choice();
            if ((User.Profession == UserQuest.UserProfession.Onee || User.Profession == UserQuest.UserProfession.Loli)
                && choice == "stay")
            {
                // 萝莉，御姐：选留下死亡
                User.SetDead();
            }
            else if (choice == "attack")
            {
                // 选择攻击：死亡
                User.SetDead();
            }
            return base.ProcessFlow();
        }
    }

    public class Act5Start : ActStartFlow
    {
        public Act5Start(UserQuest user)
            : base(user)
        {
            CurrentProgress = UserQuest.GameProgress.Act5Start;
            PreviousProgress = UserQuest.GameProgress.Act4AfterChoose;
        }

        public override bool ProcessFlow()
        {
            string choice = GetChoiceDescriptor().GetAct4Choice();
            if (choice == "unknow")
            {
                User.SetDead();
            }
            else if ((User.Profession == UserQuest.UserProfession.Futa || User.Profession == UserQuest.UserProfession.Onee)
                && choice == "left")
            {
                // 御姐，扶她：选左死亡
                User.SetDead();
            }

            if (User.Profession == UserQuest.UserProfession.Futa)
            {
                User.Progress = UserQuest.GameProgress.Act5Start;
            }
            else
            {
                User.Progress = (choice == "right") ? UserQuest.GameProgress.Act5StartRight : UserQuest.GameProgress.Act5Start;
            }
            return true;
        }
    }

    public class Act5Extra : ActChooseFlow
    {
        public Act5Extra(UserQuest user, string choice)
            : base(user, choice)
        {
            CurrentProgress = UserQuest.GameProgress.Act5Start;
            PreviousProgress = UserQuest.GameProgress.Act5StartRight;
        }

        public override bool ProcessFlow()
        {
            if ((User.Profession != UserQuest.UserProfession.Futa
                && User.Profession != UserQuest.UserProfession.Onee) || Choice == "no")
            {
                User.SetDead();
            }
            return base.ProcessFlow();
        }

        protected override bool SetChoice()
        {
            return true;
        }

    }

    public class Act5BeforeChoose : ActStartFlow
    {
        public Act5BeforeChoose(UserQuest user)
            : base(user)
        {
            CurrentProgress = UserQuest.GameProgress.Act5BeforeChoose;
            PreviousProgress = UserQuest.GameProgress.Act5Start;
        }
    }

    public class Act5bStart : ActStartFlow
    {
        public Act5bStart(UserQuest user)
            : base(user)
        {
            CurrentProgress = UserQuest.GameProgress.Act5bStart;
            PreviousProgress = UserQuest.GameProgress.Act5AfterChoose;
        }

        public override bool ProcessFlow()
        {
            if (GetChoiceDescriptor().GetAct5Choice() != "gan")
            {
                User.SetDead();
            }
            return base.ProcessFlow();
        }
    }

    public class Act1Choose : ActChooseFlow
    {
        private Dictionary<string, int> _professionStats;

        public Act1Choose(UserQuest user, string choice, Dictionary<string, int> ProfessionStats)
            : base(user, choice)
        {
            CurrentProgress = UserQuest.GameProgress.Act1AfterChoose;
            PreviousProgress = UserQuest.GameProgress.Act1BeforeChoose;
            _professionStats = ProfessionStats;
        }

        protected override bool SetChoice()
        {
            var profession = UserQuest.GetProfession(Choice);
            if (profession == UserQuest.UserProfession.None)
            {
                return false;
            }
            User.Profession = profession;
            User.AddTitle(profession);
            int count = 0;
            string key = profession.ToString().ToLower();
            _professionStats.TryGetValue(key, out count);
            _professionStats[key] = count + 1;
            return true;
        }

        public override object JsonResponse()
        {
            return new
            {
                success = true,
                isdead = User.IsDead,
                deathcount = User.DeathCount,
                stats = _professionStats,
            };
        }
    }

    public class Act2Choose : ActChooseFlow
    {
        public Act2Choose(UserQuest user, string choice) 
            : base(user, choice)
        {
            CurrentProgress = UserQuest.GameProgress.Act2AfterChoose;
            PreviousProgress = UserQuest.GameProgress.Act2Start;
        }

        protected override bool SetChoice()
        {
            Descriptor.SetAct2Choice(Choice);
            return true;
        }
    }

    public class Act3Choose : ActChooseFlow
    {
        public Act3Choose(UserQuest user, string choice)
            : base(user, choice)
        {
            CurrentProgress = UserQuest.GameProgress.Act3AfterChoose;
            PreviousProgress = UserQuest.GameProgress.Act3Start;
        }

        protected override bool SetChoice()
        {
            return Choice != null && Descriptor.SetAct3Choice(Choice);
        }
    }

    public class Act4Choose : ActChooseFlow
    {
        public Act4Choose(UserQuest user, string choice)
            : base(user, choice)
        {
            CurrentProgress = UserQuest.GameProgress.Act4AfterChoose;
            PreviousProgress = UserQuest.GameProgress.Act4Start;
        }

        protected override bool SetChoice()
        {
            return Choice != null && Descriptor.SetAct4Choice(Choice);
        }
    }

    public class Act5Choose : ActChooseFlow
    {
        public Act5Choose(UserQuest user, string choice)
            : base(user, choice)
        {
            CurrentProgress = UserQuest.GameProgress.Act5AfterChoose;
            PreviousProgress = UserQuest.GameProgress.Act5BeforeChoose;
        }

        protected override bool SetChoice()
        {
            return Choice != null && Descriptor.SetAct5Choice(Choice);
        }
    }

    public class Act5bChoose : ActChooseFlow
    {
        public Act5bChoose(UserQuest user, string choice)
            : base(user, choice)
        {
            CurrentProgress = UserQuest.GameProgress.Act5bAfterChoose;
            PreviousProgress = UserQuest.GameProgress.Act5bStart;
        }

        protected override bool SetChoice()
        {
            return Choice != null && Descriptor.SetAct5bChoice(Choice);
        }
    }

    public class Act6Start : ActStartFlow
    {
        public Act6Start(UserQuest user)
            : base(user)
        {
            CurrentProgress = UserQuest.GameProgress.Act6Start;
            PreviousProgress = UserQuest.GameProgress.Act5bAfterChoose;
        }

        public override bool ProcessFlow()
        {
            if (GetChoiceDescriptor().GetAct5bChoice() != "think")
            {
                User.SetDead();
            }
            return base.ProcessFlow();
        }
    }

    public class Act6Choose : ActChooseFlow
    {
        public Act6Choose(UserQuest user, string choice)
            : base(user, choice)
        {
            CurrentProgress = UserQuest.GameProgress.Act6AfterChoose;
            PreviousProgress = UserQuest.GameProgress.Act6Start;
        }

        protected override bool SetChoice()
        {
            return Choice != null && Descriptor.SetAct6Choice(Choice);
        }
    }

    public class Act6Q1 : ActStartFlow
    {
        public Act6Q1(UserQuest user)
            : base(user)
        {
            CurrentProgress = UserQuest.GameProgress.Act6Q1;
            PreviousProgress = UserQuest.GameProgress.Act6AfterChoose;
        }

        public override bool ProcessFlow()
        {
            GetChoiceDescriptor().GenerateQuestions();
            return base.ProcessFlow();
        }

        public override object JsonResponse()
        {
            return new {
                question = GetChoiceDescriptor().GetQuestion(0)
            };
        }
    }

    public class Act6Q2 : ActChooseFlow
    {
        public Act6Q2(UserQuest user, string choice)
            : base(user, choice)
        {
            CurrentProgress = UserQuest.GameProgress.Act6Q2;
            PreviousProgress = UserQuest.GameProgress.Act6Q1;
        }

        public override object JsonResponse()
        {
            return new
            {
                question = Descriptor.GetQuestion(1),
                correct = Descriptor.CheckAnswer(0, Choice.ElementAt(0))
            };
        }

        protected override bool SetChoice()
        {
            if (string.IsNullOrEmpty(Choice))
            {
                return false;
            }
            if (!Descriptor.CheckAnswer(0, Choice.ElementAt(0))) {
                User.SetDead();
            }
            return true;
        }
    }

    public class Act6Q3 : ActChooseFlow
    {
        public Act6Q3(UserQuest user, string choice)
            : base(user, choice)
        {
            CurrentProgress = UserQuest.GameProgress.Act6Q3;
            PreviousProgress = UserQuest.GameProgress.Act6Q2;
        }

        public override object JsonResponse()
        {
            return new
            {
                question = Descriptor.GetQuestion(2),
                correct = Descriptor.CheckAnswer(1, Choice.ElementAt(0))
            };
        }

        protected override bool SetChoice()
        {
            if (string.IsNullOrEmpty(Choice))
            {
                return false;
            }
            if (!Descriptor.CheckAnswer(1, Choice.ElementAt(0)))
            {
                User.SetDead();
            }
            return true;
        }
    }

    public class Act6AfterQ : ActChooseFlow
    {
        public Act6AfterQ(UserQuest user, string choice)
            : base(user, choice)
        {
            CurrentProgress = UserQuest.GameProgress.Act6AfterQ;
            PreviousProgress = UserQuest.GameProgress.Act6Q3;
        }

        public override object JsonResponse()
        {
            return new
            {
                correct = Descriptor.CheckAnswer(2, Choice.ElementAt(0))
            };
        }

        protected override bool SetChoice()
        {
            if (string.IsNullOrEmpty(Choice))
            {
                return false;
            }
            if (!Descriptor.CheckAnswer(2, Choice.ElementAt(0)))
            {
                User.SetDead();
            }
            return true;
        }
    }

    public class Act6Leave : ActStartFlow
    {
        public Act6Leave(UserQuest user)
            : base(user)
        {
            CurrentProgress = UserQuest.GameProgress.Act6Leave;
            PreviousProgress = UserQuest.GameProgress.Act6AfterQ;
        }
    }

    public class Act6Stay : ActChooseFlow
    {
        public Act6Stay(UserQuest user, string choice)
            : base(user, choice)
        {
            CurrentProgress = UserQuest.GameProgress.Act6Stay;
            PreviousProgress = UserQuest.GameProgress.Act6AfterQ;
        }

        protected override bool SetChoice()
        {
            var profession = UserQuest.GetProfession(Choice);
            if (profession != UserQuest.UserProfession.None)
            {
                User.Profession = profession;
            }
            if (User.Profession == UserQuest.UserProfession.Loli && Descriptor.GetAct6Choice() == "true")
            {
                User.AddTitle(UserQuest.UserProfession.ShiroiStocking);
            }
            return true;
        }
    }
}
