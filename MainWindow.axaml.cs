using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Input.Platform;

namespace ResidueCalc;

public partial class MainWindow : Window
{
    const string UA = "АБВГҐДЕЄЖЗИІЇЙКЛМНОПРСТУФХЦЧШЩЬЮЯ";
    const string EN = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    string lastAlphabet = EN;

    public MainWindow() { InitializeComponent(); }

    static long Mod(long x, long m) => ((x % m) + m) % m;

    static long PowFast(long a, long e, long m)
    {
        long r = 1;
        a %= m;
        while (e > 0)
        {
            if ((e & 1) == 1) r = r * a % m;
            a = a * a % m;
            e >>= 1;
        }
        return r;
    }

    static bool IsPrime(long p)
    {
        if (p < 2) return false;
        if (p < 4) return true;
        for (long a = 2; a <= Math.Min(p - 2, 20); a++)
            if (PowFast(a, p - 1, p) != 1) return false;
        return true;
    }
    
    static long Gcd(long a, long b)
    {
        while (b != 0)
        {
            long r = a % b;
            a = b;
            b = r;
        }
        return a;
    }

    static long Phi(long n)
    {
        long result = n;
        for (long i = 2; i * i <= n; i++)
            if (n % i == 0)
            {
                while (n % i == 0)
                    n /= i;
                result -= result / i;
            }
        if (n > 1)
            result -= result / n;
        return result;
    }

    void Run(Func<long, long, long, long> op, bool needB = true, bool isPow = false)
    {
        ErrorText.Text = "";
        ResultText.Text = "";

        if (!long.TryParse(MBox.Text, out long m) || m < 2)
        { ErrorText.Text = "m must be an integer greater than 1"; return; }

        if (!long.TryParse(ABox.Text, out long a))
        { ErrorText.Text = "Enter an integer a"; return; }

        long b = 0;
        if (needB && !long.TryParse(BBox.Text, out b))
        { ErrorText.Text = "Enter an integer b"; return; }

        if (isPow && b < 0)
        { ErrorText.Text = "Exponent b must be 0 or greater"; return; }

        a = Mod(a, m);
        if (!isPow) b = Mod(b, m);

        ResultText.Text = op(a, b, m).ToString();
    }

    void OnAdd(object? s, RoutedEventArgs e) => Run((a, b, m) => Mod(a + b, m));
    void OnSub(object? s, RoutedEventArgs e) => Run((a, b, m) => Mod(a - b, m));
    void OnMul(object? s, RoutedEventArgs e) => Run((a, b, m) => Mod(a * b, m));
    void OnPow(object? s, RoutedEventArgs e) => Run(PowFast, true, true);
    void OnNeg(object? s, RoutedEventArgs e) => Run((a, b, m) => Mod(-a, m), false);
    void OnSoon(object? s, RoutedEventArgs e) => ErrorText.Text = "This operation will be added later";

    void OnClear(object? s, RoutedEventArgs e)
    {
        MBox.Text = ABox.Text = BBox.Text = XBox.Text = YBox.Text = PBox.Text = QBox.Text = WordBox.Text = KBox.Text = ErrorText.Text = InfoText.Text = "";
        ResultText.Text = "";
    }

    void OnIsPrime(object? s, RoutedEventArgs e)
    {
        ErrorText.Text = ""; ResultText.Text = "";
        if (!long.TryParse(ABox.Text, out long n) || n < 1 || n > 2000000000)
        { ErrorText.Text = "a must be an integer from 1 to 2000000000"; return; }
        ResultText.Text = IsPrime(n) ? "prime" : "not prime";
    }

    void OnGenPrime(object? s, RoutedEventArgs e)
    {
        ErrorText.Text = ""; ResultText.Text = "";
        if (!long.TryParse(ABox.Text, out long A) || A < 2 || A > 2000000000)
        { ErrorText.Text = "A must be an integer from 2 to 2000000000"; return; }
        for (long p = A; p >= 2; p--)
            if (IsPrime(p)) { ResultText.Text = p.ToString(); return; }
    }

