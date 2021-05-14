# установка и загрузка библиотек
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
library(ggplot2)

# импорт данных из csv-файла
mydata <- read.csv("data.csv", sep=";", header=TRUE, fileEncoding="UTF-8-BOM")
mydata <- as.data.frame(mydata)
# отображение заголовка данных в консоли
head(mydata)

# нормализация данных
normalize <- function(x) {
  return ((x - min(x)) / (max(x) - min(x)))
}
mydata.norm <- as.data.frame(lapply(mydata, normalize))
# отображение заголовка нормализованных данных в консоли
head(mydata.norm)

# # отображение данных на диаграмме
# ggpairs(mydata, title = "Scatterplot Matrix of the Features of the Data Set")

# разделение данных на обучающую и тестовую выборки
mydata_train <- sample_frac(tbl = mydata.norm, replace = FALSE, size = 0.80)
mydata_test <- anti_join(mydata.norm, mydata_train)

#---ОБУЧЕНИЕ-----------------------------------------------------------------------------

# обучение нейронной сети
set.seed(12321)
NN <- neuralnet(formula = Q ~ K + H + Pr + Pwf,
                data = mydata_train,
                hidden=c(16,16),
                linear.output=TRUE,
                threshold=0.0001,
                learningrate = 0.001,
                )

# отображение нейронной сети
NN_train_SSE <- sum((as.data.frame(NN$net.result) - mydata_train[, 5])^2)/2
paste("SSE: ", round(NN_train_SSE, 4))

# денормализация результатов обучения нейронной сети
predicted = as.data.frame(NN$net.result)*abs(diff(range(mydata$Q))) + min(mydata$Q)
actual = as.data.frame(mydata_train[, 5])*abs(diff(range(mydata$Q))) + min(mydata$Q)
# Кросс-плот результатов обучения нейронной сети


binded <- cbind(actual, predicted)
colnames(binded) = c("actual", "predicted")
print(ggplot(data = binded, aes(x=predicted, y=actual)) + 
        geom_point(lwd = 5) + 
        ggtitle("���������� ��������") + 
        xlab("���������� ��������") + 
        ylab("����������� ��������") + 
        theme_bw())

# Определение MAPE и MAE
NN_train_MAPE <- 100*sum(abs((actual - predicted)/actual))/nrow(actual)
paste("Train MAPE: ", round(NN_train_MAPE, 1),"%")
NN_train_MAE <- sum(abs(actual - predicted))/nrow(actual)
paste("Train MAE: ", round(NN_train_MAE, 2))

write.csv(cbind(actual,predicted),'Train_Results.csv')

#---ТЕСТИРОВАНИЕ---------------------------------------------------------------------------

# Тестирование нейронной сети
NN.test <- compute(NN, mydata_test[,1:4])

# денормализация результатов тестирования нейронной сети
predicted = as.data.frame(NN.test$net.result)*abs(diff(range(mydata$Q))) + min(mydata$Q)
actual = as.data.frame(mydata_test[, 5])*abs(diff(range(mydata$Q))) + min(mydata$Q)
# Кросс-плот результатов обучения нейронной сети





binded <- cbind(actual, predicted)
colnames(binded) = c("actual", "predicted")
print(ggplot(data = binded, aes(x=predicted, y=actual)) + 
        geom_point(lwd = 5) + 
        ggtitle("���������� ������������") + 
        xlab("���������� ��������") + 
        ylab("����������� ��������") + 
        theme_bw())












# Определение MAPE и MAE
NN_test_MAPE <- 100*sum(abs((actual - predicted)/actual))/nrow(actual)
paste("Test MAPE: ", round(NN_test_MAPE, 1),"%")
NN_test_MAE <- sum(abs(actual - predicted))/nrow(actual)
paste("Test MAE: ", round(NN_train_MAE, 2))

write.csv(cbind(actual,predicted),'Test_Results.csv')

#---ПРОГНОЗ--------------------------------------------------------------------------------

# импорт данных для прогноза из csv-файла
mydata.predict <- read.csv("predict.csv", sep=";", header=TRUE, fileEncoding="UTF-8-BOM")
mydata.predict <- as.data.frame(mydata.predict)
# отображение заголовка данных для прогноза в консоли
head(mydata.predict)

# нормализация данных для прогноза
mydata.predict.norm <- as.data.frame(lapply(mydata.predict, normalize))
# отображение заголовка нормализованных данных в консоли
head(mydata.predict.norm)

# Прогнозирование данных
NN.predict <- compute(NN, mydata.predict.norm[,1:4])

# денормализация результатов прогноза
predicted = as.data.frame(NN.predict$net.result)*abs(diff(range(mydata.predict$Q))) + min(mydata.predict$Q)
actual = as.data.frame(mydata.predict[, 5])
# Кросс-плот результатов обучения нейронной сети
ggplot(cbind(predicted, actual), xlab = "Predicted values", ylab = "Actual values", main = "���������� ������������")

# Определение MAPE и MAE
NN_predict_MAPE <- 100*sum(abs((actual - predicted)/actual))/nrow(actual)
paste("Prediction MAPE: ", round(NN_predict_MAPE, 1),"%")
NN_predict_MAE <- sum(abs(actual - predicted))/nrow(actual)
paste("Prediction MAE: ", round(NN_predict_MAE, 2))

write.csv(cbind(actual,predicted),'Prediction_Results.csv')
 
