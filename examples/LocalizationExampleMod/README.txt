本地化示例模组 (Localization Example Mod)
==========================================

功能说明：
---------
此模组演示如何：
1. 为现有语言添加/覆盖文本
2. 添加全新的语言支持（法语、德语、西班牙语）

文件结构：
---------
LocalizationExampleMod/
├── manifest.json                    # 模组配置
├── README.txt                       # 说明文档
└── Localization/
    ├── languageConfig.json          # 新语言配置（声明要添加的新语言）
    ├── CN.json                      # 简体中文文本
    ├── English.json                 # 英文文本
    ├── French.json                  # 法语文本（新语言）
    ├── German.json                  # 德语文本（新语言）
    └── Spanish.json                 # 西班牙语文本（新语言）

语言文件格式：
-------------
{
    "fields": ["ID", "Text"],
    "types": ["int", "string"],
    "data": [
        { "ID": 90001, "Text": "模组添加的文本" },
        { "ID": 1, "Text": "覆盖游戏原有文本（ID=1是语言名称）" }
    ]
}

文本ID说明：
-----------
- ID 1-89999: 游戏内置文本ID，模组可以覆盖
- ID 90000+: 建议模组使用的ID范围，避免与游戏冲突

添加新语言步骤：
--------------
1. 在 languageConfig.json 中声明新语言：
   {
       "Languages": [
           {
               "Code": "French",           // 语言代码（用于文件名）
               "DisplayName": "Français",  // 在设置中显示的名称
               "FontName": "Roboto-Bold SDF",  // 使用的字体
               "RightToLeft": false        // 是否从右到左书写
           }
       ]
   }

2. 创建对应的语言文件 French.json，包含翻译文本

3. 游戏会自动识别并在语言设置中显示新语言

注意事项：
---------
- 语言文件名必须与 languageConfig.json 中的 Code 一致
- 如果只是扩展现有语言，不需要 languageConfig.json
- 模组的文本会覆盖游戏原有的同ID文本
- 多个模组修改同一ID时，后加载的模组优先
