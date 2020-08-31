import { Scenario, Effects } from "app/shared/adv-game/scenario";

type Intro = Scenario<void>;

export const August2020: Intro = {
  title: null,
  narrators: [{
    avatar: "/assets/eternal-circle/chara/L娘.png",
    name: "L娘"
  }, {
      avatar: "/assets/eternal-circle/chara/人间入间.png",
      name: "人站长"
    }, {
      avatar: "/assets/eternal-circle/chara/F酱.jpg",
      name: "F酱"
    }, {
      avatar: "/assets/eternal-circle/chara/杉崎键.png",
      name: "杉崎键"
    }, {
      avatar: "/assets/eternal-circle/chara/立华奏的粉丝.png",
      name: "立华奏的粉丝"
    }, {
      avatar: "/assets/eternal-circle/chara/里奥.jpg",
      name: "里奥"
    }, {
      avatar: "/assets/eternal-circle/chara/乌墨.jpg",
      name: "乌墨"
    }, {
      avatar: "/assets/eternal-circle/chara/真诗君.png",
      name: "真诗君"
    }, {
      avatar: "/assets/eternal-circle/chara/erowalker.png",
      name: "Erowalker"
    }, {
      avatar: "/assets/eternal-circle/chara/nazoshinshi.jpg",
      name: "众人"
    }, {
      avatar: "/assets/eternal-circle/chara/nazoshinshi.jpg",
      name: "？？？"
    },
  ],
  dialogs: [
    {
      bgImg: "/assets/eternal-circle/black.jpg",
      texts: [
        ["", "2020年的某日，绅士之庭管理室内。"],
      ]
    },
    {
      bgImg: "/assets/eternal-circle/管理室.png",
      texts: [
        ["L娘", `“已经七年了么。。。”`],
        ["", `L娘一边制作着抽奖券，一边喃喃自语着。`],
        ["人站长", `“是啊，已经过了这么长时间了。”`],
        ["", `人间一边整理着手边的文件和待审核的稿件，一边接着话，顺手把一个真人猎奇的投稿扔到了漫游火焰中。`],
        ["L娘", `“遥想当年进庭的时候，我还只是个十岁的小女孩啊。”`],
        ["人站长", `“欸。。。那现在呢？”`],
        ["L娘", `“现在已经是个十岁的女孩了。”`],
        ["人站长", `“哦，已经。。。嗯？没有长大吗？”`],
        ["L娘", `“这里倒是变大了。”`],
        ["人站长", `“啊啊啊啊！！！你这个痴女萝莉！又不穿内裤！”`],
        ["L娘", `“卟卟卟~~~！”`],
        ["杉崎键", `“早上好。。。欸，我是不是来的不是时候？”`],
        ["", `推开门后，脚还没落地的杉崎Key又悻悻的将脚缩了回去。`],
        ["人站长", `“不，你来的正是时候。快点过来帮我！我要被侵犯了！”`],
        ["杉崎键", `“诶，诶。。。。”`],
        ["", `十数分钟后，各位管理都来到了管理室内。那之中还出现了一个熟悉的身影——`],
        ["众人", `“Erowalker！E老前辈怎么也来了？”`],
        ["", `新人管理们拜见初代目，对老前辈到来的原因交头接耳着。`],
        ["Erowalker", `“不用紧张，近期地外行星管理混乱，我只是来这里一起参加七周年的准备工作顺便暂时避险而已。”`],
        ["人站长", `“哦哦。。。”`],
        ["", `E老站长握着人站长的手`],
        ["Erowalker", `“小人啊，你也是个出色的站长了。”`],
        ["人站长", `“。。。谢谢，就是这个称呼不太舒服。”`],
        ["Erowalker", `“恩。。。好像是有点，那就。。。小间？”`],
        ["人站长", `“您就正常的叫人站长好吧。”`],
        ["Erowalker", `“。。。啊啊，现在，你是站长了啊。”`],
        ["", `E老站长一副怅然若失的样子。`],
      ]
    },
    {
      bgImg: "/assets/eternal-circle/black.jpg",
      texts: [
        ["", `突然，几声脆响，管理室变的一片漆黑。`],
        ["？？？", `“啊啊啊啊啊啊啊啊啊啊！！！”`],
        ["", `然后是一声凄厉的惨叫。`],
        ["杉崎键", `“什么！发生什么了！”`],
        ["真诗君", `“好像是灯泡被打碎了！”`],
        ["立华奏的粉丝", `“我这里有手机！”`],
        ["F酱", `“快让火焰烧的旺一点！”`],
      ],
      effect: [{ pos: 1, kind: Effects.SHAKE }],
    },
    {
      bgImg: "/assets/eternal-circle/管理室.png",
      texts: [
        ["", `管理室内恢复了光明。
管理们四下环视。`],
        ["林檎", `“人站长呢？人站长怎么不见了？”`],
        ["人站长", `“我在。。。。这。。。。”`],
      ],
    },
    {
      bgImg: "/assets/eternal-circle/管理室-金玉.jpg",
      texts: [
        ["", `人站长蜷缩在地上捂着下体，带着马赛克的金色物体静静的躺在他的面前。`],
        ["", `那是，人间的蛋蛋。它被放在了人间的视线的正前方。`],
        ["L娘", `“呜哇。。。。好惨。”`],
        ["里奥", `“接回去还能用么？”`],
        ["L娘", `“等等，这是。。。”`],
        ["", `L娘捡起了人间的蛋蛋，上面刻着四个大字：<span style="color:red">反 人 复 E</span>`],
        ["L娘", `“果然。。。今年的周年庆也不会和平啊。”`]
      ]
    },
    {
      bgImg: "/assets/eternal-circle/black.jpg",
      texts: [
        ["", `异变，果然还是到来了。`],
        ["", `但是，异变才刚刚开始！`],
      ]
    },
    {
      bgImg: "/assets/eternal-circle/研究室.png",
      texts: [
        ["", `地下室又传来了轰鸣声，在刚才的混乱中，抽卡姬，算法娘和服务器娘被连接在了一起，能量的碰撞产生了强大的冲击波。
在一声“我操你大爷的又搞我服务器”的充满悲痛的怒吼声中，众人都被能量波吞噬了。`],
      ]
    },
    {
      bgImg: "/assets/eternal-circle/white.png",
      texts: [
        ["", `。。。<br>。。。`],
        ["L娘", `“呜呜。。这是？什，什么情况？！”`]
      ],
    },
    {
      bgImg: "/assets/eternal-circle/floor.png",
      texts: [
        ["", `醒来的L娘发现自己的身体被封印进入了卡牌之中，不，不止是自己，管理员全员都被封印进入了卡牌之中。`],
        ["L娘", `“叛徒。。。一定就在我们之中。”`],
        ["", `L娘看着地上躺着的管理员卡牌们说道。`],
      ],
    },
    {
      bgImg: "/assets/eternal-circle/black.jpg",
      texts: [
        ["", `顺便一说，人间的蛋蛋没有变成卡牌。
是变成了抽奖券。`],
        ["", `在七周年之际，引起异变，夺人蛋蛋，意图反人复E的究竟是谁？收集众人的证词（卡牌描述）从中寻找出线索与答案吧！同时收集人间的蛋蛋来获取奖品抽取权，真相。。。蛋落实出！`],
        ["", `七周年特异点——只人：蛋落X度。`],
        ["乌墨", `“顺便一问人间的蛋蛋有那么多的吗？”`],
        ["", `因为L娘有践踏人间的下体的习惯所以进化出了无限再生的功能所以没关系。嘛，只不过还是会很痛就对了。`],
        ["乌墨", `“好过分！”`],
      ]
    },
  ]
}
