using BubbleChartOilWells.Contracts;
using BubbleChartOilWells.Contracts.Models.ViewModels;
using RDotNet;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using BubbleChartOilWells.Contracts.Models.Dto;
using BubbleChartOilWells.DataAccess.Repositories;

namespace BubbleChartOilWells.BusinessLogic.Services
{
    public class NeuralNetService
    {
        private REngine engine;

        private List<string> rFunctions = new List<string>()
        {
            "normalize.R",
            "extended_neuralnet.R",
            "predict_neuralnet.R",
            "split_data.R",
            "create_formula.R"
        };

        private const int SKIP_COLUMNS_COUNT = 5;

        public NeuralNetService()
        {
            REngine.SetEnvironmentVariables();
            engine = REngine.GetInstance();
        }

        public ResultResponse<NeuralNetVM> TrainNeuralNetwork(NeuralNetVM nnVM)
        {
            try
            {
                if (!engine.IsRunning) engine = REngine.GetInstance();

                // connecting libraries
                engine.Evaluate($@"source(""{Path.GetFullPath("RFunctions/libraries.R").Replace(@"\", @"/")}"")");

                // defining functions
                foreach (var rFuncion in rFunctions)
                {
                    engine.Evaluate($@"source(""{Path.GetFullPath("RFunctions/" + rFuncion).Replace(@"\", @"/")}"")");
                }

                engine.Evaluate(
                    $@"results <- extended_neuralnet(""{nnVM.TrainingDataFileName.Replace(@"\", @"/")}"",
                                                        0.9,
                                                        {RArrayConverter(nnVM.Hidden)},
                                                        {nnVM.Threshold.ToString(System.Globalization.CultureInfo.InvariantCulture)},
                                                        {nnVM.Stepmax.ToString(System.Globalization.CultureInfo.InvariantCulture)},
                                                        {nnVM.Rep.ToString(System.Globalization.CultureInfo.InvariantCulture)},
                                                        {nnVM.LearningRate.ToString(System.Globalization.CultureInfo.InvariantCulture)},
                                                        ""{nnVM.Algorithm}"",
                                                        ""{nnVM.ErrorFunction}"",
                                                        ""{nnVM.ActivationFunction}"",
                                                        12421)");
                engine.Evaluate(
                    $@"TrainMAPE <- results$TrainMAPE
                    TrainMAE <- results$TrainMAE
                    TrainSSE <- results$TrainSSE

                    TestMAPE <- results$TestMAPE
                    TestMAE <- results$TestMAE

                    TrainActual <- results$TrainActual
                    TrainPredicted <- results$TrainPredicted
                    
                    TestActual <- results$TestActual
                    TestPredicted <- results$TestPredicted
            
                    BindedTrainResults <- results$BindedTrainResults
                    BindedTestResults <- results$BindedTestResults
                    
                    BestNNIndex <- results$BestNNIndex
                    NN <- results$NN

                    InitialTrainData <- results$InitialTrainData
                    Error <- results$NN$result.matrix[1] 
                    Headers <- results$Headers");


                // engine.SetSymbol("Headers", engine.CreateCharacterVector(nnVM.ImportedDataHeaders));
                // var debugHeaders = engine.GetSymbol("Headers").AsVector();
                //

                Directory.CreateDirectory(Path.GetDirectoryName(nnVM.TrainingDataFileName) +
                                          $"\\Results");
                var trainResultsFileName = Path.GetDirectoryName(nnVM.TrainingDataFileName) +
                                           $"\\Results\\TrainResults~{DateTime.Now.ToString("dd.MM.yyyy_HH.mm")}.csv";
                var testResultsFileName = Path.GetDirectoryName(nnVM.TrainingDataFileName) +
                                          $"\\Results\\TestResults~{DateTime.Now.ToString("dd.MM.yyyy_HH.mm")}.csv";
                engine.Evaluate(
                    $@"
                    write.table(BindedTrainResults, row.names = FALSE, file = ""{trainResultsFileName.Replace(@"\", @"/")}"", fileEncoding = ""UTF-8"", sep = "";"", quote=FALSE)
                    write.table(BindedTestResults, row.names = FALSE, file = ""{testResultsFileName.Replace(@"\", @"/")}"", fileEncoding = ""UTF-8"", sep = "";"", quote=FALSE)");

                nnVM.TrainMAPE = engine.GetSymbol("TrainMAPE").AsNumeric().ToList()[0];
                nnVM.TrainMAE = engine.GetSymbol("TrainMAE").AsNumeric().ToList()[0];
                nnVM.TrainSSE = engine.GetSymbol("TrainSSE").AsNumeric().ToList()[0];

                nnVM.TestMAPE = engine.GetSymbol("TestMAPE").AsNumeric().ToList()[0];
                nnVM.TestMAE = engine.GetSymbol("TestMAE").AsNumeric().ToList()[0];

                nnVM.TrainActual = engine.GetSymbol("TrainActual").AsNumeric().ToList();
                nnVM.TrainPredicted = engine.GetSymbol("TrainPredicted").AsNumeric().ToList();

                nnVM.TestActual = engine.GetSymbol("TestActual").AsNumeric().ToList();
                nnVM.TestPredicted = engine.GetSymbol("TestPredicted").AsNumeric().ToList();

                //nnVM.Error = engine.GetSymbol("Error").AsNumeric().ToList()[0];

                nnVM.NN = engine.GetSymbol("NN");
                nnVM.InitialTrainData = engine.GetSymbol("InitialTrainData");
                nnVM.BestNNIndex = engine.GetSymbol("BestNNIndex").AsNumeric().ToList()[0];

                nnVM.TrainFullResults = CsvToDataTable(trainResultsFileName);
                nnVM.TestFullResults = CsvToDataTable(testResultsFileName);

                return ResultResponse<NeuralNetVM>.GetSuccessResponse(nnVM);
            }
            catch (Exception e)
            {
                // ConvertImportedDataHeaders(nnVM.TrainingDataFileName, nnVM.ImportedDataHeaders);
                // TODO: write error to log
                return ResultResponse<NeuralNetVM>.GetErrorResponse($@"Ошибка обучения нейросети.{Environment.NewLine}
                                                                    {e.Message}{Environment.NewLine}
                                                                    {e.StackTrace}");
            }
        }

        public ResultResponse<NeuralNetVM> PredictLiquidDebitGain(NeuralNetVM nnVM)
        {
            try
            {
                // connecting libraries
                engine.Evaluate($@"source(""{Path.GetFullPath("RFunctions/libraries.R").Replace(@"\", @"/")}"")");

                // defining functions
                foreach (var rFuncion in rFunctions)
                {
                    engine.Evaluate($@"source(""{Path.GetFullPath("RFunctions/" + rFuncion).Replace(@"\", @"/")}"")");
                }


                engine.SetSymbol("NN", nnVM.NN as SymbolicExpression);
                engine.SetSymbol("TrainData", nnVM.InitialTrainData as SymbolicExpression);
                engine.SetSymbol("BestNNIndex", engine.CreateNumeric(nnVM.BestNNIndex));

                // setting directory for files
                engine.Evaluate(
                    $@"results <- predict_neuralnet(""{nnVM.PredictDataFileName.Replace(@"\", @"/")}"",
                                                    TrainData,
                                                    NN,
                                                    BestNNIndex)");

                engine.Evaluate($@"BindedPredictionResults <- results$BindedPredictionResults");

                var predictionResultsFileName = Path.GetDirectoryName(nnVM.PredictDataFileName) +
                                                $"\\Results\\PredictionResults~{DateTime.Now.ToString("dd.MM.yyyy_HH.mm")}.csv";
                engine.Evaluate(
                    $@"
                    write.table(BindedPredictionResults, row.names = FALSE, file = ""{predictionResultsFileName.Replace(@"\", @"/")}"", fileEncoding = ""UTF-8"", sep = "";"", quote=FALSE)");

                nnVM.PredictionFullResults = CsvToDataTable(predictionResultsFileName);

                return ResultResponse<NeuralNetVM>.GetSuccessResponse(nnVM);
            }
            catch (Exception e)
            {
                return ResultResponse<NeuralNetVM>.GetErrorResponse(
                    $@"Ошибка вычисления прироста дебита жидкости.{Environment.NewLine}
                                                              {e.Message}{Environment.NewLine}
                                                              {e.StackTrace}");
            }
        }

        public DataTable CsvToDataTable(string fileName)
        {
            var dataTable = new DataTable();

            using (var stream = new StreamReader(fileName, System.Text.Encoding.GetEncoding("UTF-8")))
            {
                var headers = stream.ReadLine().Split(';').ToList();
                headers.ForEach(x => dataTable.Columns.Add(x.Replace(".", " ")));
                
                
                for (int i = 0; i < dataTable.Columns.Count; i++)
                {
                    dataTable.Columns[i].DataType = i < 5 ? typeof(string) : typeof(Decimal);
                }

                while (!stream.EndOfStream)
                {
                    var line = stream.ReadLine().Split(';').ToList();

                    DataRow newRow = dataTable.Rows.Add();
                    for (int j = 0; j < line.Count; j++)
                    {
                        try
                        {
                            if (j < 5) throw new Exception();

                            var parsedValue = Decimal.Parse(line[j],
                                new NumberFormatInfo() {NumberDecimalSeparator = "."});
                            newRow[j] = Math.Round(parsedValue, 2);
                        }
                        catch
                        {
                            newRow[j] = line[j];
                        }
                    }
                }

                stream.Close();
            }

            return dataTable;
        }

        private string RArrayConverter(int[] array)
        {
            var result = "c(";

            for (int i = 0; i < array.Length; i++)
            {
                result += array[i].ToString();
                if (i + 1 != array.Length)
                {
                    result += ", ";
                }
            }

            result += ")";

            return result;
        }
    }
}