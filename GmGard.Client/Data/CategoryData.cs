namespace GmGard.Client.Data;

public static class CategoryData
{
    private static string BracketFormat(string input) => $"[{input}]";
    private static string RoundBracketFormat(string input) => $"({input})";

    public static List<TitleCategory> GetAllCategories() => new()
    {
        new() { Id = 10, Name = "资讯", Fields = new(), Comment = "资讯分类没有固定格式要求。资讯分类接受的投稿包括：新番动画、里番动画、漫画、galgame、轻小说等二次元物的介绍/推荐，二次元新闻等。资讯类投稿均要求原创内容，转载二次元新闻等请注明转载，并加上自己的吐槽与评论。注：R18类资讯请加上\"工口物介绍\"标签。" },
        new() { Id = 11, Name = "站务", Fields = new(), Comment = "站务公告用分类。" },
        new() { Id = 12, Name = "心得感想", Fields = new(), Comment = "没有固定格式要求。心得感想分类接受的投稿包括：ACG作品相关感想，二次元游戏攻略、心得等。" },
        new() { Id = 13, Name = "工具", Fields = new(), Comment = "没有固定格式要求。工具分类接受的投稿包括：二次元工具软件/网站教程等。" },
        new() { Id = 14, Name = "商业动画", Fields = new()
        {
            new() { Name = "字幕组名称", Required = false, Hint = "没有可填\"生肉\"", Format = BracketFormat },
            new() { Name = "制作公司", Required = true, Format = BracketFormat },
            new() { Name = "原标题", Required = true, Hint = "日文原名" },
        }, Comment = "由于商业动画分类只包括以动画形式发售的商业作品资源，商业游戏的提取动画或录制动画等请投到商业作CG分类。" },
        new() { Id = 39, Name = "同人贩卖动画", Fields = new()
        {
            new() { Name = "提取动画", Fixed = true, Required = false, Format = _ => "[提取动画]" },
            new() { Name = "字幕组名称", Required = false, Format = BracketFormat },
            new() { Name = "RJ号", Required = true, Hint = "也可填dmm编号或\"无RJ号\"", Format = BracketFormat },
            new() { Name = "制作组", Required = true, Format = BracketFormat },
            new() { Name = "原标题", Required = true, Hint = "日文原名" },
        }, Comment = "本分类接受DLsite、FANZA同人等平台以单品形式发售的动画投稿。当制作组同时在上述两个平台发售作品时，优先以RJ号进行标题命名。同人贩卖作品的动画提取也归于本分类，标题需在前面标注[提取动画]。" },
        new() { Id = 40, Name = "同人粉丝动画", Fields = new()
        {
            new() { Name = "制作者名", Required = true, Format = BracketFormat },
            new() { Name = "题材", Required = true, Hint = "角色的出处，多出处则填\"多同人\"；无出处则填\"原创\"", Format = v => $" {v} " },
            new() { Name = "原标题", Required = true, Hint = "日文原名" },
            new() { Name = "收录日期区间", Required = false, Hint = "投合集时，请填写收录作品的时间", Format = BracketFormat },
            new() { Name = "来源平台名", Required = true, Format = BracketFormat },
        }, Comment = "本分类接受各类免费公开或赞助解锁平台以投放形式发布的动画投稿。包括但不限于Twitter、Pixiv、Fanbox、Fantia、Patreon、Subscribestar。同个作品若有多种画质，若已存在赞助版，则不接受免费版或更低赞助档位版本投稿。本分类接受欧美物，但不接受瞎眼重口/血腥猎奇的作品。（最终解释权归本分类管理所有）本分类不接受时长少于1分钟的单部作品，专做循环&小短篇内容的创作者只接受合集投稿。MMD形式的粉丝动画优先投至MMD分类。" },
        new() { Id = 16, Name = "MMD", Fields = new()
        {
            new() { Name = "MMD", Fixed = true, Required = true, Default = "MMD", Format = BracketFormat },
            new() { Name = "原标题", Required = true, Hint = "日文原名" },
            new() { Name = "制作者名", Required = true, Format = v => $"(by {v})" },
        }, Comment = "MMD分类不接受只有磁链或种子的投稿。" },
        new() { Id = 17, Name = "表番", Fields = new()
        {
            new() { Name = "合集", Fixed = true, Required = false, Format = _ => "(合集)" },
            new() { Name = "字幕组名称", Required = false, Format = BracketFormat },
            new() { Name = "原标题", Required = true, Hint = "如果是多季番要标出第几季" },
            new() { Name = "话数", Required = true, Format = BracketFormat },
            new() { Name = "剧场版", Fixed = true, Required = false, Format = _ => "[剧场版]" },
            new() { Name = "字幕语言", Required = true, Hint = "简体为[GB]，繁体为[BIG5]", Format = BracketFormat },
            new() { Name = "分辨率", Required = true, Format = BracketFormat },
            new() { Name = "文件格式", Required = true, Format = BracketFormat },
        }, Comment = "表番类动画只接受完整合集（至少一季），不接受单话投稿，亦不接受只有磁链或种子的投稿。另外擦边球类动画请投到商业动画分类（投稿格式和要求仍与表番类相同）注：不接受中国大陆的商业动画的投稿，亦不接受在中国大陆正式上映过的剧场版动画投稿。" },
        new() { Id = 18, Name = "商业CG", Fields = new()
        {
            new() { Name = "提取CG", Fixed = true, Required = true, Default = "提取CG", Format = BracketFormat },
            new() { Name = "发售日期", Required = true, Format = BracketFormat },
            new() { Name = "制作公司", Required = true, Format = BracketFormat },
            new() { Name = "原标题", Required = true, Hint = "日文原名" },
        }, Comment = "CG分类不接受只有磁链或种子的投稿。" },
        new() { Id = 19, Name = "同人CG", Fields = new()
        {
            new() { Name = "提取CG", Fixed = true, Required = false, Format = _ => "[提取CG]" },
            new() { Name = "汉化者", Required = false, Format = BracketFormat },
            new() { Name = "RJ号", Required = true, Hint = "可填\"无RJ号\"", Format = BracketFormat },
            new() { Name = "制作者名", Required = true, Format = BracketFormat },
            new() { Name = "原标题", Required = true, Hint = "日文原名" },
        }, Comment = "CG分类不接受只有磁链或种子的投稿。" },
        new() { Id = 20, Name = "商业作游戏", Fields = new()
        {
            new() { Name = "发售日期", Required = true, Format = BracketFormat },
            new() { Name = "制作公司", Required = true, Format = BracketFormat },
            new() { Name = "原标题", Required = true, Hint = "日文原名" },
        }, Comment = "游戏分类不接受的投稿种类：网游，含有联网内容的手游，在中国大陆正式发售的游戏（包括含内购收费内容的免费游戏）。" },
        new() { Id = 21, Name = "同人游戏", Fields = new()
        {
            new() { Name = "RJ号", Required = true, Hint = "可填\"无RJ号\"", Format = BracketFormat },
            new() { Name = "制作组", Required = true, Format = BracketFormat },
            new() { Name = "原标题", Required = true, Hint = "日文原名" },
        }, Comment = "游戏分类不接受的投稿种类：网游，含有联网内容的手游，在中国大陆正式发售的游戏（包括含内购收费内容的免费游戏）。" },
        new() { Id = 22, Name = "全年龄游戏", Fields = new()
        {
            new() { Name = "发售日期或RJ号", Required = true, Format = BracketFormat },
            new() { Name = "制作公司", Required = true, Format = BracketFormat },
            new() { Name = "原标题", Required = true, Hint = "日文原名" },
        }, Comment = "游戏分类不接受的投稿种类：网游，含有联网内容的手游，在中国大陆正式发售的游戏（包括含内购收费内容的免费游戏）。" },
        new() { Id = 36, Name = "补丁存档", Fields = new()
        {
            new() { Name = "发售日期或RJ号", Required = true, Format = BracketFormat },
            new() { Name = "制作公司", Required = true, Format = BracketFormat },
            new() { Name = "原标题", Required = true, Hint = "日文原名" },
        }, Comment = "游戏分类不接受的投稿种类：网游，含有联网内容的手游，在中国大陆正式发售的游戏（包括含内购收费内容的免费游戏）。" },
        new() { Id = 37, Name = "人物卡", Fields = new(), Comment = "I社游戏（恋活，AI少女，Honey Select等），或其他游戏的自定义人物存档分享。" },
        new() { Id = 23, Name = "同人志", Fields = new()
        {
            new() { Name = "汉化者", Required = false, Format = BracketFormat },
            new() { Name = "展会名", Required = true, Format = RoundBracketFormat },
            new() { Name = "社团或作者名", Required = true, Format = BracketFormat },
            new() { Name = "原标题", Required = true, Hint = "日文原名" },
            new() { Name = "题材", Required = false, Format = RoundBracketFormat },
        }, Comment = "漫画分类不接受只有磁链或种子的投稿。汉化本必须标明汉化者，若找不到汉化者信息且本中没有注明汉化者则标[汉化者不明]而不是[中国翻訳]。对于只在DLsite等网站发售了电子版，而没在展会贩卖过的本子请写RJ号" },
        new() { Id = 24, Name = "单行本", Fields = new()
        {
            new() { Name = "汉化者", Required = false, Format = BracketFormat },
            new() { Name = "扫图者", Required = false, Format = BracketFormat },
            new() { Name = "社团或作者名", Required = true, Format = BracketFormat },
            new() { Name = "原标题", Required = true, Hint = "日文原名" },
        }, Comment = "漫画分类不接受只有磁链或种子的投稿。非民间汉化的中文版单行本最前边标[中文]而不是[中国翻訳]，如果有扫图者信息的话在[中文]后边标出[扫图者]。" },
        new() { Id = 25, Name = "杂志", Fields = new()
        {
            new() { Name = "整本杂志", Required = false, Fixed = true, Format = _ => "(成年コミック・雑誌)" },
            new() { Name = "汉化者", Required = false, Format = BracketFormat },
            new() { Name = "扫图者", Required = false, Format = BracketFormat },
            new() { Name = "杂志名", Required = true, Format = RoundBracketFormat },
            new() { Name = "社团或作者名", Required = true, Format = BracketFormat },
            new() { Name = "原标题", Required = true, Hint = "日文原名" },
        }, Comment = "漫画分类不接受只有磁链或种子的投稿。非民间汉化的中文版单行本最前边标[中文]而不是[中国翻訳]，如果有扫图者信息的话在[中文]后边标出[扫图者]。" },
        new() { Id = 26, Name = "全年龄漫画", Fields = new()
        {
            new() { Name = "汉化者", Required = false, Format = BracketFormat },
            new() { Name = "展会或杂志名", Required = true, Format = RoundBracketFormat },
            new() { Name = "社团或作者名", Required = true, Format = BracketFormat },
            new() { Name = "原标题", Required = true, Hint = "日文原名" },
            new() { Name = "题材", Required = false, Format = RoundBracketFormat },
        }, Comment = "漫画分类不接受只有磁链或种子的投稿。另请参考同人志、单行本、杂志的格式。" },
        new() { Id = 27, Name = "工口画集", Fields = new()
        {
            new() { Name = "汉化者", Required = false, Format = BracketFormat },
            new() { Name = "P站ID", Required = false, Format = BracketFormat },
            new() { Name = "展会或杂志名", Required = true, Format = RoundBracketFormat },
            new() { Name = "社团/作者/画师名", Required = true, Format = BracketFormat },
            new() { Name = "原标题", Required = true, Hint = "日文原名" },
            new() { Name = "题材", Required = false, Format = RoundBracketFormat },
        }, Comment = "商业画集只需要写明画集名称即可。P站画集请在文中附上P站对应链接。图包类需要有一个画师/社团类的主题，且图包中图片数量不少于50。不接受只有磁链或种子的投稿。" },
        new() { Id = 28, Name = "全年龄画集", Fields = new()
        {
            new() { Name = "汉化者", Required = false, Format = BracketFormat },
            new() { Name = "P站ID", Required = false, Format = BracketFormat },
            new() { Name = "展会或杂志名", Required = true, Format = RoundBracketFormat },
            new() { Name = "社团/作者/画师名", Required = true, Format = BracketFormat },
            new() { Name = "原标题", Required = true, Hint = "日文原名" },
            new() { Name = "题材", Required = false, Format = RoundBracketFormat },
        }, Comment = "商业画集只需要写明画集名称即可。P站画集请在文中附上P站对应链接。图包类需要有一个画师/社团类的主题，且图包中图片数量不少于50。不接受只有磁链或种子的投稿。" },
        new() { Id = 29, Name = "音乐", Fields = new()
        {
            new() { Name = "发售日期", Required = false, Hint = "同人音乐请填展会名", Format = BracketFormat },
            new() { Name = "展会名", Required = false, Format = RoundBracketFormat },
            new() { Name = "同人音乐", Required = false, Fixed = true, Format = _ => "(同人音乐)" },
            new() { Name = "制作组", Required = false, Hint = "仅同人音乐", Format = BracketFormat },
            new() { Name = "原标题", Required = true, Hint = "日文原名" },
            new() { Name = "文件格式", Required = true, Hint = "如flac+cue，320kmp3等", Format = RoundBracketFormat },
        }, Comment = "本站只接受ACG相关音乐。正文中请至少列出曲目表。不接受只有磁链或种子的投稿。" },
        new() { Id = 30, Name = "同人音声", Fields = new()
        {
            new() { Name = "RJ号", Required = true, Hint = "可填\"无RJ号\"", Format = BracketFormat },
            new() { Name = "同人音声", Required = true, Fixed = true, Default = "同人音声", Format = RoundBracketFormat },
            new() { Name = "制作组", Required = false, Hint = "仅同人音乐", Format = BracketFormat },
            new() { Name = "原标题", Required = true, Hint = "日文原名" },
        }, Comment = "不接受只有磁链或种子的投稿。" },
        new() { Id = 31, Name = "官能小说", Fields = new()
        {
            new() { Name = "版本", Required = true, Hint = "汉化组/台版，原版请写生肉", Format = BracketFormat },
            new() { Name = "18禁小説", Required = true, Fixed = true, Default = "18禁小説", Format = RoundBracketFormat },
            new() { Name = "作者", Required = true, Format = BracketFormat },
            new() { Name = "原标题", Required = true, Hint = "日文原名" },
            new() { Name = "卷数", Required = false, Format = RoundBracketFormat },
            new() { Name = "文件格式", Required = true, Hint = "txt pdf jpg png等", Format = RoundBracketFormat },
        }, Comment = "不接受只有磁链或种子的投稿。只接受轻小说资源的投稿，小说资源请在压缩包中附上小说插图。" },
        new() { Id = 32, Name = "站友原创", Fields = new(), Comment = "没有格式要求。原创小说分类请将小说内容直接贴在正文，并确保所发内容不是完全二次元无关。" },
        new() { Id = 33, Name = "一般小说", Fields = new()
        {
            new() { Name = "版本", Required = true, Hint = "汉化组/台版，原版请写生肉", Format = BracketFormat },
            new() { Name = "一般小说", Required = true, Fixed = true, Default = "一般小说" },
            new() { Name = "作者", Required = true, Format = BracketFormat },
            new() { Name = "原标题", Required = true, Hint = "日文原名" },
            new() { Name = "卷数", Required = false, Format = RoundBracketFormat },
            new() { Name = "文件格式", Required = true, Hint = "txt pdf jpg png等", Format = RoundBracketFormat },
        }, Comment = "不接受只有磁链或种子的投稿。只接受轻小说资源的投稿，小说资源请在压缩包中附上小说插图。" },
        new() { Id = 34, Name = "一般绘画", Fields = new(), Comment = "没有格式要求。绘画分类接受各种原创绘画的投稿（临摹也可以，只要是自己画的）。" },
        new() { Id = 35, Name = "工口绘画", Fields = new(), Comment = "没有格式要求。绘画分类接受各种原创绘画的投稿（临摹也可以，只要是自己画的）。" },
    };
}
