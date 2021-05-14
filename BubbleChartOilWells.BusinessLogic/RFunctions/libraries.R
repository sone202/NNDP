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