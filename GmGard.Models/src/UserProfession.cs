using System;
using System.Collections;
using System.Collections.Generic;

namespace GmGard.Models
{
    public partial class UserQuest
    {
        public enum UserProfession
        {
            None = 0,
            Loli,
            Onee,
            Otokonoko,
            Futa,
            Shiro,
            ShiroiStocking,
            起始之光,
            双星物语,
            咸鱼之证,
            叹息之墙,
            天选之人,
            非洲人,
            lowb,
            原谅她,
            亲妈卫星,
            唱歌好听,
            大友,
            非洲提督,
            沙福林,
            此世所有之恶,
            伟大的哈里发,
            站长的老婆,
            恶臭难闻,
            欧洲人,
            王的力量,
            德国骨科,
            eden,
            斑比酱的眷属,
            欧洲提督,
            乐园的巫女,
            偷心的魔法使,
            完美潇洒的从者,
            永远鲜红的幼月,
            风之祭祀,
            魔理沙的后宫,
            七色的人偶使,
            七曜的大法师,
            恶魔之妹,
            半灵庭师,
            幽冥公主,
            十七岁的妖怪贤者,
            理想中的女主角,
            未战先败,
            欧派不是力量,
            教科书般的黑丝,
            教科书般的傲娇,
            最远的最近,
            路人女主的养成方法,
            BlessingSoftware,
            神经病凡是女人,
            那是什么暗号么,
            窥探深渊之人,
            彩虹Project,
            请叫我橘,
            没有点兔看我要死啦,
            这都怪我太可爱了,
            虹色萝莉薄,
            晓之地平线,
            埃罗奈芙老师,
            绅士之魂,
            站长在看着你,
            权限亦或苦力,
            群众里面有坏人,
            愚人节快乐,
            真相的探求者,
            资本的力量,
            轮回独断,
            永恒的恋人,
            好朋友,
            高级搜集者,
            克苏鲁机械,
            至暗の交易,
            迷影重重,
            南瓜头,
            莫比乌斯之环,
            闪光的哈萨维
        }

        private static readonly Dictionary<UserProfession, string> professionName = new Dictionary<UserProfession, string>
        {
            { UserProfession.Loli, "萝莉" },
            { UserProfession.Onee, "御姐" },
            { UserProfession.Otokonoko, "伪娘" },
            { UserProfession.Futa, "扶她" },
            { UserProfession.Shiro, "白学家" },
            { UserProfession.ShiroiStocking, "白丝萝莉" },
            { UserProfession.起始之光,   "起始之光" },
            { UserProfession.双星物语,   "双星物语" },
            { UserProfession.咸鱼之证,   "咸鱼之证" },
            { UserProfession.叹息之墙,   "叹息之墙" },
            { UserProfession.天选之人,   "天选之人" },
            { UserProfession.非洲人,     "非洲人" },
            { UserProfession.lowb,      "lowb" },
            { UserProfession.原谅她,     "原谅她" },
            { UserProfession.亲妈卫星,   "亲妈卫星" },
            { UserProfession.唱歌好听,   "唱歌好听" },
            { UserProfession.大友,       "大友" },
            { UserProfession.非洲提督,   "非洲提督" },
            { UserProfession.沙福林,     "沙福林" },
            { UserProfession.此世所有之恶,"此世所有之恶" },
            { UserProfession.伟大的哈里发,"伟大的哈里发" },
            { UserProfession.站长的老婆, "站长的老婆" },
            { UserProfession.恶臭难闻,   "恶臭难闻" },
            { UserProfession.欧洲人,   "欧洲人" },
            { UserProfession.王的力量,   "王的力量" },
            { UserProfession.德国骨科,   "德国骨科" },
            { UserProfession.eden,      "eden*" },
            { UserProfession.斑比酱的眷属,"斑比酱的眷属" },
            { UserProfession.欧洲提督,   "欧洲提督" },
            { UserProfession.乐园的巫女, "乐园的巫女" },
            { UserProfession.偷心的魔法使, "偷心的魔法使" },
            { UserProfession.完美潇洒的从者, "完美潇洒的从者" },
            { UserProfession.永远鲜红的幼月, "永远鲜红的幼月" },
            { UserProfession.风之祭祀, "风之祭祀" },
            { UserProfession.魔理沙的后宫, "魔理沙的后宫" },
            { UserProfession.七色的人偶使, "七色的人偶使" },
            { UserProfession.七曜的大法师, "七曜的大法师" },
            { UserProfession.恶魔之妹, "恶魔之妹" },
            { UserProfession.半灵庭师, "半灵庭师" },
            { UserProfession.幽冥公主, "幽冥公主" },
            { UserProfession.十七岁的妖怪贤者, "十七岁的妖怪贤者" },
            { UserProfession.理想中的女主角,     "理想中的女主角" },
            { UserProfession.未战先败,          "未战先败" },
            { UserProfession.欧派不是力量,       "欧派不是力量" },
            { UserProfession.教科书般的黑丝,     "教科书般的黑丝" },
            { UserProfession.教科书般的傲娇,     "教科书般的傲娇" },
            { UserProfession.最远的最近,         "最远的最近" },
            { UserProfession.路人女主的养成方法, "路人女主的养成方法" },
            { UserProfession.BlessingSoftware,  "Blessing Software" },
            { UserProfession.神经病凡是女人,     "神经病，凡是女人" },
            { UserProfession.那是什么暗号么,     "那是什么暗号么？" },
            { UserProfession.窥探深渊之人,       "窥探深渊之人" },
            { UserProfession.彩虹Project,       "彩虹Project" },
            { UserProfession.请叫我橘,          "请叫我橘" },
            { UserProfession.没有点兔看我要死啦, "没有点兔看我要死啦" },
            { UserProfession.这都怪我太可爱了,   "这都怪我太可爱了" },
            { UserProfession.晓之地平线, "晓之地平线" },
            { UserProfession.虹色萝莉薄,         "虹色萝莉薄" },
            { UserProfession.埃罗奈芙老师,         "埃罗奈芙老师" },
            { UserProfession.站长在看着你, "站长在看着你" },
            { UserProfession.权限亦或苦力, "权限亦或苦力？" },
            { UserProfession.群众里面有坏人, "群众里面有坏人" },
            { UserProfession.愚人节快乐, "愚人节快乐" },
            { UserProfession.真相的探求者, "真相的探求者" },
            { UserProfession.资本的力量, "资本的力量" },
            { UserProfession.绅士之魂, "绅士之魂" },
            { UserProfession.轮回独断, "轮回独断" },
            { UserProfession.永恒的恋人, "永恒的恋人" },
            { UserProfession.好朋友, "好朋友" },
            { UserProfession.高级搜集者, "高级搜集者" },
            { UserProfession.克苏鲁机械, "克苏鲁机械" },
            { UserProfession.至暗の交易, "至暗の交♂易" },
            { UserProfession.迷影重重, "迷影重重" },
            { UserProfession.南瓜头, "🎃" },
            { UserProfession.莫比乌斯之环, "莫比乌斯之环" },
            { UserProfession.闪光的哈萨维, "闪光的哈萨维" },
        };

