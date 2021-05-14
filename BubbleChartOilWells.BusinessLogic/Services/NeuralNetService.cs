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
            "split_data.R"
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

                // getting original headers
                nnVM.ImportedDataHeaders = ConvertImportedDataHeaders(nnVM.TrainingDataFileName);

                engine.Evaluate(
                    $@"Sys.setlocale(""LC_ALL"", "".1251"")
                    results <- extended_neuralnet(""{nnVM.TrainingDataFileName.Replace(@"\", @"/")}"",
                                                        0.8,
                                                        formula = {CreateNeuralNetFormula(nnVM.ImportedDataHeaders.Count, SKIP_COLUMNS_COUNT)},
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
                    Error <- results$NN$result.matrix[1] ");

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

                var bindedTrainResults = engine.GetSymbol("BindedTrainResults").AsDataFrame();
                var bindedTestResults = engine.GetSymbol("BindedTestResults").AsDataFrame();
                
                nnVM.TrainFullResults = RDataFrameToDataTableConverter(bindedTrainResults, nnVM.ImportedDataHeaders);
                nnVM.TestFullResults = RDataFrameToDataTableConverter(bindedTestResults, nnVM.ImportedDataHeaders);

                ConvertImportedDataHeaders(nnVM.TrainingDataFileName, nnVM.ImportedDataHeaders);

                WriteDataTableToCsv(
                    Path.GetDirectoryName(nnVM.TrainingDataFileName) +
                    $"\\TrainResults~{DateTime.Now.ToString("dd.MM.yyyy_HH.mm")}.csv", nnVM.TrainFullResults);
                WriteDataTableToCsv(
                    Path.GetDirectoryName(nnVM.TrainingDataFileName) +
                    $"\\TestResults~{DateTime.Now.ToString("dd.MM.yyyy_HH.mm")}.csv", nnVM.TestFullResults);

                return ResultResponse<NeuralNetVM>.GetSuccessResponse(nnVM);
            }
            catch (Exception e)
            {
                ConvertImportedDataHeaders(nnVM.TrainingDataFileName, nnVM.ImportedDataHeaders);
                // TODO: write error to log
                return ResultResponse<NeuralNetVM>.GetErrorResponse($@"Ошибка обучения нейросети.{Environment.NewLine}
                                                                    {e.Message}{Environment.NewLine}
                                                                    {e.StackTrace}");
            }
        }

        public void WriteDataTableToCsv(string fileName, DataTable dataTable)
        {
            try
            {
                using (var stream = new StreamWriter(fileName, false, System.Text.Encoding.GetEncoding("Windows-1251")))
                {
                    // writing columns
                    var columns = dataTable.Columns;
                    for (int i = 0; i < columns.Count - 1; i++)
                    {
                        stream.Write(columns[i].ColumnName + ";");
                    }

                    stream.WriteLine(columns[columns.Count - 1].ColumnName);

                    // writing data
                    foreach (DataRow row in dataTable.Rows)
                    {
                        for (int i = 0; i < columns.Count - 1; i++)
                        {
                            stream.Write(row[i] + ";");
                        }

                        stream.WriteLine(row[columns.Count - 1]);
                    }

                    stream.Close();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public ResultResponse<NeuralNetVM> PredictLiquidDebitGain(NeuralNetVM nnVM)
        {
            var predictionDataHeaders = new List<string>();
            try
            {
                predictionDataHeaders = ConvertImportedDataHeaders(nnVM.PredictDataFileName);

                // connecting libraries
                engine.Evaluate($@"source(""{Path.GetFullPath("RFunctions/libraries.R").Replace(@"\", @"/")}"")");

                // defining functions
                engine.Evaluate(
                    $@"source(""{Path.GetFullPath("RFunctions/predict_neuralnet.R").Replace(@"\", @"/")}"")");

                engine.SetSymbol("NN", nnVM.NN as SymbolicExpression);
                engine.SetSymbol("TrainData", nnVM.InitialTrainData as SymbolicExpression);
                engine.SetSymbol("BestNNIndex", engine.CreateNumeric(nnVM.BestNNIndex));

                // setting directory for files
                engine.Evaluate($@"setwd(""{Path.GetDirectoryName(nnVM.PredictDataFileName).Replace(@"\", @"/")}"")");
                engine.Evaluate(
                    $@"results <- predict_neuralnet(""{nnVM.PredictDataFileName.Replace(@"\", @"/")}"",
                                                    TrainData,
                                                    NN,
                                                    BestNNIndex)");

                engine.Evaluate($@"BindedPredictionResults <- results$BindedPredictionResults");


                var PredictionResult = engine.GetSymbol("BindedPredictionResults").AsDataFrame();
                nnVM.PredictionFullResults = RDataFrameToDataTableConverter(PredictionResult, nnVM.ImportedDataHeaders);

                ConvertImportedDataHeaders(nnVM.PredictDataFileName, predictionDataHeaders);

                WriteDataTableToCsv(
                    Path.GetDirectoryName(nnVM.PredictDataFileName) +
                    $"\\PredictionResults~{DateTime.Now.ToString("dd.MM.yyyy_HH.mm")}.csv", nnVM.PredictionFullResults);

                return ResultResponse<NeuralNetVM>.GetSuccessResponse(nnVM);
            }
            catch (Exception e)
            {
                ConvertImportedDataHeaders(nnVM.PredictDataFileName, predictionDataHeaders);
                return ResultResponse<NeuralNetVM>.GetErrorResponse(
                    $@"Ошибка вычисления прироста дебита жидкости.{Environment.NewLine}
                                                              {e.Message}{Environment.NewLine}
                                                              {e.StackTrace}");
            }
        }

        private List<string> ConvertImportedDataHeaders(string fileName, List<string> targetHeaders = null)
        {
            var headers = new List<string>();
            using (var stream = new StreamReader(fileName, System.Text.Encoding.GetEncoding("Windows-1251")))
            {
                var line = stream.ReadLine();
                headers = line.Split(';').ToList();

                stream.Close();
            }

            if (targetHeaders == null)
            {
                targetHeaders = new List<string>();
                for (int j = 0; j < headers.Count; j++)
                {
                    targetHeaders.Add($@"x{j}");
                }
            }

            var lines = File.ReadAllLines(fileName, System.Text.Encoding.GetEncoding("Windows-1251")).ToList();
            using (var stream = new StreamWriter(fileName, false, System.Text.Encoding.GetEncoding("Windows-1251")))
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    if (i == 0)
                    {
                        for (int j = 0; j < headers.Count - 1; j++)
                        {
                            stream.Write(targetHeaders[j] + ";");
                        }

                        stream.WriteLine(targetHeaders.Last());
                    }
                    else
                    {
                        stream.WriteLine(lines[i]);
                    }
                }

                stream.Close();
            }

            return headers;
        }

        private string CreateNeuralNetFormula(int headerCount, int skipColumnsCount)
        {
            var result = $@"x{headerCount - 1} ~ ";

            headerCount -= skipColumnsCount;
            for (int i = 0; i < headerCount - 1; i++)
            {
                if (i == headerCount - 2)
                    result += $@"x{i + skipColumnsCount}";
                else
                    result += $@"x{i + skipColumnsCount} + ";
            }

            return result;
        }

        private DataTable RDataFrameToDataTableConverter(DataFrame dataFrame, List<string> fileHeaders)
        {
            var dataTable = new DataTable();
            for (int i = 0; i < dataFrame.ColumnCount - 1; ++i)
            {
                dataTable.Columns.Add(fileHeaders[i]);
            }
            dataTable.Columns.Add(dataFrame.ColumnNames.Last());
            
            for (int i = 0; i < dataFrame.RowCount; i++)
            {
                DataRow newRow = dataTable.Rows.Add();
                for (int j = 0; j < dataFrame.ColumnCount; j++)
                {
                    newRow[j] = (dataFrame[i, j].GetType() == typeof(double))
                        ? Math.Round((double) dataFrame[i, j], 4)
                        : dataFrame[i, j];
                }
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