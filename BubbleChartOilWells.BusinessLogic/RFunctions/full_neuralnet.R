if (!require("neuralnet")) install.packages("neuralnet", dependencies = TRUE);
library("neuralnet")

#---SETTING WORK DIRECTORY------------------------------------------------------

setwd("C:/Users/KharisovMN/Desktop/Гильметдинов/v.1.0/BubbleChartOilWells/Files/LearnData")

#---FUNCTIONS-------------------------------------------------------------------

split_data <- function(data, ratio)
{
  train <- head(data, nrow(data)*ratio)
  test <- tail(data, nrow(data)-nrow(train))
  
  return(list("train_data" = train, "test_data" = test))
}

extended_neuralnet <- function(filepath,
                           ratio = 0.8,
                           hidden = c(64, 64),
                           threshold = 0.01,
                           learningrate = 0.001,
                           algorithm = "backprop",
                           errorFunc = "sse",
                           actFunc = "logistic",
                           rep = 1)
{
  imported_data <- read.csv(filepath, sep = ";", header = TRUE, fileEncoding = "UTF-8-BOM")
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
  
  resultList <- list("MAPE" = MAPE,
                     "efficient_model" = efficient_model,
                     "train" = train,
                     "test" = test,
                     "prediction" = prediction) 
  return(resultList)
}

#---MAPE RESULTS----------------------------------------------------------------


  results <- extended_neuralnet (filepath = "train.csv",
                            ratio = 0.8,
                            hidden = c(64, 64),
                            threshold = 0.01,
                            learningrate = 0.01,
                            algorithm = "backprop",
                            errorFunc = "sse",
                            actFunc = "logistic",
                            rep = 1)

model <- results$efficient_model
 
show(model$result.matrix[1])

#---TRAINING--------------------------------------------------------------------

# path to the file for training
filepath <- "train.csv"

# read the input file
imported_data <- read.csv(filepath,sep=";", header=TRUE, fileEncoding="UTF-8-BOM")

# splitting data in train and test sets
splitted_data <- split_data(imported_data, 0.7)
train <- splitted_data$train_data

# train the model based on output from input
model <- neuralnet(formula = deltfromGTM~watercut+seamtop+densityRR+pressure+conductivity,
                   data = train,
                   hidden=c(64),
                   threshold=0.01,
                   learningrate = 0.001,
                   linear.output = FALSE,
                   algorithm = "backprop")

# obtain the relative importance of input variables
#garson(model)
#plot(model)

# сheck the data - actual and predicted
#train_output <- cbind(train, as.data.frame(model$net.result))

train_output$NeuralNet_Output <- as.vector(model$net.result)

colnames(train_output) = c("watercut",
                           "seamtop",
                           "densityRR",
                           "pressure",
                           "conductivity",
                           "Expected_Output",
                           "NeuralNet_Output")

cat("\n\n\n\t\t\tTraining results\n\n")
print(train_output)

# write train_output in file
write.table(train_output, "Train_output.csv", sep=";", row.names = FALSE)

#---TESTING---------------------------------------------------------------------

# getting testing data
test <- splitted_data$test_data

# predicting values
prediction <- predict(model, test)

# сheck the data - actual and predicted
test_output <- cbind(test$watercut,
                     test$seamtop,
                     test$densityRR,
                     test$pressure, 
                     test$conductivity, 
                     test$deltfromGTM,
                     as.data.frame(prediction))

colnames(test_output) = c("watercut",
                           "seamtop",
                           "densityRR",
                           "pressure",
                           "conductivity",
                           "Expected_Output",
                           "Neural_Net_Output")

cat("\n\n\n\t\t\tTesting results\n\n")
print(test_output)

# write train_output in file
write.table(test_output, "Test_output.csv", sep=";", row.names = FALSE)

#---PREDICTING------------------------------------------------------------------

# read the input file
predict <- read.csv("predict.csv", sep=";", header=TRUE, fileEncoding="UTF-8-BOM")

# predicting
predicted <- predict(model, predict)

predict_output <- cbind(predict$watercut,
                        predict$seamtop, 
                        predict$densityRR, 
                        predict$pressure, 
                        predict$conductivity,
                        as.data.frame(predicted))

colnames(predict_output) <- c("watercut",
                              "seamtop",
                              "densityRR",
                              "pressure",
                              "conductivity",
                              "Neural_Net_Output" )

cat("\n\n\n\t\t\tPrediction results\n\n")
print(predict_output)

# write predict_output in file
write.table(predict_output, "Predicted_output.csv", sep=";", row.names = FALSE)

#---MAPE------------------------------------------------------------------------

MAPE <- sum(abs(1 - as.data.frame(model$net.result)/train$deltfromGTM))/nrow(train)


  