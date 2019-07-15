# AzureCognitiveTranslator
Use [this NuGet package](https://www.nuget.org/packages/AzureCognitiveTranslator/) for batch translation on CognitiveServices Translator API 3.0

* This package can help you on Batch-Translation, **easily** and **quickly**. 
1. **How it work**: It'll transform your contents to some perfect packages for translating.
1. **Speed**: On my PC, about 300~500 items(*not characters*) per second.

Here are **steps**:


1. Create an instance of Translator with your BaseUrl and Key:

        Translator translator = new Translator(BaseUrl, Key);
1. Add content to the Translator:

        translator.AddContent("哈啰");
        //Here you can add many times, more than 100, 1000 or 10000.
        //You can also set the property "Contents" instead.
1. Get results aysnc:

        List<string> translation = await translator.TranslateAsync("en");



* More **Examples**

1. A simple example, add content to translator **one by one**

        //Get a instance of Translator:
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

1. **Batch** adding

        //Prepare a List<string>, in which there are many items to translate
        //Here is an Exmp List<string> with two items
        List<string> contents = new List<string>() { "哈啰，","世界！" };

        //Set the List<string> to the property "Contents"
        translator.Contents = contents;

        //Get results
        Console.WriteLine("Now translating, please wait for a moment...");
        List<string> translation = await translator.TranslateAsync("en");
        Console.WriteLine("Translation:");
        for (int i = 0; i < translation.Count; i++)
        {
            Console.WriteLine(translation[i]);
        }

1. Get **translatable** languages

        //Prepare a Dictionary<string code,Language>, you can also use "var" instead
        //Language is a Struct, consist of three string field: name, nativeName, dir
        Dictionary<string, Translator.Language> translatableLanguages = translator.TranslatableLanguages;

        foreach (string code in translatableLanguages.Keys)
        {
            Console.WriteLine();
            Console.WriteLine("code: {0}", code);
            Console.WriteLine("name: {0}  nativeName: {1}  dir: {2} ", translatableLanguages[code].name, translatableLanguages[code].nativeName, translatableLanguages[code].dir);
        }
