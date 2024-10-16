﻿using System;
using System.Text.RegularExpressions;
using Yale.Engine;

namespace Yale.InteractiveConsole;

internal class Program
{
    private readonly ComputeInstance instance =
        new(
            options: new ComputeInstanceOptions
            {
                Recalculate = ComputeInstanceOptions.RecalculateMode.Lazy
            }
        );
    private readonly Regex isValue = new("^[a-zA-Z]+[=][\\w]+$");
    private readonly Regex isExpression = new("[a-zA-Z]+[:].+$");
    private readonly Regex isEvaluate = new("[a-zA-Z]+");

    private void Run()
    {
        Console.WriteLine(
            "Syntax:\nAdd value: x=3 \nAdd expression: square:x^2 \nEvaluate: square\n\n\n"
        );

        while (true)
        {
            var input = Console.ReadLine().Trim();

            if (input == "exit")
                break;

            if (isValue.IsMatch(input))
                AddValue(input);
            else if (isExpression.IsMatch(input))
                AddExpression(input);
            else if (isEvaluate.IsMatch(input))
                TryEvaluate(input);
        }
    }

    private void TryEvaluate(string input)
    {
        if (instance.ContainsExpression(input))
        {
            var result = instance.GetResult(input);
            Console.WriteLine($"Result: {result}");
        }
        else
        {
            Console.WriteLine("Expression not found");
        }
    }

    private void AddExpression(string input)
    {
        var values = input.Split(':');
        instance.SetExpression(values[0], values[1]);
    }

    private void AddValue(string input)
    {
        var values = input.Split('=');
        var key = values[0];
        var value = values[1];

        if (bool.TryParse(value, out var boolean))
        {
            instance.Variables[key] = boolean;
        }
        else if (int.TryParse(value, out var integer))
        {
            instance.Variables[key] = integer;
        }
        else if (double.TryParse(value, out var number))
        {
            instance.Variables[key] = number;
        }
        else
        {
            instance.Variables[key] = value;
        }
    }

    private static void Main(string[] _) => new Program().Run();
}
