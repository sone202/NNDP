using BubbleChartOilWells.Contracts;
using BubbleChartOilWells.Contracts.Models.ViewModels;
using RDotNet;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace BubbleChartOilWells.BusinessLogic.Services
{
    public class NeuralNetService
    {

        REngine engine;

        public NeuralNetService()
        {
            //REngine.SetEnvironmentVariables();
            //engine = REngine.GetInstance();
        }

        public ResultResponse<NeuralNetVM> TrainNeuralNetwork(NeuralNetVM nnVM)
        {

            try
            {
                engine.Evaluate(
                    @"if (!require(""neuralnet"")) install.packages(""neuralnet"", dependencies = TRUE);
                    library(""neuralnet"")
                    if (!require(""NeuralNetTools"")) install.packages(""NeuralNetTools"", dependencies = TRUE);
                    library(NeuralNetTools)");

                engine.Evaluate(
                    @"split_data <- function(data, ratio)
                    {
                        train <- head(data, nrow(data) * ratio)
                        test <- tail(data, nrow(data) - nrow(train))
                        return (list(""train_data"" = train, ""test_data"" = test))
                    }
                    rm(""c"")");

                engine.Evaluate(
                    @"neuralnet_test <- function(filepath,
                                                 ratio,
                                                 hidden,
                                                 threshold,
                                                 learningrate,
                                                 algorithm,
                                                 errorFunc,
                                                 actFunc,
                                                 rep)
                    {
                        imported_data <- read.csv(filepath, sep = "";"", header = TRUE, fileEncoding = ""UTF-8-BOM"")
                        splitted_data <- split_data(imported_data, ratio)

                        train <- splitted_data$train_data
                        test <- splitted_data$test_data

                        min_error <- 999
                        efficient_model <- -1
                
                        for (i in 1:rep)
                        {
                            model <- neuralnet(formula = deltfromGTM~watercut + seamtop + densityRR + pressure + conductivity,
                                               data = train,
                                               hidden = hidden,
                                               threshold = threshold,
                                               learningrate = learningrate,
                                               algorithm = algorithm,
                                               err.fct = errorFunc,
                                               act.fct = actFunc)
                
                            if (min_error > model$result.matrix[1])
                            {
                                min_error <- model$result.matrix[1]
                                efficient_model <- model
                            }
                        }
                        
                        prediction <- predict(efficient_model, test)
                
                        MAPE <- sum(abs(1 - as.data.frame(prediction) / test$deltfromGTM)) / nrow(test)
                        
                        train_result <- cbind(train, as.data.frame(efficient_model$net.result))
                        colnames(train_result) = c(""watercut"",
                                                   ""seamtop"",
                                                   ""densityRR"",
                                                   ""pressure"",
                                                   ""conductivity"",
                                                   ""Expected output"",
                                                   ""NeuralNet output"")

                        test_result <- cbind(test, as.data.frame(prediction))
                        colnames(test_result) = c(""watercut"",
                                                   ""seamtop"",
                                                   ""densityRR"",
                                                   ""pressure"",
                                                   ""conductivity"",
                                                   ""Expected output"",
                                                   ""NeuralNet output"")

                        resultList <- list(""MAPE"" = MAPE,
                                           ""efficient_model"" = efficient_model,
                                           ""train_result"" = train_result,
                                           ""test_result"" = test_result) 
                        return(resultList)
                    }");

                engine.Evaluate(
                    $@"resultList <- neuralnet_test(""{ RFilePathConverter(nnVM.trainingDataFileName)}"",
                                                    0.8,
                                                    {RArrayConverter(nnVM.hidden)},
                                                    {nnVM.threshold.ToString(System.Globalization.CultureInfo.InvariantCulture)},
                                                    {nnVM.learningRate.ToString(System.Globalization.CultureInfo.InvariantCulture)},
                                                    ""{nnVM.algorithm}"",
                                                    ""{nnVM.errorFunction}"",
                                                    ""{nnVM.activationFunction}"",
                                                    {nnVM.rep})");
                engine.Evaluate(
                    $@"MAPE <- resultList$MAPE
                    model <- resultList$efficient_model
                    train_result <- resultList$train_result
                    test_result <- resultList$test_result
                    error <- model$result.matrix[1]");

                nnVM.MAPE = engine.GetSymbol("MAPE").AsNumeric().ToList()[0];
                nnVM.error = engine.GetSymbol("error").AsNumeric().ToList()[0];
                nnVM.Model = engine.GetSymbol("model");

                var trainResult = engine.GetSymbol("train_result").AsDataFrame();
                var testResult = engine.GetSymbol("test_result").AsDataFrame();

                nnVM.TrainResult = RDataFrameConverter(trainResult);
                nnVM.TestResult = RDataFrameConverter(testResult);

                return ResultResponse<NeuralNetVM>.GetSuccessResponse(nnVM);
            }
            catch (Exception e)
            {
                // TODO: write error to log
                return ResultResponse<NeuralNetVM>.GetErrorResponse($@"Ошибка обучения нейросети.{Environment.NewLine}
                                                              {e.Message}{Environment.NewLine}
                                                              {e.StackTrace}");
            }
        }

        public ResultResponse<NeuralNetVM> CalculateLiquidDebitGain(NeuralNetVM nnVM)
        {
            try
            {
                engine.SetSymbol("model", nnVM.Model as SymbolicExpression);
                engine.Evaluate(
                    $@"data <- read.csv(""{RFilePathConverter(nnVM.initialDataFileName)}"", sep = "";"", header = TRUE, fileEncoding = ""UTF-8-BOM"")
                    prediction <- predict(model, data)
                    
                    result <- cbind(data, prediction)
                    colnames(result) = c(""watercut"",
                                         ""seamtop"",
                                         ""densityRR"",
                                         ""pressure"",
                                         ""conductivity"",
                                         ""NeuralNet result"")");

                var CalculatedResult = engine.GetSymbol("result").AsDataFrame();
                nnVM.PredictionResult = RDataFrameConverter(CalculatedResult);

                return ResultResponse<NeuralNetVM>.GetSuccessResponse(nnVM);
            }
            catch (Exception e)
            {
                return ResultResponse<NeuralNetVM>.GetErrorResponse($@"Ошибка вычисления прироста дебита жидкости.{Environment.NewLine}
                                                              {e.Message}{Environment.NewLine}
                                                              {e.StackTrace}");
            }

        }

        private DataTable RDataFrameConverter(DataFrame df)
        {
            var dt = new DataTable();
            for (int i = 0; i < df.ColumnCount; ++i)
            {
                dt.Columns.Add(df.ColumnNames[i]);
            }

            for (int i = 0; i < df.RowCount; i++)
            {
                DataRow newRow = dt.Rows.Add();
                for (int j = 0; j < df.ColumnCount; j++)
                {
                    newRow[j] = df[i, j];
                }
            }

            return dt;
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

        private string RFilePathConverter(string filePath)
        {
            return filePath.Replace("\\", "/");
        }
    }
}
