split_data <- function(data, ratio)
{
  train <- head(data, nrow(data) * ratio)
  test <- tail(data, nrow(data) - nrow(train))
  return (list("train_data" = train, "test_data" = test))
}