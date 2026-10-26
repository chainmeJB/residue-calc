using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Collections.Generic;

namespace ResidueCalc;

public partial class MainWindow : Window
{
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
        MBox.Text = ABox.Text = BBox.Text = ErrorText.Text = InfoText.Text = "";
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
}