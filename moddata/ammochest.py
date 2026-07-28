import json

def update_ammobox_prefabs(boxes_file_path, exportitems_file_path, output_file_path):
    print("⏳ 正在读取数据...")
    
    # 1. 读取你的自定义弹药箱 JSON
    try:
        with open(boxes_file_path, 'r', encoding='utf-8') as f:
            boxes_data = json.load(f)
    except Exception as e:
        print(f"❌ 读取弹药箱文件失败: {e}")
        return

    # 2. 读取原版物品数据库 (exportitems)
    try:
        with open(exportitems_file_path, 'r', encoding='utf-8') as f:
            export_items = json.load(f)
    except Exception as e:
        print(f"❌ 读取 exportitems 文件失败: {e}")
        return

    update_count = 0

    print("🔍 开始匹配和替换 Prefab 路径...")
    
    # 3. 遍历所有弹药箱并替换
    for box_id, box_info in boxes_data.items():
        try:
            # 找到 giftdata 里的 item 数组
            items_list = box_info["_customprops"]["StaticBoxData"]["giftdata"][0]["item"]
            
            # 确保数组里至少有2个元素（索引0是箱子，索引1是子弹）
            if len(items_list) > 1:
                ammo_tpl = items_list[1]["_tpl"]
            else:
                print(f"⚠️ 跳过 [{box_id}]: item 列表里没有找到子弹数据")
                continue
            
            # 在 exportitems 里查找这个子弹的 tpl
            if ammo_tpl in export_items:
                # 提取子弹的 Prefab path
                ammo_prefab_path = export_items[ammo_tpl]["_props"]["Prefab"]["path"]
                
                # 替换当前弹药箱的 Prefab path
                box_info["_props"]["Prefab"]["path"] = ammo_prefab_path
                update_count += 1
            else:
                print(f"⚠️ 警告: 在 exportitems 中找不到子弹 TPL [{ammo_tpl}] (属于箱子: {box_id})")
                
        except KeyError as e:
            print(f"⚠️ 跳过 [{box_id}]: 数据结构异常，找不到键值 {e}")
        except Exception as e:
            print(f"⚠️ 跳过 [{box_id}]: 发生未知错误 {e}")

    # 4. 输出到新文件
    with open(output_file_path, 'w', encoding='utf-8') as f:
        json.dump(boxes_data, f, ensure_ascii=False, indent=4)
        
    print("---")
    print(f"✅ 搞定！成功更新了 {update_count} 个弹药箱的模型路径。")
    print(f"📁 新文件已保存至: {output_file_path}")

if __name__ == "__main__":
    # 在这里填入你的文件名
    # 1. 你的弹药箱文件
    BOXES_JSON = "your_ammo_boxes.json" 
    # 2. 塔科夫的物品数据库文件
    EXPORT_ITEMS_JSON = "exportitems.json" 
    # 3. 生成的新文件
    OUTPUT_JSON = "updated_ammo_boxes.json" 
    
    update_ammobox_prefabs(BOXES_JSON, EXPORT_ITEMS_JSON, OUTPUT_JSON)