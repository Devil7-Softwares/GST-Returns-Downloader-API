using System;
using System.Collections.Generic;
using System.Text;

namespace Devil7.Automation.GSTR.Downloader.Misc {
    /// <summary>
    /// Based on CA Advanced Authendication Client's Javascript Library. For Spoofing getDNA function ;p
    /// </summary>
    public class MESC {
        private static long calibrationStartTime = 0;
        private static int mescIterationCount = 0;
        private static string mescValue = "";
        private static int mesccalibrationDuration = 150;
        private static int mescmaxIterations = 2;
        private static int mescintervalDelay = 30;
        private static void stopRun () {
            try {
                long endTime = getTime ();
                long elapsedTime = endTime - calibrationStartTime;
                Console.WriteLine ("MESC stopRun --- updating mescValue with elapsed " + elapsedTime);
                MESC.mescValue = MESC.mescValue + ";ldi=" + elapsedTime;
            } catch (Exception e) { Console.WriteLine (e.Message); }
        }
        private static int calculateMESC () {
            int num_iter = 0;
            try {
                long currentTime = getTime ();
                long endTime = getTime () + MESC.mesccalibrationDuration;
                while (currentTime < endTime) {
                    num_iter++;
                    currentTime = getTime ();
                }
            } catch (Exception e) {
                Console.WriteLine ("Error: Unable to invoke MESC method");
                Console.WriteLine ("Error: " + e.Message);
            }
            return num_iter;
        }
        private static double getAverageMESC () {
            double numberOfSamples = 1;
            double average = 0;
            try {
                double total = 0;
                for (int i = 0; i < numberOfSamples; i++) {
                    double sample = MESC.calculateMESC ();
                    total += sample;
                }
                average = Math.Round (total / numberOfSamples);
            } catch (Exception e) { Console.WriteLine (e.Message); }
            return average;
        }

        private static void newCollectMESCFunc () {
            MESC.mescIterationCount = 0;
            try {
                object newVal = MESC.getAverageMESC ();
                MESC.mescValue += ";mesc=" + newVal;
                MESC.mescIterationCount += 1;
                while (MESC.mescIterationCount < MESC.mescmaxIterations) {
                    System.Threading.Thread.Sleep (MESC.mescintervalDelay);
                    newVal = MESC.getAverageMESC ();
                    MESC.mescValue += ";mesc=" + newVal;
                    MESC.mescIterationCount += 1;
                }
            } catch (Exception e) { Console.WriteLine (e.Message); }
        }
        public static string getMESC () {
            string mesc = "";
            MESC.mescValue = "mi=" + MESC.mescmaxIterations + ";cd=" + MESC.mesccalibrationDuration + ";id=" + MESC.mescintervalDelay;
            try {
                MESC.newCollectMESCFunc ();
                mesc = MESC.mescValue;
            } catch (Exception e) { Console.WriteLine (e.Message); }
            return mesc;
        }

        private static long getTime () {
            long retval = 0;
            var st = new DateTime (1970, 1, 1);
            TimeSpan t = (DateTime.Now.ToUniversalTime () - st);
            retval = (long) (t.TotalMilliseconds + 0.5);
            return retval;
        }
    }
}