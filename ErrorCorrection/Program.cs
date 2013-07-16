﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

namespace ErrorCorrection
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            int mult;
            int divide;

            Stopwatch watch = new Stopwatch();
            Thread.Sleep( 250; );
            Thread.SpinWait( 1000 );

            watch.Start();

            // GF(2^4), with field generator poly p(x) = x^4 + x + 1 --> 10011 == 19 == 0x13
            // n = 15, k = 11, 2t = 4
            AntiduhEncoder encoder = new AntiduhEncoder( 4, 11, 0x13 );

            GaloisField field = new GaloisField( 16, 0x13 );

            mult = field.Mult( 10, 13 );
            divide = field.Divide( 11, 10 );
            watch.Stop();

            Console.Out.WriteLine( "10 * 13 = " + mult );
            Console.Out.WriteLine( "10 / 11 = " + divide );
            Console.Out.WriteLine( "Elapsed: " + watch.ElapsedTicks / (10.0*1000.0) );
            
            // GF(2^8) with field generatory poly p(x) = x^8 + x^4 + x^3 + x^2 + 1 ---> 100011101 == 285 == 0x011D
            // n = 255, k = 239, 2t = 16
            //AntiduhEncoder encoder = new AntiduhEncoder( 8, 239, 0x011D );

            Console.Out.Flush();
        }

        public static void PrintList( List<int> list )
        {
            for( int i = 0; i < list.Count; i++ )
            {
                Console.Out.WriteLine( "{0}: {1}", i, list[i] );
            }
        }

    }
}