    void OnGcd(object? s, RoutedEventArgs e)
    {
        ErrorText.Text = ""; ResultText.Text = "";
        if (!long.TryParse(ABox.Text, out long a) || !long.TryParse(BBox.Text, out long b) || a < 1 || b < 1)
        { ErrorText.Text = "a and b must be positive integers"; return; }
        long g = Gcd(a, b);
        ResultText.Text = g == 1 ? "1 (coprime)" : g.ToString();
    }

    void OnPhi(object? s, RoutedEventArgs e)
    {
        ErrorText.Text = ""; ResultText.Text = "";
        if (!long.TryParse(ABox.Text, out long n) || n < 1)
        { ErrorText.Text = "a must be a positive integer"; return; }
        ResultText.Text = Phi(n).ToString();
    }

    void OnInv(object? s, RoutedEventArgs e)
    {
        ErrorText.Text = ""; ResultText.Text = "";
        if (!long.TryParse(MBox.Text, out long m) || m < 2 || m > 2000000000)
        { ErrorText.Text = "m must be an integer from 2 to 2000000000"; return; }
        if (!long.TryParse(ABox.Text, out long a))
        { ErrorText.Text = "Enter an integer a"; return; }

        a = Mod(a, m);
        if (Gcd(a, m) != 1)
        { ErrorText.Text = "Inverse does not exist (a and m are not coprime)"; return; }

        ResultText.Text = PowFast(a, Phi(m) - 1, m).ToString();
    }

    static long Order(long a, long m)
    {
        long x = a % m, k = 1;
        while (x != 1)
        {
            x = x * a % m;
            k++;
        }
        return k;
    }

    static bool IsGenerator(long g, long p)
    {
        long n = p - 1, t = n;
        for (long q = 2; q * q <= t; q++)
            if (t % q == 0)
            {
                if (PowFast(g, n / q, p) == 1) return false;
                while (t % q == 0) t /= q;
            }
        if (t > 1 && PowFast(g, n / t, p) == 1) return false;
        return true;
    }

    static string Decode(List<long> codes, string alphabet)
    {
        return new string(codes.Select(d =>
            d >= 1 && d <= alphabet.Length ? alphabet[(int)d - 1] : '?').ToArray());
    }

    bool ReadPrime(out long p)
    {
        if (!long.TryParse(MBox.Text, out p) || p < 3 || p > 2000000000 || !IsPrime(p))
        { ErrorText.Text = "m must be a prime number from 3 to 2000000000"; return false; }
        return true;
    }

    void OnOrder(object? s, RoutedEventArgs e)
    {
        ErrorText.Text = ""; ResultText.Text = ""; InfoText.Text = "";
        if (!long.TryParse(MBox.Text, out long m) || m < 2 || m > 10000000)
        { ErrorText.Text = "m must be an integer from 2 to 10000000"; return; }
        if (!long.TryParse(ABox.Text, out long a))
        { ErrorText.Text = "Enter an integer a"; return; }
        a = Mod(a, m);
        if (Gcd(a, m) != 1)
        { ErrorText.Text = "a and m are not coprime"; return; }
        ResultText.Text = Order(a, m).ToString();
    }

    void OnIsGen(object? s, RoutedEventArgs e)
    {
        ErrorText.Text = ""; ResultText.Text = ""; InfoText.Text = "";
        if (!ReadPrime(out long p)) return;
        if (!long.TryParse(ABox.Text, out long g) || g < 2 || g >= p)
        { ErrorText.Text = "a must be an integer from 2 to m-1"; return; }
        ResultText.Text = IsGenerator(g, p) ? "generator" : "not generator";
    }

    void OnMinGen(object? s, RoutedEventArgs e)
    {
        ErrorText.Text = ""; ResultText.Text = ""; InfoText.Text = "";
        if (!ReadPrime(out long p)) return;
        for (long g = 2; g < p; g++)
            if (IsGenerator(g, p))
            {
                ResultText.Text = g.ToString();
                InfoText.Text = "Number of generators: " + Phi(p - 1);
                return;
            }
    }

