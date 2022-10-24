using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using NUnit.Framework;

public static class DictionaryExtensionMethods
        {
            public static bool ContentEquals<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, Dictionary<TKey, TValue> otherDictionary)
            {
                return (otherDictionary ?? new Dictionary<TKey, TValue>())
                    .OrderBy(kvp => kvp.Key)
                    .SequenceEqual((dictionary ?? new Dictionary<TKey, TValue>())
                                    .OrderBy(kvp => kvp.Key));
            }
        }


public class Program {
/*

Class JobSkillsCount
members: 
jobTitle - getting job title from Json file.
skillsCount - a dictionary to get skill's name as key and increment the count as it encountered again.

*/

    public class JobSkillsCount {
        string jobTitle;
        string fileName;
        string textfileName;
        Dictionary<string, int> skillsCount;
        HashSet<string> skillNameSet;
        List<List<Dictionary<string, object>>> Competencylist = new List<List<Dictionary<string, object>>>();
        
        /* constructor to set job title and init dcitonary */
        public JobSkillsCount(string file, string textFile) {
            fileName = file;
            textfileName = textFile;
            skillsCount = new Dictionary<string, int>();
            skillNameSet = new HashSet<string>();
        }

        /*returns a list of key value pairs sorted in descending order */
        public List<KeyValuePair<string, int>> sortSkillsDecsending() {
            List<KeyValuePair<string, int>> sortedDict = skillsCount.ToList();
            sortedDict.Sort(
                delegate(KeyValuePair<string, int> pair1,
                KeyValuePair<string, int> pair2) {
                    return pair1.Value.CompareTo(pair2.Value);
                }
            );
            sortedDict.Reverse();
            return sortedDict;
        }

        //set skill to 1 if seen for the first time, increment if encountered again
        public void setSkillCount(string skill) {
            if (skillsCount.ContainsKey(skill))
                skillsCount[skill] = (int)skillsCount[skill] + 1;
            else 
                skillsCount.Add(skill, 1);
        }
        
        /*get json content and fetch job title and competency list*/
        void getJsonContent() {
            using (StreamReader r = new StreamReader(fileName)) {
                string json = r.ReadToEnd();
                dynamic JsonParsed = JObject.Parse(json);
                dynamic resultJsonEntry = JsonParsed["result"].Children();
                foreach(dynamic result in resultJsonEntry) { //2
                    foreach(dynamic entry in result) {  //3
                        foreach(dynamic e in entry) { // vals
                            if (e.GetType() == typeof(Newtonsoft.Json.Linq.JArray)) {
                                foreach(dynamic arr in e) {
                                    var values = JObject.FromObject(arr).ToObject<Dictionary<string, object>>();
                                    bool prevTitle = false;
                                    bool prevComp = false;
                                    foreach(KeyValuePair<string, object> kvp in values) {
                                        if(kvp.Key == "_name" && (string)kvp.Value == "PositionTitle") {
                                            prevTitle = true;
                                        } else if (prevTitle) {
                                            jobTitle += (string)kvp.Value;
                                            prevTitle = false;
                                        } else if (kvp.Key == "_name" && (string)kvp.Value == "Competency") {
                                            prevComp = true;
                                        } else if (prevComp) {
                                            Competencylist = ((JArray)kvp.Value).ToObject<List<List<Dictionary<string, object>>>>();
                                        }
                                    }
                                }
                            }
                        } // entry
                    } // result
                } // resultJsonEntry
            } // using
        }

        /* return json file */
        public string getSortedJson() {
            getJsonContent();
            foreach(dynamic arr in Competencylist) {
                foreach(dynamic dict in arr) {
                    bool seenName = false;
                    bool skillAliasArray = false;
                    foreach(KeyValuePair<string, object> kvp in dict) { //get skillname 
                        if (kvp.Key == "_name" && (string)kvp.Value == "skillName") {
                            seenName = true;
                        } else if (seenName && (string)kvp.Key == "_value") {
                            // setSkillCount((string)kvp.Value);
                            skillNameSet.Add((string)kvp.Value);
                            seenName = false;
                        } else if (kvp.Key == "_name" && (string)kvp.Value == "skillAliasArray")  {
                            skillAliasArray = true;
                        } else if (skillAliasArray && (string)kvp.Key == "_value") {
                            // foreach(dynamic skill in (JArray)kvp.Value)  // Remove comment to add skillAliasArray to count
                            //     setSkillCount((string)skill);
                            // skillAliasArray = false;
                        }
                    }
                }
            }
            using (StreamReader r = new StreamReader(textfileName)) {
                string jobText = r.ReadToEnd().ToLower();
                // replace all non alphanumeric characters with empty string
                foreach(string skill in skillNameSet) {
                    //regex matches O(MxN)
                    int count = Regex.Matches(jobText, skill.ToLower()).Count;
                    skillsCount.Add(skill, count);
                }
            }
            var jsonList = sortSkillsDecsending();
            var transformedData = jsonList.Select(e => new {name = e.Key, count = e.Value});
            var JsonDict = new Dictionary<string, object>();
            JsonDict.Add("filename", textfileName); //format the json as per description
            JsonDict.Add("jobTitle", jobTitle);
            JsonDict.Add("Skills", transformedData);
            var output = Newtonsoft.Json.JsonConvert.SerializeObject(JsonDict, Formatting.Indented);
            // Console.WriteLine(output);
            return output;
        }

        //get counter for testing
        public Dictionary<string, int> getSkillCounterDict() {
            return skillsCount;
        }

        //print for debug
        public void printObject() {
            Console.WriteLine(jobTitle);
            foreach(KeyValuePair<string, int> de in skillsCount)
                Console.WriteLine(de.Key + " --> " + de.Value);
        }
    }
  


  public static void Main (string[] args) {
    if (args.Length != 3) {
        Console.WriteLine("How to use the program: ./prog <json file> <text file> {0}", args.Length);
        Environment.Exit(1);
    }
    try {
        string json = args[1];
        string txt = args[2];
        var myObj = new JobSkillsCount(json, txt);
        var result = myObj.getSortedJson();
        Console.Write(result);

    } catch  (Exception e) {
        Console.WriteLine("Error: {0}", e);
        Environment.Exit(1);
    }
  }
}
