using NUnit.Framework;
using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections;
using System.Collections.Specialized;

namespace Test
{
    public class Tests
    {

        [Test] //Expecting dictionaries to be equal.
        public void Test1()
        {
            Dictionary<string, int> ExpectedResults = new Dictionary<string, int>();
                ExpectedResults.Add("Oracle", 5);
                ExpectedResults.Add("SQL", 3);
                ExpectedResults.Add("UNIX", 0);
            
            var myObj = new Program.JobSkillsCount("jsonTest.json", "testText.txt");
            var _ = myObj.getSortedJson();
            var counter  = myObj.getSkillCounterDict();
            Debug.Assert(DictionaryExtensionMethods.ContentEquals(ExpectedResults, counter));
        }

        [Test] //check for the complement
        public void Test2()
        {
            Dictionary<string, int> ExpectedResults = new Dictionary<string, int>();
                ExpectedResults.Add("Oracle", 1);
                ExpectedResults.Add("SQL", 4);
                ExpectedResults.Add("UNIX", 0);
            
            var myObj = new Program.JobSkillsCount("jsonTest.json", "testText.txt");
            var _ = myObj.getSortedJson();
            var counter  = myObj.getSkillCounterDict();
            Debug.Assert(!DictionaryExtensionMethods.ContentEquals(ExpectedResults, counter));
        }

        //helper for Test3
        static bool isSorted(List<KeyValuePair<string, int>> a) {
            int j = a.Count;
            if (j < 1) return true;
            for (int i=1; i < j; ++i)
                if (a[i-1].Value < a[i].Value) return false;
            return true;
        }
        
        [Test] //test if sort function works by reutrning the list of key value pairs
        public void Test3()
        {   
            var myObj = new Program.JobSkillsCount("jsonTest.json", "testText.txt");
            var _ = myObj.getSortedJson();
            var list  = myObj.sortSkillsDecsending();
            Debug.Assert(isSorted(list));
        }
    }
}