    void OnAllGen(object? s, RoutedEventArgs e)
    {
        ErrorText.Text = ""; ResultText.Text = ""; InfoText.Text = "";
        if (!ReadPrime(out long p)) return;
        if (p > 1000)
        { ErrorText.Text = "For the full list m must be at most 1000"; return; }
        long g1 = 2;
        while (!IsGenerator(g1, p)) g1++;
        var list = new List<long>();
        for (long b = 1; b < p - 1; b++)
            if (Gcd(b, p - 1) == 1)
                list.Add(PowFast(g1, b, p));
        list.Sort();
        ResultText.Text = list.Count.ToString();
        InfoText.Text = "Generators: " + string.Join(", ", list);
    }

    void OnDh(object? s, RoutedEventArgs e)
    {
        ErrorText.Text = ""; ResultText.Text = ""; InfoText.Text = "";
        if (!ReadPrime(out long p)) return;
        if (!long.TryParse(ABox.Text, out long g) || g < 2 || g >= p)
        { ErrorText.Text = "g (field a) must be an integer from 2 to m-1"; return; }
        if (!long.TryParse(XBox.Text, out long x) || !long.TryParse(YBox.Text, out long y)
            || x < 1 || y < 1 || x >= p - 1 || y >= p - 1)
        { ErrorText.Text = "Secrets A and B must be integers from 1 to m-2"; return; }

        long rA = PowFast(g, x, p);
        long rB = PowFast(g, y, p);
        long kA = PowFast(rB, x, p);
        long kB = PowFast(rA, y, p);

        ResultText.Text = kA == kB ? kA.ToString() : "error";
        InfoText.Text = "rA = " + rA + ", rB = " + rB + "\nK(A) = " + kA + ", K(B) = " + kB
            + (IsGenerator(g, p) ? "" : "\nWarning: g is not a generator");
    }

    void OnRsaKeys(object? s, RoutedEventArgs e)
    {
        ErrorText.Text = ""; ResultText.Text = ""; InfoText.Text = "";
        if (!long.TryParse(PBox.Text, out long p) || !long.TryParse(QBox.Text, out long q)
            || p < 3 || q < 3 || !IsPrime(p) || !IsPrime(q) || p == q)
        { ErrorText.Text = "p and q must be different primes"; return; }
        if (p * q > 2000000000)
        { ErrorText.Text = "p*q must be at most 2000000000"; return; }

        long m = p * q;
        long n = (p - 1) * (q - 1);
        long phiN = Phi(n);

        var pairs = new List<string>();
        for (long x = 2; x < n && pairs.Count < 5; x++)
            if (Gcd(x, n) == 1)
                pairs.Add("(" + x + ", " + PowFast(x, phiN - 1, n) + ")");

        MBox.Text = m.ToString();
        ResultText.Text = m.ToString();
        InfoText.Text = "m = " + m + ", n = " + n + ", phi(n) = " + phiN
            + ", keys = " + phiN / 2 + "\nPairs (x, y): " + string.Join(" ", pairs);

        if (long.TryParse(ABox.Text, out long xs) && xs > 1 && xs < n)
        {
            if (Gcd(xs, n) != 1)
                ErrorText.Text = "x is not coprime with n";
            else
                InfoText.Text += "\nFor x = " + xs + ": y = " + PowFast(xs, phiN - 1, n);
        }
    }

