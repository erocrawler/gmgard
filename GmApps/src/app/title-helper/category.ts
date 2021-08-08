export class Category {
  id: number;
  name: string;
  fields: CategoryField[];
  comment: string;
}

export class CategoryField {
  name: string;
  required: boolean;
  fixed?: boolean;
  default?: string;
  format?: (input: string) => string;
  hint?: string;
}

function BracketFormat(input: string) {
  return `[${input}]`;
}

function RroundBracketFormat(input: string) {
  return `(${input})`;
}

export const CATEGORIES: Category[] = [
  {
    id: 10, name: "资讯", fields: [],
    comment: `资讯分类没有固定格式要求。资讯分类接受的投稿包括：
                  新番动画、里番动画、漫画、galgame、轻小说等二次元物的介绍/推荐，二次元新闻等。
                  资讯类投稿均要求原创内容，转载二次元新闻等请注明转载，并加上自己的吐槽与评论。
                  注：R18类资讯请加上“工口物介绍”标签。`
  },
  {
    id: 11, name: "站务", fields: [],
    comment: "站务公告用分类。"
  },
  {
    id: 12, name: "心得感想", fields: [],
    comment: "没有固定格式要求。心得感想分类接受的投稿包括：ACG作品相关感想，二次元游戏攻略、心得等。"
  },
  {
    id: 13, name: "工具", fields: [],
    comment: "没有固定格式要求。工具分类接受的投稿包括：二次元工具软件/网站教程等。"
  },
  {
    id: 14, name: "商业动画", fields: [
      { name: "字幕组名称", required: false, hint: "没有可填“生肉”", format: BracketFormat },
      { name: "制作公司", required: true, format: BracketFormat },
      { name: "原标题", required: true, hint: "日文原名" },
    ],
    comment: "由于商业动画分类只包括以动画形式发售的商业作品资源，商业游戏的提取动画或录制动画等请投到商业作CG分类。",
  },
  {
    id: 39, name: "同人贩卖动画", fields: [
      { name: "提取动画", fixed: true, required: false, format: () => "[提取动画]" },
      { name: "字幕组名称", required: false, format: BracketFormat },
      { name: "RJ号", required: true, hint: "也可填dmm编号或“无RJ号”", format: BracketFormat },
      { name: "制作组", required: true, format: BracketFormat },
      { name: "原标题", required: true, hint: "日文原名" },
    ],
    comment: "本分类接受DLsite、FANZA同人等平台以单品形式发售的动画投稿。当制作组同时在上述两个平台发售作品时，优先以RJ号进行标题命名。同人贩卖作品的动画提取也归于本分类，标题需在前面标注[提取动画]。",
  },
  {
    id: 40, name: "同人粉丝动画", fields: [
      { name: "制作者名", required: true, format: BracketFormat },
      { name: "题材", required: true, hint: "角色的出处，多出处则填“多同人”；无出处则填“原创”", format: (v) => ` ${v} ` },
      { name: "原标题", required: true, hint: "日文原名" },
      { name: "收录日期区间", required: false, hint: "投合集时，请填写收录作品的时间", format: BracketFormat },
      { name: "来源平台名", required: true, format: BracketFormat },
    ],
    comment: `本分类接受各类免费公开或赞助解锁平台以投放形式发布的动画投稿。
包括但不限于Twitter、Pixiv、Fanbox、Fantia、Patreon、Subscribestar。
同个作品若有多种画质，若已存在赞助版，则不接受免费版或更低赞助档位版本投稿。
本分类接受欧美物，但不接受瞎眼重口/血腥猎奇的作品。（最终解释权归本分类管理所有）
本分类不接受时长少于1分钟的单部作品，专做循环&小短篇内容的创作者只接受合集投稿。
MMD形式的粉丝动画优先投至MMD分类。`,
  },
  {
    id: 16, name: "MMD", fields: [
      { name: "MMD", fixed: true, required: true, default: "MMD", format: BracketFormat },
      { name: "原标题", required: true, hint: "日文原名" },
      { name: "制作者名", required: true, format: (v) => `(by ${v})` },
    ],
    comment: "MMD分类不接受只有磁链或种子的投稿。",
  },
  {
    id: 17, name: "表番", fields: [
      { name: "合集", fixed: true, required: false, format: () => "(合集)" },
      { name: "字幕组名称", required: false, format: BracketFormat },
      { name: "原标题", required: true, hint: "如果是多季番要标出第几季" },
      { name: "话数", required: true, format: BracketFormat },
      { name: "剧场版", fixed: true, required: false, format: () => "[剧场版]" },
      { name: "字幕语言", required: true, hint: "简体为[GB]，繁体为[BIG5]", format: BracketFormat },
      { name: "分辨率", required: true, format: BracketFormat },
      { name: "文件格式", required: true, format: BracketFormat },
    ],
    comment: `表番类动画只接受完整合集（至少一季），不接受单话投稿，亦不接受只有磁链或种子的投稿。
                  另外擦边球类动画请投到商业动画分类（投稿格式和要求仍与表番类相同）
                  注：不接受中国大陆的商业动画的投稿，亦不接受在中国大陆正式上映过的剧场版动画投稿。`
  },
  {
    id: 18, name: "商业CG", fields: [
      { name: "提取CG", fixed: true, required: true, default: "提取CG", format: BracketFormat },
      { name: "发售日期", required: true, format: BracketFormat },
      { name: "制作公司", required: true, format: BracketFormat },
      { name: "原标题", required: true, hint: "日文原名" },
    ],
    comment: "CG分类不接受只有磁链或种子的投稿。",
  },
  {
    id: 19, name: "同人CG", fields: [
      { name: "提取CG", fixed: true, required: false, format: () => "[提取CG]" },
      { name: "汉化者", required: false, format: BracketFormat },
      { name: "RJ号", required: true, hint: "可填“无RJ号”", format: BracketFormat },
      { name: "制作者名", required: true, format: BracketFormat },
      { name: "原标题", required: true, hint: "日文原名" },
    ],
    comment: "CG分类不接受只有磁链或种子的投稿。",
  },
  {
    id: 20, name: "商业作游戏", fields: [
      { name: "发售日期", required: true, format: BracketFormat },
      { name: "制作公司", required: true, format: BracketFormat },
      { name: "原标题", required: true, hint: "日文原名" },
    ],
    comment: "游戏分类不接受的投稿种类：网游，含有联网内容的手游，在中国大陆正式发售的游戏（包括含内购收费内容的免费游戏）。"
  },
  {
    id: 21, name: "同人游戏", fields: [
      { name: "RJ号", required: true, hint: "可填“无RJ号”", format: BracketFormat },
      { name: "制作组", required: true, format: BracketFormat },
      { name: "原标题", required: true, hint: "日文原名" },
    ],
    comment: "游戏分类不接受的投稿种类：网游，含有联网内容的手游，在中国大陆正式发售的游戏（包括含内购收费内容的免费游戏）。"
  },
  {
    id: 22, name: "全年龄游戏", fields: [
      { name: "发售日期或RJ号", required: true, format: BracketFormat },
      { name: "制作公司", required: true, format: BracketFormat },
      { name: "原标题", required: true, hint: "日文原名" },
    ],
    comment: "游戏分类不接受的投稿种类：网游，含有联网内容的手游，在中国大陆正式发售的游戏（包括含内购收费内容的免费游戏）。"
  },
  {
    id: 36, name: "补丁存档", fields: [
      { name: "发售日期或RJ号", required: true, format: BracketFormat },
      { name: "制作公司", required: true, format: BracketFormat },
      { name: "原标题", required: true, hint: "日文原名" },
    ],
    comment: "游戏分类不接受的投稿种类：网游，含有联网内容的手游，在中国大陆正式发售的游戏（包括含内购收费内容的免费游戏）。"
  },
  {
    id: 37, name: "人物卡", fields: [],
    comment: "I社游戏（恋活，AI少女，Honey Select等），或其他游戏的自定义人物存档分享。"
  },
  {
    id: 23, name: "同人志", fields: [
      { name: "汉化者", required: false, format: BracketFormat },
      { name: "展会名", required: true, format: RroundBracketFormat },
      { name: "社团或作者名", required: true, format: BracketFormat },
      { name: "原标题", required: true, hint: "日文原名" },
      { name: "题材", required: false, format: RroundBracketFormat },
    ],
    comment: "漫画分类不接受只有磁链或种子的投稿。汉化本必须标明汉化者，若找不到汉化者信息且本中没有注明汉化者则标[汉化者不明]而不是[中国翻訳]。对于只在DLsite等网站发售了电子版，而没在展会贩卖过的本子请写RJ号"
  },
  {
    id: 24, name: "单行本", fields: [
      { name: "汉化者", required: false, format: BracketFormat },
      { name: "扫图者", required: false, format: BracketFormat },
      { name: "社团或作者名", required: true, format: BracketFormat },
      { name: "原标题", required: true, hint: "日文原名" },
    ],
    comment: "漫画分类不接受只有磁链或种子的投稿。非民间汉化的中文版单行本最前边标[中文]而不是[中国翻訳]，如果有扫图者信息的话在[中文]后边标出[扫图者]。"
  },
  {
    id: 25, name: "杂志", fields: [
      { name: "整本杂志", required: false, fixed: true, format: () => "(成年コミック・雑誌)" },
      { name: "汉化者", required: false, format: BracketFormat },
      { name: "扫图者", required: false, format: BracketFormat },
      { name: "杂志名", required: true, format: RroundBracketFormat },
      { name: "社团或作者名", required: true, format: BracketFormat },
      { name: "原标题", required: true, hint: "日文原名" },
    ],
    comment: "漫画分类不接受只有磁链或种子的投稿。非民间汉化的中文版单行本最前边标[中文]而不是[中国翻訳]，如果有扫图者信息的话在[中文]后边标出[扫图者]。"
  },
  {
    id: 26, name: "全年龄漫画", fields: [
      { name: "汉化者", required: false, format: BracketFormat },
      { name: "展会或杂志名", required: true, format: RroundBracketFormat },
      { name: "社团或作者名", required: true, format: BracketFormat },
      { name: "原标题", required: true, hint: "日文原名" },
      { name: "题材", required: false, format: RroundBracketFormat },
    ],
    comment: "漫画分类不接受只有磁链或种子的投稿。另请参考同人志、单行本、杂志的格式。"
  },
  {
    id: 27, name: "工口画集", fields: [
      { name: "汉化者", required: false, format: BracketFormat },
      { name: "P站ID", required: false, format: BracketFormat },
      { name: "展会或杂志名", required: true, format: RroundBracketFormat },
      { name: "社团/作者/画师名", required: true, format: BracketFormat },
      { name: "原标题", required: true, hint: "日文原名" },
      { name: "题材", required: false, format: RroundBracketFormat },
    ],
    comment: "商业画集只需要写明画集名称即可。P站画集请在文中附上P站对应链接。图包类需要有一个画师/社团类的主题，且图包中图片数量不少于50。不接受只有磁链或种子的投稿。"
  },
  {
    id: 28, name: "全年龄画集", fields: [
      { name: "汉化者", required: false, format: BracketFormat },
      { name: "P站ID", required: false, format: BracketFormat },
      { name: "展会或杂志名", required: true, format: RroundBracketFormat },
      { name: "社团/作者/画师名", required: true, format: BracketFormat },
      { name: "原标题", required: true, hint: "日文原名" },
      { name: "题材", required: false, format: RroundBracketFormat },
    ],
    comment: "商业画集只需要写明画集名称即可。P站画集请在文中附上P站对应链接。图包类需要有一个画师/社团类的主题，且图包中图片数量不少于50。不接受只有磁链或种子的投稿。"
  },
  {
    id: 29, name: "音乐", fields: [
      { name: "发售日期", required: false, hint: "同人音乐请填展会名", format: BracketFormat },
      { name: "展会名", required: false, format: RroundBracketFormat },
      { name: "同人音乐", required: false, fixed: true, format: () => "(同人音乐)" },
      { name: "制作组", required: false, hint: "仅同人音乐", format: BracketFormat },
      { name: "原标题", required: true, hint: "日文原名" },
      { name: "文件格式", required: true, hint: "如flac+cue，320kmp3等", format: RroundBracketFormat },
    ],
    comment: "本站只接受ACG相关音乐。正文中请至少列出曲目表。不接受只有磁链或种子的投稿。"
  },
  {
    id: 30, name: "同人音声", fields: [
      { name: "RJ号", required: true, hint: "可填“无RJ号”", format: BracketFormat },
      { name: "同人音声", required: true, fixed: true, default: "同人音声", format: RroundBracketFormat },
      { name: "制作组", required: false, hint: "仅同人音乐", format: BracketFormat },
      { name: "原标题", required: true, hint: "日文原名" },
    ],
    comment: "不接受只有磁链或种子的投稿。"
  },
  {
    id: 31, name: "官能小说", fields: [
      { name: "版本", required: true, hint: "汉化组/台版，原版请写生肉", format: BracketFormat },
      { name: "18禁小説", required: true, fixed: true, default: "18禁小説", format: RroundBracketFormat },
      { name: "作者", required: true, format: BracketFormat },
      { name: "原标题", required: true, hint: "日文原名" },
      { name: "卷数", required: false, format: RroundBracketFormat },
      { name: "文件格式", required: true, hint: "txt pdf jpg png等", format: RroundBracketFormat },
    ],
    comment: "不接受只有磁链或种子的投稿。只接受轻小说资源的投稿，小说资源请在压缩包中附上小说插图。"
  },
  {
    id: 32, name: "站友原创", fields: [],
    comment: "没有格式要求。原创小说分类请将小说内容直接贴在正文，并确保所发内容不是完全二次元无关。"
  },
  {
    id: 33, name: "一般小说", fields: [
      { name: "版本", required: true, hint: "汉化组/台版，原版请写生肉", format: BracketFormat },
      { name: "一般小说", required: true, fixed: true, default: "一般小说" },
      { name: "作者", required: true, format: BracketFormat },
      { name: "原标题", required: true, hint: "日文原名" },
      { name: "卷数", required: false, format: RroundBracketFormat },
      { name: "文件格式", required: true, hint: "txt pdf jpg png等", format: RroundBracketFormat },
    ],
    comment: "不接受只有磁链或种子的投稿。只接受轻小说资源的投稿，小说资源请在压缩包中附上小说插图。"
  },
  {
    id: 34, name: "一般绘画", fields: [],
    comment: "没有格式要求。绘画分类接受各种原创绘画的投稿（临摹也可以，只要是自己画的）。"
  },
  {
    id: 35, name: "工口绘画", fields: [],
    comment: "没有格式要求。绘画分类接受各种原创绘画的投稿（临摹也可以，只要是自己画的）。"
  },
]
