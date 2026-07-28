import json

# 读取文件中的 JSON 数据
def read_json(file_path):
    with open(file_path, 'r', encoding='utf-8') as file:
        return json.load(file)

# 将修改后的 JSON 数据写入另一个文件
def write_json(data, file_path):
    with open(file_path, 'w', encoding='utf-8') as file:
        json.dump(data, file, ensure_ascii=False, indent=4)

# 修改数据，将type转为$type并放置到每个giftdata对象中
def modify_data(data):
    for item in data.values():
        if "_customprops" in item:
            for key in ["StaticBoxData", "SpecialBoxData"]:
                if key in item["_customprops"]:
                    for gift in item["_customprops"][key]["giftdata"]:
                        if isinstance(gift, dict):
                            # 添加$type，值为gift对象的第一个key
                            first_key = list(gift.keys())[0]
                            gift["$type"] = first_key
                            # 删除原来的type键
                            if "type" in gift:
                                del gift["type"]
    return data

# 主程序
input_file = 'items_ammochest.json'  # 输入文件路径
output_file = 'items_ammochest1.json'  # 输出文件路径

# 从文件读取 JSON 数据
data = read_json(input_file)

# 进行数据修改
modified_data = modify_data(data)

# 将修改后的数据写入到另一个文件
write_json(modified_data, output_file)

print(f"数据已成功从 {input_file} 读取并修改，修改后的结果已保存到 {output_file}.")