    void OnEncrypt(object? s, RoutedEventArgs e)
    {
        ErrorText.Text = ""; ResultText.Text = ""; InfoText.Text = "";
        if (!long.TryParse(MBox.Text, out long m) || m < 2 || m > 2000000000)
        { ErrorText.Text = "m must be an integer from 2 to 2000000000"; return; }
        if (!long.TryParse(BBox.Text, out long key) || key < 1)
        { ErrorText.Text = "Enter the public key y in field b"; return; }

        string word = (WordBox.Text ?? "").ToUpper().Replace(" ", "");
        if (word.Length == 0)
        { ErrorText.Text = "Enter a word"; return; }

        bool hasUa = word.Any(c => UA.Contains(c));
        bool hasEn = word.Any(c => EN.Contains(c));
        if (hasUa && hasEn)
        { ErrorText.Text = "Do not mix Ukrainian and English letters"; return; }
        if (!hasUa && !hasEn)
        { ErrorText.Text = "Use Ukrainian or English letters"; return; }

        string alphabet = hasEn ? EN : UA;
        var codes = new List<long>();
        foreach (char c in word)
        {
            int idx = alphabet.IndexOf(c) + 1;
            if (idx == 0)
            { ErrorText.Text = "Letter '" + c + "' is not in the " + (hasEn ? "English" : "Ukrainian") + " alphabet"; return; }
            if (idx >= m)
            { ErrorText.Text = "Letter code must be less than m"; return; }
            codes.Add(idx);
        }

        lastAlphabet = alphabet;
        ResultText.Text = string.Join(" ", codes.Select(a => PowFast(a, key, m)));
        InfoText.Text = "Alphabet: " + (hasEn ? "English" : "Ukrainian")
            + "\nCodes: " + string.Join(" ", codes);
    }

    void OnDecrypt(object? s, RoutedEventArgs e)
    {
        ErrorText.Text = ""; ResultText.Text = ""; InfoText.Text = "";
        if (!long.TryParse(MBox.Text, out long m) || m < 2 || m > 2000000000)
        { ErrorText.Text = "m must be an integer from 2 to 2000000000"; return; }
        if (!long.TryParse(BBox.Text, out long key) || key < 1)
        { ErrorText.Text = "Enter the secret key x in field b"; return; }

        var parts = (WordBox.Text ?? "").Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
        var nums = new List<long>();
        foreach (var part in parts)
        {
            if (!long.TryParse(part, out long v) || v < 0 || v >= m)
            { ErrorText.Text = "Enter numbers from 0 to m-1 separated by spaces"; return; }
            nums.Add(v);
        }
        if (nums.Count == 0)
        { ErrorText.Text = "Enter the encrypted numbers"; return; }

        var dec = nums.Select(b => PowFast(b, key, m)).ToList();
        ResultText.Text = Decode(dec, lastAlphabet);
        InfoText.Text = "Codes: " + string.Join(" ", dec)
            + "\nUkrainian: " + Decode(dec, UA)
            + "\nEnglish: " + Decode(dec, EN);
    }

    void OnElgKeys(object? s, RoutedEventArgs e)
    {
        ErrorText.Text = ""; ResultText.Text = ""; InfoText.Text = "";
        if (!ReadPrime(out long p)) return;
        if (!long.TryParse(ABox.Text, out long g) || g < 2 || g > p - 2)
        { ErrorText.Text = "g (field a) must be an integer from 2 to m-2"; return; }
        if (!IsGenerator(g, p))
        { ErrorText.Text = "g is not a generator of the group"; return; }
        if (!long.TryParse(XBox.Text, out long x) || x < 2 || x > p - 2)
        { ErrorText.Text = "Secret x (field Secret A) must be an integer from 2 to m-2"; return; }

        long y = PowFast(g, x, p);
        YBox.Text = y.ToString();
        ResultText.Text = y.ToString();
        InfoText.Text = "Public: p = " + p + ", g = " + g + ", y = " + y + "\nSecret: x = " + x;
    }

    bool ReadCodes(long p, out List<long> codes, out string alphabet)
    {
        codes = new List<long>();
        alphabet = UA;
        string word = (WordBox.Text ?? "").ToUpper().Replace(" ", "");
        if (word.Length == 0)
        { ErrorText.Text = "Enter a word"; return false; }

        bool hasUa = word.Any(c => UA.Contains(c));
        bool hasEn = word.Any(c => EN.Contains(c));
        if (hasUa && hasEn)
        { ErrorText.Text = "Do not mix Ukrainian and English letters"; return false; }
        if (!hasUa && !hasEn)
        { ErrorText.Text = "Use Ukrainian or English letters"; return false; }

        alphabet = hasEn ? EN : UA;
        foreach (char c in word)
        {
            int idx = alphabet.IndexOf(c) + 1;
            if (idx == 0)
            { ErrorText.Text = "Letter '" + c + "' is not in the selected alphabet"; return false; }
            if (idx >= p)
            { ErrorText.Text = "Letter code must be less than m"; return false; }
            codes.Add(idx);
        }
        return true;
    }

