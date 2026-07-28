import json

# 读取文件中的 JSON 数据
def read_json(file_path):
    with open(file_path, 'r', encoding='utf-8') as file:
        return json.load(file)

# 将修改后的 JSON 数据写入另一个文件
def write_json(data, file_path):
    with open(file_path, 'w', encoding='utf-8') as file:
        json.dump(data, file, ensure_ascii=False, indent=4)

# 转移_props中的isSpecialBox和SpecialBoxData到_customprops
def transfer_data(data):
    for item in data.values():
        if "_props" in item and "_customprops" in item:
            if "isSpecialBox" in item["_props"]:
                item["_customprops"]["isSpecialBox"] = item["_props"]["isSpecialBox"]
            if "SpecialBoxData" in item["_props"]:
                item["_customprops"]["SpecialBoxData"] = item["_props"]["SpecialBoxData"]
            
            # 删除_props中的isSpecialBox和SpecialBoxData
            item["_props"].pop("isSpecialBox", None)
            item["_props"].pop("SpecialBoxData", None)

    return data

# 主程序
input_file = 'items_skillchest.json'  # 输入文件路径
output_file = 'items_skillchest1.json'  # 输出文件路径

# 从文件读取 JSON 数据
data = read_json(input_file)

# 进行数据转移
modified_data = transfer_data(data)

# 将修改后的数据写入到另一个文件
write_json(modified_data, output_file)

print(f"数据已成功从 {input_file} 读取并修改，修改后的结果已保存到 {output_file}.")