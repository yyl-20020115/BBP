using System.Globalization;
using System.Numerics;
using System.Text;

namespace BBP;

internal class Program
{
    static string FromHexStringText(string all_decimals)
        => all_decimals.IndexOf('.') is int idx && idx >= 0 ?
            FromHexStringText(all_decimals[0..idx], all_decimals[(idx + 1)..])
            : FromHexStringText(all_decimals, "");
    /// <summary>
    /// 十六进制小数到十进制小数的转换方法
    /// </summary>
    /// <param name="integral"></param>
    /// <param name="pure_decimals"></param>
    /// <returns></returns>
    static string FromHexStringText(string integral, string pure_decimals)
    {
        var builder = new StringBuilder();
        if (!string.IsNullOrEmpty(integral))
        {
            if (BigInteger.TryParse(integral, NumberStyles.HexNumber, null, out var b))
            {
                builder.Append(b.ToString());
            }
        }
        if (builder.Length == 0)
        {
            builder.Append('0');
        }
        if (!string.IsNullOrEmpty(pure_decimals))
        {
            builder.Append('.');
            if (BigInteger.TryParse(pure_decimals, NumberStyles.HexNumber, null, out var pure))
            {
                BigInteger result = BigInteger.Zero;
                var pure_dec = pure.ToString();
                var rt = pure_dec.ToString();
                if (rt.Length > 0)
                {
                    List<BigInteger> factors = [];
                    BigInteger m = 10*BigInteger.Pow(10, rt.Length);
                    for(int i= 0; i < pure_decimals.Length; i++)
                    {
                        m >>= 4;
                        var b = int.TryParse(pure_decimals[i..(i + 1)], NumberStyles.HexNumber,
                            null, out var val);
                        if (b)
                        {
                            result += val * m;
                        }
                    }
                    builder.Append(result.ToString());
                }
            }
        }
        return builder.ToString();
    }

    static double FromHexString(string text)
    {
        var result = 0.0;
        var idx = text.IndexOf('.');
        if (idx >= 0)
        {
            var decs = text[(idx + 1)..];
            var dec = 0.0;
            for (int i = 0; i < decs.Length; i++)
            {
                var c = decs[i];
                var v = c - '0';
                switch (c)
                {
                    case >= '0' and <= '9':
                        v = c - '0';
                        break;
                    case >= 'a' and <= 'f':
                        v = c - 'a' + 10;
                        break;
                    case >= 'A' and <= 'F':
                        v = c - 'A' + 10;
                        break;
                }
                dec += (v * double.Pow(1.0 / 16, i + 1));
            }

            result = dec;
            text = text[..idx];
        }
        if (long.TryParse(text, NumberStyles.HexNumber, null, out var value))
        {
            result += value;
        }
        return result;
    }
    static string ToHexString(double d)
    {
        var builder = new StringBuilder();
        long t = (long)Math.Truncate(d);
        builder.Append(t.ToString("X"));
        double r = d - t;
        if (r != 0)
        {
            builder.Append('.');
        }
        while (r != 0)
        {
            r *= 16.0;
            long b = (long)Math.Truncate(r);
            builder.Append(b.ToString("X"));
            r -= b;
        }
        return builder.ToString();
    }
    //8n+t
    static BigInteger S(int t, int digit_index, int digits_count, BigInteger mask)
    {
        var shift = digits_count << 2;
        BigInteger left = 0;
        for (var _n = 0; _n <= digit_index; _n++)
        {
            BigInteger r = _n * 8 + t;
            left = (left + (BigInteger.ModPow(16, digit_index - _n, r) << shift) / r) & mask;
        }
        BigInteger right = 0;
        for (var _n = digit_index + 1; ; _n++)
        {
            BigInteger rnew = right + BigInteger.Pow(16, digits_count + digit_index - _n) / (_n * 8 + t);
            if (right == rnew) break;
            right = rnew;
        }
        return left + right;
    }
    static BigInteger Compute(int digit_index, int digits_count)
    {
        var mask = BigInteger.Pow(16, digits_count) - 1;
        if (digit_index == 0)
        {
            mask = BigInteger.Pow(2, digits_count * 4 + 2) - 1;
        }
        var s0 = 4 * S(1, digit_index, digits_count, mask);
        var s1 = 2 * S(4, digit_index, digits_count, mask);
        var s2 = 1 * S(5, digit_index, digits_count, mask);
        var s3 = 1 * S(6, digit_index, digits_count, mask);
        var result = s0 - s1 - s2 - s3;
        result &= mask;
        return result;
    }
    static void Main(string[] args)
    {
        double d = Math.PI;
        var tx = ToHexString(d);
        var xt = FromHexString(tx);
        if (xt == d)
        {
            Console.WriteLine("Pi verified!");

            var ty = FromHexStringText(tx);

        }
        var builder = new StringBuilder();
        //存在数字重叠
        var sumx = BigInteger.Zero;
        var sumd = BigInteger.Zero;
        const int max_hex_digits = 256;
        for (int digit_index = 0; digit_index < max_hex_digits; digit_index += 4)
        {
            var result = Compute(digit_index, 8);
            sumx |= (result >> 16) & (digit_index == 0 ? 0x3ffff : 0xffff);
            sumx <<= 16;

            sumd |= (result >> 16) & (0xffff);
            sumd <<= 16;
        }
        sumx >>= 16;
        sumd >>= 16;
        var text = sumx.ToString("X");
        Console.WriteLine($"HEX:{text}");
        Console.WriteLine();
        Console.WriteLine($"DEC:{FromHexStringText("3", text[1..])}");
    }
}
