import { GachaSetting } from "app/models/GachaSetting";

export interface GachaPool {
  poolName: string;
  poolDisplay: string;
  poolDescription: string;
  hasIntro?: boolean;
  poolSetting?: GachaSetting;
}

export const GACHA_POOLS: { [key: string]: GachaPool } = {
  "common": {
    poolName: "common",
    poolDisplay: "普通卡池",
    poolDescription: `<h3>普通卡池</h3>
        <p>普通卡池中可召唤往前活动中的卡牌。但是不会出现☆5卡牌。</p>
        <p>十连召唤可至少获得1枚三星以上（含三星）卡牌！</p>`
  },
  "touhou1st": {
    poolName: "touhou1st",
    poolDisplay: "幻想乡推荐召唤第一弹",
    poolDescription: `<h3>【幻想乡推荐召唤第一弹】！</h3>
        <p>【幻想乡推荐召唤第一弹】卡池限时开启！</p>
        <p>活动时间：2018年2月6日 维护后 ~ 4月30日 23:59 </p>
        <p>东方世界的神主【☆5(SSR) ZUN】，以及在幻想乡世界大放异彩，人气高涨的12位角色推荐召唤！</p>
        <p>十连召唤可至少获得1枚三星以上（含三星）卡牌！</p>
        <p>推荐召唤卡池中，三星以上（含三星）将只出现新加入的卡牌。</p>`
  },
  "april2018": {
    poolName: "april2018",
    poolDisplay: "路人女主的养成方法",
    poolDescription: `<h3>【路人女主的养成方法召唤】！</h3>
        <p>活动时间：2018年4月1日 维护后 ~ 4月15日 23:59 </p>
        <p>限时活动「路人女主的养成方法（你老婆）」开启！</p>
        <p>在4月1日这个美好的日子里，你能不能与自己命中注定的另一半相遇呢？</p>
        <p>SSR出现率2倍！<s>3%</s>→<strong>6%</strong>！</p>
        <p>十连召唤可至少获得1枚三星以上（含三星）卡牌！</p>
        <p>推荐召唤卡池中，五星卡牌将只出现新加入的卡牌。</p>`
  },
  "june2018": {
    poolName: "june2018",
    poolDisplay: "少女の辉",
    poolDescription: `<h3>儿童节活动限定卡池【少女の辉】！</h3>
        <p>活动时间：2018年6月4日 维护后 ~ 6月30日 23:59 </p>
        <p>限时活动「少女の辉」限定卡池开启！</p>
        <p>活动期间，只要在卡池中进行首次十连，将必定获得随机5☆限定卡其中之一！只属于你的那份色彩，你一定要好好的收下哦，欧尼酱！</p>
        <p><strong>首次十连召唤必得至少1张☆5卡牌！</strong></p>
        <p>十连召唤可至少获得1枚三星以上（含三星）卡牌！</p>
        <p>推荐召唤卡池中，五星卡牌将只出现新加入的卡牌。</p>`
  },
  "april2019": {
    poolName: "april2019",
    poolDisplay: "绅士之魂",
    poolDescription: `<h3>【绅士之魂】诞生！</h3>
        <p>活动时间：2019年4月1日 维护后 ~ 4月30日 23:59 </p>
        <p>限时活动「绅士之魂」限定卡池开启！</p>
        <p>绅士之庭/北绅之庭 今日起正式并入魂+/北+/南+/白+ 旗下，成为其下属站点啦！为了纪念这一重大事件，本站决定举办新的抽卡活动。</p>
        <p>十连召唤可至少获得1枚三星以上（含三星）卡牌！</p>
        <p>推荐召唤卡池中，五星卡牌将只出现新加入的卡牌。</p>`
  },
  "august2020": {
    poolName: "august2020",
    poolDisplay: "七周年特异点",
    hasIntro: true,
    poolDescription: `<h3>七周年特异点——【只人：蛋落X度】！</h3>
        <p>活动时间：2020年8月30日 维护后 ~ 9月30日 23:59 </p>
        <p>限时活动「只人：蛋落X度」限定卡池开启！</p>
        <p>收集众人的证词（卡牌描述）从中寻找出线索与答案吧！同时收集人间的蛋蛋来获取奖品抽取权，真相。。。蛋落实出！</p>
        <p>十连召唤可至少获得1枚三星以上（含三星）卡牌！</p>
        <p>推荐召唤卡池中，五星卡牌将只出现新加入的卡牌。</p>`
  },
}
