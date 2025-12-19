var text = """
    key1=value1
    key2=value2
    ---
    header1,header2,header3
    data1,data2,data3
    data4,data5,data6
""";

var topLevelParts = text.Split("---\n");
if (topLevelParts.Length != 2)
{
    throw new Exception("Invalid input");
}

