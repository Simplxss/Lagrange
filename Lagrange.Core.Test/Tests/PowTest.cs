﻿using Lagrange.Core.Utility.Extension;
using Lagrange.Core.Utility.Tencent;

namespace Lagrange.Core.Test.Tests;

internal class PowTest
{
    public static void Test()
    {
        var t546 = "0102010200010000008058F6988F8AEED74F7C6BAE35CF1CAA1CDD34E133FFA35121CBDF568F6C610FB6688905AF72BC05FB5A6C038D84C3A096E9B9D262216E4B338D951F13FE059B556DBDC14CC09E58913897FF2BBE8D4E2A817897294694D901AE05EE7A9583C0E9F3E001B3E1868F99057CB9D5F7F56DB2276139F4918D48C8FC43FE4A8D6398C400204FF892EF27BC5916D8450FA93CEDACAAAE48FB3246A9816FAC28E6C1BE740E6600AC0102010200010000008058F6988F8AEED74F7C6BAE35CF1CAA1CDD34E133FFA35121CBDF568F6C610FB6688905AF72BC05FB5A6C038D84C3A096E9B9D262216E4B338D951F13FE059B556DBDC14CC09E58913897FF2BBE8D4E2A817897294694D901AE05EE7A9583C0E9F3E001B3E1868F99057CB9D5F7F56DB2276139F4918D48C8FC43FE4A8D6398C400204FF892EF27BC5916D8450FA93CEDACAAAE48FB3246A9816FAC28E6C1BE740E66";

        var pow = new PowValue(t546.UnHex());
        var result = ClientPow.GetPow(pow);

        Console.WriteLine(result.Hex());
    }
}
