using BDOLanguageUpdater.Service.Serializer;

var ENGLISH_FILE_PATH = "languagedata_en.loc";
var SPANISH_FILE_PATH = "languagedata_es.loc";
var FINAL_FILE_PATH = "languagedata_final.loc";

var englishFileContent = File.ReadAllBytes(ENGLISH_FILE_PATH);
var spanishFileContent = File.ReadAllBytes(SPANISH_FILE_PATH);

var locSerializer = new LocSerializer();

var englishTSV = await locSerializer.Deserialize(typeof(string), englishFileContent, string.Empty);
var spanishTSV = await locSerializer.Deserialize(typeof(string), spanishFileContent, string.Empty);
var finalTsv = DictionaryUtils.Merge(englishTSV.ToString(), spanishTSV.ToString());

var finalFile = await locSerializer.Serialize(finalTsv);   
await File.WriteAllBytesAsync(FINAL_FILE_PATH, finalFile);