        public static Dictionary<UserProfession, string> AllTitleBackground { get; } = new Dictionary<UserProfession, string>
        {
            { UserProfession.乐园的巫女, "乐园的巫女" },
            { UserProfession.偷心的魔法使, "偷心的魔法使" },
            { UserProfession.完美潇洒的从者, "完美潇洒的从者" },
            { UserProfession.永远鲜红的幼月, "永远鲜红的幼月" },
            { UserProfession.风之祭祀, "风之祭祀" },
            { UserProfession.魔理沙的后宫, "魔理沙的后宫" },
            { UserProfession.七色的人偶使, "七色的人偶使" },
            { UserProfession.七曜的大法师, "七曜的大法师" },
            { UserProfession.恶魔之妹, "恶魔之妹" },
            { UserProfession.半灵庭师, "半灵庭师" },
            { UserProfession.幽冥公主, "幽冥公主" },
            { UserProfession.十七岁的妖怪贤者, "十七岁的妖怪贤者" },
            { UserProfession.理想中的女主角,     "路人女主1" },
            { UserProfession.未战先败,          "路人女主1" },
            { UserProfession.欧派不是力量,       "路人女主1" },
            { UserProfession.教科书般的黑丝,     "路人女主1" },
            { UserProfession.教科书般的傲娇,     "路人女主1" },
            { UserProfession.最远的最近,         "路人女主1" },
            { UserProfession.神经病凡是女人,     "路人女主1" },
            { UserProfession.路人女主的养成方法,  "路人女主2" },
            { UserProfession.那是什么暗号么,     "那是什么暗号么" },
            { UserProfession.窥探深渊之人,       "窥探深渊之人" },
            { UserProfession.彩虹Project,       "彩虹Project" },
            { UserProfession.请叫我橘,          "请叫我橘" },
            { UserProfession.没有点兔看我要死啦, "没有点兔看我要死啦" },
            { UserProfession.这都怪我太可爱了,   "这都怪我太可爱了" },
            { UserProfession.晓之地平线,         "晓之地平线" },
            { UserProfession.虹色萝莉薄,         "虹色萝莉薄" },
            { UserProfession.轮回独断, "轮回独断" },
        };

        public static string TitleBackground(UserProfession title)
        {
            if (AllTitleBackground.TryGetValue(title, out string v))
            {
                return v;
            }
            return string.Empty;
        }

        public static string ProfessionName(UserProfession profession)
        {
            string name = string.Empty;
            professionName.TryGetValue(profession, out name);
            return name;
        }

        public static UserProfession GetProfession(string profession)
        {
            foreach (var pair in professionName)
            {
                if (profession == pair.Value)
                {
                    return pair.Key;
                }
            }
            return UserProfession.None;
        }

        public bool HasTitle(UserProfession profession)
        {
            if (profession == UserProfession.None)
            {
                return true;
            }
            if (Titles == null)
            {
                return false;
            }
            BitArray ba = new BitArray(Titles);
            if (ba.Length < (int)profession)
            {
                return false;
            }
            return ba.Get((int)profession - 1);
        }

        public List<UserProfession> AllTitles()
        {
            var titles = new List<UserProfession>();
            if (Titles == null)
            {
                return titles;
            }
            BitArray ba = new BitArray(Titles);
            for (int i = 0; i < ba.Count; i++)
            {
                if (ba.Get(i) && Enum.IsDefined(typeof(UserProfession), i + 1))
                {
                    titles.Add((UserProfession)(i + 1));
                }
            }
            return titles;
        }

        public void AddTitle(UserProfession profession)
        {
            if (Titles == null)
            {
                Titles = new byte[1];
            }
            var bitPos = (int)profession - 1;
            int len = Math.Max(Titles.Length, (bitPos + 8) / 8);
            BitArray ba = new BitArray(Titles)
            {
                Length = len * 8
            };
            ba.Set(bitPos, true);
            var newTitles = new byte[len];
            ba.CopyTo(newTitles, 0);
            Titles = newTitles;
        }
    }
}