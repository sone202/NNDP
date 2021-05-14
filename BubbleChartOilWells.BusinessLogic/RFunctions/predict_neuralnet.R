predict_neuralnet <- function(filename,
                    train_data,
                    NN,
                    best.nn.number)
{
  Sys.setlocale("LC_ALL", ".1251")
  
  # номер колонки с выходным параметром
  Q_COL_INDEX = ncol(train_data) - 5
  
  # импорт данных для прогноза из csv-файла
  imported_data <- read.csv(filename, sep=";", header=TRUE, fileEncoding="1251")
  imported_data <- as.data.frame(imported_data)
    # копия данных
  full_data <- imported_data

  # отделяем информацию о скважинах
  oilwell_data <- imported_data[, 1:5]
  imported_data <- imported_data[, 6:(Q_COL_INDEX + 5 - 1)] 
  train_data <- train_data[, 6:(Q_COL_INDEX + 5)] 

  # нормализация данных для прогноза
  imported_data.norm <- as.data.frame(normalize(imported_data, train_data))
  
  # Прогнозирование данных
  NN.predict <- compute(NN, imported_data.norm, rep = best.nn.number)
  
  # денормализация результатов прогноза
  predicted = as.data.frame(NN.predict$net.result)*abs(diff(range(train_data[, Q_COL_INDEX]))) + min(train_data[, Q_COL_INDEX])
  colnames(predicted) <- "Predicted"

  # Сохранение результатов прогноза
  binded_prediction_results <- cbind(oilwell_data, imported_data, predicted)  
  results <- list("BindedPredictionResults" = binded_prediction_results) 

  return(results)
}