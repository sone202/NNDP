extended_neuralnet <- function(filepath,
                               ratio,
                               formula,
                               hidden,
                               threshold,
                               stepmax,
                               rep,
                               learningrate,
                               algorithm,
                               errorFunc,
                               actFunc,
                               seed)
{
  
  
  # импорт данных из csv-файла
  imported_data <- read.csv(filepath, sep = ";", header = TRUE, fileEncoding = "UTF-8")
  imported_data <- as.data.frame(imported_data)
  
  # for(i in 1:ncol(imported_data))
  # {
  #   imported_data[,i] = iconv(imported_data[,i], from = "Windows-1251", to="UTF-8")
  # }
  
  # копия данных
  full_data <- imported_data
  
  # номер колонки с выходным параметром
  Q_COL_INDEX = ncol(imported_data) - 5
  
  
  # перемешивание и разделение данных
  imported_data.train = sample_frac(tbl = imported_data, replace = FALSE, size = ratio)
  imported_data.test = anti_join(imported_data, imported_data.train)
  
  # отделяем информацию о скважинах от значений ее пареметров
  oilwell_data.train <- imported_data.train[, 1:5]
  imported_data.train <- imported_data.train[, 6:(Q_COL_INDEX + 5)]
  
  oilwell_data.test <- imported_data.test[, 1:5]
  imported_data.test <- imported_data.test[, 6:(Q_COL_INDEX + 5)]
  
  imported_data <- imported_data[, 6:(Q_COL_INDEX + 5)]
  
  # нормализация данных
  train <- as.data.frame(normalize(imported_data.train, imported_data))
  test <- as.data.frame(normalize(imported_data.test, imported_data))
  
  # обучение нейронной сети
  #set.seed(seed)
  NN <- neuralnet(formula = formula,
                  data = train,
                  hidden = hidden,
                  threshold = threshold,
                  stepmax = stepmax,
                  rep = rep,
                  learningrate = learningrate,
                  #algorithm = algorithm,
                  err.fct = errorFunc,
                  act.fct = actFunc,
                  linear.output = TRUE)
  
  # лучшая нейронная сеть
  best.nn.number <- which.min(NN$result.matrix[1,])
  
  # денормализация результатов обучения нейронной сети
  predicted_train = as.data.frame(NN$net.result[best.nn.number])*abs(diff(range(imported_data[, Q_COL_INDEX]))) + min(imported_data[, Q_COL_INDEX])
  actual_train = as.data.frame(train[, Q_COL_INDEX])*abs(diff(range(imported_data[, Q_COL_INDEX]))) + min(imported_data[, Q_COL_INDEX])    
  
  colnames(predicted_train) <- c("predicted_train")
  binded_train_results <- cbind(oilwell_data.train, imported_data.train, predicted_train)
  
  # определение SSE
  NN_train_SSE <- sum((as.data.frame(NN$net.result[best.nn.number]) - train[, Q_COL_INDEX])^2)/2
  NN_train_SSE <- round(NN_train_SSE, 4)
  
  # определение MAPE
  NN_train_MAPE <- 100*sum(abs((actual_train - predicted_train)/actual_train))/nrow(actual_train)
  NN_train_MAPE <- round(NN_train_MAPE, 1)
  
  # определение MAE
  NN_train_MAE <- sum(abs(actual_train - predicted_train))/nrow(actual_train)
  NN_train_MAE <- round(NN_train_MAE, 2)
  
  
  #--------------------ТЕСТИРОВАНИЕ----------------------------------------------------------
  NN.test <- compute(NN, test[, 1:(Q_COL_INDEX-1)], rep = best.nn.number)
  
  # денормализация результатов обучения нейронной сети
  predicted_test = as.data.frame(NN.test$net.result)*abs(diff(range(imported_data[, Q_COL_INDEX]))) + min(imported_data[, Q_COL_INDEX])
  actual_test = as.data.frame(test[, Q_COL_INDEX])*abs(diff(range(imported_data[, Q_COL_INDEX]))) + min(imported_data[, Q_COL_INDEX])
  
  colnames(predicted_test) <- c("predicted_test")
  binded_test_results <- cbind(oilwell_data.test, imported_data.test, predicted_test)
  
  # определение MAPE
  NN_test_MAPE <- 100*sum(abs((actual_test - predicted_test)/actual_test))/nrow(actual_test)
  NN_test_MAPE <-  round(NN_test_MAPE, 1)
  
  # определение MAE
  NN_test_MAE <- sum(abs(actual_test - predicted_test))/nrow(actual_test)
  NN_test_MAE <- round(NN_test_MAE, 2)
  
  
  
  results <- list("TrainMAPE" = NN_train_MAPE,
                  "TrainMAE" = NN_train_MAE,
                  "TrainSSE" = NN_train_SSE,
                  "TestMAPE" = NN_test_MAPE,
                  "TestMAE" = NN_test_MAE,
                  "InitialTrainData" =  full_data,
                  "TrainActual" = actual_train[,1],
                  "TrainPredicted" = predicted_train[,1],
                  "TestActual" = actual_test[,1],
                  "TestPredicted" = predicted_test[,1],
                  "BindedTrainResults" =  binded_train_results,
                  "BindedTestResults" =  binded_test_results,
                  "BestNNIndex" = best.nn.number,
                  "NN" = NN) 
  return(results)
}