    void OnElgEnc(object? s, RoutedEventArgs e)
    {
        ErrorText.Text = ""; ResultText.Text = ""; InfoText.Text = "";
        if (!ReadPrime(out long p)) return;
        if (!long.TryParse(ABox.Text, out long g) || g < 2 || g > p - 2)
        { ErrorText.Text = "g (field a) must be an integer from 2 to m-2"; return; }
        if (!long.TryParse(YBox.Text, out long y) || y < 1 || y >= p)
        { ErrorText.Text = "Public key y (field Secret B) must be from 1 to m-1"; return; }
        if (!ReadCodes(p, out var codes, out string alphabet)) return;

        var keys = new List<long>();
        var kParts = (KBox.Text ?? "").Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
        if (kParts.Length == 0)
        {
            if (p - 3 < codes.Count)
            { ErrorText.Text = "m is too small for different session keys"; return; }
            var rnd = new Random();
            while (keys.Count < codes.Count)
            {
                long k = rnd.NextInt64(2, p - 1);
                if (!keys.Contains(k)) keys.Add(k);
            }
        }
        else
        {
            foreach (var part in kParts)
            {
                if (!long.TryParse(part, out long k) || k < 2 || k > p - 2)
                { ErrorText.Text = "Each session key must be an integer from 2 to m-2"; return; }
                keys.Add(k);
            }
            if (keys.Count != codes.Count)
            { ErrorText.Text = "Number of session keys must equal the number of letters"; return; }
            if (keys.Distinct().Count() != keys.Count)
            { ErrorText.Text = "Session keys must be different"; return; }
        }

        lastAlphabet = alphabet;
        var pairs = new List<string>();
        for (int i = 0; i < codes.Count; i++)
        {
            long a = PowFast(g, keys[i], p);
            long b = PowFast(y, keys[i], p) * codes[i] % p;
            pairs.Add("(" + a + "," + b + ")");
        }
        ResultText.Text = string.Join(" ", pairs);
        InfoText.Text = "Alphabet: " + (alphabet == EN ? "English" : "Ukrainian")
            + "\nCodes: " + string.Join(" ", codes)
            + "\nSession keys: " + string.Join(" ", keys);
    }

    void OnElgDec(object? s, RoutedEventArgs e)
    {
        ErrorText.Text = ""; ResultText.Text = ""; InfoText.Text = "";
        if (!ReadPrime(out long p)) return;
        if (!long.TryParse(XBox.Text, out long x) || x < 2 || x > p - 2)
        { ErrorText.Text = "Secret x (field Secret A) must be an integer from 2 to m-2"; return; }

        var nums = Regex.Matches(WordBox.Text ?? "", @"\d+")
            .Select(mt => long.Parse(mt.Value)).ToList();
        if (nums.Count == 0 || nums.Count % 2 != 0)
        { ErrorText.Text = "Enter pairs a,b: (29,33) (42,6) ..."; return; }
        if (nums.Any(v => v >= p))
        { ErrorText.Text = "All numbers must be less than m"; return; }

        var dec = new List<long>();
        for (int i = 0; i < nums.Count; i += 2)
        {
            long a = nums[i], b = nums[i + 1];
            if (a == 0)
            { ErrorText.Text = "a must not be 0"; return; }
            dec.Add(b * PowFast(a, p - 1 - x, p) % p);
        }

        ResultText.Text = Decode(dec, lastAlphabet);
        InfoText.Text = "Codes: " + string.Join(" ", dec)
            + "\nUkrainian: " + Decode(dec, UA)
            + "\nEnglish: " + Decode(dec, EN);
    }

    async Task CopyText(string? text, Button btn)
    {
        if (string.IsNullOrEmpty(text) || Clipboard == null) return;
        await Clipboard.SetTextAsync(text);
        btn.Content = "Copied";
        await Task.Delay(1200);
        btn.Content = "Copy";
    }

    async void OnCopy(object? s, RoutedEventArgs e) => await CopyText(ResultText.Text, CopyBtn);

}