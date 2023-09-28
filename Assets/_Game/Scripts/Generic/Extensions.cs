using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    private static System.Random rng = new System.Random();

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static string ShuffleString(this string str)
    {
        char[] array = str.ToCharArray();
        int n = array.Length;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            var value = array[k];
            array[k] = array[n];
            array[n] = value;
        }
        return new string(array);
    }

    public static void ShuffleParallel<T1, T2>(this IList<T1> listA, IList<T2> listB)
    {
        if (listA.Count != listB.Count) throw new ArgumentException();
        int n = listA.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T1 valueA = listA[k];
            listA[k] = listA[n];
            listA[n] = valueA;
            T2 valueB = listB[k];
            listB[k] = listB[n];
            listB[n] = valueB;
        }
    }

    public static void InsertionSort<T>(IList<T> list, Comparison<T> comparison)
    {
        if (list == null)
            throw new ArgumentNullException("list");
        if (comparison == null)
            throw new ArgumentNullException("comparison");

        int count = list.Count;
        for (int j = 1; j < count; j++)
        {
            T key = list[j];

            int i = j - 1;
            for (; i >= 0 && comparison(list[i], key) > 0; i--)
            {
                list[i + 1] = list[i];
            }
            list[i + 1] = key;
        }
    }

    public static string AddOrdinal(int num)
    {
        if (num <= 0) return num.ToString();

        switch (num % 100)
        {
            case 11:
            case 12:
            case 13:
                return num + "th";
        }

        switch (num % 10)
        {
            case 1:
                return num + "st";
            case 2:
                return num + "nd";
            case 3:
                return num + "rd";
            default:
                return num + "th";
        }
    }

    public static string NumberToWords(int number)
    {
        if (number == 0)
            return "zero";

        if (number < 0)
            return "minus " + NumberToWords(Math.Abs(number));

        string words = "";

        if ((number / 1000000) > 0)
        {
            words += NumberToWords(number / 1000000) + " million ";
            number %= 1000000;
        }

        if ((number / 1000) > 0)
        {
            words += NumberToWords(number / 1000) + " thousand ";
            number %= 1000;
        }

        if ((number / 100) > 0)
        {
            words += NumberToWords(number / 100) + " hundred ";
            number %= 100;
        }

        if (number > 0)
        {
            if (words != "")
                words += "and ";

            var unitsMap = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
            var tensMap = new[] { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

            if (number < 20)
                words += unitsMap[number];
            else
            {
                words += tensMap[number / 10];
                if ((number % 10) > 0)
                    words += "-" + unitsMap[number % 10];
            }
        }

        return words;
    }

    public static string ForceFirstCharToUpper(string st)
    {
        if (string.IsNullOrEmpty(st) || st.Length == 1)
            return st;
        else
            return $"{st[0].ToString().ToUpperInvariant()}{st.Substring(1)}";
    }

    public static bool Spellchecker(string input, List<string> validAnswers)
    {
        double similarity = 0;
        for (int j = 0; j < validAnswers.Count; j++)
        {
            similarity = input.ToLowerInvariant().CalculateSimilarity(validAnswers[j].ToLowerInvariant());
            if (similarity > 0.8)
            {
                return true;
            }
        }
        return false;
    }

    public static bool IsPrime(this int n)
    {
        if (n == 2 || n == 3)
            return true;

        if (n <= 1 || n % 2 == 0 || n % 3 == 0)
            return false;

        for (int i = 5; i * i <= n; i += 6)
        {
            if (n % i == 0 || n % (i + 2) == 0)
                return false;
        }

        return true;
    }

    public static T PickRandom<T>(this IList<T> list)
    {
        return list[UnityEngine.Random.Range(0, list.Count)];
    }

    public static T PickRandom<T>(this T[] arr)
    {
        return arr[UnityEngine.Random.Range(0, arr.Length)];
    }

    public static string PointOrPoints(int x)
    {
        return x == 1 ? $"{x} POINT" : $"{x} POINTS";
    }
}
