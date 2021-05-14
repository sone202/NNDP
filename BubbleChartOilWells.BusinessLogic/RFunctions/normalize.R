# функция нормализации данных
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