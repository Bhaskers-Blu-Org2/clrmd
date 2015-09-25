﻿using System;
using System.IO;

class Program
{
    public static void Main(string[] args)
    {
        Outer();    /* seq */
    }

    private static void Outer()
    {
        Middle();    /* seq */
    }

    private static void Middle()
    {
        Inner();    /* seq */
    }

    private static void Inner()
    {
        try
        {
            throw new FileNotFoundException("FNF Message");    /* seq */
        }
        catch (FileNotFoundException e)
        {
            throw new InvalidOperationException("IOE Message", e);    /* seq */
        }
    }
}
