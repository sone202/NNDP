# ��������� � �������� ���������
if (!require("stats")) install.packages("stats", dependencies = TRUE);
library(stats)
if (!require("tidyverse")) install.packages("tidyverse", dependencies = TRUE);
library(tidyverse)
if (!require("GGally")) install.packages("GGally", dependencies = TRUE);
library(GGally)
if (!require("neuralnet")) install.packages("neuralnet", dependencies = TRUE);
library(neuralnet)
if (!require("Rsagacmd")) install.packages("Rsagacmd", dependencies = TRUE);
library(Rsagacmd)

# ������ ������ �� csv-�����
imported_data <- read.csv("data.csv", sep=";", header=TRUE, fileEncoding="UTF-8-BOM")
imported_data <- as.data.frame(imported_data)

# ������� ������������ ������
normalize <- function(x, y) {
  normal.matrix <- 0
  for (i in 1:ncol(x)) {
    normal.col <- round(((x[,i] - min(y[,i])) / (max(y[,i]) - min(y[,i]))),6)
    normal.matrix <- cbind(normal.matrix,normal.col)
    colnames(normal.matrix)[i+1] <- colnames(x)[i]
  }
  normal.matrix <- normal.matrix[,2:ncol(normal.matrix)]
  return (normal.matrix)
}

# ������������ ������
imported_data.norm <- as.data.frame(normalize(imported_data,imported_data))

# # ����������� ������ �� ���������
# ggpairs(imported_data, title = "Scatterplot Matrix of the Features of the Data Set")

# ���������� ������ �� ��������� � �������� �������
imported_data_train <- sample_frac(tbl = imported_data.norm, replace = FALSE, size = 0.80)
imported_data_test <- anti_join(imported_data.norm, imported_data_train)

#---��������-----------------------------------------------------------------------------

# �������� ��������� ����
set.seed(12321)
NN <- neuralnet(formula = Q ~ K + H + Pr + Pwf,
                data = imported_data_train,
                hidden=c(8,8),
                linear.output=TRUE,
                threshold=0.0001,
                learningrate = 0.001,
                rep = 5,
                )

# ������ ��������� ����
best.nn.number <- which.min(NN$result.matrix[1,])

# ����������� ��������� ����
plot(NN, rep = 'best')
NN_train_SSE <- sum((as.data.frame(NN$net.result[best.nn.number]) - imported_data_train[, 5])^2)/2
paste("SSE: ", round(NN_train_SSE, 4))

# �������������� ����������� �������� ��������� ����
predicted = as.data.frame(NN$net.result[best.nn.number])*abs(diff(range(imported_data$Q))) + min(imported_data$Q)
actual = as.data.frame(imported_data_train[, 5])*abs(diff(range(imported_data$Q))) + min(imported_data$Q)

# �����-���� ����������� �������� ��������� ����
plot(cbind(predicted, actual), xlab = "Predicted values", ylab = "Actual values")

# ����������� MAPE � MAE
NN_train_MAPE <- 100*sum(abs((actual - predicted)/actual))/nrow(actual)
paste("Train MAPE: ", round(NN_train_MAPE, 1),"%")
NN_train_MAE <- sum(abs(actual - predicted))/nrow(actual)
paste("Train MAE: ", round(NN_train_MAE, 2))

# ���������� ����������� �������� ��������� ����
NN.results <- cbind(actual, predicted)
colnames(NN.results) <- c("Actual","Predicted")
write.table(NN.results,'Train_Results.csv', sep = ";", row.names = FALSE, quote = FALSE)

#---������������---------------------------------------------------------------------------

# ������������ ��������� ����
NN.test <- compute(NN, imported_data_test[,1:4], rep = best.nn.number)

# �������������� ����������� ������������ ��������� ����
predicted = as.data.frame(NN.test$net.result)*abs(diff(range(imported_data$Q))) + min(imported_data$Q)
actual = as.data.frame(imported_data_test[, 5])*abs(diff(range(imported_data$Q))) + min(imported_data$Q)
# �����-���� ����������� �������� ��������� ����
plot(cbind(predicted, actual), xlab = "Predicted values", ylab = "Actual values")

# ����������� MAPE � MAE
NN_test_MAPE <- 100*sum(abs((actual - predicted)/actual))/nrow(actual)
paste("Test MAPE: ", round(NN_test_MAPE, 1),"%")
NN_test_MAE <- sum(abs(actual - predicted))/nrow(actual)
paste("Test MAE: ", round(NN_train_MAE, 2))

# ���������� ����������� ������������ ��������� ����
NN.results <- cbind(actual, predicted)
colnames(NN.results) <- c("Actual","Predicted")
write.table(NN.results,'Test_Results.csv', sep = ";", row.names = FALSE, quote = FALSE)

#---������Ǘ--------------------------------------------------------------------------------

# ������ ������ ��� �������� �� csv-�����
imported_data.predict <- read.csv("prediction.csv", sep=";", header=TRUE, fileEncoding="UTF-8-BOM")
imported_data.predict <- as.data.frame(imported_data.predict[,1:5])

# ������������ ������ ��� ��������
imported_data.predict.norm <- as.data.frame(normalize(imported_data.predict,imported_data))

# ��������������� ������
NN.predict <- compute(NN, imported_data.predict.norm[,1:4], rep = best.nn.number)

# �������������� ����������� ��������
predicted = as.data.frame(NN.predict$net.result)*abs(diff(range(imported_data$Q))) + min(imported_data$Q)
actual = as.data.frame(imported_data.predict[, 5])
# ����������� MAPE � MAE
plot(cbind(predicted, actual), xlab = "Predicted values", ylab = "Actual values")

# ����������� MAPE � MAE
NN_predict_MAPE <- 100*sum(abs((actual - predicted)/actual))/nrow(actual)
paste("Prediction MAPE: ", round(NN_predict_MAPE, 1),"%")
NN_predict_MAE <- sum(abs(actual - predicted))/nrow(actual)
paste("Prediction MAE: ", round(NN_predict_MAE, 2))

# ���������� ����������� ��������
NN.results <- cbind(imported_data.predict, predicted)
colnames(NN.results)[ncol(NN.results)] <- "Q.prediction"
write.table(NN.results,'prediction.csv', sep = ";", row.names = FALSE, quote = FALSE)
  
 
