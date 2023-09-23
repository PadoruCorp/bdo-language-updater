using BDOLanguageUpdater.Service.Serializer;

var ENGLISH_FILE_PATH = "languagedata_en.loc";
var SPANISH_FILE_PATH = "languagedata_es.loc";
var FINAL_FILE_PATH = "languagedata_final.loc";

var englishFileContent = File.ReadAllBytes(ENGLISH_FILE_PATH);
var spanishFileContent = File.ReadAllBytes(SPANISH_FILE_PATH);

var englishTSV = await LocSerializer.Decompress(englishFileContent);
var spanishTSV = await LocSerializer.Decompress(spanishFileContent);
var finalTsv = DictionaryUtils.Merge(englishTSV, spanishTSV);

var finalFile = await LocSerializer.Compress(finalTsv);
File.WriteAllBytes(FINAL_FILE_PATH, finalFile);
