using BDOLanguageUpdater.Service.Serializer;
using System.Text;

var ENGLISH_FILE_PATH = "D:\\dev\\bdo-language-updater\\TestApp\\languagedata_en.loc";
var SPANISH_FILE_PATH = "D:\\dev\\bdo-language-updater\\TestApp\\languagedata_es.loc";
var FINAL_FILE_PATH = "D:\\dev\\bdo-language-updater\\TestApp\\languagedata_final.loc";

var englishFileContent = File.ReadAllBytes(ENGLISH_FILE_PATH);
var spanishFileContent = File.ReadAllBytes(SPANISH_FILE_PATH);

var englishTSV = await LocSerializer.Decompress(englishFileContent);
var spanishTSV = await LocSerializer.Decompress(spanishFileContent);

var finalTsv = DictionaryUtils.Merge(englishTSV, spanishTSV);
File.WriteAllText(FINAL_FILE_PATH, finalTsv, Encoding.Unicode);
var finalFile = await LocSerializer.Compress(finalTsv);

//File.WriteAllBytes(FINAL_FILE_PATH, finalFile);
