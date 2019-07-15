using System;
using AzureCognitiveTranslator;
using resources=ExmpForTranslator.Properties.Resources;
using System.Collections.Generic;
using System.Threading.Tasks;
//This Exmp program use AzureCognitive Nuget by mujiannan
//For Microsoft Azure Cognitive Translator Text API v3.0
namespace ExmpForTranslator
{
    class Program
    {
        private static Translator CreateNewTranslator()
        {
            //MyAzureCognitiveBaseUrl is determined by your religion
            //You can find the proper BaseUrl at https://docs.microsoft.com/en-us/azure/cognitive-services/translator/reference/v3-0-reference
            return new Translator(resources.MyAzureCognitiveBaseUrl, Secret.MyAzureCogitiveKey);
        }
        private static List<string> _indicates;
        static void Main()
        {
            _indicates = new List<string>() { "0. Exit", "1. " + exmp1, "2. " + exmp2, "3. "+exmp3 };
            SelectExmpAsync().Wait();
        }
        private async static Task SelectExmpAsync()
        {
            do
            {
                Console.WriteLine("Please Select:");
                for (int i = 0; i < _indicates.Count; i++)
                {
                    Console.WriteLine(_indicates[i]);
                }
                string input = Console.ReadLine();
                if (input == "0")
                {
                    break;
                }
                switch (input)
                {
                    case "1":
                        await Exmp1();
                        break;
                    case "2":
                        await Exmp2();
                        break;
                    case "3":
                        Exmp3();
                        break;
                    default:
                        break;
                }
            } while (true);
        }
        private static readonly string exmp1 = "A simple example, add content to translator one by one";
        private static async Task Exmp1()//
        {
            Console.WriteLine();
            Console.WriteLine("<Exmp1 {0}>",exmp1);
            //Get a instance of Translator
            Translator translator = new Translator(resources.MyAzureCognitiveBaseUrl, Secret.MyAzureCogitiveKey);

            translator.AddContent("哈啰，");//Add a string object to the translator
            translator.AddContent("世界！");//Add another

            Console.WriteLine("Now translating, please wait for a moment...");

            //Create a List<string> to receive result, and don't forgeive to give "TranslateAsync" a languge code
            List<string> translation = await translator.TranslateAsync("en");//You can get language codes by Translator Property "TranlatableLanguages"
            Console.WriteLine("Translation:");
            for (int i = 0; i < translation.Count; i++)
            {
                Console.WriteLine(translation[i]);
            }
            Console.WriteLine("</Exmp1>");
            Console.WriteLine();
        }
        private static readonly string exmp2 = "A simple example, batch adding";
        private static async Task Exmp2()
        {
            Console.WriteLine();
            Console.WriteLine("<Exmp2 {0}>",exmp2);
            Translator translator = CreateNewTranslator();

            //Prepare a List<string>, in which there are many items to translate
            //Here is an Exmp List<string> with two items
            List<string> contents = new List<string>() { "哈啰，","世界！" };

            //Set the List<string> to the property "Contents"
            translator.Contents = contents;

            //Get the result
            Console.WriteLine("Now translating, please wait for a moment...");
            List<string> translation = await translator.TranslateAsync("en");
            Console.WriteLine("Translation:");
            for (int i = 0; i < translation.Count; i++)
            {
                Console.WriteLine(translation[i]);
            }
            Console.WriteLine("</Exmp2>");
            Console.WriteLine();
        }

        private static readonly string exmp3 = "Get translatable languages";
        private static void Exmp3()
        {
            Console.WriteLine();
            Console.WriteLine("<Exmp3 {0}>", exmp3);
            Translator translator = CreateNewTranslator();

            //Prepare a Dictionary<string code,Language>, you can use "var" insteadly
            //Language is a Struct, consist of three string field: Name, nativeName
            Dictionary<string, Translator.Language> translatableLanguages = translator.TranslatableLanguages;

            foreach (string code in translatableLanguages.Keys)
            {
                Console.WriteLine();
                Console.WriteLine("code: {0}", code);
                Console.WriteLine("name: {0}  nativeName: {1}  dir: {2} ", translatableLanguages[code].name, translatableLanguages[code].nativeName, translatableLanguages[code].dir);
            }
            Console.WriteLine();
            Console.WriteLine("</Exmp3>");
            Console.WriteLine();
        }
    }
}
