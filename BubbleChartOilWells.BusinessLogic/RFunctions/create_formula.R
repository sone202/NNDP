# �������� �������
create_formula <- function(headers)
{
  formula <- headers[length(headers)]
  formula <- paste(formula, " ~ ", sep = "")
  for (i in 1:(length(headers) - 1))
  {
    formula <- paste(formula, headers[i], sep = "")
    if (i != length(headers) - 1)
    {
      formula <- paste(formula, " + ", sep = "")
    }
  }
  return (formula)
}