import json

# 1. 技能关键字 -> Bundle 文件名的映射表
# 你只需要把搓好的 35 个 bundle 名称跟技能对应上
SKILL_BUNDLE_MAP = {
    "瞄准": "skill_aiming.pack",
    "专注": "skill_attention.pack",
    "工艺": "skill_craft.pack",
    "代谢": "skill_daixie.pack",
    "精确射手步枪": "skill_dmr.pack",
    "耐力": "skill_endurance.pack",
    "感知": "skill_ganzhi.pack",
    "投掷物": "skill_gernade.pack", # 注意：原文件名拼写为 gernade
    "下挂发射器": "skill_grenadelauncher.pack",
    "健康": "skill_health.pack",
    "重型护甲": "skill_heavyarmor.pack",
    "藏身处管理": "skill_hideout.pack",
    "重机枪": "skill_hmg.pack",
    "活力": "skill_huoli.pack",
    "抗压": "skill_kangya.pack",
    "榴弹发射器": "skill_launcher.pack",
    "轻型护甲": "skill_lightarmor.pack",
    "轻机枪": "skill_lmg.pack",
    "弹匣训练": "skill_mags.pack",
    "魅力": "skill_meili.pack",
    "近战": "skill_melee.pack",
    "免疫": "skill_mianyi.pack",
    "故障排除": "skill_paizhang.pack", # 对应排障
    "手枪": "skill_pistol.pack",
    "力量": "skill_power.pack",
    "隐蔽行动": "skill_qianxing.pack", # 对应潜行
    "武器维护": "skill_repair.pack",
    "突击步枪": "skill_rifle.pack",
    "搜索": "skill_search.pack",
    "霰弹枪": "skill_shotgun.pack",
    "冲锋枪": "skill_smg.pack",
    "栓动式步枪": "skill_sniper.pack",
    "手术": "skill_surgrey.pack", # 注意：原文件名拼写为 surgrey
    "智力": "skill_zhili.pack",
    "左轮手枪": "skill_zhuanlun.pack" # 对应转轮
    # TODO: 补全剩下的31个技能...
}

def update_bundles(input_json_path, output_json_path):
    # 读取你现有的 JSON 文件
    try:
        with open(input_json_path, 'r', encoding='utf-8') as f:
            items_data = json.load(f)
    except Exception as e:
        print(f"❌ 读取文件失败: {e}")
        return

    update_count = 0

    # 遍历所有物品
    for item_id, item_data in items_data.items():
        # item_id 类似 "初级耐力技能经验箱"
        for skill_key, bundle_path in SKILL_BUNDLE_MAP.items():
            # 如果关键字（如"耐力"）在这个物品的名字里
            if skill_key in item_id:
                # 定位到 Prefab 节点并替换 path
                try:
                    item_data["_props"]["Prefab"]["path"] = bundle_path
                    update_count += 1
                except KeyError:
                    print(f"⚠️ 警告: 物品 {item_id} 的数据结构异常，找不到 Prefab.path")
                
                # 匹配到就跳出内层循环，继续检查下一个物品
                break 

    # 将修改后的数据写回新的 JSON 文件
    with open(output_json_path, 'w', encoding='utf-8') as f:
        json.dump(items_data, f, ensure_ascii=False, indent=4)
        
    print(f"✅ 搞定！成功更新了 {update_count} 个物品的 bundle 路径。")
    print(f"📁 文件已保存至: {output_json_path}")

if __name__ == "__main__":
    # 把 "your_original_items.json" 替换成你现在那个配置文件的名字
    # 跑完之后会生成一个新的 "updated_items.json"
    update_bundles("your_original_items.json", "updated_items.json")