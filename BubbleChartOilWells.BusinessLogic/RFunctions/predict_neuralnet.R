predict_neuralnet <- function(filename,
                    train_data,
                    NN,
                    best.nn.number)
{
  Sys.setlocale("LC_ALL", ".1251")
  
  # ����� ������� � �������� ����������
  Q_COL_INDEX = ncol(train_data) - 5
  
  # ������ ������ ��� �������� �� csv-�����
  imported_data <- read.csv(filename, sep=";", header=TRUE, fileEncoding="1251")
  imported_data <- as.data.frame(imported_data)
    # ����� ������
  full_data <- imported_data

  # �������� ���������� � ���������
  oilwell_data <- imported_data[, 1:5]
  imported_data <- imported_data[, 6:(Q_COL_INDEX + 5 - 1)] 
  train_data <- train_data[, 6:(Q_COL_INDEX + 5)] 

  # ������������ ������ ��� ��������
  imported_data.norm <- as.data.frame(normalize(imported_data, train_data))
  
  # ��������������� ������
  NN.predict <- compute(NN, imported_data.norm, rep = best.nn.number)
  
  # �������������� ����������� ��������
  predicted = as.data.frame(NN.predict$net.result)*abs(diff(range(train_data[, Q_COL_INDEX]))) + min(train_data[, Q_COL_INDEX])
  colnames(predicted) <- "Predicted"

  # ���������� ����������� ��������
  binded_prediction_results <- cbind(oilwell_data, imported_data, predicted)  
  results <- list("BindedPredictionResults" = binded_prediction_results) 

  return(results)
}